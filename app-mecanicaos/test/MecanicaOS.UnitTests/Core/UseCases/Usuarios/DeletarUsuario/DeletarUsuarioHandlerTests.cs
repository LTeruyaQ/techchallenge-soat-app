using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.UseCases.Usuarios.DeletarUsuario;
using MecanicaOS.UnitTests.Fixtures;

namespace MecanicaOS.UnitTests.Core.UseCases.Usuarios.DeletarUsuario
{
    /// <summary>
    /// Testes unitários para o handler DeletarUsuarioHandler
    /// </summary>
    public class DeletarUsuarioHandlerTests
    {
        /// <summary>
        /// Verifica se o handler deleta usuário com sucesso
        /// </summary>
        [Fact]
        public async Task Handle_ComUsuarioExistente_DeveRetornarTrue()
        {
            // Arrange
            var usuarioGatewayMock = UsuarioHandlerFixture.CriarUsuarioGatewayMock();
            var unidadeDeTrabalhoMock = UsuarioHandlerFixture.CriarUnidadeDeTrabalhMock();
            
            var usuario = UsuarioHandlerFixture.CriarUsuario();
            
            usuarioGatewayMock.ObterPorIdAsync(usuario.Id).Returns(usuario);
            unidadeDeTrabalhoMock.Commit().Returns(true);
            
            var logGatewayMock = Substitute.For<ILogGateway<DeletarUsuarioHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new DeletarUsuarioHandler(
                usuarioGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            var resultado = await handler.Handle(usuario.Id);
            
            // Assert
            resultado.Should().BeTrue("deve retornar true quando usuário é deletado com sucesso");
            
            await usuarioGatewayMock.Received(1).ObterPorIdAsync(usuario.Id);
            await usuarioGatewayMock.Received(1).DeletarAsync(Arg.Any<Usuario>());
            await unidadeDeTrabalhoMock.Received(1).Commit();
        }

        /// <summary>
        /// Verifica se o handler lança exceção quando usuário não é encontrado
        /// </summary>
        [Fact]
        public async Task Handle_ComUsuarioInexistente_DeveLancarDadosNaoEncontradosException()
        {
            // Arrange
            var usuarioGatewayMock = UsuarioHandlerFixture.CriarUsuarioGatewayMock();
            var unidadeDeTrabalhoMock = UsuarioHandlerFixture.CriarUnidadeDeTrabalhMock();
            
            var usuarioId = Guid.NewGuid();
            
            usuarioGatewayMock.ObterPorIdAsync(usuarioId).Returns((Usuario?)null);
            
            var logGatewayMock = Substitute.For<ILogGateway<DeletarUsuarioHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new DeletarUsuarioHandler(
                usuarioGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            Func<Task> act = async () => await handler.Handle(usuarioId);
            
            // Assert
            await act.Should().ThrowAsync<DadosNaoEncontradosException>()
                .WithMessage("Usuário não encontrado");
            
            await usuarioGatewayMock.Received(1).ObterPorIdAsync(usuarioId);
            await usuarioGatewayMock.DidNotReceive().DeletarAsync(Arg.Any<Usuario>());
        }

        /// <summary>
        /// Verifica se o handler lança exceção quando commit falha
        /// </summary>
        [Fact]
        public async Task Handle_QuandoCommitFalha_DeveLancarPersistirDadosException()
        {
            // Arrange
            var usuarioGatewayMock = UsuarioHandlerFixture.CriarUsuarioGatewayMock();
            var unidadeDeTrabalhoMock = UsuarioHandlerFixture.CriarUnidadeDeTrabalhMock();
            
            var usuario = UsuarioHandlerFixture.CriarUsuario();
            
            usuarioGatewayMock.ObterPorIdAsync(usuario.Id).Returns(usuario);
            unidadeDeTrabalhoMock.Commit().Returns(false);
            
            var logGatewayMock = Substitute.For<ILogGateway<DeletarUsuarioHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new DeletarUsuarioHandler(
                usuarioGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            Func<Task> act = async () => await handler.Handle(usuario.Id);
            
            // Assert
            await act.Should().ThrowAsync<PersistirDadosException>()
                .WithMessage("Erro ao deletar usuário");
            
            await usuarioGatewayMock.Received(1).DeletarAsync(Arg.Any<Usuario>());
            await unidadeDeTrabalhoMock.Received(1).Commit();
        }
    }
}
