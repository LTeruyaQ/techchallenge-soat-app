# ============================================
# ECR Repository
# ============================================

resource "random_id" "ecr_suffix" {
  byte_length = 4
}

resource "aws_ecr_repository" "app" {
  name         = "${var.docker_image_repo}-${random_id.ecr_suffix.hex}"
  force_delete = true

  image_scanning_configuration {
    scan_on_push = true
  }

  tags = {
    Project = var.project_name
    Env     = var.environment
  }
}
