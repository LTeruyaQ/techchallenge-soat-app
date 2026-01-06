# ============================================
# Outputs - AWS Academy
# ============================================

output "vpc_id" {
  description = "ID da VPC"
  value       = aws_vpc.main.id
}

output "vpc_cidr" {
  description = "CIDR da VPC"
  value       = aws_vpc.main.cidr_block
}

output "subnet_ids" {
  description = "IDs das subnets"
  value       = aws_subnet.public[*].id
}

output "eks_cluster_name" {
  description = "Nome do cluster EKS"
  value       = aws_eks_cluster.eks.name
}

output "eks_cluster_endpoint" {
  description = "Endpoint do cluster EKS"
  value       = aws_eks_cluster.eks.endpoint
}

output "ecr_repository_url" {
  description = "URL do repositório ECR"
  value       = aws_ecr_repository.app.repository_url
}

output "docker_image" {
  description = "Imagem Docker utilizada no deployment"
  value       = local.docker_image
}

# ============================================
# Comandos Úteis
# ============================================

output "kubectl_config_command" {
  description = "Comando para configurar kubectl"
  value       = "aws eks update-kubeconfig --region ${var.aws_region} --name ${aws_eks_cluster.eks.name}"
}

# ============================================
# Output do API Gateway
# ============================================

output "api_gateway_invoke_url" {
  description = "URL pública para invocar a API através do API Gateway"
  value       = aws_apigatewayv2_stage.default.invoke_url
}

# ============================================
# Outputs do RDS
# ============================================

output "rds_master_password" {
  description = "Senha master do banco de dados RDS (obtida do Secrets Manager)"
  value       = random_password.db_master_password.result
  sensitive   = true
}

output "rds_hostname" {
  description = "Endpoint do banco de dados RDS"
  value       = aws_db_instance.postgres_db.address
}

output "rds_port" {
  description = "Porta do banco de dados RDS"
  value       = aws_db_instance.postgres_db.port
}

output "rds_username" {
  description = "Usuário master do banco de dados RDS"
  value       = aws_db_instance.postgres_db.username
}
