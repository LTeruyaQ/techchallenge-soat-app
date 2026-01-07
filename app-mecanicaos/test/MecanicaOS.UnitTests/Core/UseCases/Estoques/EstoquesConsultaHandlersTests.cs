using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.UseCases.Estoques.ObterEstoque;
using Core.UseCases.Estoques.ObterTodosEstoques;
using Core.UseCases.Estoques.ObterEstoqueCritico;

namespace MecanicaOS.UnitTests.Core.UseCases.Estoques
{
    /// <summary>
    /// Testes consolidados para handlers de consulta de Estoque
    /// ROI ALTO: Consultas são operações frequentes - performance e precisão são críticas.
    /// Importância: Valida busca, listagem e filtros de estoque.
    /// </summary>
    public class EstoquesConsultaHandlersTests
    {
        private readonly IEstoqueGateway _estoqueGateway;
        private readonly ILogGateway<ObterEstoqueHandler> _logGatewayObter;
        private readonly ILogGateway<ObterTodosEstoquesHandler> _logGatewayTodos;
        private readonly ILogGateway<ObterEstoqueCriticoHandler> _logGatewayCritico;
        private readonly IUnidadeDeTrabalhoGateway _udtGateway;
        private readonly IUsuarioLogadoServicoGateway _usuarioLogadoGateway;

        public EstoquesConsultaHandlersTests()
        {
            _estoqueGateway = Substitute.For<IEstoqueGateway>();
            _logGatewayObter = Substitute.For<ILogGateway<ObterEstoqueHandler>>();
            _logGatewayTodos = Substitute.For<ILogGateway<ObterTodosEstoquesHandler>>();
            _logGatewayCritico = Substitute.For<ILogGateway<ObterEstoqueCriticoHandler>>();
            _udtGateway = Substitute.For<IUnidadeDeTrabalhoGateway>();
            _usuarioLogadoGateway = Substitute.For<IUsuarioLogadoServicoGateway>();
        }

        #region ObterEstoqueHandler

        /// <summary>
        /// Verifica se obtém estoque por ID com sucesso
        /// Importância: ALTA - Operação de consulta principal
        /// Contribuição: Garante busca correta de produtos
        /// </summary>
        [Fact]
        public async Task ObterEstoque_ComIdExistente_DeveRetornarEstoque()
        {
            // Arrange
            var handler = new ObterEstoqueHandler(_estoqueGateway, _logGatewayObter, _udtGateway, _usuarioLogadoGateway);
            var estoqueId = Guid.NewGuid();
            var estoque = new Estoque
            {
                Id = estoqueId,
                Insumo = "Óleo 5W30",
                Descricao = "Óleo sintético",
                QuantidadeDisponivel = 50,
                Preco = 45.90m
            };

            _estoqueGateway.ObterPorIdAsync(estoqueId).Returns(Task.FromResult<Estoque?>(estoque));

            // Act
            var resultado = await handler.Handle(estoqueId);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Id.Should().Be(estoqueId);
            resultado.Insumo.Should().Be("Óleo 5W30");
        }

        /// <summary>
        /// Verifica se lança exceção quando estoque não existe
        /// Importância: ALTA - Tratamento de erro obrigatório
        /// Contribuição: Previne retorno de dados inválidos
        /// </summary>
        [Fact]
        public async Task ObterEstoque_ComIdInexistente_DeveLancarDadosNaoEncontradosException()
        {
            // Arrange
            var handler = new ObterEstoqueHandler(_estoqueGateway, _logGatewayObter, _udtGateway, _usuarioLogadoGateway);
            var estoqueId = Guid.NewGuid();

            _estoqueGateway.ObterPorIdAsync(estoqueId).Returns(Task.FromResult<Estoque?>(null));

            // Act & Assert
            await handler.Invoking(h => h.Handle(estoqueId))
                .Should().ThrowAsync<DadosNaoEncontradosException>()
                .WithMessage("Estoque não encontrado");
        }

        #endregion

        #region ObterTodosEstoquesHandler

        /// <summary>
        /// Verifica se obtém todos os estoques
        /// Importância: ALTA - Listagem completa de produtos
        /// Contribuição: Garante visualização de todo o inventário
        /// </summary>
        [Fact]
        public async Task ObterTodosEstoques_DeveRetornarListaCompleta()
        {
            // Arrange
            var handler = new ObterTodosEstoquesHandler(_estoqueGateway, _logGatewayTodos, _udtGateway, _usuarioLogadoGateway);
            var estoques = new List<Estoque>
            {
                new Estoque { Id = Guid.NewGuid(), Insumo = "Produto 1", Preco = 10.00m },
                new Estoque { Id = Guid.NewGuid(), Insumo = "Produto 2", Preco = 20.00m },
                new Estoque { Id = Guid.NewGuid(), Insumo = "Produto 3", Preco = 30.00m }
            };

            _estoqueGateway.ObterTodosAsync().Returns(Task.FromResult<IEnumerable<Estoque>>(estoques));

            // Act
            var resultado = await handler.Handle();

            // Assert
            resultado.Should().HaveCount(3);
            resultado.Should().Contain(e => e.Insumo == "Produto 1");
            resultado.Should().Contain(e => e.Insumo == "Produto 2");
            resultado.Should().Contain(e => e.Insumo == "Produto 3");
        }

