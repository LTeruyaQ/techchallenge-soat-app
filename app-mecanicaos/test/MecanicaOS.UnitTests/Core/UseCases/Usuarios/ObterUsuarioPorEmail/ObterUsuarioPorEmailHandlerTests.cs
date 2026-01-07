using Core.Entidades;
using Core.Interfaces.Gateways;
using Core.UseCases.Usuarios.ObterUsuarioPorEmail;
using MecanicaOS.UnitTests.Fixtures;

namespace MecanicaOS.UnitTests.Core.UseCases.Usuarios.ObterUsuarioPorEmail
{
    /// <summary>
    /// Testes unitários para o handler ObterUsuarioPorEmailHandler
    /// </summary>
    public class ObterUsuarioPorEmailHandlerTests
    {
        /// <summary>
        /// Verifica se o handler retorna usuário quando encontrado por email
        /// </summary>
        [Fact]
        public async Task Handle_ComEmailExistente_DeveRetornarUsuario()
        {
            // Arrange
            var usuarioGatewayMock = UsuarioHandlerFixture.CriarUsuarioGatewayMock();
            var unidadeDeTrabalhoMock = UsuarioHandlerFixture.CriarUnidadeDeTrabalhMock();
            
            var usuario = UsuarioHandlerFixture.CriarUsuario();
            var email = "usuario@teste.com";
            usuario.Email = email;
            
            usuarioGatewayMock.ObterPorEmailAsync(email).Returns(usuario);
            
            var logGatewayMock = Substitute.For<ILogGateway<ObterUsuarioPorEmailHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new ObterUsuarioPorEmailHandler(
                usuarioGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            var resultado = await handler.Handle(email);
            
            // Assert
            resultado.Should().NotBeNull("o resultado não deve ser nulo");
            resultado!.Id.Should().Be(usuario.Id, "o ID deve corresponder");
            resultado.Email.Should().Be(email, "o email deve corresponder");
            
            await usuarioGatewayMock.Received(1).ObterPorEmailAsync(email);
        }

        /// <summary>
        /// Verifica se o handler retorna null quando email não é encontrado
        /// </summary>
        [Fact]
        public async Task Handle_ComEmailInexistente_DeveRetornarNull()
        {
            // Arrange
            var usuarioGatewayMock = UsuarioHandlerFixture.CriarUsuarioGatewayMock();
            var unidadeDeTrabalhoMock = UsuarioHandlerFixture.CriarUnidadeDeTrabalhMock();
            
            var email = "inexistente@teste.com";
            
            usuarioGatewayMock.ObterPorEmailAsync(email).Returns((Usuario?)null);
            
            var logGatewayMock = Substitute.For<ILogGateway<ObterUsuarioPorEmailHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new ObterUsuarioPorEmailHandler(
                usuarioGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            var resultado = await handler.Handle(email);
            
            // Assert
            resultado.Should().BeNull("o resultado deve ser nulo quando email não existe");
            
            await usuarioGatewayMock.Received(1).ObterPorEmailAsync(email);
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
            var logGatewayMock = Substitute.For<ILogGateway<ObterUsuarioPorEmailHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();

            var email = "teste@teste.com";
            usuarioGatewayMock.ObterPorEmailAsync(email).Returns(Task.FromException<Usuario?>(new InvalidOperationException("Erro no banco")));

            var handler = new ObterUsuarioPorEmailHandler(
                usuarioGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await handler.Handle(email));
        }
    }
}
