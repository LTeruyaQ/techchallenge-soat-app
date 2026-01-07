using Core.DTOs.UseCases.Estoque;
using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.UseCases.Estoques.AtualizarEstoque;
using Core.UseCases.Estoques.CadastrarEstoque;
using Core.UseCases.Estoques.DeletarEstoque;
using Core.UseCases.Estoques.ObterEstoque;
using Core.UseCases.Estoques.ObterEstoqueCritico;
using Core.UseCases.Estoques.ObterTodosEstoques;

namespace MecanicaOS.UnitTests.Core.UseCases.Estoques
{
    /// <summary>
    /// Testes completos para todos os handlers de Estoques
    /// ROI CRÍTICO: Estoque é fundamental para controle de insumos e alertas.
    /// Importância: Controle de estoque impacta operação e custos da oficina.
    /// </summary>
    public class EstoquesHandlersCompletosTests
    {
        private readonly IEstoqueGateway _estoqueGateway;
        private readonly ILogGateway<CadastrarEstoqueHandler> _logGateway;
        private readonly IUnidadeDeTrabalhoGateway _udtGateway;
        private readonly IUsuarioLogadoServicoGateway _usuarioLogadoGateway;

        public EstoquesHandlersCompletosTests()
        {
            _estoqueGateway = Substitute.For<IEstoqueGateway>();
            _logGateway = Substitute.For<ILogGateway<CadastrarEstoqueHandler>>();
            _udtGateway = Substitute.For<IUnidadeDeTrabalhoGateway>();
            _usuarioLogadoGateway = Substitute.For<IUsuarioLogadoServicoGateway>();
            _udtGateway.Commit().Returns(Task.FromResult(true));
        }

        #region CadastrarEstoqueHandler

