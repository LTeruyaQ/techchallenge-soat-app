import os
import json
import psycopg2
import jwt
import datetime

def lambda_handler(event, context):
    """
    Handles user authentication.
    Validates a CPF against the database and returns a JWT if successful.
    """
    try:
        body = json.loads(event.get('body', '{}'))
        cpf = body.get('cpf')

        if not cpf or not is_cpf_valid(cpf):
            return {
                'statusCode': 400,
                'body': json.dumps({'message': 'CPF inválido.'})
            }

        # Conectar ao banco de dados
        conn = psycopg2.connect(
            host=os.environ['DB_HOST'],
            port=os.environ['DB_PORT'],
            dbname=os.environ['DB_NAME'],
            user=os.environ['DB_USER'],
            password=os.environ['DB_PASSWORD']
        )
        cursor = conn.cursor()

        # Verificar se o cliente existe e está ativo
        cursor.execute("SELECT Id, Nome FROM public.Clientes WHERE Documento = %s AND Ativo = TRUE", (cpf,))
        user_record = cursor.fetchone()

        cursor.close()
        conn.close()

        if user_record:
            user_id, user_name = user_record
            # Gerar o token JWT
            token = create_jwt(str(user_id), user_name)
            return {
                'statusCode': 200,
                'body': json.dumps({'token': token})
            }
        else:
            return {
                'statusCode': 404,
                'body': json.dumps({'message': 'Cliente não encontrado ou inativo.'})
            }

    except Exception as e:
        # Log do erro (visível no CloudWatch)
        print(f"Erro: {e}")
        return {
            'statusCode': 500,
            'body': json.dumps({'message': 'Erro interno do servidor.'})
        }

def is_cpf_valid(cpf: str) -> bool:
    """Valida o formato de um CPF."""
    return cpf and isinstance(cpf, str) and len(cpf) == 11 and cpf.isdigit()

def create_jwt(user_id: str, user_name: str) -> str:
    """Cria um token JWT."""
    payload = {
        'sub': user_id,
        'name': user_name,
        'iss': os.environ['JWT_ISSUER'],
        'aud': os.environ['JWT_AUDIENCE'],
        'iat': datetime.datetime.utcnow(),
        'exp': datetime.datetime.utcnow() + datetime.timedelta(minutes=int(os.environ.get('JWT_EXPIRY_MINUTES', 120)))
    }
    secret_key = os.environ['JWT_SECRET_KEY']
    token = jwt.encode(payload, secret_key, algorithm='HS256')
    return token
