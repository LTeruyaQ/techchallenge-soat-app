using Core.DTOs.UseCases.Estoque;
using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.UseCases.Estoques.AtualizarEstoque;

namespace MecanicaOS.UnitTests.Core.UseCases.Estoques.AtualizarEstoque
{
    /// <summary>
    /// Testes para AtualizarEstoqueHandler
    /// ROI ALTO: Atualização de estoque é operação frequente - dados precisam estar corretos.
    /// Importância: Valida atualização parcial de campos e manutenção de integridade.
    /// </summary>
    public class AtualizarEstoqueHandlerTests
    {
        private readonly IEstoqueGateway _estoqueGateway;
        private readonly ILogGateway<AtualizarEstoqueHandler> _logGateway;
        private readonly IUnidadeDeTrabalhoGateway _udtGateway;
        private readonly IUsuarioLogadoServicoGateway _usuarioLogadoGateway;

        public AtualizarEstoqueHandlerTests()
        {
            _estoqueGateway = Substitute.For<IEstoqueGateway>();
            _logGateway = Substitute.For<ILogGateway<AtualizarEstoqueHandler>>();
            _udtGateway = Substitute.For<IUnidadeDeTrabalhoGateway>();
            _usuarioLogadoGateway = Substitute.For<IUsuarioLogadoServicoGateway>();
            _udtGateway.Commit().Returns(Task.FromResult(true));
        }

