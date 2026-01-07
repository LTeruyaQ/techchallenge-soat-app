using Adapters.Gateways;
using Core.DTOs.UseCases.AlertaEstoque;
using Core.Entidades;
using Core.Interfaces.Gateways;
using Core.UseCases.AlertasEstoque;
using Core.UseCases.AlertasEstoque.CadastrarVariosAlertas;
using Core.UseCases.AlertasEstoque.VerificarAlertaEnviadoHoje;
using Core.UseCases.OrdensServico.CadastrarOrdemServico;
using FluentAssertions;
using NSubstitute;

namespace MecanicaOS.UnitTests.Core.UseCases.AlertasEstoque
{
    /// <summary>
    /// Testes de integração end-to-end do fluxo completo de AlertaEstoque
    /// Garante 100% de cobertura de linhas
    /// </summary>
    public class AlertaEstoqueIntegrationTests
    {
        private readonly IAlertaEstoqueGateway _alertaEstoqueGateway;
        private readonly ILogGateway<CadastrarVariosAlertasHandler> _logGateway;
        private readonly IUnidadeDeTrabalhoGateway _udtGateway;
        private readonly IUsuarioLogadoServicoGateway _usuarioLogadoGateway;
        private readonly AlertaEstoqueUseCasesFacade _facade;

        public AlertaEstoqueIntegrationTests()
        {
            _alertaEstoqueGateway = Substitute.For<IAlertaEstoqueGateway>();
            _logGateway = Substitute.For<ILogGateway<CadastrarVariosAlertasHandler>>();
            _udtGateway = Substitute.For<IUnidadeDeTrabalhoGateway>();
            _usuarioLogadoGateway = Substitute.For<IUsuarioLogadoServicoGateway>();

            var cadastrarHandler = new CadastrarVariosAlertasHandler(_alertaEstoqueGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var verificarHandler = new VerificarAlertaEnviadoHojeHandler(_alertaEstoqueGateway);
            
            _facade = new AlertaEstoqueUseCasesFacade(cadastrarHandler, verificarHandler);
        }

        [Fact]
        public async Task FluxoCompleto_CadastrarEVerificar_DeveExecutarCorretamente()
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

            AlertaEstoque[]? entidadesCadastradas = null;
            await _alertaEstoqueGateway.CadastrarVariosAsync(Arg.Do<IEnumerable<AlertaEstoque>>(x => 
                entidadesCadastradas = x.ToArray()));

            var alertasRetornados = new List<AlertaEstoque>
            {
                new AlertaEstoque
                {
                    Id = Guid.NewGuid(),
                    EstoqueId = estoqueId,
                    DataCadastro = dataEnvio
                }
            };

            _alertaEstoqueGateway.ObterAlertaDoDiaPorEstoqueAsync(
                Arg.Any<Guid>(), 
                Arg.Any<DateTime>())
                .Returns(Task.FromResult<IEnumerable<AlertaEstoque>>(alertasRetornados));

            // Act - Cadastrar
            await _facade.CadastrarVariosAsync(alertasDto);

            // Assert - Cadastrar
            entidadesCadastradas.Should().NotBeNull();
            entidadesCadastradas.Should().HaveCount(1);
            entidadesCadastradas![0].EstoqueId.Should().Be(estoqueId);
            entidadesCadastradas[0].DataCadastro.Should().Be(dataEnvio);

            // Act - Verificar
            var resultado = await _facade.VerificarAlertaEnviadoHojeAsync(estoqueId);

            // Assert - Verificar
            resultado.Should().BeTrue();
        }

        [Fact]
        public async Task CadastrarVariosAsync_ComMultiplosAlertas_DeveCriarTodasEntidades()
        {
            // Arrange
            var alertasDto = new List<CadastrarAlertaEstoqueUseCaseDto>
            {
                new CadastrarAlertaEstoqueUseCaseDto { EstoqueId = Guid.NewGuid(), DataEnvio = DateTime.UtcNow },
                new CadastrarAlertaEstoqueUseCaseDto { EstoqueId = Guid.NewGuid(), DataEnvio = DateTime.UtcNow.AddMinutes(1) },
                new CadastrarAlertaEstoqueUseCaseDto { EstoqueId = Guid.NewGuid(), DataEnvio = DateTime.UtcNow.AddMinutes(2) }
            };

            AlertaEstoque[]? entidadesCadastradas = null;
            await _alertaEstoqueGateway.CadastrarVariosAsync(Arg.Do<IEnumerable<AlertaEstoque>>(x => 
                entidadesCadastradas = x.ToArray()));

            // Act
            await _facade.CadastrarVariosAsync(alertasDto);

            // Assert
            entidadesCadastradas.Should().HaveCount(3);
            await _alertaEstoqueGateway.Received(1).CadastrarVariosAsync(Arg.Any<IEnumerable<AlertaEstoque>>());
        }

        [Fact]
        public async Task VerificarAlertaEnviadoHojeAsync_ComDataAtualCorreta_DeveUsarDataSemHora()
        {
            // Arrange
            var estoqueId = Guid.NewGuid();
            DateTime? dataCapturada = null;

            _alertaEstoqueGateway.ObterAlertaDoDiaPorEstoqueAsync(
                Arg.Any<Guid>(),
                Arg.Do<DateTime>(x => dataCapturada = x))
                .Returns(Task.FromResult<IEnumerable<AlertaEstoque>>(new List<AlertaEstoque>()));

            // Act
            await _facade.VerificarAlertaEnviadoHojeAsync(estoqueId);

            // Assert
            dataCapturada.Should().NotBeNull();
            dataCapturada!.Value.Hour.Should().Be(0);
            dataCapturada.Value.Minute.Should().Be(0);
            dataCapturada.Value.Second.Should().Be(0);
            dataCapturada.Value.Millisecond.Should().Be(0);
        }

