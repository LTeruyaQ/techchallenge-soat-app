using Core.DTOs.UseCases.Estoque;
using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.UseCases.Estoques.CadastrarEstoque;

namespace MecanicaOS.UnitTests.Core.UseCases.Estoques.CadastrarEstoque
{
    /// <summary>
    /// Testes para CadastrarEstoqueHandler
    /// ROI CRÍTICO: Gestão de estoque é fundamental para controle de custos e disponibilidade.
    /// Importância: Valida cadastro de produtos, prevenção de duplicatas e integridade de dados.
    /// </summary>
    public class CadastrarEstoqueHandlerTests
    {
        private readonly IEstoqueGateway _estoqueGateway;
        private readonly ILogGateway<CadastrarEstoqueHandler> _logGateway;
        private readonly IUnidadeDeTrabalhoGateway _udtGateway;
        private readonly IUsuarioLogadoServicoGateway _usuarioLogadoGateway;

        public CadastrarEstoqueHandlerTests()
        {
            _estoqueGateway = Substitute.For<IEstoqueGateway>();
            _logGateway = Substitute.For<ILogGateway<CadastrarEstoqueHandler>>();
            _udtGateway = Substitute.For<IUnidadeDeTrabalhoGateway>();
            _usuarioLogadoGateway = Substitute.For<IUsuarioLogadoServicoGateway>();
            _udtGateway.Commit().Returns(Task.FromResult(true));
        }

