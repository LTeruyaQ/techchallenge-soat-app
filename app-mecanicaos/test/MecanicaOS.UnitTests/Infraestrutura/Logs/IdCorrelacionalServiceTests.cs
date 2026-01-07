using FluentAssertions;
using Infraestrutura.Logs;

namespace MecanicaOS.UnitTests.Infraestrutura.Logs
{
    /// <summary>
    /// Testes para IdCorrelacionalService (Infraestrutura)
    /// 
    /// IMPORTÂNCIA: Serviço crítico para rastreamento de requisições através de múltiplos serviços.
    /// O CorrelationId permite correlacionar logs de uma mesma operação distribuída.
    /// 
    /// COBERTURA: Valida geração, obtenção e configuração de IDs de correlação.
    /// Essencial para debugging e análise de logs em produção.
    /// </summary>
    public class IdCorrelacionalServiceTests
    {
        [Fact]
        public void Construtor_DeveGerarCorrelationIdAutomaticamente()
        {
            // Arrange & Act
            var servico = new IdCorrelacionalService();
            var correlationId = servico.GetCorrelationId();

            // Assert
            correlationId.Should().NotBeNullOrEmpty("CorrelationId deve ser gerado automaticamente no construtor");
            Guid.TryParse(correlationId, out _).Should().BeTrue("CorrelationId deve ser um GUID válido");
        }

        [Fact]
        public void GetCorrelationId_DeveRetornarMesmoIdEmChamadasConsecutivas()
        {
            // Arrange
            var servico = new IdCorrelacionalService();

            // Act
            var id1 = servico.GetCorrelationId();
            var id2 = servico.GetCorrelationId();

            // Assert
            id1.Should().Be(id2, "GetCorrelationId deve retornar o mesmo ID em chamadas consecutivas");
        }

        [Fact]
        public void SetCorrelationId_ComGuidValido_DeveAtualizarCorrelationId()
        {
            // Arrange
            var servico = new IdCorrelacionalService();
            var idOriginal = servico.GetCorrelationId();
            var novoId = Guid.NewGuid();

            // Act
            servico.SetCorrelationId(novoId);
            var idAtualizado = servico.GetCorrelationId();

            // Assert
            idAtualizado.Should().NotBe(idOriginal, "CorrelationId deve ser atualizado");
            idAtualizado.Should().Be(novoId.ToString(), "CorrelationId deve ser igual ao novo GUID fornecido");
        }

        [Fact]
        public void SetCorrelationId_ComGuidEmpty_NaoDeveAtualizarCorrelationId()
        {
            // Arrange
            var servico = new IdCorrelacionalService();
            var idOriginal = servico.GetCorrelationId();

            // Act
            servico.SetCorrelationId(Guid.Empty);
            var idDepois = servico.GetCorrelationId();

            // Assert
            idDepois.Should().Be(idOriginal, "CorrelationId não deve ser atualizado quando Guid.Empty é fornecido");
        }

        [Fact]
        public void SetCorrelationId_ComNull_NaoDeveAtualizarCorrelationId()
        {
            // Arrange
            var servico = new IdCorrelacionalService();
            var idOriginal = servico.GetCorrelationId();

            // Act
            servico.SetCorrelationId(null);
            var idDepois = servico.GetCorrelationId();

            // Assert
            idDepois.Should().Be(idOriginal, "CorrelationId não deve ser atualizado quando null é fornecido");
        }

        [Fact]
        public void SetCorrelationId_SemParametro_NaoDeveAtualizarCorrelationId()
        {
            // Arrange
            var servico = new IdCorrelacionalService();
            var idOriginal = servico.GetCorrelationId();

            // Act
            servico.SetCorrelationId();
            var idDepois = servico.GetCorrelationId();

            // Assert
            idDepois.Should().Be(idOriginal, "CorrelationId não deve ser atualizado quando nenhum parâmetro é fornecido");
        }

        [Fact]
        public void SetCorrelationId_DevePermitirMultiplasAtualizacoes()
        {
            // Arrange
            var servico = new IdCorrelacionalService();
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var id3 = Guid.NewGuid();

            // Act
            servico.SetCorrelationId(id1);
            var resultado1 = servico.GetCorrelationId();

            servico.SetCorrelationId(id2);
            var resultado2 = servico.GetCorrelationId();

            servico.SetCorrelationId(id3);
            var resultado3 = servico.GetCorrelationId();

            // Assert
            resultado1.Should().Be(id1.ToString());
            resultado2.Should().Be(id2.ToString());
            resultado3.Should().Be(id3.ToString());
            resultado1.Should().NotBe(resultado2);
            resultado2.Should().NotBe(resultado3);
        }

        [Fact]
        public void CorrelationId_DeveSerUnicoParaCadaInstancia()
        {
            // Arrange & Act
            var servico1 = new IdCorrelacionalService();
            var servico2 = new IdCorrelacionalService();

            var id1 = servico1.GetCorrelationId();
            var id2 = servico2.GetCorrelationId();

            // Assert
            id1.Should().NotBe(id2, "Cada instância deve ter seu próprio CorrelationId único");
        }

        [Fact]
        public void SetCorrelationId_ComGuidValido_DeveConverterParaString()
        {
            // Arrange
            var servico = new IdCorrelacionalService();
            var guid = Guid.NewGuid();

            // Act
            servico.SetCorrelationId(guid);
            var resultado = servico.GetCorrelationId();

            // Assert
            resultado.Should().BeOfType<string>("CorrelationId deve ser retornado como string");
            resultado.Should().Be(guid.ToString(), "String deve corresponder à representação do GUID");
        }
    }
}