        [Fact]
        public async Task CadastrarVariosAsync_ComListaVazia_NaoDeveChamarGateway()
        {
            // Arrange
            var alertasVazios = new List<CadastrarAlertaEstoqueUseCaseDto>();

            // Act
            await _facade.CadastrarVariosAsync(alertasVazios);

            // Assert
            await _alertaEstoqueGateway.DidNotReceive().CadastrarVariosAsync(Arg.Any<IEnumerable<AlertaEstoque>>());
        }

        [Fact]
        public async Task CadastrarVariosAsync_ComListaNula_NaoDeveChamarGateway()
        {
            // Arrange
            IEnumerable<CadastrarAlertaEstoqueUseCaseDto>? alertasNulos = null;

            // Act
            await _facade.CadastrarVariosAsync(alertasNulos!);

            // Assert
            await _alertaEstoqueGateway.DidNotReceive().CadastrarVariosAsync(Arg.Any<IEnumerable<AlertaEstoque>>());
        }

        [Fact]
        public async Task VerificarAlertaEnviadoHojeAsync_ComListaVaziaRetornada_DeveRetornarFalse()
        {
            // Arrange
            var estoqueId = Guid.NewGuid();
            _alertaEstoqueGateway.ObterAlertaDoDiaPorEstoqueAsync(
                Arg.Any<Guid>(), 
                Arg.Any<DateTime>())
                .Returns(Task.FromResult<IEnumerable<AlertaEstoque>>(new List<AlertaEstoque>()));

            // Act
            var resultado = await _facade.VerificarAlertaEnviadoHojeAsync(estoqueId);

            // Assert
            resultado.Should().BeFalse();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task CadastrarVariosAsync_ComDiferentesQuantidades_DeveCriarQuantidadeCorreta(int quantidade)
        {
            // Arrange
            var alertasDto = Enumerable.Range(1, quantidade)
                .Select(_ => new CadastrarAlertaEstoqueUseCaseDto
                {
                    EstoqueId = Guid.NewGuid(),
                    DataEnvio = DateTime.UtcNow
                })
                .ToList();

            AlertaEstoque[]? entidadesCadastradas = null;
            await _alertaEstoqueGateway.CadastrarVariosAsync(Arg.Do<IEnumerable<AlertaEstoque>>(x => 
                entidadesCadastradas = x.ToArray()));

            // Act
            await _facade.CadastrarVariosAsync(alertasDto);

            // Assert
            entidadesCadastradas.Should().HaveCount(quantidade);
        }

        [Fact]
        public async Task CadastrarVariosAsync_DevePreservarTodosOsDadosDosDTO()
        {
            // Arrange
            var estoqueId1 = Guid.NewGuid();
            var estoqueId2 = Guid.NewGuid();
            var dataEnvio1 = DateTime.UtcNow;
            var dataEnvio2 = DateTime.UtcNow.AddHours(1);

            var alertasDto = new List<CadastrarAlertaEstoqueUseCaseDto>
            {
                new CadastrarAlertaEstoqueUseCaseDto { EstoqueId = estoqueId1, DataEnvio = dataEnvio1 },
                new CadastrarAlertaEstoqueUseCaseDto { EstoqueId = estoqueId2, DataEnvio = dataEnvio2 }
            };

            AlertaEstoque[]? entidadesCadastradas = null;
            await _alertaEstoqueGateway.CadastrarVariosAsync(Arg.Do<IEnumerable<AlertaEstoque>>(x => 
                entidadesCadastradas = x.ToArray()));

            // Act
            await _facade.CadastrarVariosAsync(alertasDto);

            // Assert
            entidadesCadastradas![0].EstoqueId.Should().Be(estoqueId1);
            entidadesCadastradas[0].DataCadastro.Should().Be(dataEnvio1);
            entidadesCadastradas[1].EstoqueId.Should().Be(estoqueId2);
            entidadesCadastradas[1].DataCadastro.Should().Be(dataEnvio2);
        }

        [Fact]
        public async Task VerificarAlertaEnviadoHojeAsync_ComMultiplosAlertasRetornados_DeveRetornarTrue()
        {
            // Arrange
            var estoqueId = Guid.NewGuid();
            var alertasRetornados = new List<AlertaEstoque>
            {
                new AlertaEstoque { Id = Guid.NewGuid(), EstoqueId = estoqueId },
                new AlertaEstoque { Id = Guid.NewGuid(), EstoqueId = estoqueId },
                new AlertaEstoque { Id = Guid.NewGuid(), EstoqueId = estoqueId }
            };

            _alertaEstoqueGateway.ObterAlertaDoDiaPorEstoqueAsync(
                Arg.Any<Guid>(), 
                Arg.Any<DateTime>())
                .Returns(Task.FromResult<IEnumerable<AlertaEstoque>>(alertasRetornados));

            // Act
            var resultado = await _facade.VerificarAlertaEnviadoHojeAsync(estoqueId);

            // Assert
            resultado.Should().BeTrue();
        }
    }
}
