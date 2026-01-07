import boto3
import json
import os
import psycopg2
import jwt
import re
from datetime import datetime, timedelta, timezone

# Caching for secrets and DB connection
secrets_cache = {}
db_connection = None

def get_secret(secret_arn):
    """Fetches a secret from AWS Secrets Manager, with caching."""
    if secret_arn in secrets_cache:
        return secrets_cache[secret_arn]

    try:
        session = boto3.session.Session()
        client = session.client(service_name='secretsmanager')
        get_secret_value_response = client.get_secret_value(SecretId=secret_arn)
        secret = get_secret_value_response['SecretString']
        secrets_cache[secret_arn] = json.loads(secret)
        return secrets_cache[secret_arn]
    except Exception as e:
        print(f"Error fetching secret {secret_arn}: {e}")
        raise e

def get_db_connection():
    """Establishes or reuses a database connection."""
    global db_connection
    if db_connection:
        # Check if the connection is still alive
        try:
            cur = db_connection.cursor()
            cur.execute("SELECT 1")
            cur.close()
            return db_connection
        except psycopg2.OperationalError:
            db_connection = None # Stale connection

    try:
        rds_secret_arn = os.environ['RDS_SECRET_ARN']
        rds_credentials = get_secret(rds_secret_arn)

        db_connection = psycopg2.connect(
            host=rds_credentials['host'],
            port=rds_credentials['port'],
            user=rds_credentials['username'],
            password=rds_credentials['password'],
            database=rds_credentials['dbname'],
            connect_timeout=5
        )
        return db_connection
    except Exception as e:
        print(f"Error connecting to database: {e}")
        raise e

def validate_cpf(cpf):
    """Validates the CPF format."""
    return re.fullmatch(r'\d{11}', cpf) is not None

def generate_iam_policy(effect, resource):
    """Generates an IAM policy document for the API Gateway authorizer."""
    return {
        "principalId": "user",
        "policyDocument": {
            "Version": "2012-10-17",
            "Statement": [{
                "Action": "execute-api:Invoke",
                "Effect": effect,
                "Resource": resource
            }]
        }
    }

def handle_authentication(event):
    """
    Handles the authentication flow: receives a CPF, validates it against the DB,
    and returns a new JWT.
    """
    try:
        body = json.loads(event.get('body', '{}'))
        cpf = body.get('cpf')

        if not cpf or not validate_cpf(cpf):
            return {"statusCode": 400, "body": json.dumps({"message": "Invalid or missing CPF"})}

        conn = get_db_connection()
        with conn.cursor() as cur:
            cur.execute("SELECT Ativo FROM Clientes WHERE Documento = %s", (cpf,))
            result = cur.fetchone()

        if not result or not result[0]:
            return {"statusCode": 404, "body": json.dumps({"message": "Client not found or inactive"})}

        jwt_secret_arn = os.environ['JWT_SECRET_ARN']
        jwt_secret = get_secret(jwt_secret_arn)['secret_key']

        payload = {
            "sub": cpf,
            "iss": "MecanicaOS",
            "aud": "MecanicaOS-API",
            "iat": datetime.now(timezone.utc),
            "exp": datetime.now(timezone.utc) + timedelta(minutes=120)
        }

        token = jwt.encode(payload, jwt_secret, algorithm="HS256")

        return {
            "statusCode": 200,
            "body": json.dumps({"token": token})
        }

    except Exception as e:
        print(f"Authentication error: {e}")
        return {"statusCode": 500, "body": json.dumps({"message": "Internal server error"})}

def handle_authorization(event):
    """
    Handles the authorization flow: receives a JWT, validates it, and returns
    an IAM policy to allow or deny access.
    """
    try:
        method_arn = event['methodArn']
        auth_header = event['headers'].get('authorization', '')

        if not auth_header.startswith('Bearer '):
            print("Authorization header missing or malformed")
            return generate_iam_policy('Deny', method_arn)

        token = auth_header.split(' ')[1]
        jwt_secret_arn = os.environ['JWT_SECRET_ARN']
        jwt_secret = get_secret(jwt_secret_arn)['secret_key']

        jwt.decode(
            token,
            jwt_secret,
            algorithms=["HS256"],
            issuer="MecanicaOS",
            audience="MecanicaOS-API"
        )

        return generate_iam_policy('Allow', method_arn)

    except (jwt.ExpiredSignatureError, jwt.InvalidTokenError) as e:
        print(f"JWT validation error: {e}")
        return generate_iam_policy('Deny', method_arn)
    except Exception as e:
        print(f"Authorization error: {e}")
        return generate_iam_policy('Deny', method_arn)

def handler(event, context):
    """
    Main Lambda handler. Differentiates between a direct invocation (authentication)
    and an authorizer invocation.
    """
    if event.get('type') == 'REQUEST':
        # This is an API Gateway REQUEST authorizer event
        return handle_authorization(event)
    else:
        # This is a direct invocation for authentication (e.g., from /auth endpoint)
        return handle_authentication(event)
