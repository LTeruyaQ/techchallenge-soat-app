# ============================================
# Kubernetes Deployment - MecanicaOS API
# ============================================

resource "kubectl_manifest" "deployment" {
  depends_on = [kubectl_manifest.namespace]

  yaml_body = <<YAML
apiVersion: apps/v1
kind: Deployment
metadata:
  name: mecanicaos-api
  namespace: mecanicaos
spec:
  replicas: ${var.replicas}
  selector:
    matchLabels:
      app: mecanicaos-api
  template:
    metadata:
      labels:
        app: mecanicaos-api
    spec:
      containers:
      - name: mecanicaos-api
        image: ${var.docker_image}
        ports:
        - containerPort: 80
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "${var.environment}"
        - name: ASPNETCORE_URLS
          value: "http://+:80"
        - name: ConnectionStrings__DefaultConnection
          value: "${local.db_connection_string}"
        - name: Jwt__SecretKey
          value: "${var.jwt_secret_key}"
        - name: Jwt__Issuer
          value: "${var.jwt_issuer}"
        - name: Jwt__Audience
          value: "${var.jwt_audience}"
        - name: Jwt__ExpiryInMinutes
          value: "${var.jwt_expiry_minutes}"
        resources:
          requests:
            cpu: "100m"
            memory: "256Mi"
          limits:
            cpu: "500m"
            memory: "512Mi"
YAML
}
