# ============================================
# Outputs - Saídas do Terraform
# ============================================
#
# Estas saídas fornecem informações importantes e comandos úteis após a criação da infraestrutura.
#

# --- Informações Críticas para Acesso ---

output "api_url" {
  description = "[FONTE OFICIAL] URL base (DNS) da API, exposta pelo Load Balancer."
  # Este output e a fonte oficial de verdade para a URL da aplicacao.
  # O valor e extraido diretamente do status do Service Kubernetes.
  # O script de deploy (deploy-completo.ps1) aguarda este valor ficar disponivel.
  value       = try(kubernetes_service.api.status.load_balancer.ingress[0].hostname, "Provisionando Load Balancer...")
}

output "swagger_url" {
  description = "URL completa para acessar a documentacao Swagger da API."
  value       = "http://${local.api_url}/swagger/index.html"
}

output "health_live_url" {
  description = "URL do endpoint de Liveness (usado pelo Kubernetes)."
  value       = "http://${local.api_url}/health/live"
}

output "health_ready_url" {
  description = "URL do endpoint de Readiness (usado pelo Load Balancer e Kubernetes)."
  value       = "http://${local.api_url}/health/ready"
}

output "kubectl_config_command" {
  description = "Comando para configurar o kubectl para acessar o cluster EKS."
  value       = "aws eks update-kubeconfig --region ${var.aws_region} --name ${aws_eks_cluster.eks.name}"
}

# --- Detalhes do Ambiente ---

output "environment_type" {
  description = "Tipo de ambiente detectado (AWS Academy ou Conta Normal)."
  value       = local.is_academy ? "AWS Academy" : "Conta Normal"
}

output "eks_cluster_name" {
  description = "Nome do cluster EKS criado."
  value       = aws_eks_cluster.eks.name
}

output "namespace" {
  description = "Namespace Kubernetes onde a aplicação foi implantada."
  value       = kubernetes_namespace.app.metadata[0].name
}

output "otel_status" {
  description = "Status da configuracao do OpenTelemetry (Observabilidade)."
  value       = var.datadog_api_key != "" || var.newrelic_license_key != "" ? "Configurado" : "Nao Configurado (Opcional)"
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
