using Core.DTOs.UseCases.OrdemServico;
using Core.Entidades;
using Core.Enumeradores;
using Core.Interfaces.Handlers.OrdensServico;
using Core.UseCases.OrdensServico;

namespace MecanicaOS.UnitTests.Core.UseCases
{
    /// <summary>
    /// Testes para OrdemServicoUseCasesFacade
    /// Importância: Valida delegação correta para handlers de OrdemServico
    /// </summary>
    public class OrdemServicoUseCasesFacadeTests
    {
        [Fact]
        public async Task CadastrarUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<ICadastrarOrdemServicoHandler>();
            var os = new OrdemServico { Id = Guid.NewGuid() };
            var dto = new CadastrarOrdemServicoUseCaseDto();
            
            handlerMock.Handle(dto).Returns(os);
            
            var facade = new OrdemServicoUseCasesFacade(
                handlerMock,
                Substitute.For<IAtualizarOrdemServicoHandler>(),
                Substitute.For<IObterOrdemServicoHandler>(),
                Substitute.For<IObterTodosOrdensServicoHandler>(),
                Substitute.For<IObterOrdemServicoPorStatusHandler>(),
                Substitute.For<IAceitarOrcamentoHandler>(),
                Substitute.For<IRecusarOrcamentoHandler>(),
                Substitute.For<IListarOrdensServicoAtivasHandler>(),
                Substitute.For<IObterOrcamentosExpiradosHandler>());
            
            // Act
            var resultado = await facade.CadastrarUseCaseAsync(dto);
            
            // Assert
            resultado.Should().Be(os);
            await handlerMock.Received(1).Handle(dto);
        }

        [Fact]
        public async Task AtualizarUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IAtualizarOrdemServicoHandler>();
            var os = new OrdemServico { Id = Guid.NewGuid() };
            var dto = new AtualizarOrdemServicoUseCaseDto();
            
            handlerMock.Handle(os.Id, dto).Returns(os);
            
            var facade = new OrdemServicoUseCasesFacade(
                Substitute.For<ICadastrarOrdemServicoHandler>(),
                handlerMock,
                Substitute.For<IObterOrdemServicoHandler>(),
                Substitute.For<IObterTodosOrdensServicoHandler>(),
                Substitute.For<IObterOrdemServicoPorStatusHandler>(),
                Substitute.For<IAceitarOrcamentoHandler>(),
                Substitute.For<IRecusarOrcamentoHandler>(),
                Substitute.For<IListarOrdensServicoAtivasHandler>(),
                Substitute.For<IObterOrcamentosExpiradosHandler>());
            
            // Act
            var resultado = await facade.AtualizarUseCaseAsync(os.Id, dto);
            
