aws_region           = "us-east-1"
project_name         = "mecanicaos"
environment          = "production"

# Roles do AWS Academy (nomes corretos obtidos do IAM)
eks_cluster_role = "LabRole"
eks_node_role    = "LabRole"

# ECR
docker_image_repo = "mecanicaos-ecr"
docker_image_tag  = "20251230-170003"
# docker_image_tag é gerado automaticamente pelo deploy-completo.ps1

# Kubernetes
replicas = 2

# Banco (Supabase - Direct URL)
db_host     = "aws-1-sa-east-1.pooler.supabase.com"
db_port     = "5432"
db_name     = "postgres"
db_username = "postgres.uniaamdoomwnxtpmgcxs"
db_password = "Q3qV@dS2Te*kKGZ"

# JWT
jwt_secret_key     = "5sBGFgI5yhtbfB50Xyohk9RLX/pWX7mRRLSP1pwceIdyD/Is6DkG6QTjELYjZSa7"
jwt_issuer         = "MecanicaOS"
jwt_audience       = "MecanicaOS-API"
jwt_expiry_minutes = 120

# OpenTelemetry
otel_service_name           = "mecanicaos-api"
otel_exporter_otlp_endpoint = "http://otel-collector.observability:4317"
datadog_api_key             = "e0f3dea9093bbb0e07c8a113afb986d3"
newrelic_license_key        = "f744030d61e4cf2b9c652cdca9024fe2FFFFNRAL"