        /// <summary>
        /// Verifica se CadastrarEstoque cadastra insumo com sucesso
        /// Importância: ALTA - Cadastro de insumos é operação frequente
        /// Contribuição: Garante que novos insumos podem ser adicionados ao estoque
        /// </summary>
        [Fact]
        public async Task CadastrarEstoque_ComDadosValidos_DeveCadastrarComSucesso()
        {
            // Arrange
            var handler = new CadastrarEstoqueHandler(_estoqueGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var request = new CadastrarEstoqueUseCaseDto
            {
                Insumo = "Óleo 5W30",
                Descricao = "Óleo sintético para motor",
                Preco = 50.00m,
                QuantidadeDisponivel = 100,
                QuantidadeMinima = 10
            };

            _estoqueGateway.CadastrarAsync(Arg.Any<Estoque>()).Returns(Task.CompletedTask);

            // Act
            var resultado = await handler.Handle(request);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Insumo.Should().Be("Óleo 5W30");
            resultado.Preco.Should().Be(50.00m);
            await _estoqueGateway.Received(1).CadastrarAsync(Arg.Any<Estoque>());
            await _udtGateway.Received(1).Commit();
        }

        /// <summary>
        /// Verifica se CadastrarEstoque lança exceção quando commit falha
        /// Importância: ALTA - Valida tratamento de erro de persistência
        /// Contribuição: Garante que falhas são tratadas adequadamente
        /// </summary>
        [Fact]
        public async Task CadastrarEstoque_ComFalhaNoCommit_DeveLancarExcecao()
        {
            // Arrange
            var handler = new CadastrarEstoqueHandler(_estoqueGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var request = new CadastrarEstoqueUseCaseDto
            {
                Insumo = "Filtro de Óleo",
                Descricao = "Filtro",
                Preco = 25.00m,
                QuantidadeDisponivel = 50,
                QuantidadeMinima = 5
            };

            _estoqueGateway.CadastrarAsync(Arg.Any<Estoque>()).Returns(Task.CompletedTask);
            _udtGateway.Commit().Returns(Task.FromResult(false));

            // Act & Assert
            await handler.Invoking(h => h.Handle(request))
                .Should().ThrowAsync<PersistirDadosException>()
                .WithMessage("Erro ao cadastrar estoque");
        }

        #endregion

        #region AtualizarEstoqueHandler

        /// <summary>
        /// Verifica se AtualizarEstoque atualiza insumo com sucesso
        /// Importância: CRÍTICA - Atualização de quantidade é operação frequente
        /// Contribuição: Garante que estoque pode ser ajustado conforme necessário
        /// </summary>
        [Fact]
        public async Task AtualizarEstoque_ComDadosValidos_DeveAtualizarComSucesso()
        {
            // Arrange
            var logGatewayAtualizar = Substitute.For<ILogGateway<AtualizarEstoqueHandler>>();
            var handler = new AtualizarEstoqueHandler(_estoqueGateway, logGatewayAtualizar, _udtGateway, _usuarioLogadoGateway);
            var estoqueId = Guid.NewGuid();
            var estoqueExistente = new Estoque
            {
                Id = estoqueId,
                Insumo = "Óleo 5W30",
                Descricao = "Óleo sintético",
                Preco = 50.00m,
                QuantidadeDisponivel = 100,
                QuantidadeMinima = 10
            };
            var request = new AtualizarEstoqueUseCaseDto
            {
                QuantidadeDisponivel = 150,
                Preco = 55.00m
            };

            _estoqueGateway.ObterPorIdAsync(estoqueId).Returns(Task.FromResult<Estoque?>(estoqueExistente));
            _estoqueGateway.EditarAsync(Arg.Any<Estoque>()).Returns(Task.CompletedTask);

            // Act
            var resultado = await handler.Handle(estoqueId, request);

            // Assert
            resultado.Should().NotBeNull();
            await _estoqueGateway.Received(1).EditarAsync(Arg.Any<Estoque>());
            await _udtGateway.Received(1).Commit();
        }

        /// <summary>
        /// Verifica se AtualizarEstoque lança exceção para estoque inexistente
        /// Importância: ALTA - Previne atualização de registros inválidos
        /// Contribuição: Garante integridade dos dados
        /// </summary>
        [Fact]
        public async Task AtualizarEstoque_ComEstoqueInexistente_DeveLancarExcecao()
        {
            // Arrange
            var logGatewayAtualizar = Substitute.For<ILogGateway<AtualizarEstoqueHandler>>();
            var handler = new AtualizarEstoqueHandler(_estoqueGateway, logGatewayAtualizar, _udtGateway, _usuarioLogadoGateway);
            var estoqueId = Guid.NewGuid();
            var request = new AtualizarEstoqueUseCaseDto { QuantidadeDisponivel = 100 };

            _estoqueGateway.ObterPorIdAsync(estoqueId).Returns(Task.FromResult<Estoque?>(null));

            // Act & Assert
            await handler.Invoking(h => h.Handle(estoqueId, request))
                .Should().ThrowAsync<DadosNaoEncontradosException>()
                .WithMessage("Estoque não encontrado");
        }

        #endregion

        #region DeletarEstoqueHandler

        /// <summary>
        /// Verifica se DeletarEstoque remove insumo com sucesso
        /// Importância: MÉDIA - Exclusão de insumos obsoletos
        /// Contribuição: Permite limpeza de estoque
        /// </summary>
        [Fact]
        public async Task DeletarEstoque_ComEstoqueExistente_DeveDeletarComSucesso()
        {
            // Arrange
            var logGatewayDeletar = Substitute.For<ILogGateway<DeletarEstoqueHandler>>();
            var handler = new DeletarEstoqueHandler(_estoqueGateway, logGatewayDeletar, _udtGateway, _usuarioLogadoGateway);
            var estoqueId = Guid.NewGuid();
            var estoque = new Estoque
            {
                Id = estoqueId,
                Insumo = "Insumo Obsoleto",
                Descricao = "Descontinuado",
                Preco = 10.00m,
                QuantidadeDisponivel = 0,
                QuantidadeMinima = 0
            };

            _estoqueGateway.ObterPorIdAsync(estoqueId).Returns(Task.FromResult<Estoque?>(estoque));
            _estoqueGateway.DeletarAsync(Arg.Any<Estoque>()).Returns(Task.CompletedTask);

            // Act
            var resultado = await handler.Handle(estoqueId);

            // Assert
            resultado.Should().BeTrue();
            await _estoqueGateway.Received(1).DeletarAsync(Arg.Any<Estoque>());
            await _udtGateway.Received(1).Commit();
        }

        /// <summary>
        /// Verifica se DeletarEstoque lança exceção para estoque inexistente
        /// Importância: ALTA - Previne exclusão de registros inválidos
        /// Contribuição: Garante integridade
        /// </summary>
        [Fact]
        public async Task DeletarEstoque_ComEstoqueInexistente_DeveLancarExcecao()
        {
            // Arrange
            var logGatewayDeletar = Substitute.For<ILogGateway<DeletarEstoqueHandler>>();
            var handler = new DeletarEstoqueHandler(_estoqueGateway, logGatewayDeletar, _udtGateway, _usuarioLogadoGateway);
            var estoqueId = Guid.NewGuid();

            _estoqueGateway.ObterPorIdAsync(estoqueId).Returns(Task.FromResult<Estoque?>(null));

            // Act & Assert
            await handler.Invoking(h => h.Handle(estoqueId))
                .Should().ThrowAsync<DadosNaoEncontradosException>()
                .WithMessage("Estoque não encontrado");
        }

        #endregion

        #region ObterEstoqueHandler

        /// <summary>
        /// Verifica se ObterEstoque retorna insumo existente
        /// Importância: ALTA - Consulta de estoque é operação frequente
        /// Contribuição: Permite verificar disponibilidade de insumos
        /// </summary>
        [Fact]
        public async Task ObterEstoque_ComIdExistente_DeveRetornarEstoque()
        {
            // Arrange
            var logGatewayObter = Substitute.For<ILogGateway<ObterEstoqueHandler>>();
            var handler = new ObterEstoqueHandler(_estoqueGateway, logGatewayObter, _udtGateway, _usuarioLogadoGateway);
            var estoqueId = Guid.NewGuid();
            var estoque = new Estoque
            {
                Id = estoqueId,
                Insumo = "Óleo 5W30",
                Descricao = "Óleo sintético",
                Preco = 50.00m,
                QuantidadeDisponivel = 100,
                QuantidadeMinima = 10
            };

            _estoqueGateway.ObterPorIdAsync(estoqueId).Returns(Task.FromResult<Estoque?>(estoque));

            // Act
            var resultado = await handler.Handle(estoqueId);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(estoqueId);
            resultado.Insumo.Should().Be("Óleo 5W30");
            resultado.QuantidadeDisponivel.Should().Be(100);
        }

        /// <summary>
        /// Verifica se ObterEstoque lança exceção para ID inexistente
        /// Importância: MÉDIA - Comportamento esperado
        /// Contribuição: Permite tratamento adequado de estoque não encontrado
        /// </summary>
        [Fact]
        public async Task ObterEstoque_ComIdInexistente_DeveLancarExcecao()
        {
            // Arrange
            var logGatewayObter = Substitute.For<ILogGateway<ObterEstoqueHandler>>();
            var handler = new ObterEstoqueHandler(_estoqueGateway, logGatewayObter, _udtGateway, _usuarioLogadoGateway);
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
        /// Verifica se ObterTodosEstoques retorna lista completa
        /// Importância: ALTA - Listagem de estoque é operação comum
        /// Contribuição: Permite visualização completa do inventário
        /// </summary>
        [Fact]
        public async Task ObterTodosEstoques_ComVariosEstoques_DeveRetornarListaCompleta()
        {
            // Arrange
            var logGatewayObterTodos = Substitute.For<ILogGateway<ObterTodosEstoquesHandler>>();
            var handler = new ObterTodosEstoquesHandler(_estoqueGateway, logGatewayObterTodos, _udtGateway, _usuarioLogadoGateway);
            var estoques = new List<Estoque>
            {
                new Estoque { Id = Guid.NewGuid(), Insumo = "Óleo 5W30", Descricao = "Óleo", Preco = 50m, QuantidadeDisponivel = 100, QuantidadeMinima = 10 },
                new Estoque { Id = Guid.NewGuid(), Insumo = "Filtro", Descricao = "Filtro de óleo", Preco = 25m, QuantidadeDisponivel = 50, QuantidadeMinima = 5 },
                new Estoque { Id = Guid.NewGuid(), Insumo = "Vela", Descricao = "Vela de ignição", Preco = 15m, QuantidadeDisponivel = 200, QuantidadeMinima = 20 }
            };

            _estoqueGateway.ObterTodosAsync().Returns(Task.FromResult<IEnumerable<Estoque>>(estoques));

            // Act
            var resultado = await handler.Handle();

            // Assert
            resultado.Should().HaveCount(3);
            resultado.Should().Contain(e => e.Insumo == "Óleo 5W30");
            resultado.Should().Contain(e => e.Insumo == "Filtro");
            resultado.Should().Contain(e => e.Insumo == "Vela");
        }

        /// <summary>
        /// Verifica se ObterTodosEstoques retorna lista vazia quando não há estoque
        /// Importância: MÉDIA - Comportamento esperado sem dados
        /// Contribuição: Previne erros em telas de listagem
        /// </summary>
        [Fact]
        public async Task ObterTodosEstoques_SemEstoques_DeveRetornarListaVazia()
        {
            // Arrange
            var logGatewayObterTodos = Substitute.For<ILogGateway<ObterTodosEstoquesHandler>>();
            var handler = new ObterTodosEstoquesHandler(_estoqueGateway, logGatewayObterTodos, _udtGateway, _usuarioLogadoGateway);

            _estoqueGateway.ObterTodosAsync().Returns(Task.FromResult<IEnumerable<Estoque>>(new List<Estoque>()));

            // Act
            var resultado = await handler.Handle();

            // Assert
            resultado.Should().BeEmpty();
        }

        #endregion

        #region ObterEstoqueCriticoHandler

        /// <summary>
        /// Verifica se ObterEstoqueCritico retorna apenas insumos abaixo do mínimo
        /// Importância: CRÍTICA - Alertas de estoque baixo são essenciais
        /// Contribuição: Previne falta de insumos e paradas na operação
        /// </summary>
        [Fact]
        public async Task ObterEstoqueCritico_ComEstoquesAbaixoDoMinimo_DeveRetornarApenasOsCriticos()
        {
            // Arrange
            var logGatewayObterCritico = Substitute.For<ILogGateway<ObterEstoqueCriticoHandler>>();
            var handler = new ObterEstoqueCriticoHandler(_estoqueGateway, logGatewayObterCritico, _udtGateway, _usuarioLogadoGateway);
            var estoquesCriticos = new List<Estoque>
            {
                new Estoque { Id = Guid.NewGuid(), Insumo = "Óleo 5W30", Descricao = "Óleo", Preco = 50m, QuantidadeDisponivel = 5, QuantidadeMinima = 10 },
                new Estoque { Id = Guid.NewGuid(), Insumo = "Filtro", Descricao = "Filtro", Preco = 25m, QuantidadeDisponivel = 2, QuantidadeMinima = 5 }
            };

            _estoqueGateway.ObterEstoqueCriticoAsync().Returns(Task.FromResult<IEnumerable<Estoque>>(estoquesCriticos));

            // Act
            var resultado = await handler.Handle();

            // Assert
            resultado.Should().HaveCount(2);
            resultado.Should().AllSatisfy(e => e.QuantidadeDisponivel.Should().BeLessThan(e.QuantidadeMinima));
        }

        /// <summary>
        /// Verifica se ObterEstoqueCritico retorna vazio quando não há críticos
        /// Importância: MÉDIA - Comportamento esperado quando estoque está OK
        /// Contribuição: Permite identificar quando não há alertas
        /// </summary>
        [Fact]
        public async Task ObterEstoqueCritico_SemEstoquesCriticos_DeveRetornarListaVazia()
        {
            // Arrange
            var logGatewayObterCritico = Substitute.For<ILogGateway<ObterEstoqueCriticoHandler>>();
            var handler = new ObterEstoqueCriticoHandler(_estoqueGateway, logGatewayObterCritico, _udtGateway, _usuarioLogadoGateway);

            _estoqueGateway.ObterEstoqueCriticoAsync().Returns(Task.FromResult<IEnumerable<Estoque>>(new List<Estoque>()));

            // Act
            var resultado = await handler.Handle();

            // Assert
            resultado.Should().BeEmpty();
        }

        #endregion
    }
}
