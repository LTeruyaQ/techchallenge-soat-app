# ============================================
# AWS Secrets Manager
# ============================================

resource "random_password" "rds_master_password" {
  length  = 16
  special = true
}

resource "random_string" "jwt_secret_key" {
  length  = 32
  special = false
}

resource "aws_secretsmanager_secret" "rds_credentials" {
  name = "${var.project_name}-rds-credentials"
}

resource "aws_secretsmanager_secret_version" "rds_credentials" {
  secret_id = aws_secretsmanager_secret.rds_credentials.id
  secret_string = jsonencode({
    username = "postgres"
    password = random_password.rds_master_password.result
  })
}

resource "aws_secretsmanager_secret" "jwt_key" {
  name = "${var.project_name}-jwt-key"
}

resource "aws_secretsmanager_secret_version" "jwt_key" {
  secret_id = aws_secretsmanager_secret.jwt_key.id
  secret_string = jsonencode({
    secret_key = random_string.jwt_secret_key.result
  })
}
