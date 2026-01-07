using Core.DTOs.Requests.OrdemServico.InsumoOS;
using Core.DTOs.UseCases.OrdemServico.InsumoOS;
using Core.Entidades;
using Core.Interfaces.Handlers.InsumosOS;
using Core.UseCases.InsumosOS;

namespace MecanicaOS.UnitTests.Core.UseCases
{
    /// <summary>
    /// Testes para InsumoOSUseCasesFacade
    /// Importância: Valida delegação correta para handlers de InsumoOS
    /// </summary>
    public class InsumoOSUseCasesFacadeTests
    {
        [Fact]
        public async Task CadastrarInsumosUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<ICadastrarInsumosHandler>();
            var ordemServicoId = Guid.NewGuid();
            var insumos = new List<InsumoOS> { new InsumoOS { Id = Guid.NewGuid() } };
            var dtos = new List<CadastrarInsumoOSUseCaseDto> 
            { 
                new CadastrarInsumoOSUseCaseDto 
                { 
                    EstoqueId = Guid.NewGuid(), 
                    Quantidade = 5 
                } 
            };
            
            handlerMock.Handle(ordemServicoId, dtos).Returns(insumos);
            
            var facade = new InsumoOSUseCasesFacade(handlerMock);
            
            // Act
            var resultado = await facade.CadastrarInsumosUseCaseAsync(ordemServicoId, dtos);
            
            // Assert
            resultado.Should().BeEquivalentTo(insumos);
            await handlerMock.Received(1).Handle(ordemServicoId, dtos);
        }

        [Fact]
        public async Task DevolverInsumosAoEstoqueUseCaseAsync_DeveRetornarTrue()
        {
            // Arrange
            var handlerMock = Substitute.For<ICadastrarInsumosHandler>();
            var facade = new InsumoOSUseCasesFacade(handlerMock);
            
            var insumosRequest = new List<DevolverInsumoOSRequest>
            {
                new DevolverInsumoOSRequest
                {
                    EstoqueId = Guid.NewGuid(),
                    Quantidade = 3
                }
            };
            
            // Act
            var resultado = await facade.DevolverInsumosAoEstoqueUseCaseAsync(insumosRequest);
            
            // Assert
            resultado.Should().BeTrue("a devolução deve retornar true");
        }
    }
}
