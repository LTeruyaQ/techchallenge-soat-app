output "db_instance_id" {
  description = "O ID da instância do RDS."
  value       = aws_db_instance.default.id
}

output "db_instance_arn" {
  description = "O ARN da instância do RDS."
  value       = aws_db_instance.default.arn
}

output "rds_security_group_id" {
  description = "O ID do Security Group do RDS."
  value       = aws_security_group.rds.id
}
