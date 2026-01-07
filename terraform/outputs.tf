# ============================================
# Outputs - AWS Academy
# ============================================

# ============================================
# Networking
# ============================================

output "vpc_id" {
  description = "ID da VPC"
  value       = aws_vpc.main.id
}

output "public_subnet_ids" {
  description = "IDs das subnets públicas"
  value       = aws_subnet.public[*].id
}

output "private_subnet_ids" {
  description = "IDs das subnets privadas"
  value       = aws_subnet.private[*].id
}

# ============================================
# EKS
# ============================================

output "eks_cluster_name" {
  description = "Nome do cluster EKS"
  value       = aws_eks_cluster.eks.name
}

output "eks_cluster_endpoint" {
  description = "Endpoint do cluster EKS"
  value       = aws_eks_cluster.eks.endpoint
}

# ============================================
# ECR
# ============================================

output "ecr_repository_url" {
  description = "URL do repositório ECR"
  value       = aws_ecr_repository.app.repository_url
}

# ============================================
# RDS
# ============================================

output "rds_endpoint" {
  description = "Endpoint do banco de dados RDS"
  value       = aws_db_instance.postgres.endpoint
}

output "rds_database_name" {
  description = "Nome do banco de dados no RDS"
  value       = aws_db_instance.postgres.db_name
}

# ============================================
# API Gateway & Lambda
# ============================================

output "api_gateway_invoke_url" {
  description = "URL pública do API Gateway para invocar a API"
  value       = aws_apigatewayv2_stage.default.invoke_url
}

output "swagger_url" {
  description = "URL do Swagger UI"
  value       = "${aws_apigatewayv2_stage.default.invoke_url}/swagger/index.html"
}

output "lambda_authorizer_name" {
  description = "Nome da função Lambda de autorização"
  value       = aws_lambda_function.authorizer.function_name
}

# ============================================
# Secrets
# ============================================

output "rds_credentials_secret_arn" {
  description = "ARN do segredo com as credenciais do RDS no Secrets Manager"
  value       = aws_secretsmanager_secret.rds_credentials.arn
}

output "jwt_secret_arn" {
  description = "ARN do segredo com a chave JWT no Secrets Manager"
  value       = aws_secretsmanager_secret.jwt_key.arn
}


# ============================================
# Comandos Úteis
# ============================================

output "kubectl_config_command" {
  description = "Comando para configurar kubectl"
  value       = "aws eks update-kubeconfig --region ${var.aws_region} --name ${aws_eks_cluster.eks.name}"
}
