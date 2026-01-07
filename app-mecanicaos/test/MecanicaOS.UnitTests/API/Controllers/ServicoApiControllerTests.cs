using API.Controllers;
using Core.DTOs.Requests.Servico;
using Core.DTOs.Responses.Servico;
using Core.Interfaces.Controllers;
using Core.Interfaces.root;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace MecanicaOS.UnitTests.API.Controllers
{
    /// <summary>
    /// Testes para ServicoController (API)
    /// 
    /// IMPORTÂNCIA: Controller para gestão de serviços oferecidos pela oficina.
    /// Essencial para cadastro e manutenção do catálogo de serviços.
    /// 
    /// COBERTURA: Valida todos os 5 endpoints HTTP:
    /// - GET /servico - Listar todos os serviços
    /// - GET /servico/disponiveis - Listar serviços disponíveis
    /// - GET /servico/{id} - Obter serviço por ID
    /// - POST /servico - Cadastrar novo serviço (Admin)
    /// - PUT /servico/{id} - Atualizar serviço (Admin)
    /// - DELETE /servico/{id} - Remover serviço (Admin)
    /// 
    /// REGRAS DE NEGÓCIO:
    /// - Endpoints de escrita requerem role Admin
    /// - Validação de ModelState em operações de escrita
    /// - Retorno 404 quando serviço não encontrado
    /// </summary>
    public class ServicoApiControllerTests
    {
        private readonly ICompositionRoot _compositionRoot;
        private readonly IServicoController _servicoController;

        public ServicoApiControllerTests()
        {
            _compositionRoot = Substitute.For<ICompositionRoot>();
            _servicoController = Substitute.For<IServicoController>();
            _compositionRoot.CriarServicoController().Returns(_servicoController);
        }

        private ServicoController CriarController()
        {
            return new ServicoController(_compositionRoot);
        }

        [Fact]
        public async Task ObterTodos_DeveRetornarOkComListaDeServicos()
        {
            // Arrange
            var servicos = new List<ServicoResponse>
            {
                new ServicoResponse { Id = Guid.NewGuid(), Nome = "Troca de Óleo", Valor = 150.00m },
                new ServicoResponse { Id = Guid.NewGuid(), Nome = "Alinhamento", Valor = 80.00m }
            };

            _servicoController.ObterTodos().Returns(Task.FromResult<IEnumerable<ServicoResponse>>(servicos));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterTodos();

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            var okResult = resultado as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(servicos);
            await _servicoController.Received(1).ObterTodos();
        }

        [Fact]
        public async Task ObterTodos_ComListaVazia_DeveRetornarOkComListaVazia()
        {
            // Arrange
            var servicosVazios = new List<ServicoResponse>();
            _servicoController.ObterTodos().Returns(Task.FromResult<IEnumerable<ServicoResponse>>(servicosVazios));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterTodos();

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            var okResult = resultado as OkObjectResult;
            var lista = okResult!.Value as IEnumerable<ServicoResponse>;
            lista.Should().BeEmpty();
        }

        [Fact]
        public async Task ObterServicosDisponiveis_DeveRetornarOkComListaDeServicosDisponiveis()
        {
            // Arrange
            var servicosDisponiveis = new List<ServicoResponse>
            {
                new ServicoResponse { Id = Guid.NewGuid(), Nome = "Troca de Óleo", Valor = 150.00m, Disponivel = true },
                new ServicoResponse { Id = Guid.NewGuid(), Nome = "Alinhamento", Valor = 80.00m, Disponivel = true }
            };

            _servicoController.ObterServicosDisponiveis().Returns(Task.FromResult<IEnumerable<ServicoResponse>>(servicosDisponiveis));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterServicosDisponiveis();

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            var okResult = resultado as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(servicosDisponiveis);
            await _servicoController.Received(1).ObterServicosDisponiveis();
        }

        [Fact]
        public async Task ObterPorId_ComIdValido_DeveRetornarOkComServico()
        {
            // Arrange
            var id = Guid.NewGuid();
            var servico = new ServicoResponse
            {
                Id = id,
                Nome = "Troca de Óleo",
                Descricao = "Troca de óleo do motor",
                Valor = 150.00m,
                Disponivel = true
            };

            _servicoController.ObterPorId(id).Returns(Task.FromResult(servico));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterPorId(id);

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            var okResult = resultado as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(servico);
            await _servicoController.Received(1).ObterPorId(id);
        }

        [Fact]
        public async Task ObterPorId_ComServicoNaoEncontrado_DeveRetornarNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            _servicoController.ObterPorId(id).Returns(Task.FromResult<ServicoResponse>(null!));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterPorId(id);

            // Assert
            resultado.Should().BeOfType<NotFoundObjectResult>();
            await _servicoController.Received(1).ObterPorId(id);
        }

        [Fact]
        public async Task Criar_ComRequestValido_DeveRetornarCreatedAtAction()
        {
            // Arrange
            var request = new CadastrarServicoRequest
            {
                Nome = "Troca de Óleo",
                Descricao = "Troca de óleo do motor",
                Valor = 150.00m,
                Disponivel = true
            };

            var servicoCriado = new ServicoResponse
            {
                Id = Guid.NewGuid(),
                Nome = request.Nome,
                Descricao = request.Descricao,
                Valor = request.Valor,
                Disponivel = request.Disponivel
            };

            _servicoController.Criar(request).Returns(Task.FromResult(servicoCriado));
            var controller = CriarController();

            // Act
            var resultado = await controller.Criar(request);

            // Assert
            resultado.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = resultado as CreatedAtActionResult;
            createdResult!.ActionName.Should().Be(nameof(ServicoController.ObterPorId));
            createdResult.RouteValues!["id"].Should().Be(servicoCriado.Id);
            createdResult.Value.Should().BeEquivalentTo(servicoCriado);
            await _servicoController.Received(1).Criar(request);
        }

        [Fact]
        public async Task Criar_ComModelStateInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            var request = new CadastrarServicoRequest
            {
                Nome = "Teste",
                Descricao = "Teste",
                Valor = 100m,
                Disponivel = true
            };
            var controller = CriarController();
            controller.ModelState.AddModelError("Nome", "O campo Nome é obrigatório");

            // Act
            var resultado = await controller.Criar(request);

            // Assert
            resultado.Should().BeOfType<BadRequestObjectResult>();
            await _servicoController.DidNotReceive().Criar(Arg.Any<CadastrarServicoRequest>());
        }

        [Fact]
        public async Task Atualizar_ComRequestValido_DeveRetornarOkComServicoAtualizado()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new EditarServicoRequest
            {
                Nome = "Troca de Óleo Premium",
                Descricao = "Troca de óleo sintético",
                Valor = 200.00m,
                Disponivel = true
            };

            var servicoAtualizado = new ServicoResponse
            {
                Id = id,
                Nome = request.Nome,
                Descricao = request.Descricao,
                Valor = request.Valor!.Value,
                Disponivel = request.Disponivel!.Value
            };

            _servicoController.Atualizar(id, request).Returns(Task.FromResult(servicoAtualizado));
            var controller = CriarController();

            // Act
            var resultado = await controller.Atualizar(id, request);

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            var okResult = resultado as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(servicoAtualizado);
            await _servicoController.Received(1).Atualizar(id, request);
        }

        [Fact]
        public async Task Atualizar_ComModelStateInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new EditarServicoRequest
            {
                Nome = "Teste",
                Descricao = "Teste",
                Valor = 100m,
                Disponivel = true
            };
            var controller = CriarController();
            controller.ModelState.AddModelError("Nome", "O campo Nome é obrigatório");

            // Act
            var resultado = await controller.Atualizar(id, request);

            // Assert
            resultado.Should().BeOfType<BadRequestObjectResult>();
            await _servicoController.DidNotReceive().Atualizar(Arg.Any<Guid>(), Arg.Any<EditarServicoRequest>());
        }

        [Fact]
        public async Task Deletar_ComSucesso_DeveRetornarNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            _servicoController.Deletar(id).Returns(Task.CompletedTask);
            var controller = CriarController();

            // Act
            var resultado = await controller.Deletar(id);

            // Assert
            resultado.Should().BeOfType<NoContentResult>();
            await _servicoController.Received(1).Deletar(id);
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
        public async Task ObterPorId_DevePassarIdCorreto()
        {
            // Arrange
            var id = Guid.NewGuid();
            var servico = new ServicoResponse { Id = id };
            _servicoController.ObterPorId(id).Returns(Task.FromResult(servico));
            var controller = CriarController();

            // Act
            await controller.ObterPorId(id);

            // Assert
            await _servicoController.Received(1).ObterPorId(id);
        }
    }
}
