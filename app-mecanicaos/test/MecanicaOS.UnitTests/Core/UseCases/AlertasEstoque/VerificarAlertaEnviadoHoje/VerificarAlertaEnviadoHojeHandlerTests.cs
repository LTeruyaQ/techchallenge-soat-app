using Core.Entidades;
using Core.Interfaces.Gateways;
using Core.UseCases.AlertasEstoque.VerificarAlertaEnviadoHoje;
using FluentAssertions;
using NSubstitute;

namespace MecanicaOS.UnitTests.Core.UseCases.AlertasEstoque.VerificarAlertaEnviadoHoje
{
    public class VerificarAlertaEnviadoHojeHandlerTests
    {
        private readonly IAlertaEstoqueGateway _alertaEstoqueGateway;
        private readonly VerificarAlertaEnviadoHojeHandler _handler;

        public VerificarAlertaEnviadoHojeHandlerTests()
        {
            _alertaEstoqueGateway = Substitute.For<IAlertaEstoqueGateway>();
            _handler = new VerificarAlertaEnviadoHojeHandler(_alertaEstoqueGateway);
        }

        [Fact]
        public void Construtor_ComGatewayNulo_DeveLancarArgumentNullException()
        {
            // Act
            var act = () => new VerificarAlertaEnviadoHojeHandler(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage("*alertaEstoqueGateway*");
        }

        [Fact]
        public async Task Handle_ComAlertasEnviadosHoje_DeveRetornarTrue()
        {
            // Arrange
            var estoqueId = Guid.NewGuid();
            var alertasHoje = new List<AlertaEstoque>
            {
                new AlertaEstoque
                {
                    Id = Guid.NewGuid(),
                    EstoqueId = estoqueId,
                    DataCadastro = DateTime.UtcNow
                }
            };

            _alertaEstoqueGateway.ObterAlertaDoDiaPorEstoqueAsync(
                Arg.Any<Guid>(), 
                Arg.Any<DateTime>())
                .Returns(Task.FromResult<IEnumerable<AlertaEstoque>>(alertasHoje));

            // Act
            var resultado = await _handler.Handle(estoqueId);

            // Assert
            resultado.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_SemAlertasEnviadosHoje_DeveRetornarFalse()
        {
            // Arrange
            var estoqueId = Guid.NewGuid();
            var alertasVazios = new List<AlertaEstoque>();

            _alertaEstoqueGateway.ObterAlertaDoDiaPorEstoqueAsync(
                Arg.Any<Guid>(), 
                Arg.Any<DateTime>())
                .Returns(Task.FromResult<IEnumerable<AlertaEstoque>>(alertasVazios));

            // Act
            var resultado = await _handler.Handle(estoqueId);

            // Assert
            resultado.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_DeveChamarGatewayComDataAtualSemHora()
        {
            // Arrange
            var estoqueId = Guid.NewGuid();
            var alertasVazios = new List<AlertaEstoque>();
            var dataAtualEsperada = DateTime.UtcNow.Date;

            Guid? estoqueIdCapturado = null;
            DateTime? dataCapturada = null;

            _alertaEstoqueGateway.ObterAlertaDoDiaPorEstoqueAsync(
                Arg.Do<Guid>(x => estoqueIdCapturado = x),
                Arg.Do<DateTime>(x => dataCapturada = x))
                .Returns(Task.FromResult<IEnumerable<AlertaEstoque>>(alertasVazios));

            // Act
            await _handler.Handle(estoqueId);

            // Assert
            estoqueIdCapturado.Should().Be(estoqueId);
            dataCapturada.Should().NotBeNull();
            dataCapturada!.Value.Date.Should().Be(dataAtualEsperada);
            dataCapturada.Value.Hour.Should().Be(0);
            dataCapturada.Value.Minute.Should().Be(0);
            dataCapturada.Value.Second.Should().Be(0);
        }

        [Fact]
        public async Task Handle_ComMultiplosAlertasHoje_DeveRetornarTrue()
        {
            // Arrange
            var estoqueId = Guid.NewGuid();
            var alertasHoje = new List<AlertaEstoque>
            {
                new AlertaEstoque { Id = Guid.NewGuid(), EstoqueId = estoqueId, DataCadastro = DateTime.UtcNow },
                new AlertaEstoque { Id = Guid.NewGuid(), EstoqueId = estoqueId, DataCadastro = DateTime.UtcNow.AddHours(-2) },
                new AlertaEstoque { Id = Guid.NewGuid(), EstoqueId = estoqueId, DataCadastro = DateTime.UtcNow.AddHours(-5) }
            };

            _alertaEstoqueGateway.ObterAlertaDoDiaPorEstoqueAsync(
                Arg.Any<Guid>(), 
                Arg.Any<DateTime>())
                .Returns(Task.FromResult<IEnumerable<AlertaEstoque>>(alertasHoje));

            // Act
            var resultado = await _handler.Handle(estoqueId);

            // Assert
            resultado.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ComEstoqueIdVazio_DeveChamarGateway()
        {
            // Arrange
            var estoqueId = Guid.Empty;
            var alertasVazios = new List<AlertaEstoque>();

            _alertaEstoqueGateway.ObterAlertaDoDiaPorEstoqueAsync(
                Arg.Any<Guid>(), 
                Arg.Any<DateTime>())
                .Returns(Task.FromResult<IEnumerable<AlertaEstoque>>(alertasVazios));

            // Act
            var resultado = await _handler.Handle(estoqueId);

            // Assert
            resultado.Should().BeFalse();
            await _alertaEstoqueGateway.Received(1).ObterAlertaDoDiaPorEstoqueAsync(
                estoqueId, 
                Arg.Any<DateTime>());
        }
    }
}
