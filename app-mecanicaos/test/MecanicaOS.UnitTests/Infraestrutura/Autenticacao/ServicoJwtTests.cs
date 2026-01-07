using Core.DTOs.Config;
using Infraestrutura.Autenticacao;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MecanicaOS.UnitTests.Infraestrutura.Autenticacao
{
    /// <summary>
    /// Testes para ServicoJwt
    /// Importância CRÍTICA: Valida geração e validação de tokens JWT, essencial para autenticação e autorização.
    /// Garante que tokens sejam gerados com claims corretos e que a validação funcione adequadamente.
    /// </summary>
    public class ServicoJwtTests
    {
        private ConfiguracaoJwt CriarConfiguracaoJwt()
        {
            return new ConfiguracaoJwt
            {
                SecretKey = "chave-secreta-super-segura-com-pelo-menos-32-caracteres-para-testes",
                Issuer = "MecanicaOS.Tests",
                Audience = "MecanicaOS.API",
                ExpiryInMinutes = 60
            };
        }

        /// <summary>
        /// Verifica se o serviço gera token JWT válido
        /// Importância: Valida o fluxo principal de geração de tokens
        /// </summary>
        [Fact]
        public void GerarToken_ComDadosValidos_DeveRetornarTokenJwt()
        {
            // Arrange
            var config = CriarConfiguracaoJwt();
            var servico = new ServicoJwt(config);
            var usuarioId = Guid.NewGuid();
            var email = "usuario@teste.com";
            var tipoUsuario = "Admin";
            
            // Act
            var token = servico.GerarToken(usuarioId, email, tipoUsuario);
            
            // Assert
            token.Should().NotBeNullOrEmpty("o token não deve ser nulo ou vazio");
            token.Split('.').Should().HaveCount(3, "um JWT válido tem 3 partes separadas por ponto");
        }

        /// <summary>
        /// Verifica se o token contém as claims obrigatórias
        /// Importância: Garante que informações essenciais estejam no token
        /// </summary>
        [Fact]
        public void GerarToken_DeveIncluirClaimsObrigatorias()
        {
            // Arrange
            var config = CriarConfiguracaoJwt();
            var servico = new ServicoJwt(config);
            var usuarioId = Guid.NewGuid();
            var email = "usuario@teste.com";
            var tipoUsuario = "Admin";
            
            // Act
            var token = servico.GerarToken(usuarioId, email, tipoUsuario);
            
            // Assert - Decodificar token para verificar claims
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == usuarioId.ToString());
            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == email);
            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == tipoUsuario);
            jwtToken.Claims.Should().Contain(c => c.Type == "tipo_usuario" && c.Value == tipoUsuario);
        }

        /// <summary>
        /// Verifica se o token inclui nome quando fornecido
        /// Importância: Valida tratamento de claims opcionais
        /// </summary>
        [Fact]
        public void GerarToken_ComNome_DeveIncluirClaimNome()
        {
            // Arrange
            var config = CriarConfiguracaoJwt();
            var servico = new ServicoJwt(config);
            var usuarioId = Guid.NewGuid();
            var email = "usuario@teste.com";
            var tipoUsuario = "Admin";
            var nome = "João Silva";
            
            // Act
            var token = servico.GerarToken(usuarioId, email, tipoUsuario, nome);
            
            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == nome);
        }

        /// <summary>
        /// Verifica se o token inclui permissões quando fornecidas
        /// Importância: Valida autorização baseada em permissões
        /// </summary>
        [Fact]
        public void GerarToken_ComPermissoes_DeveIncluirClaimsPermissoes()
        {
            // Arrange
            var config = CriarConfiguracaoJwt();
            var servico = new ServicoJwt(config);
            var usuarioId = Guid.NewGuid();
            var email = "usuario@teste.com";
            var tipoUsuario = "Admin";
            var permissoes = new[] { "criar_cliente", "editar_cliente", "deletar_cliente" };
            
            // Act
            var token = servico.GerarToken(usuarioId, email, tipoUsuario, null, permissoes);
            
            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            var claimsPermissoes = jwtToken.Claims.Where(c => c.Type == "permissao").Select(c => c.Value).ToList();
            claimsPermissoes.Should().HaveCount(3, "devem existir 3 permissões");
            claimsPermissoes.Should().Contain("criar_cliente");
            claimsPermissoes.Should().Contain("editar_cliente");
            claimsPermissoes.Should().Contain("deletar_cliente");
        }

        /// <summary>
        /// Verifica se o token gerado é validado corretamente
        /// Importância: Valida o ciclo completo de geração e validação
        /// </summary>
        [Fact]
        public void ValidarToken_ComTokenValido_DeveRetornarTrue()
        {
            // Arrange
            var config = CriarConfiguracaoJwt();
            var servico = new ServicoJwt(config);
            var usuarioId = Guid.NewGuid();
            var token = servico.GerarToken(usuarioId, "usuario@teste.com", "Admin");
            
            // Act
            var resultado = servico.ValidarToken(token);
            
            // Assert
            resultado.Should().BeTrue("um token gerado pelo próprio serviço deve ser válido");
        }

        /// <summary>
        /// Verifica se tokens inválidos são rejeitados
        /// Importância: Essencial para segurança - valida que tokens malformados sejam rejeitados
        /// </summary>
        [Fact]
        public void ValidarToken_ComTokenInvalido_DeveRetornarFalse()
        {
            // Arrange
            var config = CriarConfiguracaoJwt();
            var servico = new ServicoJwt(config);
            var tokenInvalido = "token.invalido.aqui";
            
            // Act
            var resultado = servico.ValidarToken(tokenInvalido);
            
            // Assert
            resultado.Should().BeFalse("um token inválido deve ser rejeitado");
        }

        /// <summary>
        /// Verifica se tokens vazios são rejeitados
        /// Importância: Valida tratamento de edge cases
        /// </summary>
        [Fact]
        public void ValidarToken_ComTokenVazio_DeveRetornarFalse()
        {
            // Arrange
            var config = CriarConfiguracaoJwt();
            var servico = new ServicoJwt(config);
            
            // Act
            var resultado = servico.ValidarToken("");
            
            // Assert
            resultado.Should().BeFalse("um token vazio deve ser rejeitado");
        }

        /// <summary>
        /// Verifica se o token tem data de expiração configurada
        /// Importância: Valida que tokens expirem conforme configuração
        /// </summary>
        [Fact]
        public void GerarToken_DeveConfigurarDataExpiracao()
        {
            // Arrange
            var config = CriarConfiguracaoJwt();
            var servico = new ServicoJwt(config);
            var usuarioId = Guid.NewGuid();
            var token = servico.GerarToken(usuarioId, "usuario@teste.com", "Admin");
            
            // Act
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            // Assert
            jwtToken.ValidTo.Should().BeAfter(DateTime.UtcNow, "o token deve expirar no futuro");
            var diferencaMinutos = (jwtToken.ValidTo - DateTime.UtcNow).TotalMinutes;
            diferencaMinutos.Should().BeApproximately(config.ExpiryInMinutes, 1, "a expiração deve corresponder à configuração");
        }
    }
}
