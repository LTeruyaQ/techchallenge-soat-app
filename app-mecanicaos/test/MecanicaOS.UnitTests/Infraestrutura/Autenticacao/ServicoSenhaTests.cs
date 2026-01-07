using Infraestrutura.Autenticacao;

namespace MecanicaOS.UnitTests.Infraestrutura.Autenticacao
{
    /// <summary>
    /// Testes para ServicoSenha
    /// Importância CRÍTICA: Valida a segurança da criptografia de senhas usando BCrypt.
    /// Garante que senhas sejam criptografadas corretamente e que a verificação funcione.
    /// Contribui para a robustez ao testar um dos componentes mais críticos de segurança.
    /// </summary>
    public class ServicoSenhaTests
    {
        /// <summary>
        /// Verifica se o serviço criptografa senhas corretamente
        /// Importância: Garante que senhas nunca sejam armazenadas em texto plano
        /// </summary>
        [Fact]
        public void CriptografarSenha_DeveRetornarHashDiferenteDaSenhaOriginal()
        {
            // Arrange
            var servico = new ServicoSenha();
            var senhaOriginal = "minhaSenhaSecreta123";
            
            // Act
            var hashSenha = servico.CriptografarSenha(senhaOriginal);
            
            // Assert
            hashSenha.Should().NotBeNullOrEmpty("o hash não deve ser nulo ou vazio");
            hashSenha.Should().NotBe(senhaOriginal, "o hash deve ser diferente da senha original");
            hashSenha.Length.Should().BeGreaterThan(50, "o hash BCrypt deve ter comprimento adequado");
        }

        /// <summary>
        /// Verifica se hashes diferentes são gerados para a mesma senha (salt aleatório)
        /// Importância: Valida que o BCrypt usa salt aleatório, aumentando segurança
        /// </summary>
        [Fact]
        public void CriptografarSenha_MesmaSenha_DeveGerarHashesDiferentes()
        {
            // Arrange
            var servico = new ServicoSenha();
            var senha = "minhaSenha123";
            
            // Act
            var hash1 = servico.CriptografarSenha(senha);
            var hash2 = servico.CriptografarSenha(senha);
            
            // Assert
            hash1.Should().NotBe(hash2, "BCrypt deve gerar hashes diferentes com salts aleatórios");
        }

        /// <summary>
        /// Verifica se a verificação de senha funciona corretamente com senha válida
        /// Importância: Essencial para autenticação - valida que senhas corretas sejam aceitas
        /// </summary>
        [Fact]
        public void VerificarSenha_ComSenhaCorreta_DeveRetornarTrue()
        {
            // Arrange
            var servico = new ServicoSenha();
            var senhaOriginal = "minhaSenhaSecreta123";
            var hashSenha = servico.CriptografarSenha(senhaOriginal);
            
            // Act
            var resultado = servico.VerificarSenha(senhaOriginal, hashSenha);
            
            // Assert
            resultado.Should().BeTrue("a senha correta deve ser verificada com sucesso");
        }

        /// <summary>
        /// Verifica se a verificação de senha rejeita senhas incorretas
        /// Importância: Essencial para segurança - valida que senhas incorretas sejam rejeitadas
        /// </summary>
        [Fact]
        public void VerificarSenha_ComSenhaIncorreta_DeveRetornarFalse()
        {
            // Arrange
            var servico = new ServicoSenha();
            var senhaOriginal = "minhaSenhaSecreta123";
            var senhaIncorreta = "senhaErrada456";
            var hashSenha = servico.CriptografarSenha(senhaOriginal);
            
            // Act
            var resultado = servico.VerificarSenha(senhaIncorreta, hashSenha);
            
            // Assert
            resultado.Should().BeFalse("uma senha incorreta não deve ser verificada");
        }

        /// <summary>
        /// Verifica se senhas vazias são criptografadas (edge case)
        /// Importância: Valida comportamento com entrada inválida
        /// </summary>
        [Fact]
        public void CriptografarSenha_ComSenhaVazia_DeveGerarHash()
        {
            // Arrange
            var servico = new ServicoSenha();
            var senhaVazia = "";
            
            // Act
            var hashSenha = servico.CriptografarSenha(senhaVazia);
            
            // Assert
            hashSenha.Should().NotBeNullOrEmpty("mesmo senha vazia deve gerar hash");
        }

        /// <summary>
        /// Verifica se o serviço é case-sensitive
        /// Importância: Valida que senhas com diferenças de maiúsculas/minúsculas sejam tratadas como diferentes
        /// </summary>
        [Fact]
        public void VerificarSenha_CaseSensitive_DeveDistinguirMaiusculasMinusculas()
        {
            // Arrange
            var servico = new ServicoSenha();
            var senha = "MinhaSenha123";
            var hashSenha = servico.CriptografarSenha(senha);
            
            // Act
            var resultadoMinuscula = servico.VerificarSenha("minhasenha123", hashSenha);
            var resultadoMaiuscula = servico.VerificarSenha("MINHASENHA123", hashSenha);
            
            // Assert
            resultadoMinuscula.Should().BeFalse("senhas devem ser case-sensitive");
            resultadoMaiuscula.Should().BeFalse("senhas devem ser case-sensitive");
        }
    }
}
