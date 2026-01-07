using Core.Entidades;
using MecanicaOS.UnitTests.Fixtures;

namespace MecanicaOS.UnitTests.Core.Entidades
{
    /// <summary>
    /// Testes unitários para a entidade Veiculo
    /// </summary>
    public class VeiculoTests
    {
        /// <summary>
        /// Verifica se um veículo criado com dados válidos tem todas as propriedades preenchidas corretamente
        /// </summary>
        [Fact]
        public void Veiculo_QuandoCriadoComDadosValidos_DeveSerValido()
        {
            // Arrange & Act
            var veiculo = VeiculoFixture.CriarVeiculoValido();

            // Assert
            veiculo.Should().NotBeNull("a entidade não deve ser nula");
            veiculo.Placa.Should().Be("ABC1234", "a placa deve corresponder ao valor definido");
            veiculo.Marca.Should().Be("Toyota", "a marca deve corresponder ao valor definido");
            veiculo.Modelo.Should().Be("Corolla", "o modelo deve corresponder ao valor definido");
            veiculo.Ano.Should().Be("2022", "o ano deve corresponder ao valor definido");
            veiculo.Cor.Should().Be("Prata", "a cor deve corresponder ao valor definido");
            veiculo.Anotacoes.Should().Be("Veículo em bom estado", "as anotações devem corresponder ao valor definido");
            veiculo.ClienteId.Should().NotBeEmpty("o ID do cliente não deve ser vazio");
        }

        /// <summary>
        /// Verifica se a propriedade Ativo é definida como true por padrão
        /// </summary>
        [Fact]
        public void Veiculo_QuandoCriado_DeveEstarAtivoPorPadrao()
        {
            // Arrange & Act
            var veiculo = new Veiculo();

            // Assert
            veiculo.Ativo.Should().BeTrue("um veículo deve estar ativo por padrão");
        }

        /// <summary>
        /// Verifica se a propriedade Id é gerada automaticamente
        /// </summary>
        [Fact]
        public void Veiculo_QuandoCriado_DeveGerarIdAutomaticamente()
        {
            // Arrange & Act
            var veiculo = new Veiculo();

            // Assert
            veiculo.Id.Should().NotBeEmpty("o Id deve ser gerado automaticamente");
        }

        /// <summary>
        /// Verifica se a propriedade DataCadastro é preenchida automaticamente
        /// </summary>
        [Fact]
        public void Veiculo_QuandoCriado_DevePreencherDataCadastro()
        {
            // Arrange & Act
            var veiculo = new Veiculo();
            var agora = DateTime.UtcNow;

            // Assert
            veiculo.DataCadastro.Should().BeCloseTo(agora, TimeSpan.FromSeconds(1), 
                "a data de cadastro deve ser próxima à data atual");
        }

        /// <summary>
        /// Verifica se a propriedade DataAtualizacao é null no construtor e preenchida ao chamar MarcarComoAtualizada
        /// </summary>
        [Fact]
        public void Veiculo_QuandoCriado_DataAtualizacaoDeveSerNull()
        {
            // Arrange & Act
            var veiculo = new Veiculo();

            // Assert
            veiculo.DataAtualizacao.Should().BeNull("a data de atualização deve ser null no construtor");
            
            // Act - Marcar como atualizada
            veiculo.MarcarComoAtualizada();
            var agora = DateTime.UtcNow;
            
            // Assert
            veiculo.DataAtualizacao.Should().NotBeNull("a data de atualização deve ser preenchida após MarcarComoAtualizada");
            veiculo.DataAtualizacao.Value.Should().BeCloseTo(agora, TimeSpan.FromSeconds(1), 
                "a data de atualização deve ser próxima à data atual");
        }

        /// <summary>
        /// Verifica se dois veículos com o mesmo Id são considerados iguais (comportamento da classe Entidade base)
        /// </summary>
        [Fact]
        public void Veiculo_ComMesmoId_DevemSerConsideradosIguais()
        {
            // Arrange
            var id = Guid.NewGuid();
            var veiculo1 = new Veiculo { Id = id, Placa = "ABC1234" };
            var veiculo2 = new Veiculo { Id = id, Placa = "XYZ9876" };

            // Act & Assert
            veiculo1.Equals(veiculo2).Should().BeTrue("veículos com o mesmo Id devem ser considerados iguais");
        }

        /// <summary>
        /// Verifica se dois veículos com placas diferentes são considerados diferentes
        /// </summary>
        [Fact]
        public void Veiculo_ComPlacasDiferentes_DevemSerConsideradosDiferentes()
        {
            // Arrange
            var veiculo1 = new Veiculo { Placa = "ABC1234" };
            var veiculo2 = new Veiculo { Placa = "XYZ9876" };

            // Act & Assert
            veiculo1.Equals(veiculo2).Should().BeFalse("veículos com placas diferentes devem ser considerados diferentes");
        }

        /// <summary>
        /// Verifica se a propriedade Anotacoes pode ser nula
        /// </summary>
        [Fact]
        public void Veiculo_ComAnotacoesNulas_DeveSerValido()
        {
            // Arrange
            var veiculo = new Veiculo
            {
                Placa = "ABC1234",
                Marca = "Toyota",
                Modelo = "Corolla",
                Ano = "2022",
                Cor = "Prata",
                Anotacoes = null,
                ClienteId = Guid.NewGuid()
            };

            // Act & Assert
            veiculo.Anotacoes.Should().BeNull("as anotações podem ser nulas");
        }

        /// <summary>
        /// Verifica se diferentes formatos de placa são aceitos
        /// </summary>
        [Theory]
        [InlineData("ABC1234")]
        [InlineData("ABC1D23")]
        [InlineData("ABC-1234")]
        public void Veiculo_ComDiferentesFormatosDePlaca_DeveSerValido(string placa)
        {
            // Arrange
            var veiculo = VeiculoFixture.CriarVeiculoComPlaca(placa);

            // Assert
            veiculo.Placa.Should().Be(placa, "a placa deve aceitar diferentes formatos");
        }
    }
}
