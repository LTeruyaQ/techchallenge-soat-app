using Core.DTOs.UseCases.AlertaEstoque;
using Core.Interfaces.Handlers.AlertasEstoque;
using Core.UseCases.AlertasEstoque;
using FluentAssertions;
using NSubstitute;

namespace MecanicaOS.UnitTests.Core.UseCases.AlertasEstoque
{
    public class AlertaEstoqueUseCasesFacadeTests
    {
        private readonly ICadastrarVariosAlertasHandler _cadastrarVariosAlertasHandler;
        private readonly IVerificarAlertaEnviadoHojeHandler _verificarAlertaEnviadoHojeHandler;
        private readonly AlertaEstoqueUseCasesFacade _facade;

        public AlertaEstoqueUseCasesFacadeTests()
        {
            _cadastrarVariosAlertasHandler = Substitute.For<ICadastrarVariosAlertasHandler>();
            _verificarAlertaEnviadoHojeHandler = Substitute.For<IVerificarAlertaEnviadoHojeHandler>();
            _facade = new AlertaEstoqueUseCasesFacade(
                _cadastrarVariosAlertasHandler,
                _verificarAlertaEnviadoHojeHandler);
        }

        [Fact]
        public void Construtor_ComCadastrarHandlerNulo_DeveLancarArgumentNullException()
        {
            // Act
            var act = () => new AlertaEstoqueUseCasesFacade(
                null!,
                _verificarAlertaEnviadoHojeHandler);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage("*cadastrarVariosAlertasHandler*");
        }

        [Fact]
        public void Construtor_ComVerificarHandlerNulo_DeveLancarArgumentNullException()
        {
            // Act
            var act = () => new AlertaEstoqueUseCasesFacade(
                _cadastrarVariosAlertasHandler,
                null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage("*verificarAlertaEnviadoHojeHandler*");
        }

        [Fact]
        public async Task CadastrarVariosAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var alertas = new List<CadastrarAlertaEstoqueUseCaseDto>
            {
                new CadastrarAlertaEstoqueUseCaseDto
                {
                    EstoqueId = Guid.NewGuid(),
                    DataEnvio = DateTime.UtcNow
                }
            };

            // Act
            await _facade.CadastrarVariosAsync(alertas);

            // Assert
            await _cadastrarVariosAlertasHandler.Received(1).Handle(alertas);
        }

        [Fact]
        public async Task VerificarAlertaEnviadoHojeAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var estoqueId = Guid.NewGuid();
            _verificarAlertaEnviadoHojeHandler.Handle(estoqueId).Returns(Task.FromResult(true));

            // Act
            var resultado = await _facade.VerificarAlertaEnviadoHojeAsync(estoqueId);

            // Assert
            resultado.Should().BeTrue();
            await _verificarAlertaEnviadoHojeHandler.Received(1).Handle(estoqueId);
        }

        [Fact]
        public async Task CadastrarVariosAsync_ComListaVazia_DeveDelegarParaHandler()
        {
            // Arrange
            var alertasVazios = new List<CadastrarAlertaEstoqueUseCaseDto>();

            // Act
            await _facade.CadastrarVariosAsync(alertasVazios);

            // Assert
            await _cadastrarVariosAlertasHandler.Received(1).Handle(alertasVazios);
        }

        [Fact]
        public async Task VerificarAlertaEnviadoHojeAsync_QuandoHandlerRetornaFalse_DeveRetornarFalse()
        {
            // Arrange
            var estoqueId = Guid.NewGuid();
            _verificarAlertaEnviadoHojeHandler.Handle(estoqueId).Returns(Task.FromResult(false));

            // Act
            var resultado = await _facade.VerificarAlertaEnviadoHojeAsync(estoqueId);

            // Assert
            resultado.Should().BeFalse();
        }
    }
}
