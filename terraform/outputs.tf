# ============================================
# Outputs - AWS Academy
# ============================================

output "vpc_id" {
  description = "ID da VPC"
  value       = data.aws_vpc.existing.id
}

output "vpc_cidr" {
  description = "CIDR da VPC"
  value       = data.aws_vpc.existing.cidr_block
}

output "subnet_ids" {
  description = "IDs das subnets"
  value       = data.aws_subnets.private.ids
}

output "eks_cluster_name" {
  description = "Nome do cluster EKS"
  value       = aws_eks_cluster.eks.name
}

output "eks_cluster_endpoint" {
  description = "Endpoint do cluster EKS"
  value       = aws_eks_cluster.eks.endpoint
}

# A saída ecr_repository_url foi removida pois o ECR
# agora é criado e gerenciado pelo script deploy-completo.ps1.

output "rds_endpoint" {
  description = "Endpoint do banco de dados RDS"
  value       = aws_db_instance.default.endpoint
}

output "rds_dbname" {
  description = "Nome do banco de dados RDS"
  value       = aws_db_instance.default.db_name
}

output "db_secret_arn" {
  description = "ARN do segredo do banco de dados no Secrets Manager"
  value       = aws_secretsmanager_secret.db_credentials.arn
}

output "api_gateway_endpoint" {
  description = "URL do API Gateway"
  value       = aws_apigatewayv2_api.http_api.api_endpoint
}

output "lambda_auth_function_name" {
  description = "Nome da função Lambda de autenticação"
  value       = aws_lambda_function.auth_lambda.function_name
}

output "docker_image" {
  description = "Imagem Docker utilizada no deployment"
  value       = var.docker_image
}

# ============================================
# Comandos Úteis
# ============================================

output "kubectl_config_command" {
  description = "Comando para configurar kubectl"
  value       = "aws eks update-kubeconfig --region ${var.aws_region} --name ${aws_eks_cluster.eks.name}"
}
