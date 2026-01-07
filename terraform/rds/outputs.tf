# rds/outputs.tf

output "db_instance_endpoint" {
  description = "Endpoint do RDS"
  value       = aws_db_instance.default.endpoint
}

output "db_instance_port" {
  description = "Porta do RDS"
  value       = aws_db_instance.default.port
}

output "db_instance_name" {
  description = "Nome do banco de dados"
  value       = aws_db_instance.default.db_name
}

output "db_instance_username" {
  description = "Usuário do banco de dados"
  value       = aws_db_instance.default.username
}

output "db_credentials_secret_arn" {
  description = "ARN do segredo com as credenciais do RDS"
  value       = aws_secretsmanager_secret.rds_credentials.arn
}
