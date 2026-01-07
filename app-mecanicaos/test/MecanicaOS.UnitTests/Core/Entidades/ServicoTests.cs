using Core.Entidades;
using MecanicaOS.UnitTests.Fixtures;

namespace MecanicaOS.UnitTests.Core.Entidades
{
    /// <summary>
    /// Testes unitários para a entidade Servico
    /// </summary>
    public class ServicoTests
    {
        /// <summary>
        /// Verifica se um serviço criado com dados válidos tem todas as propriedades preenchidas corretamente
        /// </summary>
        [Fact]
        public void Servico_QuandoCriadoComDadosValidos_DeveSerValido()
        {
            // Arrange & Act
            var servico = ServicoFixture.CriarServicoValido();

            // Assert
            servico.Should().NotBeNull("a entidade não deve ser nula");
            servico.Nome.Should().Be("Troca de Óleo", "o nome deve corresponder ao valor definido");
            servico.Descricao.Should().Be("Troca de óleo do motor e filtro", "a descrição deve corresponder ao valor definido");
            servico.Valor.Should().Be(120.00m, "o valor deve corresponder ao valor definido");
            servico.Disponivel.Should().BeTrue("o serviço deve estar disponível");
        }

        /// <summary>
        /// Verifica se a propriedade Ativo é definida como true por padrão
        /// </summary>
        [Fact]
        public void Servico_QuandoCriado_DeveEstarAtivoPorPadrao()
        {
            // Arrange & Act
            var servico = new Servico { Nome = "Teste", Descricao = "Teste" };

            // Assert
            servico.Ativo.Should().BeTrue("um serviço deve estar ativo por padrão");
        }

        /// <summary>
        /// Verifica se a propriedade Id é gerada automaticamente
        /// </summary>
        [Fact]
        public void Servico_QuandoCriado_DeveGerarIdAutomaticamente()
        {
            // Arrange & Act
            var servico = new Servico { Nome = "Teste", Descricao = "Teste" };

            // Assert
            servico.Id.Should().NotBeEmpty("o Id deve ser gerado automaticamente");
        }

        /// <summary>
        /// Verifica se a propriedade DataCadastro é preenchida automaticamente
        /// </summary>
        [Fact]
        public void Servico_QuandoCriado_DevePreencherDataCadastro()
        {
            // Arrange & Act
            var servico = new Servico { Nome = "Teste", Descricao = "Teste" };
            var agora = DateTime.UtcNow;

            // Assert
            servico.DataCadastro.Should().BeCloseTo(agora, TimeSpan.FromSeconds(1), 
                "a data de cadastro deve ser próxima à data atual");
        }

        /// <summary>
        /// Verifica se a propriedade DataAtualizacao é null no construtor e preenchida ao chamar MarcarComoAtualizada
        /// </summary>
        [Fact]
        public void Servico_QuandoCriado_DataAtualizacaoDeveSerNull()
        {
            // Arrange & Act
            var servico = new Servico { Nome = "Teste", Descricao = "Teste" };

            // Assert
            servico.DataAtualizacao.Should().BeNull("a data de atualização deve ser null no construtor");
            
            // Act - Marcar como atualizada
            servico.MarcarComoAtualizada();
            var agora = DateTime.UtcNow;
            
            // Assert
            servico.DataAtualizacao.Should().NotBeNull("a data de atualização deve ser preenchida após MarcarComoAtualizada");
            servico.DataAtualizacao.Value.Should().BeCloseTo(agora, TimeSpan.FromSeconds(1), 
                "a data de atualização deve ser próxima à data atual");
        }

        /// <summary>
        /// Verifica se a disponibilidade pode ser atualizada
        /// </summary>
        [Fact]
        public void Servico_QuandoAtualizaDisponibilidade_DeveAlterarDisponibilidade()
        {
            // Arrange
            var servico = ServicoFixture.CriarServicoValido();
            
            // Act
            servico.Disponivel = false;
            
            // Assert
            servico.Disponivel.Should().BeFalse("a disponibilidade deve ser atualizada");
        }

        /// <summary>
        /// Verifica se o valor pode ser atualizado
        /// </summary>
        [Fact]
        public void Servico_QuandoAtualizaValor_DeveAlterarValor()
        {
            // Arrange
            var servico = ServicoFixture.CriarServicoValido();
            
            // Act
            servico.Valor = 150.00m;
            
            // Assert
            servico.Valor.Should().Be(150.00m, "o valor deve ser atualizado");
        }

