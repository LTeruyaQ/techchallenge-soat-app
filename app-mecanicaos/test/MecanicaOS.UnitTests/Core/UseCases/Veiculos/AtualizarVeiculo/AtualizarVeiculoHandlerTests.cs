using Core.DTOs.UseCases.Veiculo;
using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.UseCases.Veiculos.AtualizarVeiculo;
using MecanicaOS.UnitTests.Fixtures;

namespace MecanicaOS.UnitTests.Core.UseCases.Veiculos.AtualizarVeiculo
{
    /// <summary>
    /// Testes para AtualizarVeiculoHandler
    /// Importância: Garante que atualizações de veículos preservem integridade dos dados
    /// </summary>
    public class AtualizarVeiculoHandlerTests
    {
        [Fact]
        public async Task Handle_ComDadosValidos_DeveAtualizarVeiculo()
        {
            // Arrange
            var veiculoGatewayMock = VeiculoHandlerFixture.CriarVeiculoGatewayMock();
            var unidadeDeTrabalhoMock = VeiculoHandlerFixture.CriarUnidadeDeTrabalhMock();
            
            var veiculo = VeiculoHandlerFixture.CriarVeiculo();
            var veiculoDto = VeiculoHandlerFixture.CriarAtualizarVeiculoUseCaseDto();
            
            veiculoGatewayMock.ObterPorIdAsync(veiculo.Id).Returns(veiculo);
            unidadeDeTrabalhoMock.Commit().Returns(true);
            
            var logGatewayMock = Substitute.For<ILogGateway<AtualizarVeiculoHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new AtualizarVeiculoHandler(
                veiculoGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            var resultado = await handler.Handle(veiculo.Id, veiculoDto);
            
            // Assert
            resultado.Should().NotBeNull();
            await veiculoGatewayMock.Received(1).ObterPorIdAsync(veiculo.Id);
            await veiculoGatewayMock.Received(1).EditarAsync(Arg.Any<Veiculo>());
        }

        [Fact]
        public async Task Handle_ComVeiculoInexistente_DeveLancarDadosNaoEncontradosException()
        {
            // Arrange
            var veiculoGatewayMock = VeiculoHandlerFixture.CriarVeiculoGatewayMock();
            var unidadeDeTrabalhoMock = VeiculoHandlerFixture.CriarUnidadeDeTrabalhMock();
            
            var veiculoId = Guid.NewGuid();
            var veiculoDto = VeiculoHandlerFixture.CriarAtualizarVeiculoUseCaseDto();
            
            veiculoGatewayMock.ObterPorIdAsync(veiculoId).Returns((Veiculo?)null);
            
            var logGatewayMock = Substitute.For<ILogGateway<AtualizarVeiculoHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new AtualizarVeiculoHandler(
                veiculoGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act & Assert
            await FluentActions.Invoking(async () => await handler.Handle(veiculoId, veiculoDto))
                .Should().ThrowAsync<DadosNaoEncontradosException>()
                .WithMessage("Veículo não encontrado");
        }
    }
}
