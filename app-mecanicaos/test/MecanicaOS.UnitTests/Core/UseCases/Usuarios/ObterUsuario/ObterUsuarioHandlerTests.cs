using Core.Entidades;
using Core.Interfaces.Gateways;
using Core.UseCases.Usuarios.ObterUsuario;
using MecanicaOS.UnitTests.Fixtures;

namespace MecanicaOS.UnitTests.Core.UseCases.Usuarios.ObterUsuario
{
    /// <summary>
    /// Testes unitários para o handler ObterUsuarioHandler
    /// </summary>
    public class ObterUsuarioHandlerTests
    {
        /// <summary>
        /// Verifica se o handler retorna usuário quando encontrado
        /// </summary>
        [Fact]
        public async Task Handle_ComIdExistente_DeveRetornarUsuario()
        {
            // Arrange
            var usuarioGatewayMock = UsuarioHandlerFixture.CriarUsuarioGatewayMock();
            var unidadeDeTrabalhoMock = UsuarioHandlerFixture.CriarUnidadeDeTrabalhMock();
            
            var usuario = UsuarioHandlerFixture.CriarUsuario();
            
            usuarioGatewayMock.ObterPorIdAsync(usuario.Id).Returns(usuario);
            
            var logGatewayMock = Substitute.For<ILogGateway<ObterUsuarioHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new ObterUsuarioHandler(
                usuarioGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            var resultado = await handler.Handle(usuario.Id);
            
            // Assert
            resultado.Should().NotBeNull("o resultado não deve ser nulo");
            resultado!.Id.Should().Be(usuario.Id, "o ID deve corresponder");
            resultado.Email.Should().Be(usuario.Email, "o email deve corresponder");
            resultado.Senha.Should().BeEmpty("a senha não deve ser retornada");
            
            await usuarioGatewayMock.Received(1).ObterPorIdAsync(usuario.Id);
        }

        /// <summary>
        /// Verifica se o handler retorna null quando usuário não é encontrado
        /// </summary>
        [Fact]
        public async Task Handle_ComIdInexistente_DeveRetornarNull()
        {
            // Arrange
            var usuarioGatewayMock = UsuarioHandlerFixture.CriarUsuarioGatewayMock();
            var unidadeDeTrabalhoMock = UsuarioHandlerFixture.CriarUnidadeDeTrabalhMock();
            
            var usuarioId = Guid.NewGuid();
            
            usuarioGatewayMock.ObterPorIdAsync(usuarioId).Returns((Usuario?)null);
            
            var logGatewayMock = Substitute.For<ILogGateway<ObterUsuarioHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new ObterUsuarioHandler(
                usuarioGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            var resultado = await handler.Handle(usuarioId);
            
            // Assert
            resultado.Should().BeNull("o resultado deve ser nulo quando usuário não existe");
            
            await usuarioGatewayMock.Received(1).ObterPorIdAsync(usuarioId);
        }

        /// <summary>
        /// Verifica se a senha é sempre removida do resultado (segurança)
        /// </summary>
        [Fact]
        public async Task Handle_DeveRemoverSenhaDoResultado()
        {
            // Arrange
            var usuarioGatewayMock = UsuarioHandlerFixture.CriarUsuarioGatewayMock();
            var unidadeDeTrabalhoMock = UsuarioHandlerFixture.CriarUnidadeDeTrabalhMock();
            
            var usuario = UsuarioHandlerFixture.CriarUsuario();
            usuario.Senha = "senha_secreta_criptografada";
            
            usuarioGatewayMock.ObterPorIdAsync(usuario.Id).Returns(usuario);
            
            var logGatewayMock = Substitute.For<ILogGateway<ObterUsuarioHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new ObterUsuarioHandler(
                usuarioGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            var resultado = await handler.Handle(usuario.Id);
            
            // Assert
            resultado!.Senha.Should().BeEmpty("a senha deve ser removida por segurança");
        }

        /// <summary>
        /// Verifica se o handler propaga exceção quando gateway falha
        /// </summary>
        [Fact]
        public async Task Handle_QuandoGatewayLancaExcecao_DevePropagar()
        {
            // Arrange
            var usuarioGatewayMock = UsuarioHandlerFixture.CriarUsuarioGatewayMock();
            var unidadeDeTrabalhoMock = UsuarioHandlerFixture.CriarUnidadeDeTrabalhMock();
            var logGatewayMock = Substitute.For<ILogGateway<ObterUsuarioHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();

            var usuarioId = Guid.NewGuid();
            usuarioGatewayMock.ObterPorIdAsync(usuarioId).Returns(Task.FromException<Usuario?>(new InvalidOperationException("Erro no banco")));

            var handler = new ObterUsuarioHandler(
                usuarioGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await handler.Handle(usuarioId));
        }
    }
}