        /// <summary>
        /// Verifica se atualiza estoque com todos os campos
        /// Importância: ALTA - Operação principal do handler
        /// Contribuição: Garante que todos os campos são atualizados corretamente
        /// </summary>
        [Fact]
        public async Task Handle_ComTodosCampos_DeveAtualizarEstoque()
        {
            // Arrange
            var handler = new AtualizarEstoqueHandler(_estoqueGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var estoqueId = Guid.NewGuid();
            var estoqueExistente = new Estoque
            {
                Id = estoqueId,
                Insumo = "Óleo Antigo",
                Descricao = "Descrição antiga",
                QuantidadeDisponivel = 50,
                QuantidadeMinima = 10,
                Preco = 40.00m
            };
            var request = new AtualizarEstoqueUseCaseDto
            {
                Insumo = "Óleo Novo",
                Descricao = "Descrição nova",
                QuantidadeDisponivel = 100,
                QuantidadeMinima = 20,
                Preco = 45.90m
            };

            _estoqueGateway.ObterPorIdAsync(estoqueId).Returns(Task.FromResult<Estoque?>(estoqueExistente));
            _estoqueGateway.EditarAsync(Arg.Any<Estoque>()).Returns(Task.CompletedTask);

            // Act
            var resultado = await handler.Handle(estoqueId, request);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Insumo.Should().Be("Óleo Novo");
            resultado.Descricao.Should().Be("Descrição nova");
            resultado.QuantidadeDisponivel.Should().Be(100);
            resultado.QuantidadeMinima.Should().Be(20);
            resultado.Preco.Should().Be(45.90m);
            await _estoqueGateway.Received(1).EditarAsync(Arg.Any<Estoque>());
            await _udtGateway.Received(1).Commit();
        }

        /// <summary>
        /// Verifica se atualiza apenas campos informados (atualização parcial)
        /// Importância: CRÍTICA - Permite atualização seletiva
        /// Contribuição: Garante que campos não informados não são alterados
        /// </summary>
        [Fact]
        public async Task Handle_ApenasComPreco_DeveAtualizarApenasPreco()
        {
            // Arrange
            var handler = new AtualizarEstoqueHandler(_estoqueGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var estoqueId = Guid.NewGuid();
            var estoqueExistente = new Estoque
            {
                Id = estoqueId,
                Insumo = "Óleo Original",
                Descricao = "Descrição original",
                QuantidadeDisponivel = 50,
                QuantidadeMinima = 10,
                Preco = 40.00m
            };
            var request = new AtualizarEstoqueUseCaseDto
            {
                Preco = 50.00m
            };

            _estoqueGateway.ObterPorIdAsync(estoqueId).Returns(Task.FromResult<Estoque?>(estoqueExistente));
            _estoqueGateway.EditarAsync(Arg.Any<Estoque>()).Returns(Task.CompletedTask);

            // Act
            var resultado = await handler.Handle(estoqueId, request);

            // Assert
            resultado.Insumo.Should().Be("Óleo Original");
            resultado.Descricao.Should().Be("Descrição original");
            resultado.QuantidadeDisponivel.Should().Be(50);
            resultado.QuantidadeMinima.Should().Be(10);
            resultado.Preco.Should().Be(50.00m);
        }

        /// <summary>
        /// Verifica se lança exceção quando estoque não existe
        /// Importância: ALTA - Validação de existência obrigatória
        /// Contribuição: Previne atualização de dados inexistentes
        /// </summary>
        [Fact]
        public async Task Handle_ComEstoqueInexistente_DeveLancarDadosNaoEncontradosException()
        {
            // Arrange
            var handler = new AtualizarEstoqueHandler(_estoqueGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var estoqueId = Guid.NewGuid();
            var request = new AtualizarEstoqueUseCaseDto
            {
                Preco = 50.00m
            };

            _estoqueGateway.ObterPorIdAsync(estoqueId).Returns(Task.FromResult<Estoque?>(null));

            // Act & Assert
            await handler.Invoking(h => h.Handle(estoqueId, request))
                .Should().ThrowAsync<DadosNaoEncontradosException>()
                .WithMessage("Estoque não encontrado");
        }

        /// <summary>
        /// Verifica se atualiza DataAtualizacao automaticamente
        /// Importância: ALTA - Auditoria de mudanças
        /// Contribuição: Garante rastreabilidade de alterações
        /// </summary>
        [Fact]
        public async Task Handle_QuandoAtualiza_DeveAtualizarDataAtualizacao()
        {
            // Arrange
            var handler = new AtualizarEstoqueHandler(_estoqueGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var estoqueId = Guid.NewGuid();
            var dataAnterior = DateTime.UtcNow.AddDays(-1);
            var estoqueExistente = new Estoque
            {
                Id = estoqueId,
                Insumo = "Produto",
                DataAtualizacao = dataAnterior,
                Preco = 40.00m
            };
            var request = new AtualizarEstoqueUseCaseDto
            {
                Preco = 45.00m
            };

            _estoqueGateway.ObterPorIdAsync(estoqueId).Returns(Task.FromResult<Estoque?>(estoqueExistente));
            _estoqueGateway.EditarAsync(Arg.Any<Estoque>()).Returns(Task.CompletedTask);

            // Act
            var resultado = await handler.Handle(estoqueId, request);

            // Assert
            resultado.DataAtualizacao.Should().BeAfter(dataAnterior);
        }

        /// <summary>
        /// Verifica se lança exceção quando commit falha
        /// Importância: ALTA - Garantia de persistência
        /// Contribuição: Previne perda de dados
        /// </summary>
        [Fact]
        public async Task Handle_QuandoCommitFalha_DeveLancarPersistirDadosException()
        {
            // Arrange
            var handler = new AtualizarEstoqueHandler(_estoqueGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var estoqueId = Guid.NewGuid();
            var estoqueExistente = new Estoque
            {
                Id = estoqueId,
                Insumo = "Produto",
                Preco = 40.00m
            };
            var request = new AtualizarEstoqueUseCaseDto
            {
                Preco = 45.00m
            };

            _estoqueGateway.ObterPorIdAsync(estoqueId).Returns(Task.FromResult<Estoque?>(estoqueExistente));
            _estoqueGateway.EditarAsync(Arg.Any<Estoque>()).Returns(Task.CompletedTask);
            _udtGateway.Commit().Returns(Task.FromResult(false));

            // Act & Assert
            await handler.Invoking(h => h.Handle(estoqueId, request))
                .Should().ThrowAsync<PersistirDadosException>()
                .WithMessage("Erro ao atualizar estoque");
        }

        /// <summary>
        /// Verifica se atualiza quantidade disponível corretamente
        /// Importância: CRÍTICA - Controle de estoque preciso
        /// Contribuição: Garante que movimentações de estoque são registradas
        /// </summary>
        [Fact]
        public async Task Handle_AtualizandoQuantidadeDisponivel_DeveAtualizarCorretamente()
        {
            // Arrange
            var handler = new AtualizarEstoqueHandler(_estoqueGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var estoqueId = Guid.NewGuid();
            var estoqueExistente = new Estoque
            {
                Id = estoqueId,
                Insumo = "Produto",
                QuantidadeDisponivel = 100,
                Preco = 40.00m
            };
            var request = new AtualizarEstoqueUseCaseDto
            {
                QuantidadeDisponivel = 85
            };

            _estoqueGateway.ObterPorIdAsync(estoqueId).Returns(Task.FromResult<Estoque?>(estoqueExistente));
            _estoqueGateway.EditarAsync(Arg.Any<Estoque>()).Returns(Task.CompletedTask);

            // Act
            var resultado = await handler.Handle(estoqueId, request);

            // Assert
            resultado.QuantidadeDisponivel.Should().Be(85);
        }

        /// <summary>
        /// Verifica se permite atualizar quantidade para zero
        /// Importância: MÉDIA - Edge case importante
        /// Contribuição: Garante flexibilidade no controle de estoque
        /// </summary>
        [Fact]
        public async Task Handle_AtualizandoQuantidadeParaZero_DevePermitir()
        {
            // Arrange
            var handler = new AtualizarEstoqueHandler(_estoqueGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var estoqueId = Guid.NewGuid();
            var estoqueExistente = new Estoque
            {
                Id = estoqueId,
                Insumo = "Produto",
                QuantidadeDisponivel = 10,
                Preco = 40.00m
            };
            var request = new AtualizarEstoqueUseCaseDto
            {
                QuantidadeDisponivel = 0
            };

            _estoqueGateway.ObterPorIdAsync(estoqueId).Returns(Task.FromResult<Estoque?>(estoqueExistente));
            _estoqueGateway.EditarAsync(Arg.Any<Estoque>()).Returns(Task.CompletedTask);

            // Act
            var resultado = await handler.Handle(estoqueId, request);

            // Assert
            resultado.QuantidadeDisponivel.Should().Be(0);
        }
    }
}
