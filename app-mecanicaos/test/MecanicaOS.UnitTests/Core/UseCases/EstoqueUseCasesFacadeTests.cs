using Core.DTOs.UseCases.Estoque;
using Core.Entidades;
using Core.Interfaces.Handlers.Estoques;
using Core.UseCases.Estoques;

namespace MecanicaOS.UnitTests.Core.UseCases
{
    /// <summary>
    /// Testes para EstoqueUseCasesFacade
    /// Importância: Valida delegação correta para handlers de Estoque
    /// </summary>
    public class EstoqueUseCasesFacadeTests
    {
        [Fact]
        public async Task CadastrarUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<ICadastrarEstoqueHandler>();
            var estoque = new Estoque { Id = Guid.NewGuid() };
            var dto = new CadastrarEstoqueUseCaseDto();
            
            handlerMock.Handle(dto).Returns(estoque);
            
            var facade = new EstoqueUseCasesFacade(
                handlerMock,
                Substitute.For<IAtualizarEstoqueHandler>(),
                Substitute.For<IDeletarEstoqueHandler>(),
                Substitute.For<IObterEstoqueHandler>(),
                Substitute.For<IObterTodosEstoquesHandler>(),
                Substitute.For<IObterEstoqueCriticoHandler>());
            
            // Act
            var resultado = await facade.CadastrarUseCaseAsync(dto);
            
            // Assert
            resultado.Should().Be(estoque);
            await handlerMock.Received(1).Handle(dto);
        }

        [Fact]
        public async Task AtualizarUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IAtualizarEstoqueHandler>();
            var estoque = new Estoque { Id = Guid.NewGuid() };
            var dto = new AtualizarEstoqueUseCaseDto();
            
            handlerMock.Handle(estoque.Id, dto).Returns(estoque);
            
            var facade = new EstoqueUseCasesFacade(
                Substitute.For<ICadastrarEstoqueHandler>(),
                handlerMock,
                Substitute.For<IDeletarEstoqueHandler>(),
                Substitute.For<IObterEstoqueHandler>(),
                Substitute.For<IObterTodosEstoquesHandler>(),
                Substitute.For<IObterEstoqueCriticoHandler>());
            
            // Act
            var resultado = await facade.AtualizarUseCaseAsync(estoque.Id, dto);
            
            // Assert
            resultado.Should().Be(estoque);
            await handlerMock.Received(1).Handle(estoque.Id, dto);
        }

        [Fact]
        public async Task DeletarUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IDeletarEstoqueHandler>();
            var id = Guid.NewGuid();
            
            handlerMock.Handle(id).Returns(true);
            
            var facade = new EstoqueUseCasesFacade(
                Substitute.For<ICadastrarEstoqueHandler>(),
                Substitute.For<IAtualizarEstoqueHandler>(),
                handlerMock,
                Substitute.For<IObterEstoqueHandler>(),
                Substitute.For<IObterTodosEstoquesHandler>(),
                Substitute.For<IObterEstoqueCriticoHandler>());
            
            // Act
            var resultado = await facade.DeletarUseCaseAsync(id);
            
            // Assert
            resultado.Should().BeTrue();
            await handlerMock.Received(1).Handle(id);
        }

        [Fact]
        public async Task ObterPorIdUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IObterEstoqueHandler>();
            var estoque = new Estoque { Id = Guid.NewGuid() };
            
            handlerMock.Handle(estoque.Id).Returns(estoque);
            
            var facade = new EstoqueUseCasesFacade(
                Substitute.For<ICadastrarEstoqueHandler>(),
                Substitute.For<IAtualizarEstoqueHandler>(),
                Substitute.For<IDeletarEstoqueHandler>(),
                handlerMock,
                Substitute.For<IObterTodosEstoquesHandler>(),
                Substitute.For<IObterEstoqueCriticoHandler>());
            
            // Act
            var resultado = await facade.ObterPorIdUseCaseAsync(estoque.Id);
            
            // Assert
            resultado.Should().Be(estoque);
            await handlerMock.Received(1).Handle(estoque.Id);
        }

        [Fact]
        public async Task ObterTodosUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IObterTodosEstoquesHandler>();
            var estoques = new List<Estoque> { new Estoque { Id = Guid.NewGuid() } };
            
            handlerMock.Handle().Returns(estoques);
            
            var facade = new EstoqueUseCasesFacade(
                Substitute.For<ICadastrarEstoqueHandler>(),
                Substitute.For<IAtualizarEstoqueHandler>(),
                Substitute.For<IDeletarEstoqueHandler>(),
                Substitute.For<IObterEstoqueHandler>(),
                handlerMock,
                Substitute.For<IObterEstoqueCriticoHandler>());
            
            // Act
            var resultado = await facade.ObterTodosUseCaseAsync();
            
            // Assert
            resultado.Should().BeEquivalentTo(estoques);
            await handlerMock.Received(1).Handle();
        }

        [Fact]
        public async Task ObterEstoqueCriticoUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IObterEstoqueCriticoHandler>();
            var estoques = new List<Estoque> { new Estoque { Id = Guid.NewGuid() } };
            
            handlerMock.Handle().Returns(estoques);
            
            var facade = new EstoqueUseCasesFacade(
                Substitute.For<ICadastrarEstoqueHandler>(),
                Substitute.For<IAtualizarEstoqueHandler>(),
                Substitute.For<IDeletarEstoqueHandler>(),
                Substitute.For<IObterEstoqueHandler>(),
                Substitute.For<IObterTodosEstoquesHandler>(),
                handlerMock);
            
            // Act
            var resultado = await facade.ObterEstoqueCriticoUseCaseAsync();
            
            // Assert
            resultado.Should().BeEquivalentTo(estoques);
            await handlerMock.Received(1).Handle();
        }
    }
}
