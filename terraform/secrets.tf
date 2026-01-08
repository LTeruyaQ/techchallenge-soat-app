# ============================================
# Geração de Segredos
# ============================================

# Gera uma senha aleatória para o banco de dados RDS
resource "random_password" "db_password" {
  length  = 16
  special = true
}

# Gera uma chave secreta aleatória para o JWT
resource "random_password" "jwt_secret_key" {
  length  = 32
  special = true
}
