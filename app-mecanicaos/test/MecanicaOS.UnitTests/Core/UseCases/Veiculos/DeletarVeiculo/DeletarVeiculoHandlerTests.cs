using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.UseCases.Veiculos.DeletarVeiculo;
using MecanicaOS.UnitTests.Fixtures;

namespace MecanicaOS.UnitTests.Core.UseCases.Veiculos.DeletarVeiculo
{
    /// <summary>
    /// Testes para DeletarVeiculoHandler
    /// Importância: Valida remoção segura de veículos do sistema
    /// </summary>
    public class DeletarVeiculoHandlerTests
    {
        [Fact]
        public async Task Handle_ComVeiculoExistente_DeveRetornarTrue()
        {
            // Arrange
            var veiculoGatewayMock = VeiculoHandlerFixture.CriarVeiculoGatewayMock();
            var unidadeDeTrabalhoMock = VeiculoHandlerFixture.CriarUnidadeDeTrabalhMock();
            
            var veiculo = VeiculoHandlerFixture.CriarVeiculo();
            veiculoGatewayMock.ObterPorIdAsync(veiculo.Id).Returns(veiculo);
            unidadeDeTrabalhoMock.Commit().Returns(true);
            
            var logGatewayMock = Substitute.For<ILogGateway<DeletarVeiculoHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new DeletarVeiculoHandler(
                veiculoGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            var resultado = await handler.Handle(veiculo.Id);
            
            // Assert
            resultado.Should().BeTrue();
            await veiculoGatewayMock.Received(1).DeletarAsync(Arg.Any<Veiculo>());
        }

        [Fact]
        public async Task Handle_ComVeiculoInexistente_DeveLancarDadosNaoEncontradosException()
        {
            // Arrange
            var veiculoGatewayMock = VeiculoHandlerFixture.CriarVeiculoGatewayMock();
            var unidadeDeTrabalhoMock = VeiculoHandlerFixture.CriarUnidadeDeTrabalhMock();
            
            var veiculoId = Guid.NewGuid();
            veiculoGatewayMock.ObterPorIdAsync(veiculoId).Returns((Veiculo?)null);
            
            var logGatewayMock = Substitute.For<ILogGateway<DeletarVeiculoHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new DeletarVeiculoHandler(
                veiculoGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act & Assert
            await FluentActions.Invoking(async () => await handler.Handle(veiculoId))
                .Should().ThrowAsync<DadosNaoEncontradosException>();
        }
    }
}
