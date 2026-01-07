using Adapters.Gateways;
using Core.Interfaces.Jobs;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace MecanicaOS.UnitTests.Adapters.Gateways
{
    public class VerificarEstoqueJobGatewayTests
    {
        private readonly IVerificarEstoqueJob _verificarEstoqueJob;
        private readonly VerificarEstoqueJobGateway _gateway;

        public VerificarEstoqueJobGatewayTests()
        {
            _verificarEstoqueJob = Substitute.For<IVerificarEstoqueJob>();
            _gateway = new VerificarEstoqueJobGateway(_verificarEstoqueJob);
        }

        [Fact]
        public async Task VerificarEstoqueAsync_DeveChamarExecutarAsyncDoJob()
        {
            // Act
            await _gateway.VerificarEstoqueAsync();

            // Assert
            await _verificarEstoqueJob.Received(1).ExecutarAsync();
        }

        [Fact]
        public async Task VerificarEstoqueAsync_DeveExecutarMultiplasVezes()
        {
            // Arrange
            var executionCount = 0;
            _verificarEstoqueJob.ExecutarAsync().Returns(callInfo =>
            {
                executionCount++;
                return Task.CompletedTask;
            });

            // Act
            await _gateway.VerificarEstoqueAsync();
            await _gateway.VerificarEstoqueAsync();
            await _gateway.VerificarEstoqueAsync();

            // Assert
            executionCount.Should().Be(3);
            await _verificarEstoqueJob.Received(3).ExecutarAsync();
        }

        [Fact]
        public async Task VerificarEstoqueAsync_QuandoJobLancaExcecao_DevePropagarExcecao()
        {
            // Arrange
            var excecaoEsperada = new InvalidOperationException("Erro ao verificar estoque");
            _verificarEstoqueJob.ExecutarAsync().Returns(Task.FromException(excecaoEsperada));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _gateway.VerificarEstoqueAsync());
            exception.Message.Should().Be("Erro ao verificar estoque");
        }
    }
}