            // Assert
            resultado.Should().Be(os);
            await handlerMock.Received(1).Handle(os.Id, dto);
        }

        [Fact]
        public async Task ObterPorIdUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IObterOrdemServicoHandler>();
            var os = new OrdemServico { Id = Guid.NewGuid() };
            
            handlerMock.Handle(os.Id).Returns(os);
            
            var facade = new OrdemServicoUseCasesFacade(
                Substitute.For<ICadastrarOrdemServicoHandler>(),
                Substitute.For<IAtualizarOrdemServicoHandler>(),
                handlerMock,
                Substitute.For<IObterTodosOrdensServicoHandler>(),
                Substitute.For<IObterOrdemServicoPorStatusHandler>(),
                Substitute.For<IAceitarOrcamentoHandler>(),
                Substitute.For<IRecusarOrcamentoHandler>(),
                Substitute.For<IListarOrdensServicoAtivasHandler>(),
                Substitute.For<IObterOrcamentosExpiradosHandler>());
            
            // Act
            var resultado = await facade.ObterPorIdUseCaseAsync(os.Id);
            
            // Assert
            resultado.Should().Be(os);
            await handlerMock.Received(1).Handle(os.Id);
        }

        [Fact]
        public async Task ObterTodosUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IObterTodosOrdensServicoHandler>();
            var ordensServico = new List<OrdemServico> { new OrdemServico { Id = Guid.NewGuid() } };
            
            handlerMock.Handle().Returns(ordensServico);
            
            var facade = new OrdemServicoUseCasesFacade(
                Substitute.For<ICadastrarOrdemServicoHandler>(),
                Substitute.For<IAtualizarOrdemServicoHandler>(),
                Substitute.For<IObterOrdemServicoHandler>(),
                handlerMock,
                Substitute.For<IObterOrdemServicoPorStatusHandler>(),
                Substitute.For<IAceitarOrcamentoHandler>(),
                Substitute.For<IRecusarOrcamentoHandler>(),
                Substitute.For<IListarOrdensServicoAtivasHandler>(),
                Substitute.For<IObterOrcamentosExpiradosHandler>());
            
            // Act
            var resultado = await facade.ObterTodosUseCaseAsync();
            
            // Assert
            resultado.Should().BeEquivalentTo(ordensServico);
            await handlerMock.Received(1).Handle();
        }

        [Fact]
        public async Task ObterPorStatusUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IObterOrdemServicoPorStatusHandler>();
            var ordensServico = new List<OrdemServico> { new OrdemServico { Id = Guid.NewGuid() } };
            var status = StatusOrdemServico.Recebida;
            
            handlerMock.Handle(status).Returns(ordensServico);
            
            var facade = new OrdemServicoUseCasesFacade(
                Substitute.For<ICadastrarOrdemServicoHandler>(),
                Substitute.For<IAtualizarOrdemServicoHandler>(),
                Substitute.For<IObterOrdemServicoHandler>(),
                Substitute.For<IObterTodosOrdensServicoHandler>(),
                handlerMock,
                Substitute.For<IAceitarOrcamentoHandler>(),
                Substitute.For<IRecusarOrcamentoHandler>(),
                Substitute.For<IListarOrdensServicoAtivasHandler>(),
                Substitute.For<IObterOrcamentosExpiradosHandler>());
            
            // Act
            var resultado = await facade.ObterPorStatusUseCaseAsync(status);
            
            // Assert
            resultado.Should().BeEquivalentTo(ordensServico);
            await handlerMock.Received(1).Handle(status);
        }

        [Fact]
        public async Task AceitarOrcamentoUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IAceitarOrcamentoHandler>();
            var id = Guid.NewGuid();
            
            handlerMock.Handle(id).Returns(true);
            
            var facade = new OrdemServicoUseCasesFacade(
                Substitute.For<ICadastrarOrdemServicoHandler>(),
                Substitute.For<IAtualizarOrdemServicoHandler>(),
                Substitute.For<IObterOrdemServicoHandler>(),
                Substitute.For<IObterTodosOrdensServicoHandler>(),
                Substitute.For<IObterOrdemServicoPorStatusHandler>(),
                handlerMock,
                Substitute.For<IRecusarOrcamentoHandler>(),
                Substitute.For<IListarOrdensServicoAtivasHandler>(),
                Substitute.For<IObterOrcamentosExpiradosHandler>());
            
            // Act
            await facade.AceitarOrcamentoUseCaseAsync(id);
            
            // Assert
            await handlerMock.Received(1).Handle(id);
        }

        [Fact]
        public async Task RecusarOrcamentoUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IRecusarOrcamentoHandler>();
            var id = Guid.NewGuid();
            
            handlerMock.Handle(id).Returns(true);
            
            var facade = new OrdemServicoUseCasesFacade(
                Substitute.For<ICadastrarOrdemServicoHandler>(),
                Substitute.For<IAtualizarOrdemServicoHandler>(),
                Substitute.For<IObterOrdemServicoHandler>(),
                Substitute.For<IObterTodosOrdensServicoHandler>(),
                Substitute.For<IObterOrdemServicoPorStatusHandler>(),
                Substitute.For<IAceitarOrcamentoHandler>(),
                handlerMock,
                Substitute.For<IListarOrdensServicoAtivasHandler>(),
                Substitute.For<IObterOrcamentosExpiradosHandler>());
            
            // Act
            await facade.RecusarOrcamentoUseCaseAsync(id);
            
            // Assert
            await handlerMock.Received(1).Handle(id);
        }
    }
}
