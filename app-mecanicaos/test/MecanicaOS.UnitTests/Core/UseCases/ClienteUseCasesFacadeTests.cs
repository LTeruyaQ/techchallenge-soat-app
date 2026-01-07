using Core.DTOs.UseCases.Cliente;
using Core.Entidades;
using Core.Interfaces.Handlers.Clientes;
using Core.UseCases.Clientes;

namespace MecanicaOS.UnitTests.Core.UseCases
{
    /// <summary>
    /// Testes para ClienteUseCasesFacade
    /// Importância: Valida que o facade delega corretamente para os handlers individuais.
    /// Garante compatibilidade com código legado e funcionamento do padrão Facade.
    /// </summary>
    public class ClienteUseCasesFacadeTests
    {
        [Fact]
        public async Task CadastrarUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<ICadastrarClienteHandler>();
            var cliente = new Cliente { Id = Guid.NewGuid() };
            var dto = new CadastrarClienteUseCaseDto();
            
            handlerMock.Handle(dto).Returns(cliente);
            
            var facade = new ClienteUseCasesFacade(
                handlerMock,
                Substitute.For<IAtualizarClienteHandler>(),
                Substitute.For<IObterClienteHandler>(),
                Substitute.For<IObterTodosClientesHandler>(),
                Substitute.For<IRemoverClienteHandler>(),
                Substitute.For<IObterClientePorDocumentoHandler>(),
                Substitute.For<IObterClientePorNomeHandler>());
            
            // Act
            var resultado = await facade.CadastrarUseCaseAsync(dto);
            
            // Assert
            resultado.Should().Be(cliente);
            await handlerMock.Received(1).Handle(dto);
        }

        [Fact]
        public async Task AtualizarUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IAtualizarClienteHandler>();
            var cliente = new Cliente { Id = Guid.NewGuid() };
            var dto = new AtualizarClienteUseCaseDto();
            var id = Guid.NewGuid();
            
            handlerMock.Handle(id, dto).Returns(cliente);
            
            var facade = new ClienteUseCasesFacade(
                Substitute.For<ICadastrarClienteHandler>(),
                handlerMock,
                Substitute.For<IObterClienteHandler>(),
                Substitute.For<IObterTodosClientesHandler>(),
                Substitute.For<IRemoverClienteHandler>(),
                Substitute.For<IObterClientePorDocumentoHandler>(),
                Substitute.For<IObterClientePorNomeHandler>());
            
            // Act
            var resultado = await facade.AtualizarUseCaseAsync(id, dto);
            
            // Assert
            resultado.Should().Be(cliente);
            await handlerMock.Received(1).Handle(id, dto);
        }

        [Fact]
        public async Task ObterPorIdUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IObterClienteHandler>();
            var cliente = new Cliente { Id = Guid.NewGuid() };
            
            handlerMock.Handle(cliente.Id).Returns(cliente);
            
            var facade = new ClienteUseCasesFacade(
                Substitute.For<ICadastrarClienteHandler>(),
                Substitute.For<IAtualizarClienteHandler>(),
                handlerMock,
                Substitute.For<IObterTodosClientesHandler>(),
                Substitute.For<IRemoverClienteHandler>(),
                Substitute.For<IObterClientePorDocumentoHandler>(),
                Substitute.For<IObterClientePorNomeHandler>());
            
            // Act
            var resultado = await facade.ObterPorIdUseCaseAsync(cliente.Id);
            
            // Assert
            resultado.Should().Be(cliente);
            await handlerMock.Received(1).Handle(cliente.Id);
        }

        [Fact]
        public async Task ObterTodosUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IObterTodosClientesHandler>();
            var clientes = new List<Cliente> { new Cliente { Id = Guid.NewGuid() } };
            
            handlerMock.Handle().Returns(clientes);
            
            var facade = new ClienteUseCasesFacade(
                Substitute.For<ICadastrarClienteHandler>(),
                Substitute.For<IAtualizarClienteHandler>(),
                Substitute.For<IObterClienteHandler>(),
                handlerMock,
                Substitute.For<IRemoverClienteHandler>(),
                Substitute.For<IObterClientePorDocumentoHandler>(),
                Substitute.For<IObterClientePorNomeHandler>());
            
            // Act
            var resultado = await facade.ObterTodosUseCaseAsync();
            
            // Assert
            resultado.Should().BeEquivalentTo(clientes);
            await handlerMock.Received(1).Handle();
        }

        [Fact]
        public async Task RemoverUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IRemoverClienteHandler>();
            var id = Guid.NewGuid();
            
            handlerMock.Handle(id).Returns(true);
            
            var facade = new ClienteUseCasesFacade(
                Substitute.For<ICadastrarClienteHandler>(),
                Substitute.For<IAtualizarClienteHandler>(),
                Substitute.For<IObterClienteHandler>(),
                Substitute.For<IObterTodosClientesHandler>(),
                handlerMock,
                Substitute.For<IObterClientePorDocumentoHandler>(),
                Substitute.For<IObterClientePorNomeHandler>());
            
            // Act
            await facade.RemoverUseCaseAsync(id);
            
            // Assert
            await handlerMock.Received(1).Handle(id);
        }

        [Fact]
        public async Task ObterPorDocumentoUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IObterClientePorDocumentoHandler>();
            var cliente = new Cliente { Id = Guid.NewGuid(), Documento = "12345678900" };
            
            handlerMock.Handle(cliente.Documento).Returns(cliente);
            
            var facade = new ClienteUseCasesFacade(
                Substitute.For<ICadastrarClienteHandler>(),
                Substitute.For<IAtualizarClienteHandler>(),
                Substitute.For<IObterClienteHandler>(),
                Substitute.For<IObterTodosClientesHandler>(),
                Substitute.For<IRemoverClienteHandler>(),
                handlerMock,
                Substitute.For<IObterClientePorNomeHandler>());
            
            // Act
            var resultado = await facade.ObterPorDocumentoUseCaseAsync(cliente.Documento);
            
            // Assert
            resultado.Should().Be(cliente);
            await handlerMock.Received(1).Handle(cliente.Documento);
        }
    }
}
