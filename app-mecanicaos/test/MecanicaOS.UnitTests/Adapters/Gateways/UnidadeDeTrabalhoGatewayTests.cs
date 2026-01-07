using Adapters.Gateways;
using Core.Interfaces.Repositorios;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace MecanicaOS.UnitTests.Adapters.Gateways
{
    public class UnidadeDeTrabalhoGatewayTests
    {
        private readonly IUnidadeDeTrabalho _unidadeDeTrabalho;
        private readonly UnidadeDeTrabalhoGateway _gateway;

        public UnidadeDeTrabalhoGatewayTests()
        {
            _unidadeDeTrabalho = Substitute.For<IUnidadeDeTrabalho>();
            _gateway = new UnidadeDeTrabalhoGateway(_unidadeDeTrabalho);
        }

        [Fact]
        public void Construtor_ComUnidadeDeTrabalhoNula_DeveLancarArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new UnidadeDeTrabalhoGateway(null!));
            exception.ParamName.Should().Be("unidadeDeTrabalho");
        }

        [Fact]
        public async Task Commit_DeveRetornarTrueQuandoCommitBemSucedido()
        {
            // Arrange
            _unidadeDeTrabalho.Commit().Returns(Task.FromResult(true));

            // Act
            var resultado = await _gateway.Commit();

            // Assert
            resultado.Should().BeTrue();
            await _unidadeDeTrabalho.Received(1).Commit();
        }

        [Fact]
        public async Task Commit_DeveRetornarFalseQuandoCommitFalhar()
        {
            // Arrange
            _unidadeDeTrabalho.Commit().Returns(Task.FromResult(false));

            // Act
            var resultado = await _gateway.Commit();

            // Assert
            resultado.Should().BeFalse();
            await _unidadeDeTrabalho.Received(1).Commit();
        }

        [Fact]
        public async Task Commit_DeveDelegarChamadaParaUnidadeDeTrabalho()
        {
            // Arrange
            var commitCount = 0;
            _unidadeDeTrabalho.Commit().Returns(callInfo =>
            {
                commitCount++;
                return Task.FromResult(true);
            });

            // Act
            await _gateway.Commit();
            await _gateway.Commit();

            // Assert
            commitCount.Should().Be(2);
            await _unidadeDeTrabalho.Received(2).Commit();
        }
    }
}
