# ============================================
# Kubernetes Service - LoadBalancer
# ============================================

resource "kubectl_manifest" "service" {
  depends_on = [kubectl_manifest.deployment]

  yaml_body = <<YAML
apiVersion: v1
kind: Service
metadata:
  name: mecanicaos-service
  namespace: mecanicaos
spec:
  selector:
    app: mecanicaos-api
  ports:
    - protocol: TCP
      port: 80
      targetPort: 80
  type: LoadBalancer
YAML
}