        /// <summary>
        /// Verifica se cadastra estoque com dados válidos
        /// Importância: CRÍTICA - Operação principal do handler
        /// Contribuição: Garante que produtos são cadastrados corretamente
        /// </summary>
        [Fact]
        public async Task Handle_ComDadosValidos_DeveCadastrarEstoque()
        {
            // Arrange
            var handler = new CadastrarEstoqueHandler(_estoqueGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var request = new CadastrarEstoqueUseCaseDto
            {
                Insumo = "Óleo 5W30",
                Descricao = "Óleo sintético para motor",
                QuantidadeDisponivel = 100,
                QuantidadeMinima = 20,
                Preco = 45.90m
            };

            _estoqueGateway.ObterTodosAsync().Returns(Task.FromResult<IEnumerable<Estoque>>(new List<Estoque>()));
            _estoqueGateway.CadastrarAsync(Arg.Any<Estoque>()).Returns(Task.CompletedTask);

            // Act
            var resultado = await handler.Handle(request);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Insumo.Should().Be("Óleo 5W30");
            resultado.Descricao.Should().Be("Óleo sintético para motor");
            resultado.QuantidadeDisponivel.Should().Be(100);
            resultado.QuantidadeMinima.Should().Be(20);
            resultado.Preco.Should().Be(45.90m);
            await _estoqueGateway.Received(1).CadastrarAsync(Arg.Any<Estoque>());
            await _udtGateway.Received(1).Commit();
        }

        /// <summary>
        /// Verifica se rejeita cadastro de produto duplicado
        /// Importância: CRÍTICA - Previne duplicação de produtos
        /// Contribuição: Garante unicidade de produtos no estoque
        /// </summary>
        [Fact]
        public async Task Handle_ComProdutoJaCadastrado_DeveLancarDadosJaCadastradosException()
        {
            // Arrange
            var handler = new CadastrarEstoqueHandler(_estoqueGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var request = new CadastrarEstoqueUseCaseDto
            {
                Insumo = "Óleo 5W30",
                Descricao = "Óleo sintético",
                QuantidadeDisponivel = 50,
                QuantidadeMinima = 10,
                Preco = 45.90m
            };

            var estoquesExistentes = new List<Estoque>
            {
                new Estoque { Insumo = "Óleo 5W30", Descricao = "Já existe", Preco = 40.00m }
            };

            _estoqueGateway.ObterTodosAsync().Returns(Task.FromResult<IEnumerable<Estoque>>(estoquesExistentes));

            // Act & Assert
            await handler.Invoking(h => h.Handle(request))
                .Should().ThrowAsync<DadosJaCadastradosException>()
                .WithMessage("Produto já cadastrado");
        }

        /// <summary>
        /// Verifica se validação de duplicata é case-insensitive
        /// Importância: ALTA - Previne duplicatas com diferenças de capitalização
        /// Contribuição: Garante consistência no cadastro
        /// </summary>
        [Theory]
        [InlineData("óleo 5w30")]
        [InlineData("ÓLEO 5W30")]
        [InlineData("Óleo 5w30")]
        public async Task Handle_ComProdutoJaCadastradoDiferenteCase_DeveLancarException(string nomeVariacao)
        {
            // Arrange
            var handler = new CadastrarEstoqueHandler(_estoqueGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var request = new CadastrarEstoqueUseCaseDto
            {
                Insumo = nomeVariacao,
                Descricao = "Teste",
                QuantidadeDisponivel = 50,
                QuantidadeMinima = 10,
                Preco = 45.90m
            };

            var estoquesExistentes = new List<Estoque>
            {
                new Estoque { Insumo = "Óleo 5W30", Descricao = "Original", Preco = 40.00m }
            };

            _estoqueGateway.ObterTodosAsync().Returns(Task.FromResult<IEnumerable<Estoque>>(estoquesExistentes));

            // Act & Assert
            await handler.Invoking(h => h.Handle(request))
                .Should().ThrowAsync<DadosJaCadastradosException>();
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
            var handler = new CadastrarEstoqueHandler(_estoqueGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var request = new CadastrarEstoqueUseCaseDto
            {
                Insumo = "Filtro de Óleo",
                Descricao = "Filtro",
                QuantidadeDisponivel = 30,
                QuantidadeMinima = 5,
                Preco = 25.00m
            };

            _estoqueGateway.ObterTodosAsync().Returns(Task.FromResult<IEnumerable<Estoque>>(new List<Estoque>()));
            _estoqueGateway.CadastrarAsync(Arg.Any<Estoque>()).Returns(Task.CompletedTask);
            _udtGateway.Commit().Returns(Task.FromResult(false));

            // Act & Assert
            await handler.Invoking(h => h.Handle(request))
                .Should().ThrowAsync<PersistirDadosException>()
                .WithMessage("Erro ao cadastrar estoque");
        }

        /// <summary>
        /// Verifica se cadastra produto com quantidade mínima zero
        /// Importância: MÉDIA - Validação de edge case
        /// Contribuição: Garante flexibilidade no cadastro
        /// </summary>
        [Fact]
        public async Task Handle_ComQuantidadeMinimaZero_DeveCadastrarComSucesso()
        {
            // Arrange
            var handler = new CadastrarEstoqueHandler(_estoqueGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var request = new CadastrarEstoqueUseCaseDto
            {
                Insumo = "Produto Especial",
                Descricao = "Sem estoque mínimo",
                QuantidadeDisponivel = 10,
                QuantidadeMinima = 0,
                Preco = 100.00m
            };

            _estoqueGateway.ObterTodosAsync().Returns(Task.FromResult<IEnumerable<Estoque>>(new List<Estoque>()));
            _estoqueGateway.CadastrarAsync(Arg.Any<Estoque>()).Returns(Task.CompletedTask);

            // Act
            var resultado = await handler.Handle(request);

            // Assert
            resultado.Should().NotBeNull();
            resultado.QuantidadeMinima.Should().Be(0);
        }

        /// <summary>
        /// Verifica se cadastra produto com valores decimais precisos
        /// Importância: ALTA - Precisão financeira é crítica
        /// Contribuição: Garante que não há perda de precisão em valores
        /// </summary>
        [Fact]
        public async Task Handle_ComValoresDecimais_DeveManterPrecisao()
        {
            // Arrange
            var handler = new CadastrarEstoqueHandler(_estoqueGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var request = new CadastrarEstoqueUseCaseDto
            {
                Insumo = "Produto Decimal",
                Descricao = "Teste de precisão",
                QuantidadeDisponivel = 15,
                QuantidadeMinima = 3,
                Preco = 99.99m
            };

            _estoqueGateway.ObterTodosAsync().Returns(Task.FromResult<IEnumerable<Estoque>>(new List<Estoque>()));
            _estoqueGateway.CadastrarAsync(Arg.Any<Estoque>()).Returns(Task.CompletedTask);

            // Act
            var resultado = await handler.Handle(request);

            // Assert
            resultado.Preco.Should().Be(99.99m);
        }

        /// <summary>
        /// Verifica se cadastra múltiplos produtos diferentes
        /// Importância: ALTA - Validação de independência entre cadastros
        /// Contribuição: Garante que produtos diferentes podem ser cadastrados
        /// </summary>
        [Fact]
        public async Task Handle_ComProdutosDiferentes_DeveCadastrarAmbos()
        {
            // Arrange
            var handler = new CadastrarEstoqueHandler(_estoqueGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            
            var request1 = new CadastrarEstoqueUseCaseDto
            {
                Insumo = "Produto A",
                Descricao = "Descrição A",
                QuantidadeDisponivel = 10,
                QuantidadeMinima = 2,
                Preco = 50.00m
            };

            var request2 = new CadastrarEstoqueUseCaseDto
            {
                Insumo = "Produto B",
                Descricao = "Descrição B",
                QuantidadeDisponivel = 20,
                QuantidadeMinima = 5,
                Preco = 75.00m
            };

            _estoqueGateway.ObterTodosAsync().Returns(
                Task.FromResult<IEnumerable<Estoque>>(new List<Estoque>()),
                Task.FromResult<IEnumerable<Estoque>>(new List<Estoque> 
                { 
                    new Estoque { Insumo = "Produto A" } 
                })
            );
            _estoqueGateway.CadastrarAsync(Arg.Any<Estoque>()).Returns(Task.CompletedTask);

            // Act
            var resultado1 = await handler.Handle(request1);
            var resultado2 = await handler.Handle(request2);

            // Assert
            resultado1.Insumo.Should().Be("Produto A");
            resultado2.Insumo.Should().Be("Produto B");
            await _estoqueGateway.Received(2).CadastrarAsync(Arg.Any<Estoque>());
        }
    }
}
