# ============================================
# JWT Secret for Lambda Authorizer
# ============================================

resource "random_string" "jwt_secret" {
  length  = 32
  special = true
}

resource "aws_secretsmanager_secret" "jwt_secret" {
  name = "${var.project_name}-jwt-secret"
  description = "JWT secret for the Lambda authorizer"
}

resource "aws_secretsmanager_secret_version" "jwt_secret_version" {
  secret_id     = aws_secretsmanager_secret.jwt_secret.id
  secret_string = random_string.jwt_secret.result
}
