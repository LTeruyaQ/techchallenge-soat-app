using Adapters.Gateways;
using Core.Interfaces.Servicos;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace MecanicaOS.UnitTests.Adapters.Gateways
{
    public class SegurancaGatewayTests
    {
        private readonly IServicoSenha _servicoSenha;
        private readonly IServicoJwt _servicoJwt;
        private readonly SegurancaGateway _gateway;

        public SegurancaGatewayTests()
        {
            _servicoSenha = Substitute.For<IServicoSenha>();
            _servicoJwt = Substitute.For<IServicoJwt>();
            _gateway = new SegurancaGateway(_servicoSenha, _servicoJwt);
        }

        [Fact]
        public void Construtor_ComServicoSenhaNulo_DeveLancarArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new SegurancaGateway(null!, _servicoJwt));
            exception.ParamName.Should().Be("servicoSenha");
        }

        [Fact]
        public void Construtor_ComServicoJwtNulo_DeveLancarArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new SegurancaGateway(_servicoSenha, null!));
            exception.ParamName.Should().Be("servicoJwt");
        }

        [Fact]
        public void VerificarSenha_DeveChamarServicoSenha()
        {
            // Arrange
            var senhaPlano = "Senha@123";
            var hashArmazenado = "$2a$11$hashedpassword";
            _servicoSenha.VerificarSenha(senhaPlano, hashArmazenado).Returns(true);

            // Act
            var resultado = _gateway.VerificarSenha(senhaPlano, hashArmazenado);

            // Assert
            resultado.Should().BeTrue();
            _servicoSenha.Received(1).VerificarSenha(senhaPlano, hashArmazenado);
        }

        [Fact]
        public void VerificarSenha_ComSenhaIncorreta_DeveRetornarFalse()
        {
            // Arrange
            var senhaPlano = "SenhaErrada";
            var hashArmazenado = "$2a$11$hashedpassword";
            _servicoSenha.VerificarSenha(senhaPlano, hashArmazenado).Returns(false);

            // Act
            var resultado = _gateway.VerificarSenha(senhaPlano, hashArmazenado);

            // Assert
            resultado.Should().BeFalse();
            _servicoSenha.Received(1).VerificarSenha(senhaPlano, hashArmazenado);
        }

        [Fact]
        public void GerarToken_DeveChamarServicoJwt()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var email = "admin@teste.com";
            var tipoUsuario = "Admin";
            var clienteId = Guid.NewGuid();
            var permissoes = new List<string> { "ler", "escrever" };
            var tokenEsperado = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";

            _servicoJwt.GerarToken(usuarioId, email, tipoUsuario, null, permissoes).Returns(tokenEsperado);

            // Act
            var resultado = _gateway.GerarToken(usuarioId, email, tipoUsuario, clienteId, permissoes);

            // Assert
            resultado.Should().Be(tokenEsperado);
            _servicoJwt.Received(1).GerarToken(usuarioId, email, tipoUsuario, null, permissoes);
        }

        [Fact]
        public void GerarToken_ComClienteIdNulo_DeveChamarServicoJwtComNulo()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var email = "admin@teste.com";
            var tipoUsuario = "Admin";
            Guid? clienteId = null;
            var permissoes = new List<string> { "ler" };
            var tokenEsperado = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";

            _servicoJwt.GerarToken(usuarioId, email, tipoUsuario, null, permissoes).Returns(tokenEsperado);

            // Act
            var resultado = _gateway.GerarToken(usuarioId, email, tipoUsuario, clienteId, permissoes);

            // Assert
            resultado.Should().Be(tokenEsperado);
            _servicoJwt.Received(1).GerarToken(usuarioId, email, tipoUsuario, null, permissoes);
        }

        [Fact]
        public void GerarToken_ComPermissoesVazias_DeveChamarServicoJwt()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var email = "cliente@teste.com";
            var tipoUsuario = "Cliente";
            var clienteId = Guid.NewGuid();
            var permissoes = Enumerable.Empty<string>();
            var tokenEsperado = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";

            _servicoJwt.GerarToken(usuarioId, email, tipoUsuario, null, permissoes).Returns(tokenEsperado);

            // Act
            var resultado = _gateway.GerarToken(usuarioId, email, tipoUsuario, clienteId, permissoes);

            // Assert
            resultado.Should().Be(tokenEsperado);
        }
    }
}
