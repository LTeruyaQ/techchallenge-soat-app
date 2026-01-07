import json
import os
import boto3
import psycopg2
import jwt
import re
from datetime import datetime, timedelta

def get_secret(secret_name):
    """Retrieves a secret from AWS Secrets Manager."""
    client = boto3.client('secretsmanager')
    try:
        get_secret_value_response = client.get_secret_value(SecretId=secret_name)
        if 'SecretString' in get_secret_value_response:
            return json.loads(get_secret_value_response['SecretString'])
        else:
            # Handle binary secret if needed
            return get_secret_value_response['SecretBinary']
    except Exception as e:
        print(f"Error retrieving secret {secret_name}: {e}")
        raise e

def validate_cpf(cpf):
    """Basic CPF validation."""
    if not re.match(r'^\d{11}$', cpf):
        return False
    return True

def lambda_handler(event, context):
    """
    Handles user authentication by validating CPF against the RDS database
    and issuing a JWT.
    """
    try:
        body = json.loads(event.get('body', '{}'))
        cpf = body.get('cpf')

        if not cpf or not validate_cpf(cpf):
            return {
                'statusCode': 400,
                'body': json.dumps({'message': 'Invalid or missing CPF'})
            }

        # Get DB credentials and JWT secret from Secrets Manager
        db_secret_arn = os.environ['DB_SECRET_ARN']
        db_secrets = get_secret(db_secret_arn)

        jwt_secret_name = os.environ['JWT_SECRET_NAME']
        jwt_secret_payload = get_secret(jwt_secret_name)
        jwt_secret = jwt_secret_payload['secret']


        # Connect to the database
        conn = psycopg2.connect(
            host=db_secrets['host'],
            port=db_secrets['port'],
            dbname=db_secrets['dbname'],
            user=db_secrets['username'],
            password=db_secrets['password']
        )

        cursor = conn.cursor()

        # Check if client exists and is active
        query = "SELECT Id FROM public.Clientes WHERE Documento = %s AND Ativo = TRUE;"
        cursor.execute(query, (cpf,))
        result = cursor.fetchone()

        cursor.close()
        conn.close()

        if not result:
            return {
                'statusCode': 401,
                'body': json.dumps({'message': 'Unauthorized: Client not found or inactive'})
            }

        client_id = result[0]

        # Generate JWT
        payload = {
            'sub': client_id,
            'cpf': cpf,
            'exp': datetime.utcnow() + timedelta(hours=1)
        }

        token = jwt.encode(payload, jwt_secret, algorithm='HS256')

        return {
            'statusCode': 200,
            'body': json.dumps({'token': token})
        }

    except psycopg2.Error as e:
        print(f"Database error: {e}")
        return {
            'statusCode': 500,
            'body': json.dumps({'message': 'Internal Server Error: Database connection failed'})
        }
    except Exception as e:
        print(f"An unexpected error occurred: {e}")
        return {
            'statusCode': 500,
            'body': json.dumps({'message': 'Internal Server Error'})
        }
