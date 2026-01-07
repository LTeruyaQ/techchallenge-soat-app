using Core.Entidades;
using Core.Interfaces.Gateways;
using Core.UseCases.Veiculos.ObterTodosVeiculos;
using MecanicaOS.UnitTests.Fixtures;

namespace MecanicaOS.UnitTests.Core.UseCases.Veiculos.ObterTodosVeiculos
{
    /// <summary>
    /// Testes para ObterTodosVeiculosHandler
    /// Importância: Valida listagem completa de veículos
    /// </summary>
    public class ObterTodosVeiculosHandlerTests
    {
        [Fact]
        public async Task Handle_DeveRetornarListaDeVeiculos()
        {
            // Arrange
            var veiculoGatewayMock = VeiculoHandlerFixture.CriarVeiculoGatewayMock();
            var unidadeDeTrabalhoMock = VeiculoHandlerFixture.CriarUnidadeDeTrabalhMock();
            
            var veiculos = new List<Veiculo>
            {
                VeiculoHandlerFixture.CriarVeiculo(),
                VeiculoHandlerFixture.CriarVeiculo()
            };
            
            veiculoGatewayMock.ObterTodosAsync().Returns(veiculos);
            
            var logGatewayMock = Substitute.For<ILogGateway<ObterTodosVeiculosHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new ObterTodosVeiculosHandler(
                veiculoGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            var resultado = await handler.Handle();
            
            // Assert
            resultado.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_QuandoGatewayLancaExcecao_DevePropagar()
        {
            // Arrange
            var veiculoGatewayMock = VeiculoHandlerFixture.CriarVeiculoGatewayMock();
            var unidadeDeTrabalhoMock = VeiculoHandlerFixture.CriarUnidadeDeTrabalhMock();
            var logGatewayMock = Substitute.For<ILogGateway<ObterTodosVeiculosHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();

            veiculoGatewayMock.ObterTodosAsync().Returns(Task.FromException<IEnumerable<Veiculo>>(new InvalidOperationException("Erro no banco")));

            var handler = new ObterTodosVeiculosHandler(
                veiculoGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await handler.Handle());
        }
    }
}
