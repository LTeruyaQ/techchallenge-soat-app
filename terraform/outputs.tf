# ============================================
# Outputs - Saídas do Terraform
# ============================================
#
# Estas saídas fornecem informações importantes e comandos úteis após a criação da infraestrutura.
#

# --- Informações Críticas para Acesso ---

output "api_url" {
  description = "URL base da API (Load Balancer). Pode levar alguns minutos para se tornar ativa."
  # O 'try' evita erros se o serviço ainda não tiver um load balancer provisionado.
  value       = try("http://${kubernetes_service.api.status.load_balancer.ingress[0].hostname}/swagger/index.html", "Aguardando provisionamento do Load Balancer...")
}

output "kubectl_config_command" {
  description = "Comando para configurar o kubectl para acessar o cluster EKS."
  value       = "aws eks update-kubeconfig --region ${var.aws_region} --name ${aws_eks_cluster.eks.name}"
}

output "health_check_command" {
  description = "Comando para verificar a saúde da aplicação (use após o Load Balancer estar ativo)."
  value       = "curl http://${try(kubernetes_service.api.status.load_balancer.ingress[0].hostname, "LOAD_BALANCER_HOSTNAME")}/health"
}

# --- Detalhes do Ambiente ---

output "account_type" {
  description = "Tipo de conta detectada (AWS Academy ou Normal)."
  value       = local.is_academy ? "AWS Academy" : "Conta Normal"
}

output "eks_cluster_name" {
  description = "Nome do cluster EKS criado."
  value       = aws_eks_cluster.eks.name
}

output "kubernetes_namespace" {
  description = "Namespace Kubernetes onde a aplicação foi implantada."
  value       = kubernetes_namespace.app.metadata[0].name
}

output "eks_node_group_name" {
  description = "Nome do Node Group do EKS."
  value       = aws_eks_node_group.nodes.node_group_name
}

# --- Informações de Build e Imagem ---

output "ecr_repository_url" {
  description = "URL do repositório ECR onde a imagem Docker foi (ou deve ser) publicada."
  value       = aws_ecr_repository.app.repository_url
}

output "docker_image" {
  description = "Nome completo e tag da imagem Docker utilizada no deployment."
  value       = local.docker_image
}
