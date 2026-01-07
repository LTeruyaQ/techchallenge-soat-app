using Core.DTOs.UseCases.Servico;
using Core.Entidades;
using Core.Interfaces.Handlers.Servicos;
using Core.UseCases.Servicos;

namespace MecanicaOS.UnitTests.Core.UseCases
{
    /// <summary>
    /// Testes para ServicoUseCasesFacade
    /// Importância: Valida delegação correta para handlers de Serviço
    /// </summary>
    public class ServicoUseCasesFacadeTests
    {
        [Fact]
        public async Task CadastrarUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<ICadastrarServicoHandler>();
            var servico = new Servico { Id = Guid.NewGuid(), Nome = "Teste", Descricao = "Teste" };
            var dto = new CadastrarServicoUseCaseDto { Nome = "Teste", Descricao = "Teste", Valor = 100, Disponivel = true };
            
            handlerMock.Handle(dto).Returns(servico);
            
            var facade = new ServicoUseCasesFacade(
                handlerMock,
                Substitute.For<IEditarServicoHandler>(),
                Substitute.For<IDeletarServicoHandler>(),
                Substitute.For<IObterServicoHandler>(),
                Substitute.For<IObterTodosServicosHandler>(),
                Substitute.For<IObterServicoPorNomeHandler>(),
                Substitute.For<IObterServicosDisponiveisHandler>());
            
            // Act
            var resultado = await facade.CadastrarServicoUseCaseAsync(dto);
            
            // Assert
            resultado.Should().Be(servico);
            await handlerMock.Received(1).Handle(dto);
        }

        [Fact]
        public async Task EditarUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IEditarServicoHandler>();
            var servico = new Servico { Id = Guid.NewGuid(), Nome = "Teste", Descricao = "Teste" };
            var dto = new EditarServicoUseCaseDto { Nome = "Teste", Descricao = "Teste", Valor = 100, Disponivel = true };
            
            handlerMock.Handle(servico.Id, dto).Returns(servico);
            
            var facade = new ServicoUseCasesFacade(
                Substitute.For<ICadastrarServicoHandler>(),
                handlerMock,
                Substitute.For<IDeletarServicoHandler>(),
                Substitute.For<IObterServicoHandler>(),
                Substitute.For<IObterTodosServicosHandler>(),
                Substitute.For<IObterServicoPorNomeHandler>(),
                Substitute.For<IObterServicosDisponiveisHandler>());
            
            // Act
            var resultado = await facade.EditarServicoUseCaseAsync(servico.Id, dto);
            
            // Assert
            resultado.Should().Be(servico);
            await handlerMock.Received(1).Handle(servico.Id, dto);
        }

        [Fact]
        public async Task DeletarUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IDeletarServicoHandler>();
            var id = Guid.NewGuid();
            
            handlerMock.Handle(id).Returns(true);
            
            var facade = new ServicoUseCasesFacade(
                Substitute.For<ICadastrarServicoHandler>(),
                Substitute.For<IEditarServicoHandler>(),
                handlerMock,
                Substitute.For<IObterServicoHandler>(),
                Substitute.For<IObterTodosServicosHandler>(),
                Substitute.For<IObterServicoPorNomeHandler>(),
                Substitute.For<IObterServicosDisponiveisHandler>());
            
            // Act
            var resultado = await facade.DeletarServicoUseCaseAsync(id);
            
            // Assert
            resultado.Should().BeTrue();
            await handlerMock.Received(1).Handle(id);
        }

        [Fact]
        public async Task ObterPorIdUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IObterServicoHandler>();
            var servico = new Servico { Id = Guid.NewGuid(), Nome = "Teste", Descricao = "Teste" };
            
            handlerMock.Handle(servico.Id).Returns(servico);
            
            var facade = new ServicoUseCasesFacade(
                Substitute.For<ICadastrarServicoHandler>(),
                Substitute.For<IEditarServicoHandler>(),
                Substitute.For<IDeletarServicoHandler>(),
                handlerMock,
                Substitute.For<IObterTodosServicosHandler>(),
                Substitute.For<IObterServicoPorNomeHandler>(),
                Substitute.For<IObterServicosDisponiveisHandler>());
            
            // Act
            var resultado = await facade.ObterServicoPorIdUseCaseAsync(servico.Id);
            
            // Assert
            resultado.Should().Be(servico);
            await handlerMock.Received(1).Handle(servico.Id);
        }

        [Fact]
        public async Task ObterTodosUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IObterTodosServicosHandler>();
            var servicos = new List<Servico> { new Servico { Id = Guid.NewGuid(), Nome = "Teste", Descricao = "Teste" } };
            
            handlerMock.Handle().Returns(servicos);
            
            var facade = new ServicoUseCasesFacade(
                Substitute.For<ICadastrarServicoHandler>(),
                Substitute.For<IEditarServicoHandler>(),
                Substitute.For<IDeletarServicoHandler>(),
                Substitute.For<IObterServicoHandler>(),
                handlerMock,
                Substitute.For<IObterServicoPorNomeHandler>(),
                Substitute.For<IObterServicosDisponiveisHandler>());
            
            // Act
            var resultado = await facade.ObterTodosUseCaseAsync();
            
            // Assert
            resultado.Should().BeEquivalentTo(servicos);
            await handlerMock.Received(1).Handle();
        }

        [Fact]
        public async Task ObterPorNomeUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IObterServicoPorNomeHandler>();
            var servico = new Servico { Id = Guid.NewGuid(), Nome = "Troca de Óleo", Descricao = "Teste" };
            
            handlerMock.Handle(servico.Nome).Returns(servico);
            
            var facade = new ServicoUseCasesFacade(
                Substitute.For<ICadastrarServicoHandler>(),
                Substitute.For<IEditarServicoHandler>(),
                Substitute.For<IDeletarServicoHandler>(),
                Substitute.For<IObterServicoHandler>(),
                Substitute.For<IObterTodosServicosHandler>(),
                handlerMock,
                Substitute.For<IObterServicosDisponiveisHandler>());
            
            // Act
            var resultado = await facade.ObterServicoPorNomeUseCaseAsync(servico.Nome);
            
            // Assert
            resultado.Should().Be(servico);
            await handlerMock.Received(1).Handle(servico.Nome);
        }

        [Fact]
        public async Task ObterServicosDisponiveisUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IObterServicosDisponiveisHandler>();
            var servicos = new List<Servico> { new Servico { Id = Guid.NewGuid(), Nome = "Teste", Descricao = "Teste" } };
            
            handlerMock.Handle().Returns(servicos);
            
            var facade = new ServicoUseCasesFacade(
                Substitute.For<ICadastrarServicoHandler>(),
                Substitute.For<IEditarServicoHandler>(),
                Substitute.For<IDeletarServicoHandler>(),
                Substitute.For<IObterServicoHandler>(),
                Substitute.For<IObterTodosServicosHandler>(),
                Substitute.For<IObterServicoPorNomeHandler>(),
                handlerMock);
            
            // Act
            var resultado = await facade.ObterServicosDisponiveisUseCaseAsync();
            
            // Assert
            resultado.Should().BeEquivalentTo(servicos);
            await handlerMock.Received(1).Handle();
        }
    }
}
