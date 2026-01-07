using Core.DTOs.UseCases.Usuario;
using Core.Entidades;
using Core.Interfaces.Handlers.Usuarios;
using Core.UseCases.Usuarios;

namespace MecanicaOS.UnitTests.Core.UseCases
{
    /// <summary>
    /// Testes para UsuarioUseCasesFacade
    /// Importância: Valida delegação correta para handlers de Usuário
    /// </summary>
    public class UsuarioUseCasesFacadeTests
    {
        [Fact]
        public async Task CadastrarUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<ICadastrarUsuarioHandler>();
            var usuario = new Usuario { Id = Guid.NewGuid() };
            var dto = new CadastrarUsuarioUseCaseDto();
            
            handlerMock.Handle(dto).Returns(usuario);
            
            var facade = new UsuarioUseCasesFacade(
                handlerMock,
                Substitute.For<IAtualizarUsuarioHandler>(),
                Substitute.For<IObterUsuarioHandler>(),
                Substitute.For<IObterTodosUsuariosHandler>(),
                Substitute.For<IDeletarUsuarioHandler>(),
                Substitute.For<IObterUsuarioPorEmailHandler>(),
                Substitute.For<IObterUsuariosParaAlertaEstoqueHandler>());
            
            // Act
            var resultado = await facade.CadastrarUseCaseAsync(dto);
            
            // Assert
            resultado.Should().Be(usuario);
            await handlerMock.Received(1).Handle(dto);
        }

        [Fact]
        public async Task AtualizarUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IAtualizarUsuarioHandler>();
            var usuario = new Usuario { Id = Guid.NewGuid() };
            var dto = new AtualizarUsuarioUseCaseDto();
            
            handlerMock.Handle(usuario.Id, dto).Returns(usuario);
            
            var facade = new UsuarioUseCasesFacade(
                Substitute.For<ICadastrarUsuarioHandler>(),
                handlerMock,
                Substitute.For<IObterUsuarioHandler>(),
                Substitute.For<IObterTodosUsuariosHandler>(),
                Substitute.For<IDeletarUsuarioHandler>(),
                Substitute.For<IObterUsuarioPorEmailHandler>(),
                Substitute.For<IObterUsuariosParaAlertaEstoqueHandler>());
            
            // Act
            var resultado = await facade.AtualizarUseCaseAsync(usuario.Id, dto);
            
            // Assert
            resultado.Should().Be(usuario);
            await handlerMock.Received(1).Handle(usuario.Id, dto);
        }

        [Fact]
        public async Task ObterPorIdUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IObterUsuarioHandler>();
            var usuario = new Usuario { Id = Guid.NewGuid() };
            
            handlerMock.Handle(usuario.Id).Returns(usuario);
            
            var facade = new UsuarioUseCasesFacade(
                Substitute.For<ICadastrarUsuarioHandler>(),
                Substitute.For<IAtualizarUsuarioHandler>(),
                handlerMock,
                Substitute.For<IObterTodosUsuariosHandler>(),
                Substitute.For<IDeletarUsuarioHandler>(),
                Substitute.For<IObterUsuarioPorEmailHandler>(),
                Substitute.For<IObterUsuariosParaAlertaEstoqueHandler>());
            
            // Act
            var resultado = await facade.ObterPorIdUseCaseAsync(usuario.Id);
            
            // Assert
            resultado.Should().Be(usuario);
            await handlerMock.Received(1).Handle(usuario.Id);
        }

        [Fact]
        public async Task ObterTodosUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IObterTodosUsuariosHandler>();
            var usuarios = new List<Usuario> { new Usuario { Id = Guid.NewGuid() } };
            
            handlerMock.Handle().Returns(usuarios);
            
            var facade = new UsuarioUseCasesFacade(
                Substitute.For<ICadastrarUsuarioHandler>(),
                Substitute.For<IAtualizarUsuarioHandler>(),
                Substitute.For<IObterUsuarioHandler>(),
                handlerMock,
                Substitute.For<IDeletarUsuarioHandler>(),
                Substitute.For<IObterUsuarioPorEmailHandler>(),
                Substitute.For<IObterUsuariosParaAlertaEstoqueHandler>());
            
            // Act
            var resultado = await facade.ObterTodosUseCaseAsync();
            
            // Assert
            resultado.Should().BeEquivalentTo(usuarios);
            await handlerMock.Received(1).Handle();
        }

        [Fact]
        public async Task DeletarUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IDeletarUsuarioHandler>();
            var id = Guid.NewGuid();
            
            handlerMock.Handle(id).Returns(true);
            
            var facade = new UsuarioUseCasesFacade(
                Substitute.For<ICadastrarUsuarioHandler>(),
                Substitute.For<IAtualizarUsuarioHandler>(),
                Substitute.For<IObterUsuarioHandler>(),
                Substitute.For<IObterTodosUsuariosHandler>(),
                handlerMock,
                Substitute.For<IObterUsuarioPorEmailHandler>(),
                Substitute.For<IObterUsuariosParaAlertaEstoqueHandler>());
            
            // Act
            var resultado = await facade.DeletarUseCaseAsync(id);
            
            // Assert
            resultado.Should().BeTrue();
            await handlerMock.Received(1).Handle(id);
        }

        [Fact]
        public async Task ObterPorEmailUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IObterUsuarioPorEmailHandler>();
            var usuario = new Usuario { Id = Guid.NewGuid(), Email = "teste@teste.com" };
            
            handlerMock.Handle(usuario.Email).Returns(usuario);
            
            var facade = new UsuarioUseCasesFacade(
                Substitute.For<ICadastrarUsuarioHandler>(),
                Substitute.For<IAtualizarUsuarioHandler>(),
                Substitute.For<IObterUsuarioHandler>(),
                Substitute.For<IObterTodosUsuariosHandler>(),
                Substitute.For<IDeletarUsuarioHandler>(),
                handlerMock,
                Substitute.For<IObterUsuariosParaAlertaEstoqueHandler>());
            
            // Act
            var resultado = await facade.ObterPorEmailUseCaseAsync(usuario.Email);
            
            // Assert
            resultado.Should().Be(usuario);
            await handlerMock.Received(1).Handle(usuario.Email);
        }
    }
}
