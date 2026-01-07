using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.UseCases.Veiculos.ObterVeiculo;
using MecanicaOS.UnitTests.Fixtures;

namespace MecanicaOS.UnitTests.Core.UseCases.Veiculos.ObterVeiculo
{
    /// <summary>
    /// Testes para ObterVeiculoHandler
    /// Importância: Valida busca de veículos por ID, essencial para operações de consulta
    /// </summary>
    public class ObterVeiculoHandlerTests
    {
        [Fact]
        public async Task Handle_ComIdExistente_DeveRetornarVeiculo()
        {
            // Arrange
            var veiculoGatewayMock = VeiculoHandlerFixture.CriarVeiculoGatewayMock();
            var unidadeDeTrabalhoMock = VeiculoHandlerFixture.CriarUnidadeDeTrabalhMock();
            
            var veiculo = VeiculoHandlerFixture.CriarVeiculo();
            veiculoGatewayMock.ObterPorIdAsync(veiculo.Id).Returns(veiculo);
            
            var logGatewayMock = Substitute.For<ILogGateway<ObterVeiculoHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new ObterVeiculoHandler(
                veiculoGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            var resultado = await handler.Handle(veiculo.Id);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(veiculo.Id);
        }

        [Fact]
        public async Task Handle_ComIdInexistente_DeveLancarDadosNaoEncontradosException()
        {
            // Arrange
            var veiculoGatewayMock = VeiculoHandlerFixture.CriarVeiculoGatewayMock();
            var unidadeDeTrabalhoMock = VeiculoHandlerFixture.CriarUnidadeDeTrabalhMock();
            
            var veiculoId = Guid.NewGuid();
            veiculoGatewayMock.ObterPorIdAsync(veiculoId).Returns((Veiculo?)null);
            
            var logGatewayMock = Substitute.For<ILogGateway<ObterVeiculoHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new ObterVeiculoHandler(
                veiculoGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act & Assert
            await FluentActions.Invoking(async () => await handler.Handle(veiculoId))
                .Should().ThrowAsync<DadosNaoEncontradosException>()
                .WithMessage($"Veículo com ID {veiculoId} não encontrado.");
        }
    }
}
