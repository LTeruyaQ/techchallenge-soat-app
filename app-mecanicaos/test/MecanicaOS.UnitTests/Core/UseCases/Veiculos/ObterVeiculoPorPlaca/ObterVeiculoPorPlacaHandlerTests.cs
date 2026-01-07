using Core.Entidades;
using Core.Interfaces.Gateways;
using Core.UseCases.Veiculos.ObterVeiculoPorPlaca;
using MecanicaOS.UnitTests.Fixtures;

namespace MecanicaOS.UnitTests.Core.UseCases.Veiculos.ObterVeiculoPorPlaca
{
    /// <summary>
    /// Testes para ObterVeiculoPorPlacaHandler
    /// Importância: Valida busca por placa, identificador único de veículos
    /// </summary>
    public class ObterVeiculoPorPlacaHandlerTests
    {
        [Fact]
        public async Task Handle_ComPlacaExistente_DeveRetornarVeiculo()
        {
            // Arrange
            var veiculoGatewayMock = VeiculoHandlerFixture.CriarVeiculoGatewayMock();
            var unidadeDeTrabalhoMock = VeiculoHandlerFixture.CriarUnidadeDeTrabalhMock();
            
            var veiculo = VeiculoHandlerFixture.CriarVeiculo();
            veiculoGatewayMock.ObterVeiculoPorPlacaAsync(veiculo.Placa).Returns(new List<Veiculo> { veiculo });
            
            var logGatewayMock = Substitute.For<ILogGateway<ObterVeiculoPorPlacaHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new ObterVeiculoPorPlacaHandler(
                veiculoGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            var resultado = await handler.Handle(veiculo.Placa);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado!.Placa.Should().Be(veiculo.Placa);
        }

        [Fact]
        public async Task Handle_ComPlacaInexistente_DeveRetornarNull()
        {
            // Arrange
            var veiculoGatewayMock = VeiculoHandlerFixture.CriarVeiculoGatewayMock();
            var unidadeDeTrabalhoMock = VeiculoHandlerFixture.CriarUnidadeDeTrabalhMock();
            
            var placa = "XYZ9999";
            veiculoGatewayMock.ObterVeiculoPorPlacaAsync(placa).Returns(new List<Veiculo>());
            
            var logGatewayMock = Substitute.For<ILogGateway<ObterVeiculoPorPlacaHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new ObterVeiculoPorPlacaHandler(
                veiculoGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            var resultado = await handler.Handle(placa);
            
            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task Handle_QuandoGatewayLancaExcecao_DevePropagar()
        {
            // Arrange
            var veiculoGatewayMock = VeiculoHandlerFixture.CriarVeiculoGatewayMock();
            var unidadeDeTrabalhoMock = VeiculoHandlerFixture.CriarUnidadeDeTrabalhMock();
            var logGatewayMock = Substitute.For<ILogGateway<ObterVeiculoPorPlacaHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();

            var placa = "ABC1234";
            veiculoGatewayMock.ObterVeiculoPorPlacaAsync(placa).Returns(Task.FromException<IEnumerable<Veiculo>>(new InvalidOperationException("Erro no banco")));

            var handler = new ObterVeiculoPorPlacaHandler(
                veiculoGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await handler.Handle(placa));
        }
    }
}
