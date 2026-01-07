# lambda_authorizer/lambda_handler.py
import os
import jwt

def lambda_handler(event, context):
    """
    Lambda Authorizer para validar um JWT vindo do header 'Authorization'.
    """

    # 1. Obter o token do header
    token = event.get('headers', {}).get('authorization', '').replace('Bearer ', '')

    if not token:
        # Nega o acesso se não houver token
        return generate_policy('user', 'Deny', event['methodArn'])

    # 2. Decodificar e validar o token
    try:
        jwt_secret = os.environ['JWT_SECRET']
        decoded = jwt.decode(token, jwt_secret, algorithms=['HS256'])

        # Extrai o CPF ou outro identificador do token para usar como principalId
        principal_id = decoded.get('cpf', 'user')

        # Permite o acesso se o token for válido
        return generate_policy(principal_id, 'Allow', event['methodArn'])

    except jwt.ExpiredSignatureError:
        # Token expirado
        return generate_policy('user', 'Deny', event['methodArn'])
    except jwt.InvalidTokenError:
        # Token inválido
        return generate_policy('user', 'Deny', event['methodArn'])
    except Exception as e:
        # Outros erros
        print(f"Erro inesperado: {e}")
        return generate_policy('user', 'Deny', event['methodArn'])

def generate_policy(principal_id, effect, resource):
    """Gera o documento de política IAM."""
    policy = {
        'principalId': principal_id,
        'policyDocument': {
            'Version': '2012-10-17',
            'Statement': [
                {
                    'Action': 'execute-api:Invoke',
                    'Effect': effect,
                    'Resource': resource,
                }
            ],
        },
    }
    return policy
