# lambda/authenticator/main.py

import os
import json
import psycopg2
import jwt
import datetime

def lambda_handler(event, context):
    """
    Handles API Gateway authorization.
    Verifies CPF, queries the database, and returns an IAM policy.
    """
    try:
        # 1. Extract token (CPF) from headers
        cpf = event['headers'].get('x-cpf')
        if not cpf:
            return generate_policy('user', 'Deny', event['methodArn'])

        # 2. Database connection
        conn = psycopg2.connect(
            host=os.environ['DB_HOST'],
            database=os.environ['DB_NAME'],
            user=os.environ['DB_USER'],
            password=os.environ['DB_PASSWORD'],
            port=os.environ['DB_PORT']
        )
        cursor = conn.cursor()

        # 3. Query client
        cursor.execute("SELECT id, nome, status FROM \"Clientes\" WHERE cpf = %s", (cpf,))
        client = cursor.fetchone()
        cursor.close()
        conn.close()

        if not client:
            return generate_policy('user', 'Deny', event['methodArn'])

        client_id, client_name, client_status = client

        # 4. Check client status
        if client_status != 'ATIVO':
             return generate_policy('user', 'Deny', event['methodArn'])

        # 5. Generate JWT
        jwt_secret = os.environ['JWT_SECRET']
        jwt_payload = {
            'sub': client_id,
            'name': client_name,
            'cpf': cpf,
            'exp': datetime.datetime.utcnow() + datetime.timedelta(minutes=int(os.environ['JWT_EXPIRY_MINUTES']))
        }
        auth_token = jwt.encode(jwt_payload, jwt_secret, algorithm='HS256')

        # 6. Generate IAM policy
        policy = generate_policy(client_id, 'Allow', event['methodArn'])
        policy['context'] = {
            'token': auth_token
        }
        return policy

    except Exception as e:
        print(f"Error: {e}")
        return generate_policy('user', 'Deny', event['methodArn'])

def generate_policy(principal_id, effect, resource):
    """
    Generates an IAM policy document.
    """
    return {
        'principalId': principal_id,
        'policyDocument': {
            'Version': '2012-10-17',
            'Statement': [{
                'Action': 'execute-api:Invoke',
                'Effect': effect,
                'Resource': resource
            }]
        }
    }
