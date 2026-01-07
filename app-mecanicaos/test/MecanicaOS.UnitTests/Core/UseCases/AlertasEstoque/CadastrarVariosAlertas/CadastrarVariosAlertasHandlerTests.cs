using Core.DTOs.UseCases.AlertaEstoque;
using Core.Entidades;
using Core.Interfaces.Gateways;
using Core.UseCases.AlertasEstoque.CadastrarVariosAlertas;
using FluentAssertions;
using NSubstitute;

namespace MecanicaOS.UnitTests.Core.UseCases.AlertasEstoque.CadastrarVariosAlertas
{
    public class CadastrarVariosAlertasHandlerTests
    {
        private readonly IAlertaEstoqueGateway _alertaEstoqueGateway;
        private readonly ILogGateway<CadastrarVariosAlertasHandler> _logGateway;
        private readonly IUnidadeDeTrabalhoGateway _udtGateway;
        private readonly IUsuarioLogadoServicoGateway _usuarioLogadoGateway;
        private readonly CadastrarVariosAlertasHandler _handler;

        public CadastrarVariosAlertasHandlerTests()
        {
            _alertaEstoqueGateway = Substitute.For<IAlertaEstoqueGateway>();
            _logGateway = Substitute.For<ILogGateway<CadastrarVariosAlertasHandler>>();
            _udtGateway = Substitute.For<IUnidadeDeTrabalhoGateway>();
            _usuarioLogadoGateway = Substitute.For<IUsuarioLogadoServicoGateway>();
            _handler = new CadastrarVariosAlertasHandler(_alertaEstoqueGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
        }

        [Fact]
        public async Task Handle_ComListaVazia_NaoDeveChamarGateway()
        {
            // Arrange
            var alertasVazios = new List<CadastrarAlertaEstoqueUseCaseDto>();

            // Act
            await _handler.Handle(alertasVazios);

            // Assert
            await _alertaEstoqueGateway.DidNotReceive().CadastrarVariosAsync(Arg.Any<IEnumerable<AlertaEstoque>>());
        }

        [Fact]
        public async Task Handle_ComListaNula_NaoDeveChamarGateway()
        {
            // Arrange
            IEnumerable<CadastrarAlertaEstoqueUseCaseDto>? alertasNulos = null;

            // Act
            await _handler.Handle(alertasNulos!);

            // Assert
            await _alertaEstoqueGateway.DidNotReceive().CadastrarVariosAsync(Arg.Any<IEnumerable<AlertaEstoque>>());
        }

        [Fact]
        public async Task Handle_ComAlertasValidos_DeveCriarEntidadesEChamarGateway()
        {
            // Arrange
            var estoqueId1 = Guid.NewGuid();
            var estoqueId2 = Guid.NewGuid();
            var dataEnvio = DateTime.UtcNow;

            var alertasDto = new List<CadastrarAlertaEstoqueUseCaseDto>
            {
                new CadastrarAlertaEstoqueUseCaseDto
                {
                    EstoqueId = estoqueId1,
                    DataEnvio = dataEnvio
                },
                new CadastrarAlertaEstoqueUseCaseDto
                {
                    EstoqueId = estoqueId2,
                    DataEnvio = dataEnvio.AddMinutes(5)
                }
            };

            AlertaEstoque[]? entidadesCapturadas = null;
            await _alertaEstoqueGateway.CadastrarVariosAsync(Arg.Do<IEnumerable<AlertaEstoque>>(x => 
                entidadesCapturadas = x.ToArray()));

            // Act
            await _handler.Handle(alertasDto);

            // Assert
            await _alertaEstoqueGateway.Received(1).CadastrarVariosAsync(Arg.Any<IEnumerable<AlertaEstoque>>());
            
            entidadesCapturadas.Should().NotBeNull();
            entidadesCapturadas.Should().HaveCount(2);
            
            entidadesCapturadas![0].EstoqueId.Should().Be(estoqueId1);
            entidadesCapturadas[0].DataCadastro.Should().Be(dataEnvio);
            
            entidadesCapturadas[1].EstoqueId.Should().Be(estoqueId2);
            entidadesCapturadas[1].DataCadastro.Should().Be(dataEnvio.AddMinutes(5));
        }

        [Fact]
        public async Task Handle_ComUmAlerta_DeveCriarEntidadeCorretamente()
        {
            // Arrange
            var estoqueId = Guid.NewGuid();
            var dataEnvio = DateTime.UtcNow;

            var alertasDto = new List<CadastrarAlertaEstoqueUseCaseDto>
            {
                new CadastrarAlertaEstoqueUseCaseDto
                {
                    EstoqueId = estoqueId,
                    DataEnvio = dataEnvio
                }
            };

            AlertaEstoque? entidadeCapturada = null;
            await _alertaEstoqueGateway.CadastrarVariosAsync(Arg.Do<IEnumerable<AlertaEstoque>>(x => 
                entidadeCapturada = x.FirstOrDefault()));

            // Act
            await _handler.Handle(alertasDto);

            // Assert
            entidadeCapturada.Should().NotBeNull();
            entidadeCapturada!.EstoqueId.Should().Be(estoqueId);
            entidadeCapturada.DataCadastro.Should().Be(dataEnvio);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task Handle_ComDiferentesQuantidades_DeveChamarGatewayComQuantidadeCorreta(int quantidade)
        {
            // Arrange
            var alertasDto = Enumerable.Range(1, quantidade)
                .Select(_ => new CadastrarAlertaEstoqueUseCaseDto
                {
                    EstoqueId = Guid.NewGuid(),
                    DataEnvio = DateTime.UtcNow
                })
                .ToList();

            AlertaEstoque[]? entidadesCapturadas = null;
            await _alertaEstoqueGateway.CadastrarVariosAsync(Arg.Do<IEnumerable<AlertaEstoque>>(x => 
                entidadesCapturadas = x.ToArray()));

            // Act
            await _handler.Handle(alertasDto);

            // Assert
            entidadesCapturadas.Should().HaveCount(quantidade);
        }
    }
}
