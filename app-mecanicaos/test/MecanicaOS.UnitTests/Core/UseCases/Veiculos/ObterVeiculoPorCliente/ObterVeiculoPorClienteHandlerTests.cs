using Core.Entidades;
using Core.Interfaces.Gateways;
using Core.UseCases.Veiculos.ObterVeiculoPorCliente;
using MecanicaOS.UnitTests.Fixtures;

namespace MecanicaOS.UnitTests.Core.UseCases.Veiculos.ObterVeiculoPorCliente
{
    /// <summary>
    /// Testes para ObterVeiculoPorClienteHandler
    /// Importância: Valida filtro de veículos por cliente, essencial para relacionamento Cliente-Veículo
    /// </summary>
    public class ObterVeiculoPorClienteHandlerTests
    {
        [Fact]
        public async Task Handle_ComClienteExistente_DeveRetornarVeiculosDoCliente()
        {
            // Arrange
            var veiculoGatewayMock = VeiculoHandlerFixture.CriarVeiculoGatewayMock();
            var unidadeDeTrabalhoMock = VeiculoHandlerFixture.CriarUnidadeDeTrabalhMock();
            
            var clienteId = Guid.NewGuid();
            var veiculos = new List<Veiculo>
            {
                VeiculoHandlerFixture.CriarVeiculo(),
                VeiculoHandlerFixture.CriarVeiculo()
            };
            
            veiculoGatewayMock.ObterVeiculoPorClienteAsync(clienteId).Returns(veiculos);
            
            var logGatewayMock = Substitute.For<ILogGateway<ObterVeiculoPorClienteHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new ObterVeiculoPorClienteHandler(
                veiculoGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            var resultado = await handler.Handle(clienteId);
            
            // Assert
            resultado.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_QuandoGatewayLancaExcecao_DevePropagar()
        {
            // Arrange
            var veiculoGatewayMock = VeiculoHandlerFixture.CriarVeiculoGatewayMock();
            var unidadeDeTrabalhoMock = VeiculoHandlerFixture.CriarUnidadeDeTrabalhMock();
            var logGatewayMock = Substitute.For<ILogGateway<ObterVeiculoPorClienteHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();

            var clienteId = Guid.NewGuid();
            veiculoGatewayMock.ObterVeiculoPorClienteAsync(clienteId).Returns(Task.FromException<IEnumerable<Veiculo>>(new InvalidOperationException("Erro no banco")));

            var handler = new ObterVeiculoPorClienteHandler(
                veiculoGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await handler.Handle(clienteId));
        }
    }
}
