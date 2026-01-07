using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.UseCases.Estoques.DeletarEstoque;

namespace MecanicaOS.UnitTests.Core.UseCases.Estoques.DeletarEstoque
{
    /// <summary>
    /// Testes para DeletarEstoqueHandler
    /// ROI MÉDIO: Deleção de estoque é operação menos frequente mas necessária.
    /// Importância: Valida remoção segura de produtos e tratamento de erros.
    /// </summary>
    public class DeletarEstoqueHandlerTests
    {
        private readonly IEstoqueGateway _estoqueGateway;
        private readonly ILogGateway<DeletarEstoqueHandler> _logGateway;
        private readonly IUnidadeDeTrabalhoGateway _udtGateway;
        private readonly IUsuarioLogadoServicoGateway _usuarioLogadoGateway;

        public DeletarEstoqueHandlerTests()
        {
            _estoqueGateway = Substitute.For<IEstoqueGateway>();
            _logGateway = Substitute.For<ILogGateway<DeletarEstoqueHandler>>();
            _udtGateway = Substitute.For<IUnidadeDeTrabalhoGateway>();
            _usuarioLogadoGateway = Substitute.For<IUsuarioLogadoServicoGateway>();
            _udtGateway.Commit().Returns(Task.FromResult(true));
        }

        /// <summary>
        /// Verifica se deleta estoque com sucesso
        /// Importância: ALTA - Operação principal do handler
        /// Contribuição: Garante que produtos são removidos corretamente
        /// </summary>
        [Fact]
        public async Task Handle_ComEstoqueExistente_DeveDeletarComSucesso()
        {
            // Arrange
            var handler = new DeletarEstoqueHandler(_estoqueGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var estoqueId = Guid.NewGuid();
            var estoqueExistente = new Estoque
            {
                Id = estoqueId,
                Insumo = "Produto a deletar",
                Descricao = "Teste",
                Preco = 50.00m
            };

            _estoqueGateway.ObterPorIdAsync(estoqueId).Returns(Task.FromResult<Estoque?>(estoqueExistente));
            _estoqueGateway.DeletarAsync(Arg.Any<Estoque>()).Returns(Task.CompletedTask);

            // Act
            var resultado = await handler.Handle(estoqueId);

            // Assert
            resultado.Should().BeTrue();
            await _estoqueGateway.Received(1).DeletarAsync(Arg.Any<Estoque>());
            await _udtGateway.Received(1).Commit();
        }

        /// <summary>
        /// Verifica se lança exceção quando estoque não existe
        /// Importância: ALTA - Validação de existência obrigatória
        /// Contribuição: Previne tentativa de deletar dados inexistentes
        /// </summary>
        [Fact]
        public async Task Handle_ComEstoqueInexistente_DeveLancarDadosNaoEncontradosException()
        {
            // Arrange
            var handler = new DeletarEstoqueHandler(_estoqueGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var estoqueId = Guid.NewGuid();

            _estoqueGateway.ObterPorIdAsync(estoqueId).Returns(Task.FromResult<Estoque?>(null));

            // Act & Assert
            await handler.Invoking(h => h.Handle(estoqueId))
                .Should().ThrowAsync<DadosNaoEncontradosException>()
                .WithMessage("Estoque não encontrado");
        }

        /// <summary>
        /// Verifica se lança exceção quando commit falha
        /// Importância: ALTA - Garantia de persistência
        /// Contribuição: Previne inconsistência de dados
        /// </summary>
        [Fact]
        public async Task Handle_QuandoCommitFalha_DeveLancarPersistirDadosException()
        {
            // Arrange
            var handler = new DeletarEstoqueHandler(_estoqueGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var estoqueId = Guid.NewGuid();
            var estoqueExistente = new Estoque
            {
                Id = estoqueId,
                Insumo = "Produto",
                Preco = 50.00m
            };

            _estoqueGateway.ObterPorIdAsync(estoqueId).Returns(Task.FromResult<Estoque?>(estoqueExistente));
            _estoqueGateway.DeletarAsync(Arg.Any<Estoque>()).Returns(Task.CompletedTask);
            _udtGateway.Commit().Returns(Task.FromResult(false));

            // Act & Assert
            await handler.Invoking(h => h.Handle(estoqueId))
                .Should().ThrowAsync<PersistirDadosException>()
                .WithMessage("Erro ao deletar estoque");
        }

        /// <summary>
        /// Verifica se deleta estoque mesmo com quantidade disponível
        /// Importância: MÉDIA - Validação de regra de negócio
        /// Contribuição: Garante que deleção não é bloqueada por quantidade
        /// </summary>
        [Fact]
        public async Task Handle_ComQuantidadeDisponivel_DevePermitirDeletar()
        {
            // Arrange
            var handler = new DeletarEstoqueHandler(_estoqueGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var estoqueId = Guid.NewGuid();
            var estoqueExistente = new Estoque
            {
                Id = estoqueId,
                Insumo = "Produto com estoque",
                QuantidadeDisponivel = 100,
                Preco = 50.00m
            };

            _estoqueGateway.ObterPorIdAsync(estoqueId).Returns(Task.FromResult<Estoque?>(estoqueExistente));
            _estoqueGateway.DeletarAsync(Arg.Any<Estoque>()).Returns(Task.CompletedTask);

            // Act
            var resultado = await handler.Handle(estoqueId);

            // Assert
            resultado.Should().BeTrue();
        }
    }
}
