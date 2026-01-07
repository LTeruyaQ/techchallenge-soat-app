using Core.DTOs.UseCases.Orcamento;
using Core.Entidades;
using Core.Interfaces.Handlers.Orcamentos;
using Core.UseCases.Orcamentos;

namespace MecanicaOS.UnitTests.Core.UseCases
{
    /// <summary>
    /// Testes para OrcamentoUseCasesFacade
    /// Importância: Valida delegação correta para handler de Orçamento
    /// </summary>
    public class OrcamentoUseCasesFacadeTests
    {
        [Fact]
        public void GerarOrcamentoUseCase_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IGerarOrcamentoHandler>();
            var ordemServico = new OrdemServico { Id = Guid.NewGuid() };
            var valorOrcamento = 1500.00m;
            
            handlerMock.Handle(Arg.Any<GerarOrcamentoUseCase>()).Returns(valorOrcamento);
            
            var facade = new OrcamentoUseCasesFacade(handlerMock);
            
            // Act
            var resultado = facade.GerarOrcamentoUseCase(ordemServico);
            
            // Assert
            resultado.Should().Be(valorOrcamento);
            handlerMock.Received(1).Handle(Arg.Any<GerarOrcamentoUseCase>());
        }
    }
}
