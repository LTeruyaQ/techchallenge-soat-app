# ============================================
# Geração Dinâmica de Segredos
# ============================================

resource "random_password" "db_password" {
  length           = 16
  special          = true
  override_special = "_%@"
}

resource "random_password" "jwt_secret_key" {
  length           = 32
  special          = false
}
