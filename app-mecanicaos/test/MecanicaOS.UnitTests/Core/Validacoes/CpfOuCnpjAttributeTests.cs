using Core.Validacoes.AtributosValidacao;

namespace MecanicaOS.UnitTests.Core.Validacoes
{
    /// <summary>
    /// Testes para CpfOuCnpjAttribute
    /// ROI CRÍTICO: Validação de documentos é essencial para integridade de dados.
    /// Importância: Previne cadastros com CPF/CNPJ inválidos.
    /// </summary>
    public class CpfOuCnpjAttributeTests
    {
        [Theory]
        [InlineData("12345678909")] // CPF válido
        [InlineData("123.456.789-09")]
        [InlineData("11222333000181")] // CNPJ válido
        [InlineData("11.222.333/0001-81")]
        public void IsValid_ComDocumentoValido_DeveRetornarTrue(string documento)
        {
            // Arrange
            var attribute = new CpfOuCnpjAttribute();

            // Act
            var resultado = attribute.IsValid(documento);

            // Assert
            resultado.Should().BeTrue();
        }

        [Theory]
        [InlineData("12345678900")] // CPF inválido
        [InlineData("11222333000180")] // CNPJ inválido
        [InlineData("00000000000")]
        [InlineData("11111111111")]
        public void IsValid_ComDocumentoInvalido_DeveRetornarFalse(string documento)
        {
            // Arrange
            var attribute = new CpfOuCnpjAttribute();

            // Act
            var resultado = attribute.IsValid(documento);

            // Assert
            resultado.Should().BeFalse();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void IsValid_ComDocumentoNuloOuVazio_DeveRetornarTrue(string? documento)
        {
            // Arrange
            var attribute = new CpfOuCnpjAttribute();

            // Act
            var resultado = attribute.IsValid(documento);

            // Assert
            resultado.Should().BeTrue(); // Permite nulo/vazio (validação opcional)
        }

        [Fact]
        public void IsValid_ComObjetoNaoString_DeveRetornarTrue()
        {
            // Arrange
            var attribute = new CpfOuCnpjAttribute();

            // Act
            var resultado = attribute.IsValid(123);

            // Assert
            resultado.Should().BeTrue();
        }
    }
}
