import json
import boto3
import psycopg2
import jwt
import os
from datetime import datetime, timedelta

secrets_client = boto3.client('secretsmanager')

def get_secret(secret_arn):
    response = secrets_client.get_secret_value(SecretId=secret_arn)
    return response['SecretString']

def get_db_connection():
    db_secret_string = get_secret(os.environ['RDS_SECRET_ARN'])
    db_secret = json.loads(db_secret_string)
    conn = psycopg2.connect(
        host=db_secret['host'],
        port=db_secret['port'],
        user=db_secret['username'],
        password=db_secret['password'],
        dbname=db_secret['dbname']
    )
    return conn

def handler(event, context):
    if 'httpMethod' in event and event['httpMethod'] == 'GET' and event['path'] == '/login-cliente':
        return handle_login(event)
    else:
        return handle_authorization(event)

import re

def is_cpf_valid(cpf):
    cpf = ''.join(re.findall(r'\d', str(cpf)))
    if not cpf or len(cpf) != 11:
        return False

    # Validação do primeiro dígito verificador
    soma = sum(int(cpf[i]) * (10 - i) for i in range(9))
    resto = (soma * 10) % 11
    if resto == 10:
        resto = 0
    if resto != int(cpf[9]):
        return False

    # Validação do segundo dígito verificador
    soma = sum(int(cpf[i]) * (11 - i) for i in range(10))
    resto = (soma * 10) % 11
    if resto == 10:
        resto = 0
    if resto != int(cpf[10]):
        return False

    return True

def handle_login(event):
    cpf = event['queryStringParameters'].get('cpf')

    if not is_cpf_valid(cpf):
        return {
            'statusCode': 400,
            'body': json.dumps({'message': 'Invalid CPF format'})
        }

    conn = get_db_connection()
    cursor = conn.cursor()
    cursor.execute("SELECT id FROM \"Clientes\" WHERE \"Documento\" = %s AND \"Ativo\" = true", (cpf,))
    user = cursor.fetchone()
    cursor.close()
    conn.close()

    if user:
        jwt_secret = get_secret(os.environ['JWT_SECRET_ARN'])
        token = jwt.encode(
            {'user_id': str(user[0]), 'exp': datetime.utcnow() + timedelta(days=1)},
            jwt_secret,
            algorithm='HS256'
        )
        return {
            'statusCode': 200,
            'body': json.dumps({'token': token})
        }
    else:
        return {
            'statusCode': 401,
            'body': json.dumps({'message': 'Unauthorized'})
        }

def handle_authorization(event):
    authorization_header = event['headers'].get('Authorization')
    if not authorization_header:
        return {"isAuthorized": False}

    try:
        token = authorization_header.split(' ')[1]
        jwt_secret = get_secret(os.environ['JWT_SECRET_ARN'])
        decoded_token = jwt.decode(token, jwt_secret, algorithms=['HS256'])
        return {
            "isAuthorized": True,
            "context": {
                "user_id": decoded_token['user_id']
            }
        }
    except (jwt.ExpiredSignatureError, jwt.InvalidTokenError, IndexError):
        return {"isAuthorized": False}