        /// <summary>
        /// Verifica se dois serviços com o mesmo Id são considerados iguais (comportamento da classe Entidade base)
        /// </summary>
        [Fact]
        public void Servico_ComMesmoId_DevemSerConsideradosIguais()
        {
            // Arrange
            var id = Guid.NewGuid();
            var servico1 = new Servico { Id = id, Nome = "Troca de Óleo", Descricao = "Descrição 1" };
            var servico2 = new Servico { Id = id, Nome = "Alinhamento", Descricao = "Descrição 2" };

            // Act & Assert
            servico1.Equals(servico2).Should().BeTrue("serviços com o mesmo Id devem ser considerados iguais");
        }

        /// <summary>
        /// Verifica se dois serviços com nomes diferentes são considerados diferentes
        /// </summary>
        [Fact]
        public void Servico_ComNomesDiferentes_DevemSerConsideradosDiferentes()
        {
            // Arrange
            var servico1 = new Servico { Nome = "Troca de Óleo", Descricao = "Troca de óleo do motor" };
            var servico2 = new Servico { Nome = "Alinhamento", Descricao = "Alinhamento das rodas" };

            // Act & Assert
            servico1.Equals(servico2).Should().BeFalse("serviços com nomes diferentes devem ser considerados diferentes");
        }

        /// <summary>
        /// Verifica se o método Atualizar atualiza todas as propriedades quando fornecidas
        /// </summary>
        [Fact]
        public void Servico_Atualizar_ComTodosParametros_DeveAtualizarTodasPropriedades()
        {
            // Arrange
            var servico = ServicoFixture.CriarServicoValido();
            var novoNome = "Alinhamento";
            var novaDescricao = "Alinhamento e balanceamento";
            var novoValor = 200.00m;
            var novaDisponibilidade = false;

            // Act
            servico.Atualizar(novoNome, novaDescricao, novoValor, novaDisponibilidade);

            // Assert
            servico.Nome.Should().Be(novoNome, "o nome deve ser atualizado");
            servico.Descricao.Should().Be(novaDescricao, "a descrição deve ser atualizada");
            servico.Valor.Should().Be(novoValor, "o valor deve ser atualizado");
            servico.Disponivel.Should().Be(novaDisponibilidade, "a disponibilidade deve ser atualizada");
        }

        /// <summary>
        /// Verifica se o método Atualizar não altera propriedades quando parâmetros são nulos ou vazios
        /// </summary>
        [Fact]
        public void Servico_Atualizar_ComParametrosNulos_NaoDeveAlterarPropriedades()
        {
            // Arrange
            var servico = ServicoFixture.CriarServicoValido();
            var nomeOriginal = servico.Nome;
            var descricaoOriginal = servico.Descricao;
            var valorOriginal = servico.Valor;
            var disponibilidadeOriginal = servico.Disponivel;

            // Act
            servico.Atualizar(null, null, null, null);

            // Assert
            servico.Nome.Should().Be(nomeOriginal, "o nome não deve ser alterado");
            servico.Descricao.Should().Be(descricaoOriginal, "a descrição não deve ser alterada");
            servico.Valor.Should().Be(valorOriginal, "o valor não deve ser alterado");
            servico.Disponivel.Should().Be(disponibilidadeOriginal, "a disponibilidade não deve ser alterada");
        }

        /// <summary>
        /// Verifica se o método Atualizar atualiza apenas o nome quando fornecido
        /// </summary>
        [Fact]
        public void Servico_Atualizar_ApenasNome_DeveAtualizarApenasNome()
        {
            // Arrange
            var servico = ServicoFixture.CriarServicoValido();
            var novoNome = "Novo Nome";
            var descricaoOriginal = servico.Descricao;
            var valorOriginal = servico.Valor;

            // Act
            servico.Atualizar(novoNome, null, null, null);

            // Assert
            servico.Nome.Should().Be(novoNome, "o nome deve ser atualizado");
            servico.Descricao.Should().Be(descricaoOriginal, "a descrição não deve ser alterada");
            servico.Valor.Should().Be(valorOriginal, "o valor não deve ser alterado");
        }

        /// <summary>
        /// Verifica se o método Atualizar não altera o nome quando string vazia é fornecida
        /// </summary>
        [Fact]
        public void Servico_Atualizar_ComNomeVazio_NaoDeveAlterarNome()
        {
            // Arrange
            var servico = ServicoFixture.CriarServicoValido();
            var nomeOriginal = servico.Nome;

            // Act
            servico.Atualizar(string.Empty, null, null, null);

            // Assert
            servico.Nome.Should().Be(nomeOriginal, "o nome não deve ser alterado quando string vazia é fornecida");
        }
    }
}
