using Core.DTOs.UseCases.Veiculo;
using Core.Entidades;
using Core.Interfaces.Handlers.Veiculos;
using Core.UseCases.Veiculos;

namespace MecanicaOS.UnitTests.Core.UseCases
{
    /// <summary>
    /// Testes para VeiculoUseCasesFacade
    /// Importância: Valida delegação correta para handlers de Veículo
    /// </summary>
    public class VeiculoUseCasesFacadeTests
    {
        [Fact]
        public async Task CadastrarUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<ICadastrarVeiculoHandler>();
            var veiculo = new Veiculo { Id = Guid.NewGuid() };
            var dto = new CadastrarVeiculoUseCaseDto();
            
            handlerMock.Handle(dto).Returns(veiculo);
            
            var facade = new VeiculoUseCasesFacade(
                handlerMock,
                Substitute.For<IAtualizarVeiculoHandler>(),
                Substitute.For<IObterVeiculoHandler>(),
                Substitute.For<IObterTodosVeiculosHandler>(),
                Substitute.For<IObterVeiculoPorClienteHandler>(),
                Substitute.For<IObterVeiculoPorPlacaHandler>(),
                Substitute.For<IDeletarVeiculoHandler>());
            
            // Act
            var resultado = await facade.CadastrarUseCaseAsync(dto);
            
            // Assert
            resultado.Should().Be(veiculo);
            await handlerMock.Received(1).Handle(dto);
        }

        [Fact]
        public async Task AtualizarUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IAtualizarVeiculoHandler>();
            var veiculo = new Veiculo { Id = Guid.NewGuid() };
            var dto = new AtualizarVeiculoUseCaseDto();
            
            handlerMock.Handle(veiculo.Id, dto).Returns(veiculo);
            
            var facade = new VeiculoUseCasesFacade(
                Substitute.For<ICadastrarVeiculoHandler>(),
                handlerMock,
                Substitute.For<IObterVeiculoHandler>(),
                Substitute.For<IObterTodosVeiculosHandler>(),
                Substitute.For<IObterVeiculoPorClienteHandler>(),
                Substitute.For<IObterVeiculoPorPlacaHandler>(),
                Substitute.For<IDeletarVeiculoHandler>());
            
            // Act
            var resultado = await facade.AtualizarUseCaseAsync(veiculo.Id, dto);
            
            // Assert
            resultado.Should().Be(veiculo);
            await handlerMock.Received(1).Handle(veiculo.Id, dto);
        }

        [Fact]
        public async Task ObterPorIdUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IObterVeiculoHandler>();
            var veiculo = new Veiculo { Id = Guid.NewGuid() };
            
            handlerMock.Handle(veiculo.Id).Returns(veiculo);
            
            var facade = new VeiculoUseCasesFacade(
                Substitute.For<ICadastrarVeiculoHandler>(),
                Substitute.For<IAtualizarVeiculoHandler>(),
                handlerMock,
                Substitute.For<IObterTodosVeiculosHandler>(),
                Substitute.For<IObterVeiculoPorClienteHandler>(),
                Substitute.For<IObterVeiculoPorPlacaHandler>(),
                Substitute.For<IDeletarVeiculoHandler>());
            
            // Act
            var resultado = await facade.ObterPorIdUseCaseAsync(veiculo.Id);
            
            // Assert
            resultado.Should().Be(veiculo);
            await handlerMock.Received(1).Handle(veiculo.Id);
        }

        [Fact]
        public async Task ObterTodosUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IObterTodosVeiculosHandler>();
            var veiculos = new List<Veiculo> { new Veiculo { Id = Guid.NewGuid() } };
            
            handlerMock.Handle().Returns(veiculos);
            
            var facade = new VeiculoUseCasesFacade(
                Substitute.For<ICadastrarVeiculoHandler>(),
                Substitute.For<IAtualizarVeiculoHandler>(),
                Substitute.For<IObterVeiculoHandler>(),
                handlerMock,
                Substitute.For<IObterVeiculoPorClienteHandler>(),
                Substitute.For<IObterVeiculoPorPlacaHandler>(),
                Substitute.For<IDeletarVeiculoHandler>());
            
            // Act
            var resultado = await facade.ObterTodosUseCaseAsync();
            
            // Assert
            resultado.Should().BeEquivalentTo(veiculos);
            await handlerMock.Received(1).Handle();
        }

        [Fact]
        public async Task ObterPorClienteUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IObterVeiculoPorClienteHandler>();
            var clienteId = Guid.NewGuid();
            var veiculos = new List<Veiculo> { new Veiculo { Id = Guid.NewGuid() } };
            
            handlerMock.Handle(clienteId).Returns(veiculos);
            
            var facade = new VeiculoUseCasesFacade(
                Substitute.For<ICadastrarVeiculoHandler>(),
                Substitute.For<IAtualizarVeiculoHandler>(),
                Substitute.For<IObterVeiculoHandler>(),
                Substitute.For<IObterTodosVeiculosHandler>(),
                handlerMock,
                Substitute.For<IObterVeiculoPorPlacaHandler>(),
                Substitute.For<IDeletarVeiculoHandler>());
            
            // Act
            var resultado = await facade.ObterPorClienteUseCaseAsync(clienteId);
            
            // Assert
            resultado.Should().BeEquivalentTo(veiculos);
            await handlerMock.Received(1).Handle(clienteId);
        }

        [Fact]
        public async Task ObterPorPlacaUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IObterVeiculoPorPlacaHandler>();
            var veiculo = new Veiculo { Id = Guid.NewGuid(), Placa = "ABC1234" };
            
            handlerMock.Handle(veiculo.Placa).Returns(veiculo);
            
            var facade = new VeiculoUseCasesFacade(
                Substitute.For<ICadastrarVeiculoHandler>(),
                Substitute.For<IAtualizarVeiculoHandler>(),
                Substitute.For<IObterVeiculoHandler>(),
                Substitute.For<IObterTodosVeiculosHandler>(),
                Substitute.For<IObterVeiculoPorClienteHandler>(),
                handlerMock,
                Substitute.For<IDeletarVeiculoHandler>());
            
            // Act
            var resultado = await facade.ObterPorPlacaUseCaseAsync(veiculo.Placa);
            
            // Assert
            resultado.Should().Be(veiculo);
            await handlerMock.Received(1).Handle(veiculo.Placa);
        }

        [Fact]
        public async Task DeletarUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IDeletarVeiculoHandler>();
            var id = Guid.NewGuid();
            
            handlerMock.Handle(id).Returns(true);
            
            var facade = new VeiculoUseCasesFacade(
                Substitute.For<ICadastrarVeiculoHandler>(),
                Substitute.For<IAtualizarVeiculoHandler>(),
                Substitute.For<IObterVeiculoHandler>(),
                Substitute.For<IObterTodosVeiculosHandler>(),
                Substitute.For<IObterVeiculoPorClienteHandler>(),
                Substitute.For<IObterVeiculoPorPlacaHandler>(),
                handlerMock);
            
            // Act
            var resultado = await facade.DeletarUseCaseAsync(id);
            
            // Assert
            resultado.Should().BeTrue();
            await handlerMock.Received(1).Handle(id);
        }
    }
}
