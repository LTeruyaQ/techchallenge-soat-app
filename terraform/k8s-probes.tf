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
        # Liveness probe aponta para /health/live - um check rapido que nao depende de nada.
        path = "/health/live"
        port = 80
      }
      initial_delay_seconds = 15 # Aumentado para dar tempo de iniciar
      period_seconds        = 20
      failure_threshold     = 3
    }
    readiness_probe = {
      http_get = {
        # Readiness probe aponta para /health/ready - valida dependencias como o banco.
        path = "/health/ready"
        port = 80
      }
      initial_delay_seconds = 20 # Aumentado para dar tempo de conectar com dependencias
      period_seconds        = 30
      failure_threshold     = 3
    }
  }
}
