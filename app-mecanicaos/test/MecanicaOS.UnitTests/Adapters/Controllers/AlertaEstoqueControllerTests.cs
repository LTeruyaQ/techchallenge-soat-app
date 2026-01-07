using Adapters.Controllers;
using Core.DTOs.Requests.Estoque;
using Core.DTOs.UseCases.AlertaEstoque;
using Core.Interfaces.root;
using Core.Interfaces.UseCases;
using FluentAssertions;
using NSubstitute;

namespace MecanicaOS.UnitTests.Adapters.Controllers
{
    public class AlertaEstoqueControllerTests
    {
        private readonly ICompositionRoot _compositionRoot;
        private readonly IAlertaEstoqueUseCases _alertaEstoqueUseCases;
        private readonly AlertaEstoqueController _controller;

        public AlertaEstoqueControllerTests()
        {
            _compositionRoot = Substitute.For<ICompositionRoot>();
            _alertaEstoqueUseCases = Substitute.For<IAlertaEstoqueUseCases>();
            
            _compositionRoot.CriarAlertaEstoqueUseCases().Returns(_alertaEstoqueUseCases);
            _controller = new AlertaEstoqueController(_compositionRoot);
        }

        [Fact]
        public void Construtor_DeveCriarInstanciaComDependencias()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var alertaEstoqueUseCases = Substitute.For<IAlertaEstoqueUseCases>();
            compositionRoot.CriarAlertaEstoqueUseCases().Returns(alertaEstoqueUseCases);

            // Act
            var controller = new AlertaEstoqueController(compositionRoot);

            // Assert
            controller.Should().NotBeNull();
            compositionRoot.Received(1).CriarAlertaEstoqueUseCases();
        }

        [Fact]
        public async Task CadastrarAlertas_ComListaVazia_NaoDeveChamarUseCases()
        {
            // Arrange
            var alertasVazios = new List<CadastrarAlertaEstoqueRequest>();

            // Act
            await _controller.CadastrarAlertas(alertasVazios);

            // Assert
            await _alertaEstoqueUseCases.DidNotReceive().CadastrarVariosAsync(Arg.Any<IEnumerable<CadastrarAlertaEstoqueUseCaseDto>>());
        }

        [Fact]
        public async Task CadastrarAlertas_ComAlertas_DeveMapeareEChamarUseCases()
        {
            // Arrange
            var estoqueId1 = Guid.NewGuid();
            var estoqueId2 = Guid.NewGuid();
            var dataEnvio1 = DateTime.UtcNow;
            var dataEnvio2 = DateTime.UtcNow.AddMinutes(5);

            var alertas = new List<CadastrarAlertaEstoqueRequest>
            {
                new CadastrarAlertaEstoqueRequest 
                { 
                    EstoqueId = estoqueId1, 
                    DataEnvio = dataEnvio1 
                },
                new CadastrarAlertaEstoqueRequest 
                { 
                    EstoqueId = estoqueId2, 
                    DataEnvio = dataEnvio2 
                }
            };

            CadastrarAlertaEstoqueUseCaseDto[]? dtosCapturados = null;
            await _alertaEstoqueUseCases.CadastrarVariosAsync(Arg.Do<IEnumerable<CadastrarAlertaEstoqueUseCaseDto>>(x => 
                dtosCapturados = x.ToArray()));

            // Act
            await _controller.CadastrarAlertas(alertas);

            // Assert
            await _alertaEstoqueUseCases.Received(1).CadastrarVariosAsync(Arg.Any<IEnumerable<CadastrarAlertaEstoqueUseCaseDto>>());
            
            dtosCapturados.Should().NotBeNull();
            dtosCapturados.Should().HaveCount(2);
            dtosCapturados![0].EstoqueId.Should().Be(estoqueId1);
            dtosCapturados[0].DataEnvio.Should().Be(dataEnvio1);
            dtosCapturados[1].EstoqueId.Should().Be(estoqueId2);
            dtosCapturados[1].DataEnvio.Should().Be(dataEnvio2);
        }

        [Fact]
        public async Task VerificarAlertaEnviadoHoje_QuandoUseCasesRetornaFalse_DeveRetornarFalse()
        {
            // Arrange
            var estoqueId = Guid.NewGuid();
            _alertaEstoqueUseCases.VerificarAlertaEnviadoHojeAsync(estoqueId).Returns(Task.FromResult(false));

            // Act
            var resultado = await _controller.VerificarAlertaEnviadoHoje(estoqueId);

            // Assert
            resultado.Should().BeFalse();
            await _alertaEstoqueUseCases.Received(1).VerificarAlertaEnviadoHojeAsync(estoqueId);
        }

        [Fact]
        public async Task VerificarAlertaEnviadoHoje_QuandoUseCasesRetornaTrue_DeveRetornarTrue()
        {
            // Arrange
            var estoqueId = Guid.NewGuid();
            _alertaEstoqueUseCases.VerificarAlertaEnviadoHojeAsync(estoqueId).Returns(Task.FromResult(true));

            // Act
            var resultado = await _controller.VerificarAlertaEnviadoHoje(estoqueId);

            // Assert
            resultado.Should().BeTrue();
            await _alertaEstoqueUseCases.Received(1).VerificarAlertaEnviadoHojeAsync(estoqueId);
        }

        [Fact]
        public async Task VerificarAlertaEnviadoHoje_ComEstoqueIdVazio_DeveChamarUseCases()
        {
            // Arrange
            var estoqueId = Guid.Empty;
            _alertaEstoqueUseCases.VerificarAlertaEnviadoHojeAsync(estoqueId).Returns(Task.FromResult(false));

            // Act
            var resultado = await _controller.VerificarAlertaEnviadoHoje(estoqueId);

            // Assert
            resultado.Should().BeFalse();
            await _alertaEstoqueUseCases.Received(1).VerificarAlertaEnviadoHojeAsync(estoqueId);
        }

        [Fact]
        public async Task CadastrarAlertas_ComListaNula_NaoDeveChamarUseCases()
        {
            // Arrange
            IEnumerable<CadastrarAlertaEstoqueRequest>? alertasNulos = null;

            // Act
            await _controller.CadastrarAlertas(alertasNulos!);

            // Assert
            await _alertaEstoqueUseCases.DidNotReceive().CadastrarVariosAsync(Arg.Any<IEnumerable<CadastrarAlertaEstoqueUseCaseDto>>());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task CadastrarAlertas_ComDiferentesQuantidades_DeveChamarUseCasesComQuantidadeCorreta(int quantidade)
        {
            // Arrange
            var alertas = Enumerable.Range(1, quantidade)
                .Select(_ => new CadastrarAlertaEstoqueRequest 
                { 
                    EstoqueId = Guid.NewGuid(), 
                    DataEnvio = DateTime.UtcNow 
                })
                .ToList();

            CadastrarAlertaEstoqueUseCaseDto[]? dtosCapturados = null;
            await _alertaEstoqueUseCases.CadastrarVariosAsync(Arg.Do<IEnumerable<CadastrarAlertaEstoqueUseCaseDto>>(x => 
                dtosCapturados = x.ToArray()));

            // Act
            await _controller.CadastrarAlertas(alertas);

            // Assert
            dtosCapturados.Should().HaveCount(quantidade);
            await _alertaEstoqueUseCases.Received(1).CadastrarVariosAsync(Arg.Any<IEnumerable<CadastrarAlertaEstoqueUseCaseDto>>());
        }
    }
}
