using Core.DTOs.UseCases.AlertaEstoque;
using FluentAssertions;

namespace MecanicaOS.UnitTests.Core.DTOs.UseCases.AlertaEstoque
{
    public class CadastrarAlertaEstoqueUseCaseDtoTests
    {
        [Fact]
        public void CadastrarAlertaEstoqueUseCaseDto_DeveInicializarComValoresPadrao()
        {
            // Act
            var dto = new CadastrarAlertaEstoqueUseCaseDto();

            // Assert
            dto.EstoqueId.Should().Be(Guid.Empty);
            dto.DataEnvio.Should().Be(default(DateTime));
        }

        [Fact]
        public void CadastrarAlertaEstoqueUseCaseDto_DevePermitirDefinirEstoqueId()
        {
            // Arrange
            var estoqueId = Guid.NewGuid();
            var dto = new CadastrarAlertaEstoqueUseCaseDto();

            // Act
            dto.EstoqueId = estoqueId;

            // Assert
            dto.EstoqueId.Should().Be(estoqueId);
        }

        [Fact]
        public void CadastrarAlertaEstoqueUseCaseDto_DevePermitirDefinirDataEnvio()
        {
            // Arrange
            var dataEnvio = DateTime.UtcNow;
            var dto = new CadastrarAlertaEstoqueUseCaseDto();

            // Act
            dto.DataEnvio = dataEnvio;

            // Assert
            dto.DataEnvio.Should().Be(dataEnvio);
        }

        [Fact]
        public void CadastrarAlertaEstoqueUseCaseDto_DevePermitirInicializacaoComObjectInitializer()
        {
            // Arrange
            var estoqueId = Guid.NewGuid();
            var dataEnvio = DateTime.UtcNow;

            // Act
            var dto = new CadastrarAlertaEstoqueUseCaseDto
            {
                EstoqueId = estoqueId,
                DataEnvio = dataEnvio
            };

            // Assert
            dto.EstoqueId.Should().Be(estoqueId);
            dto.DataEnvio.Should().Be(dataEnvio);
        }

        [Fact]
        public void CadastrarAlertaEstoqueUseCaseDto_DevePermitirAlterarValores()
        {
            // Arrange
            var dto = new CadastrarAlertaEstoqueUseCaseDto
            {
                EstoqueId = Guid.NewGuid(),
                DataEnvio = DateTime.UtcNow
            };

            var novoEstoqueId = Guid.NewGuid();
            var novaDataEnvio = DateTime.UtcNow.AddDays(1);

            // Act
            dto.EstoqueId = novoEstoqueId;
            dto.DataEnvio = novaDataEnvio;

            // Assert
            dto.EstoqueId.Should().Be(novoEstoqueId);
            dto.DataEnvio.Should().Be(novaDataEnvio);
        }

        [Fact]
        public void CadastrarAlertaEstoqueUseCaseDto_ComEstoqueIdVazio_DeveAceitarGuidEmpty()
        {
            // Arrange & Act
            var dto = new CadastrarAlertaEstoqueUseCaseDto
            {
                EstoqueId = Guid.Empty,
                DataEnvio = DateTime.UtcNow
            };

            // Assert
            dto.EstoqueId.Should().Be(Guid.Empty);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(365)]
        public void CadastrarAlertaEstoqueUseCaseDto_ComDiferentesDatas_DeveArmazenarCorretamente(int diasOffset)
        {
            // Arrange
            var dataBase = DateTime.UtcNow;
            var dataEnvio = dataBase.AddDays(diasOffset);

            // Act
            var dto = new CadastrarAlertaEstoqueUseCaseDto
            {
                EstoqueId = Guid.NewGuid(),
                DataEnvio = dataEnvio
            };

            // Assert
            dto.DataEnvio.Should().Be(dataEnvio);
        }

        [Fact]
        public void CadastrarAlertaEstoqueUseCaseDto_DeveSerReferenceType()
        {
            // Arrange
            var dto1 = new CadastrarAlertaEstoqueUseCaseDto
            {
                EstoqueId = Guid.NewGuid(),
                DataEnvio = DateTime.UtcNow
            };

            // Act
            var dto2 = dto1;
            dto2.EstoqueId = Guid.NewGuid();

            // Assert
            dto1.EstoqueId.Should().Be(dto2.EstoqueId);
        }
    }
}
