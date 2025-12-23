# ============================================
# Kubernetes Health Probes - Verificações de Saúde
# ============================================
#
# Define as Readiness e Liveness Probes para o deployment da aplicação.
# Estas probes garantem que o Kubernetes possa gerenciar a saúde dos Pods de forma eficaz.
#
# - readinessProbe: Confirma se a aplicação está pronta para aceitar tráfego.
#   Um Pod que falha na readinessProbe é temporariamente removido do Service Load Balancer.
#
# - livenessProbe: Confirma se a aplicação ainda está em execução.
#   Se a livenessProbe falhar, o Kubernetes reiniciará o contêiner.
#
locals {
  # Bloco de configuração das probes, reutilizável no deployment.
  # https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/
  probes = {
    liveness_probe = {
      http_get = {
        # O endpoint foi padronizado para /api/v1/health para consistencia e visibilidade no Swagger.
        path = "/api/v1/health"
        port = 80
      }
      # A sonda começa 10s após o contêiner iniciar.
      initial_delay_seconds = 10
      # A verificação é repetida a cada 10s.
      period_seconds = 10
      # O contêiner é considerado "morto" após 3 falhas consecutivas.
      failure_threshold = 3
    }
    readiness_probe = {
      http_get = {
        path = "/api/v1/health"
        port = 80
      }
      initial_delay_seconds = 10
      period_seconds        = 10
      failure_threshold     = 3
    }
  }
}