        /// <summary>
        /// Verifica se retorna lista vazia quando não há estoques
        /// Importância: MÉDIA - Tratamento de caso vazio
        /// Contribuição: Previne erros com inventário vazio
        /// </summary>
        [Fact]
        public async Task ObterTodosEstoques_SemEstoques_DeveRetornarListaVazia()
        {
            // Arrange
            var handler = new ObterTodosEstoquesHandler(_estoqueGateway, _logGatewayTodos, _udtGateway, _usuarioLogadoGateway);

            _estoqueGateway.ObterTodosAsync().Returns(Task.FromResult<IEnumerable<Estoque>>(new List<Estoque>()));

            // Act
            var resultado = await handler.Handle();

            // Assert
            resultado.Should().BeEmpty();
        }

        /// <summary>
        /// Verifica se propaga exceção quando gateway falha
        /// </summary>
        [Fact]
        public async Task ObterTodosEstoques_QuandoGatewayLancaExcecao_DevePropagar()
        {
            // Arrange
            var handler = new ObterTodosEstoquesHandler(_estoqueGateway, _logGatewayTodos, _udtGateway, _usuarioLogadoGateway);
            _estoqueGateway.ObterTodosAsync().Returns(Task.FromException<IEnumerable<Estoque>>(new InvalidOperationException("Erro no banco")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await handler.Handle());
        }

        #endregion

        #region ObterEstoqueCriticoHandler

        /// <summary>
        /// Verifica se obtém apenas estoques críticos
        /// Importância: CRÍTICA - Alertas de estoque baixo são essenciais
        /// Contribuição: Previne falta de produtos
        /// </summary>
        [Fact]
        public async Task ObterEstoqueCritico_DeveRetornarApenasEstoquesCriticos()
        {
            // Arrange
            var handler = new ObterEstoqueCriticoHandler(_estoqueGateway, _logGatewayCritico, _udtGateway, _usuarioLogadoGateway);
            var estoquesCriticos = new List<Estoque>
            {
                new Estoque 
                { 
                    Id = Guid.NewGuid(), 
                    Insumo = "Produto Crítico 1", 
                    QuantidadeDisponivel = 5,
                    QuantidadeMinima = 10,
                    Preco = 10.00m 
                },
                new Estoque 
                { 
                    Id = Guid.NewGuid(), 
                    Insumo = "Produto Crítico 2", 
                    QuantidadeDisponivel = 2,
                    QuantidadeMinima = 15,
                    Preco = 20.00m 
                }
            };

            _estoqueGateway.ObterEstoqueCriticoAsync().Returns(Task.FromResult<IEnumerable<Estoque>>(estoquesCriticos));

            // Act
            var resultado = await handler.Handle();

            // Assert
            resultado.Should().HaveCount(2);
            resultado.Should().AllSatisfy(e => 
                e.QuantidadeDisponivel.Should().BeLessThan(e.QuantidadeMinima)
            );
        }

        /// <summary>
        /// Verifica se retorna vazio quando não há estoques críticos
        /// Importância: MÉDIA - Cenário positivo (estoque OK)
        /// Contribuição: Confirma que sistema identifica quando tudo está OK
        /// </summary>
        [Fact]
        public async Task ObterEstoqueCritico_SemEstoquesCriticos_DeveRetornarVazio()
        {
            // Arrange
            var handler = new ObterEstoqueCriticoHandler(_estoqueGateway, _logGatewayCritico, _udtGateway, _usuarioLogadoGateway);

            _estoqueGateway.ObterEstoqueCriticoAsync().Returns(Task.FromResult<IEnumerable<Estoque>>(new List<Estoque>()));

            // Act
            var resultado = await handler.Handle();

            // Assert
            resultado.Should().BeEmpty();
        }

        /// <summary>
        /// Verifica se identifica múltiplos níveis de criticidade
        /// Importância: ALTA - Priorização de reposição
        /// Contribuição: Permite ação baseada em urgência
        /// </summary>
        [Fact]
        public async Task ObterEstoqueCritico_ComDiferentesNiveisCriticos_DeveRetornarTodos()
        {
            // Arrange
            var handler = new ObterEstoqueCriticoHandler(_estoqueGateway, _logGatewayCritico, _udtGateway, _usuarioLogadoGateway);
            var estoquesCriticos = new List<Estoque>
            {
                new Estoque 
                { 
                    Insumo = "Muito Crítico", 
                    QuantidadeDisponivel = 0,
                    QuantidadeMinima = 20,
                    Preco = 10.00m 
                },
                new Estoque 
                { 
                    Insumo = "Pouco Crítico", 
                    QuantidadeDisponivel = 18,
                    QuantidadeMinima = 20,
                    Preco = 20.00m 
                }
            };

            _estoqueGateway.ObterEstoqueCriticoAsync().Returns(Task.FromResult<IEnumerable<Estoque>>(estoquesCriticos));

            // Act
            var resultado = await handler.Handle();

            // Assert
            resultado.Should().HaveCount(2);
            resultado.Should().Contain(e => e.Insumo == "Muito Crítico");
            resultado.Should().Contain(e => e.Insumo == "Pouco Crítico");
        }

        /// <summary>
        /// Verifica se propaga exceção quando gateway falha
        /// </summary>
        [Fact]
        public async Task ObterEstoqueCritico_QuandoGatewayLancaExcecao_DevePropagar()
        {
            // Arrange
            var handler = new ObterEstoqueCriticoHandler(_estoqueGateway, _logGatewayCritico, _udtGateway, _usuarioLogadoGateway);
            _estoqueGateway.ObterEstoqueCriticoAsync().Returns(Task.FromException<IEnumerable<Estoque>>(new InvalidOperationException("Erro no banco")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await handler.Handle());
        }

        #endregion
    }
}
