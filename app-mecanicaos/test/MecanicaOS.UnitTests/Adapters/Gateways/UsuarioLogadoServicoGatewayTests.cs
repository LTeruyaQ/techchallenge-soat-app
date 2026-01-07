using Adapters.Gateways;
using Core.Entidades;
using Core.Enumeradores;
using Core.Interfaces.Servicos;
using FluentAssertions;
using NSubstitute;
using System.Security.Claims;
using Xunit;

namespace MecanicaOS.UnitTests.Adapters.Gateways
{
    public class UsuarioLogadoServicoGatewayTests
    {
        private readonly IUsuarioLogadoServico _usuarioLogadoServico;
        private readonly UsuarioLogadoServicoGateway _gateway;

        public UsuarioLogadoServicoGatewayTests()
        {
            _usuarioLogadoServico = Substitute.For<IUsuarioLogadoServico>();
            _gateway = new UsuarioLogadoServicoGateway(_usuarioLogadoServico);
        }

        [Fact]
        public void Construtor_ComUsuarioLogadoServicoNulo_DeveLancarArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new UsuarioLogadoServicoGateway(null!));
            exception.ParamName.Should().Be("usuarioLogadoServico");
        }

        [Fact]
        public void UsuarioId_DeveRetornarValorDoServico()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            _usuarioLogadoServico.UsuarioId.Returns(usuarioId);

            // Act
            var resultado = _gateway.UsuarioId;

            // Assert
            resultado.Should().Be(usuarioId);
        }

        [Fact]
        public void Email_DeveRetornarValorDoServico()
        {
            // Arrange
            var email = "admin@teste.com";
            _usuarioLogadoServico.Email.Returns(email);

            // Act
            var resultado = _gateway.Email;

            // Assert
            resultado.Should().Be(email);
        }

        [Fact]
        public void Nome_DeveRetornarValorDoServico()
        {
            // Arrange
            var nome = "Administrador";
            _usuarioLogadoServico.Nome.Returns(nome);

            // Act
            var resultado = _gateway.Nome;

            // Assert
            resultado.Should().Be(nome);
        }

        [Fact]
        public void TipoUsuario_DeveRetornarValorDoServico()
        {
            // Arrange
            var tipoUsuario = TipoUsuario.Admin;
            _usuarioLogadoServico.TipoUsuario.Returns(tipoUsuario);

            // Act
            var resultado = _gateway.TipoUsuario;

            // Assert
            resultado.Should().Be(tipoUsuario);
        }

        [Fact]
        public void EstaAutenticado_DeveRetornarValorDoServico()
        {
            // Arrange
            _usuarioLogadoServico.EstaAutenticado.Returns(true);

            // Act
            var resultado = _gateway.EstaAutenticado;

            // Assert
            resultado.Should().BeTrue();
        }

        [Fact]
        public void EstaNaRole_DeveChamarServicoERetornarResultado()
        {
            // Arrange
            var role = "Admin";
            _usuarioLogadoServico.EstaNaRole(role).Returns(true);

            // Act
            var resultado = _gateway.EstaNaRole(role);

            // Assert
            resultado.Should().BeTrue();
            _usuarioLogadoServico.Received(1).EstaNaRole(role);
        }

        [Fact]
        public void PossuiPermissao_DeveChamarServicoERetornarResultado()
        {
            // Arrange
            var permissao = "ler";
            _usuarioLogadoServico.PossuiPermissao(permissao).Returns(true);

            // Act
            var resultado = _gateway.PossuiPermissao(permissao);

            // Assert
            resultado.Should().BeTrue();
            _usuarioLogadoServico.Received(1).PossuiPermissao(permissao);
        }

        [Fact]
        public void ObterTodasClaims_DeveRetornarClaimsDoServico()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, "admin@teste.com"),
                new Claim(ClaimTypes.Role, "Admin")
            };

            _usuarioLogadoServico.ObterTodasClaims().Returns(claims);

            // Act
            var resultado = _gateway.ObterTodasClaims();

            // Assert
            resultado.Should().HaveCount(3);
            resultado.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == "admin@teste.com");
        }

        [Fact]
        public void ObterUsuarioLogado_DeveRetornarUsuarioDoServico()
        {
            // Arrange
            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Email = "admin@teste.com",
                TipoUsuario = TipoUsuario.Admin,
                DataCadastro = DateTime.Now
            };

            _usuarioLogadoServico.ObterUsuarioLogado().Returns(usuario);

            // Act
            var resultado = _gateway.ObterUsuarioLogado();

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Email.Should().Be("admin@teste.com");
            resultado.TipoUsuario.Should().Be(TipoUsuario.Admin);
        }

        [Fact]
        public void ObterUsuarioLogado_ComUsuarioNaoAutenticado_DeveRetornarNull()
        {
            // Arrange
            _usuarioLogadoServico.ObterUsuarioLogado().Returns((Usuario?)null);

            // Act
            var resultado = _gateway.ObterUsuarioLogado();

            // Assert
            resultado.Should().BeNull();
        }
    }
}
