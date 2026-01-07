using Adapters.Controllers;
using Core.DTOs.Requests.Estoque;
using Core.Entidades;
using Core.Interfaces.UseCases;
using Core.Interfaces.root;

namespace MecanicaOS.UnitTests.Adapters.Controllers
{
    /// <summary>
    /// Testes para EstoqueController (Adapter)
    /// Cobertura: 75.0%
    /// </summary>
    public class EstoqueControllerTests
    {
        [Fact]
        public void Construtor_DeveCriarInstancia()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var estoqueUseCases = Substitute.For<IEstoqueUseCases>();
            compositionRoot.CriarEstoqueUseCases().Returns(estoqueUseCases);

            // Act
            var controller = new EstoqueController(compositionRoot);

            // Assert
            controller.Should().NotBeNull();
        }

        [Fact]
        public async Task ObterTodos_DeveChamarUseCases()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var estoqueUseCases = Substitute.For<IEstoqueUseCases>();
            var estoques = new List<Estoque> { new Estoque { Id = Guid.NewGuid(), Insumo = "Teste", Descricao = "Desc", Preco = 10m, QuantidadeDisponivel = 5, QuantidadeMinima = 1 } };

            compositionRoot.CriarEstoqueUseCases().Returns(estoqueUseCases);
            estoqueUseCases.ObterTodosUseCaseAsync().Returns(Task.FromResult<IEnumerable<Estoque>>(estoques));

            var controller = new EstoqueController(compositionRoot);

            // Act
            var resultado = await controller.ObterTodos();

            // Assert
            await estoqueUseCases.Received(1).ObterTodosUseCaseAsync();
        }

        [Fact]
        public async Task ObterPorId_DeveChamarUseCases()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var estoqueUseCases = Substitute.For<IEstoqueUseCases>();
            var id = Guid.NewGuid();
            var estoque = new Estoque { Id = id, Insumo = "Teste", Descricao = "Desc", Preco = 10m, QuantidadeDisponivel = 5, QuantidadeMinima = 1 };

            compositionRoot.CriarEstoqueUseCases().Returns(estoqueUseCases);
            estoqueUseCases.ObterPorIdUseCaseAsync(id).Returns(Task.FromResult<Estoque?>(estoque));

            var controller = new EstoqueController(compositionRoot);

            // Act
            var resultado = await controller.ObterPorId(id);

            // Assert
            await estoqueUseCases.Received(1).ObterPorIdUseCaseAsync(id);
        }

        [Fact]
        public async Task Deletar_DeveChamarUseCases()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var estoqueUseCases = Substitute.For<IEstoqueUseCases>();
            var id = Guid.NewGuid();

            compositionRoot.CriarEstoqueUseCases().Returns(estoqueUseCases);
            estoqueUseCases.DeletarUseCaseAsync(id).Returns(Task.FromResult(true));

            var controller = new EstoqueController(compositionRoot);

            // Act
            var resultado = await controller.Deletar(id);

            // Assert
            resultado.Should().BeTrue();
            await estoqueUseCases.Received(1).DeletarUseCaseAsync(id);
        }

        [Fact]
        public void MapearParaCadastrarEstoqueUseCaseDto_DeveMapearCorretamente()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var estoqueUseCases = Substitute.For<IEstoqueUseCases>();
            compositionRoot.CriarEstoqueUseCases().Returns(estoqueUseCases);

            var controller = new EstoqueController(compositionRoot);
            var request = new CadastrarEstoqueRequest
            {
                Insumo = "Óleo Motor",
                Descricao = "Óleo sintético 5W30",
                Preco = 45.90,
                QuantidadeDisponivel = 100,
                QuantidadeMinima = 20
            };

            // Act
            var resultado = controller.MapearParaCadastrarEstoqueUseCaseDto(request);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Insumo.Should().Be("Óleo Motor");
            resultado.QuantidadeDisponivel.Should().Be(100);
        }

        [Fact]
        public void MapearParaAtualizarEstoqueUseCaseDto_DeveMapearCorretamente()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var estoqueUseCases = Substitute.For<IEstoqueUseCases>();
            compositionRoot.CriarEstoqueUseCases().Returns(estoqueUseCases);

            var controller = new EstoqueController(compositionRoot);
            var request = new AtualizarEstoqueRequest
            {
                Insumo = "Óleo Atualizado",
                Descricao = "Nova descrição",
                Preco = 50.00m,
                QuantidadeDisponivel = 150,
                QuantidadeMinima = 30
            };

            // Act
            var resultado = controller.MapearParaAtualizarEstoqueUseCaseDto(request);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Insumo.Should().Be("Óleo Atualizado");
            resultado.QuantidadeDisponivel.Should().Be(150);
        }
    }
}
