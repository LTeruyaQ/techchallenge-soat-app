using Core.Entidades;
using Core.Enumeradores;
using MecanicaOS.UnitTests.Fixtures;

namespace MecanicaOS.UnitTests.Core.Entidades
{
    /// <summary>
    /// Testes unitários para a entidade OrdemServico
    /// </summary>
    public class OrdemServicoTests
    {
        /// <summary>
        /// Verifica se uma ordem de serviço criada com dados válidos tem todas as propriedades preenchidas corretamente
        /// </summary>
        [Fact]
        public void OrdemServico_QuandoCriadaComDadosValidos_DeveSerValida()
        {
            // Arrange & Act
            var ordem = OrdemServicoFixture.CriarOrdemServicoValida();

            // Assert
            ordem.Should().NotBeNull("a entidade não deve ser nula");
            ordem.ClienteId.Should().NotBeEmpty("o ID do cliente não deve ser vazio");
            ordem.VeiculoId.Should().NotBeEmpty("o ID do veículo não deve ser vazio");
            ordem.ServicoId.Should().NotBeEmpty("o ID do serviço não deve ser vazio");
            ordem.Descricao.Should().Be("Manutenção preventiva", "a descrição deve corresponder ao valor definido");
            ordem.Status.Should().Be(StatusOrdemServico.Recebida, "o status deve ser Recebida por padrão");
        }

        /// <summary>
        /// Verifica se a propriedade Ativo é definida como true por padrão
        /// </summary>
        [Fact]
        public void OrdemServico_QuandoCriada_DeveEstarAtivaPorPadrao()
        {
            // Arrange & Act
            var ordem = new OrdemServico();

            // Assert
            ordem.Ativo.Should().BeTrue("uma ordem de serviço deve estar ativa por padrão");
        }

        /// <summary>
        /// Verifica se a propriedade Id é gerada automaticamente
        /// </summary>
        [Fact]
        public void OrdemServico_QuandoCriada_DeveGerarIdAutomaticamente()
        {
            // Arrange & Act
            var ordem = new OrdemServico();

            // Assert
            ordem.Id.Should().NotBeEmpty("o Id deve ser gerado automaticamente");
        }

        /// <summary>
        /// Verifica se a propriedade DataCadastro é preenchida automaticamente
        /// </summary>
        [Fact]
        public void OrdemServico_QuandoCriada_DevePreencherDataCadastro()
        {
            // Arrange & Act
            var ordem = new OrdemServico();
            var agora = DateTime.UtcNow;

            // Assert
            ordem.DataCadastro.Should().BeCloseTo(agora, TimeSpan.FromSeconds(1), 
                "a data de cadastro deve ser próxima à data atual");
        }

        /// <summary>
        /// Verifica se a propriedade DataAtualizacao é null no construtor e preenchida ao chamar MarcarComoAtualizada
        /// </summary>
        [Fact]
        public void OrdemServico_QuandoCriada_DataAtualizacaoDeveSerNull()
        {
            // Arrange & Act
            var ordem = new OrdemServico();

            // Assert
            ordem.DataAtualizacao.Should().BeNull("a data de atualização deve ser null no construtor");
            
            // Act - Marcar como atualizada
            ordem.MarcarComoAtualizada();
            var agora = DateTime.UtcNow;
            
            // Assert
            ordem.DataAtualizacao.Should().NotBeNull("a data de atualização deve ser preenchida após MarcarComoAtualizada");
            ordem.DataAtualizacao.Value.Should().BeCloseTo(agora, TimeSpan.FromSeconds(1), 
                "a data de atualização deve ser próxima à data atual");
        }

        /// <summary>
        /// Verifica se o status da ordem de serviço pode ser atualizado
        /// </summary>
        [Fact]
        public void OrdemServico_QuandoAtualizaStatus_DeveAlterarStatus()
        {
            // Arrange
            var ordem = OrdemServicoFixture.CriarOrdemServicoValida();
            
            // Act
            ordem.Status = StatusOrdemServico.EmDiagnostico;
            
            // Assert
            ordem.Status.Should().Be(StatusOrdemServico.EmDiagnostico, "o status deve ser atualizado");
        }


