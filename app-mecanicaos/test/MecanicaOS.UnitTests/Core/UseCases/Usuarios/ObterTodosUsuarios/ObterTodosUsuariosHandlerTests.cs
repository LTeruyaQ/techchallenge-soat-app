using Core.Entidades;
using Core.Interfaces.Gateways;
using Core.UseCases.Usuarios.ObterTodosUsuarios;
using MecanicaOS.UnitTests.Fixtures;

namespace MecanicaOS.UnitTests.Core.UseCases.Usuarios.ObterTodosUsuarios
{
    /// <summary>
    /// Testes unitários para o handler ObterTodosUsuariosHandler
    /// </summary>
    public class ObterTodosUsuariosHandlerTests
    {
        /// <summary>
        /// Verifica se o handler retorna lista de usuários
        /// </summary>
        [Fact]
        public async Task Handle_DeveRetornarListaDeUsuarios()
        {
            // Arrange
            var usuarioGatewayMock = UsuarioHandlerFixture.CriarUsuarioGatewayMock();
            var unidadeDeTrabalhoMock = UsuarioHandlerFixture.CriarUnidadeDeTrabalhMock();
            
            var usuarios = new List<Usuario>
            {
                UsuarioHandlerFixture.CriarUsuario(),
                UsuarioHandlerFixture.CriarUsuario(),
                UsuarioHandlerFixture.CriarUsuario()
            };
            
            usuarioGatewayMock.ObterTodosAsync().Returns(usuarios);
            
            var logGatewayMock = Substitute.For<ILogGateway<ObterTodosUsuariosHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new ObterTodosUsuariosHandler(
                usuarioGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            var resultado = await handler.Handle();
            
            // Assert
            resultado.Should().NotBeNull("o resultado não deve ser nulo");
            resultado.Should().HaveCount(3, "devem ser retornados 3 usuários");
            resultado.All(u => u.Senha == string.Empty).Should().BeTrue("nenhum usuário deve ter senha retornada");
            
            await usuarioGatewayMock.Received(1).ObterTodosAsync();
        }

        /// <summary>
        /// Verifica se o handler retorna lista vazia quando não há usuários
        /// </summary>
        [Fact]
        public async Task Handle_SemUsuarios_DeveRetornarListaVazia()
        {
            // Arrange
            var usuarioGatewayMock = UsuarioHandlerFixture.CriarUsuarioGatewayMock();
            var unidadeDeTrabalhoMock = UsuarioHandlerFixture.CriarUnidadeDeTrabalhMock();
            
            usuarioGatewayMock.ObterTodosAsync().Returns(new List<Usuario>());
            
            var logGatewayMock = Substitute.For<ILogGateway<ObterTodosUsuariosHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new ObterTodosUsuariosHandler(
                usuarioGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            var resultado = await handler.Handle();
            
            // Assert
            resultado.Should().NotBeNull("o resultado não deve ser nulo");
            resultado.Should().BeEmpty("a lista deve estar vazia");
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
            var logGatewayMock = Substitute.For<ILogGateway<ObterTodosUsuariosHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();

            usuarioGatewayMock.ObterTodosAsync().Returns(Task.FromException<IEnumerable<Usuario>>(new InvalidOperationException("Erro no banco")));

            var handler = new ObterTodosUsuariosHandler(
                usuarioGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await handler.Handle());
        }
    }
}
