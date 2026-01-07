using API.Controllers;
using Core.DTOs.Requests.Estoque;
using Core.DTOs.Responses.Estoque;
using Core.Interfaces.Controllers;
using Core.Interfaces.root;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace MecanicaOS.UnitTests.API.Controllers
{
    /// <summary>
    /// Testes para EstoqueController (API)
    /// 
    /// IMPORTÂNCIA: Controller crítico para gestão de estoque de insumos.
    /// Essencial para controle de disponibilidade e reposição de materiais.
    /// 
    /// COBERTURA: Valida todos os 5 endpoints HTTP:
    /// - GET /estoque - Listar todos os estoques
    /// - GET /estoque/{id} - Obter estoque por ID
    /// - POST /estoque - Cadastrar novo estoque
    /// - PUT /estoque/{id} - Atualizar estoque
    /// - DELETE /estoque/{id} - Remover estoque
    /// 
    /// REGRAS DE NEGÓCIO:
    /// - Apenas usuários Admin podem acessar
    /// - Validação de ModelState em operações de escrita
    /// - Retorno 201 Created com Location header no cadastro
    /// - Retorno 204 NoContent na remoção bem-sucedida
    /// </summary>
    public class EstoqueApiControllerTests
    {
        private readonly ICompositionRoot _compositionRoot;
        private readonly IEstoqueController _estoqueController;

        public EstoqueApiControllerTests()
        {
            _compositionRoot = Substitute.For<ICompositionRoot>();
            _estoqueController = Substitute.For<IEstoqueController>();
            _compositionRoot.CriarEstoqueController().Returns(_estoqueController);
        }

        private EstoqueController CriarController()
        {
            return new EstoqueController(_compositionRoot);
        }

        [Fact]
        public async Task ObterTodos_DeveRetornarOkComListaDeEstoques()
        {
            // Arrange
            var estoques = new List<EstoqueResponse>
            {
                new EstoqueResponse { Id = Guid.NewGuid(), Insumo = "Óleo 10W-40" },
                new EstoqueResponse { Id = Guid.NewGuid(), Insumo = "Filtro de Óleo" }
            };

            _estoqueController.ObterTodos().Returns(Task.FromResult<IEnumerable<EstoqueResponse>>(estoques));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterTodos();

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            var okResult = resultado as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(estoques);
            await _estoqueController.Received(1).ObterTodos();
        }

        [Fact]
        public async Task ObterTodos_ComListaVazia_DeveRetornarOkComListaVazia()
        {
            // Arrange
            _estoqueController.ObterTodos().Returns(Task.FromResult<IEnumerable<EstoqueResponse>>(new List<EstoqueResponse>()));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterTodos();

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            var okResult = resultado as OkObjectResult;
            var lista = okResult!.Value as IEnumerable<EstoqueResponse>;
            lista.Should().BeEmpty();
        }

        [Fact]
        public async Task ObterPorId_ComIdValido_DeveRetornarOkComEstoque()
        {
            // Arrange
            var id = Guid.NewGuid();
            var estoque = new EstoqueResponse
            {
                Id = id,
                Insumo = "Óleo 10W-40",
                QuantidadeDisponivel = 10,
                QuantidadeMinima = 5
            };

            _estoqueController.ObterPorId(id).Returns(Task.FromResult(estoque));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterPorId(id);

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            var okResult = resultado as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(estoque);
            await _estoqueController.Received(1).ObterPorId(id);
        }

        [Fact]
        public async Task Criar_ComRequestValido_DeveRetornarCreatedAtAction()
        {
            // Arrange
            var request = new CadastrarEstoqueRequest
            {
                Insumo = "Óleo 10W-40",
                QuantidadeDisponivel = 10,
                QuantidadeMinima = 5,
                Preco = 50.00
            };

            var estoqueCriado = new EstoqueResponse
            {
                Id = Guid.NewGuid(),
                Insumo = request.Insumo,
                QuantidadeDisponivel = request.QuantidadeDisponivel,
                QuantidadeMinima = request.QuantidadeMinima,
                Preco = (double)request.Preco!
            };

            _estoqueController.Cadastrar(request).Returns(Task.FromResult(estoqueCriado));
            var controller = CriarController();

            // Act
            var resultado = await controller.Criar(request);

            // Assert
            resultado.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = resultado as CreatedAtActionResult;
            createdResult!.ActionName.Should().Be(nameof(EstoqueController.ObterPorId));
            createdResult.RouteValues!["id"].Should().Be(estoqueCriado.Id);
            createdResult.Value.Should().BeEquivalentTo(estoqueCriado);
            await _estoqueController.Received(1).Cadastrar(request);
        }

        [Fact]
        public async Task Criar_ComModelStateInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            var request = new CadastrarEstoqueRequest();
            var controller = CriarController();
            controller.ModelState.AddModelError("Insumo", "O campo Insumo é obrigatório");

            // Act
            var resultado = await controller.Criar(request);

            // Assert
            resultado.Should().BeOfType<BadRequestObjectResult>();
            await _estoqueController.DidNotReceive().Cadastrar(Arg.Any<CadastrarEstoqueRequest>());
        }

        [Fact]
        public async Task Atualizar_ComRequestValido_DeveRetornarOkComEstoqueAtualizado()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new AtualizarEstoqueRequest
            {
                Insumo = "Óleo 10W-40 Atualizado",
                QuantidadeDisponivel = 15,
                QuantidadeMinima = 8,
                Preco = 55.00m
            };

            var estoqueAtualizado = new EstoqueResponse
            {
                Id = id,
                Insumo = request.Insumo,
                QuantidadeDisponivel = request.QuantidadeDisponivel!.Value,
                QuantidadeMinima = request.QuantidadeMinima!.Value,
                Preco = (double)request.Preco!
            };

            _estoqueController.Atualizar(id, request).Returns(Task.FromResult(estoqueAtualizado));
            var controller = CriarController();

            // Act
            var resultado = await controller.Atualizar(id, request);

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            var okResult = resultado as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(estoqueAtualizado);
            await _estoqueController.Received(1).Atualizar(id, request);
        }

        [Fact]
        public async Task Atualizar_ComModelStateInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new AtualizarEstoqueRequest();
            var controller = CriarController();
            controller.ModelState.AddModelError("Insumo", "O campo Insumo é obrigatório");

            // Act
            var resultado = await controller.Atualizar(id, request);

            // Assert
            resultado.Should().BeOfType<BadRequestObjectResult>();
            await _estoqueController.DidNotReceive().Atualizar(Arg.Any<Guid>(), Arg.Any<AtualizarEstoqueRequest>());
        }

        [Fact]
        public async Task Remover_ComSucesso_DeveRetornarNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            _estoqueController.Deletar(id).Returns(Task.FromResult(true));
            var controller = CriarController();

            // Act
            var resultado = await controller.Remover(id);

            // Assert
            resultado.Should().BeOfType<NoContentResult>();
            await _estoqueController.Received(1).Deletar(id);
        }

        [Fact]
        public async Task Remover_ComFalha_DeveRetornarInternalServerError()
        {
            // Arrange
            var id = Guid.NewGuid();
            _estoqueController.Deletar(id).Returns(Task.FromResult(false));
            var controller = CriarController();

            // Act
            var resultado = await controller.Remover(id);

            // Assert
            resultado.Should().BeOfType<ObjectResult>();
            var objectResult = resultado as ObjectResult;
            objectResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            await _estoqueController.Received(1).Deletar(id);
        }

        [Fact]
        public void Controller_DeveHerdarDeBaseApiController()
        {
            // Arrange & Act
            var controller = CriarController();

            // Assert
            controller.Should().BeAssignableTo<BaseApiController>();
        }

        [Fact]
        public async Task Criar_ComValoresDecimais_DevePreservarPrecisao()
        {
            // Arrange
            var request = new CadastrarEstoqueRequest
            {
                Insumo = "Óleo Premium",
                QuantidadeDisponivel = 5,
                QuantidadeMinima = 2,
                Preco = 99.99
            };

            var estoqueCriado = new EstoqueResponse
            {
                Id = Guid.NewGuid(),
                Preco = (double)request.Preco!,
                Insumo = request.Insumo,
                QuantidadeDisponivel = request.QuantidadeDisponivel,
                QuantidadeMinima = request.QuantidadeMinima
            };

            _estoqueController.Cadastrar(request).Returns(Task.FromResult(estoqueCriado));
            var controller = CriarController();

            // Act
            var resultado = await controller.Criar(request);

            // Assert
            var createdResult = resultado as CreatedAtActionResult;
            var response = createdResult!.Value as EstoqueResponse;
            response!.Preco.Should().Be(99.99);
        }

        [Fact]
        public async Task ObterPorId_DevePassarIdCorreto()
        {
            // Arrange
            var id = Guid.NewGuid();
            var estoque = new EstoqueResponse { Id = id };
            _estoqueController.ObterPorId(id).Returns(Task.FromResult(estoque));
            var controller = CriarController();

            // Act
            await controller.ObterPorId(id);

            // Assert
            await _estoqueController.Received(1).ObterPorId(Arg.Is<Guid>(g => g == id));
        }
    }
}