        /// <summary>
        /// Verifica se todos os status possíveis são aceitos
        /// </summary>
        [Theory]
        [InlineData(StatusOrdemServico.Recebida)]
        [InlineData(StatusOrdemServico.EmDiagnostico)]
        [InlineData(StatusOrdemServico.AguardandoAprovacao)]
        [InlineData(StatusOrdemServico.EmExecucao)]
        [InlineData(StatusOrdemServico.Finalizada)]
        [InlineData(StatusOrdemServico.Cancelada)]
        public void OrdemServico_ComDiferentesStatus_DeveSerValida(StatusOrdemServico status)
        {
            // Arrange
            var ordem = OrdemServicoFixture.CriarOrdemServicoComStatus(status);
            
            // Assert
            ordem.Status.Should().Be(status, "a ordem deve aceitar o status especificado");
        }

        /// <summary>
        /// Verifica se a descrição pode ser atualizada
        /// </summary>
        [Fact]
        public void OrdemServico_QuandoAtualizaDescricao_DeveAlterarDescricao()
        {
            // Arrange
            var ordem = OrdemServicoFixture.CriarOrdemServicoValida();
            
            // Act
            ordem.Descricao = "Nova descrição";
            
            // Assert
            ordem.Descricao.Should().Be("Nova descrição", "a descrição deve ser atualizada");
        }

        [Fact]
        public void OrdemServico_Atualizar_ComTodosParametros_DeveAtualizarTodasPropriedades()
        {
            // Arrange
            var ordem = OrdemServicoFixture.CriarOrdemServicoValida();
            var novoClienteId = Guid.NewGuid();
            var novoVeiculoId = Guid.NewGuid();
            var novoServicoId = Guid.NewGuid();
            var novaDescricao = "Nova descrição completa";
            var novoStatus = StatusOrdemServico.EmExecucao;

            // Act
            ordem.Atualizar(novoClienteId, novoVeiculoId, novoServicoId, novaDescricao, novoStatus);

            // Assert
            ordem.ClienteId.Should().Be(novoClienteId);
            ordem.VeiculoId.Should().Be(novoVeiculoId);
            ordem.ServicoId.Should().Be(novoServicoId);
            ordem.Descricao.Should().Be(novaDescricao);
            ordem.Status.Should().Be(novoStatus);
        }

        [Fact]
        public void OrdemServico_Atualizar_ComParametrosNulos_NaoDeveAlterarPropriedades()
        {
            // Arrange
            var ordem = OrdemServicoFixture.CriarOrdemServicoValida();
            var clienteIdOriginal = ordem.ClienteId;
            var veiculoIdOriginal = ordem.VeiculoId;
            var servicoIdOriginal = ordem.ServicoId;
            var descricaoOriginal = ordem.Descricao;
            var statusOriginal = ordem.Status;

            // Act
            ordem.Atualizar(null, null, null, null, null);

            // Assert
            ordem.ClienteId.Should().Be(clienteIdOriginal);
            ordem.VeiculoId.Should().Be(veiculoIdOriginal);
            ordem.ServicoId.Should().Be(servicoIdOriginal);
            ordem.Descricao.Should().Be(descricaoOriginal);
            ordem.Status.Should().Be(statusOriginal);
        }

        [Fact]
        public void OrdemServico_Atualizar_ApenasStatus_DeveAtualizarApenasStatus()
        {
            // Arrange
            var ordem = OrdemServicoFixture.CriarOrdemServicoValida();
            var novoStatus = StatusOrdemServico.Finalizada;
            var clienteIdOriginal = ordem.ClienteId;

            // Act
            ordem.Atualizar(null, null, null, null, novoStatus);

            // Assert
            ordem.Status.Should().Be(novoStatus);
            ordem.ClienteId.Should().Be(clienteIdOriginal);
        }

        [Fact]
        public void OrdemServico_Atualizar_ComDescricaoVazia_NaoDeveAlterarDescricao()
        {
            // Arrange
            var ordem = OrdemServicoFixture.CriarOrdemServicoValida();
            var descricaoOriginal = ordem.Descricao;

            // Act
            ordem.Atualizar(null, null, null, string.Empty, null);

            // Assert
            ordem.Descricao.Should().Be(descricaoOriginal);
        }

    }
}
