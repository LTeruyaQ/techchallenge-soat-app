# ============================================
# ECR Repository
# ============================================

resource "aws_ecr_repository" "app" {
  name = var.docker_image_repo

  image_scanning_configuration {
    scan_on_push = true
  }

  tags = {
    Project = var.project_name
    Env     = var.environment
  }
}
