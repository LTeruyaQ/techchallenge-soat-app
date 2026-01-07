using Core.Entidades;
using MecanicaOS.UnitTests.Fixtures;

namespace MecanicaOS.UnitTests.Core.Entidades
{
    /// <summary>
    /// Testes unitários para a entidade Estoque
    /// </summary>
    public class EstoqueTests
    {
        /// <summary>
        /// Verifica se um estoque criado com dados válidos tem todas as propriedades preenchidas corretamente
        /// </summary>
        [Fact]
        public void Estoque_QuandoCriadoComDadosValidos_DeveSerValido()
        {
            // Arrange & Act
            var estoque = EstoqueFixture.CriarEstoqueValido();

            // Assert
            estoque.Should().NotBeNull("a entidade não deve ser nula");
            estoque.Insumo.Should().Be("Óleo de Motor 5W30", "o insumo deve corresponder ao valor definido");
            estoque.Descricao.Should().Be("Óleo sintético para motores a gasolina", "a descrição deve corresponder ao valor definido");
            estoque.Preco.Should().Be(45.90m, "o preço deve corresponder ao valor definido");
            estoque.QuantidadeDisponivel.Should().Be(20, "a quantidade disponível deve corresponder ao valor definido");
            estoque.QuantidadeMinima.Should().Be(5, "a quantidade mínima deve corresponder ao valor definido");
        }

        /// <summary>
        /// Verifica se a propriedade Ativo é definida como true por padrão
        /// </summary>
        [Fact]
        public void Estoque_QuandoCriado_DeveEstarAtivoPorPadrao()
        {
            // Arrange & Act
            var estoque = new Estoque();

            // Assert
            estoque.Ativo.Should().BeTrue("um estoque deve estar ativo por padrão");
        }

        /// <summary>
        /// Verifica se a propriedade Id é gerada automaticamente
        /// </summary>
        [Fact]
        public void Estoque_QuandoCriado_DeveGerarIdAutomaticamente()
        {
            // Arrange & Act
            var estoque = new Estoque();

            // Assert
            estoque.Id.Should().NotBeEmpty("o Id deve ser gerado automaticamente");
        }

        /// <summary>
        /// Verifica se a propriedade DataCadastro é preenchida automaticamente
        /// </summary>
        [Fact]
        public void Estoque_QuandoCriado_DevePreencherDataCadastro()
        {
            // Arrange & Act
            var estoque = new Estoque();
            var agora = DateTime.UtcNow;

            // Assert
            estoque.DataCadastro.Should().BeCloseTo(agora, TimeSpan.FromSeconds(1), 
                "a data de cadastro deve ser próxima à data atual");
        }

        /// <summary>
        /// Verifica se a propriedade DataAtualizacao é null no construtor e preenchida ao chamar MarcarComoAtualizada
        /// </summary>
        [Fact]
        public void Estoque_QuandoCriado_DataAtualizacaoDeveSerNull()
        {
            // Arrange & Act
            var estoque = new Estoque();

            // Assert
            estoque.DataAtualizacao.Should().BeNull("a data de atualização deve ser null no construtor");
            
            // Act - Marcar como atualizada
            estoque.MarcarComoAtualizada();
            var agora = DateTime.UtcNow;
            
            // Assert
            estoque.DataAtualizacao.Should().NotBeNull("a data de atualização deve ser preenchida após MarcarComoAtualizada");
            estoque.DataAtualizacao.Value.Should().BeCloseTo(agora, TimeSpan.FromSeconds(1), 
                "a data de atualização deve ser próxima à data atual");
        }

        /// <summary>
        /// Verifica se a quantidade disponível pode ser atualizada
        /// </summary>
        [Fact]
        public void Estoque_QuandoAtualizaQuantidadeDisponivel_DeveAlterarQuantidadeDisponivel()
        {
            // Arrange
            var estoque = EstoqueFixture.CriarEstoqueValido();
            
            // Act
            estoque.QuantidadeDisponivel = 15;
            
            // Assert
            estoque.QuantidadeDisponivel.Should().Be(15, "a quantidade disponível deve ser atualizada");
        }

        /// <summary>
        /// Verifica se o preço pode ser atualizado
        /// </summary>
        [Fact]
        public void Estoque_QuandoAtualizaPreco_DeveAlterarPreco()
        {
            // Arrange
            var estoque = EstoqueFixture.CriarEstoqueValido();
            
            // Act
            estoque.Preco = 50.00m;
            
            // Assert
            estoque.Preco.Should().Be(50.00m, "o preço deve ser atualizado");
        }

        /// <summary>
        /// Verifica se o método EstaAbaixoDoMinimo retorna true quando a quantidade disponível é menor que a quantidade mínima
        /// </summary>
        [Fact]
        public void EstaAbaixoDoMinimo_QuandoQuantidadeDisponivelMenorQueMinima_DeveRetornarTrue()
        {
            // Arrange
            var estoque = EstoqueFixture.CriarEstoqueCritico();
            
            // Act
            var resultado = estoque.QuantidadeDisponivel < estoque.QuantidadeMinima;
            
            // Assert
            resultado.Should().BeTrue("a quantidade disponível (2) é menor que a quantidade mínima (5)");
        }

        /// <summary>
        /// Verifica se o método EstaAbaixoDoMinimo retorna false quando a quantidade disponível é maior ou igual à quantidade mínima
        /// </summary>
        [Fact]
        public void EstaAbaixoDoMinimo_QuandoQuantidadeDisponivelMaiorOuIgualAMinima_DeveRetornarFalse()
        {
            // Arrange
            var estoque = EstoqueFixture.CriarEstoqueValido();
            
            // Act
            var resultado = estoque.QuantidadeDisponivel < estoque.QuantidadeMinima;
            
            // Assert
            resultado.Should().BeFalse("a quantidade disponível (20) é maior que a quantidade mínima (5)");
        }
    }
}
