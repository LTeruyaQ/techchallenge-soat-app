using API.Controllers;
using Core.DTOs.Requests.OrdemServico;
using Core.DTOs.Requests.OrdemServico.InsumoOS;
using Core.DTOs.Responses.OrdemServico;
using Core.DTOs.Responses.OrdemServico.InsumoOrdemServico;
using Core.Enumeradores;
using Core.Interfaces.Controllers;
using Core.Interfaces.root;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace MecanicaOS.UnitTests.API.Controllers
{
    /// <summary>
    /// Testes para OrdemServicoController (API)
    /// 
    /// IMPORTÂNCIA: Controller central da API REST para gerenciamento de ordens de serviço.
    /// Responsável por orquestrar todas as operações CRUD e ações específicas (aceitar/recusar orçamento).
    /// 
    /// COBERTURA: Valida todos os 8 endpoints HTTP:
    /// - GET /ordemservico - Listar todas
    /// - GET /ordemservico/{id} - Obter por ID
    /// - GET /ordemservico/status/{status} - Filtrar por status
    /// - POST /ordemservico - Criar nova ordem
    /// - PUT /ordemservico/{id} - Atualizar ordem
    /// - POST /ordemservico/{id}/insumos - Adicionar insumos
    /// - PATCH /ordemservico/{id}/aceitar-orcamento - Aceitar orçamento
    /// - PATCH /ordemservico/{id}/recusar-orcamento - Recusar orçamento
    /// </summary>
    public class OrdemServicoApiControllerTests
    {
        private readonly ICompositionRoot _compositionRoot;
        private readonly IOrdemServicoController _ordemServicoController;
        private readonly IInsumoOSController _insumoOSController;

        public OrdemServicoApiControllerTests()
        {
            _compositionRoot = Substitute.For<ICompositionRoot>();
            _ordemServicoController = Substitute.For<IOrdemServicoController>();
            _insumoOSController = Substitute.For<IInsumoOSController>();

            _compositionRoot.CriarOrdemServicoController().Returns(_ordemServicoController);
            _compositionRoot.CriarInsumoOSController().Returns(_insumoOSController);
        }

        private OrdemServicoController CriarController()
        {
            return new OrdemServicoController(_compositionRoot);
        }

        [Fact]
        public void Construtor_DeveCriarInstanciaComDependencias()
        {
            // Arrange & Act
            var controller = CriarController();

            // Assert
            controller.Should().NotBeNull();
            _compositionRoot.Received(1).CriarOrdemServicoController();
            _compositionRoot.Received(1).CriarInsumoOSController();
        }

        [Fact]
        public async Task ObterTodos_DeveRetornarOkComListaDeOrdens()
        {
            // Arrange
            var ordens = new List<OrdemServicoResponse>
            {
                new OrdemServicoResponse { Id = Guid.NewGuid(), Status = StatusOrdemServico.Recebida },
                new OrdemServicoResponse { Id = Guid.NewGuid(), Status = StatusOrdemServico.EmExecucao }
            };

            _ordemServicoController.ObterTodos().Returns(Task.FromResult<IEnumerable<OrdemServicoResponse>>(ordens));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterTodos();

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            var okResult = resultado as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(ordens);
            await _ordemServicoController.Received(1).ObterTodos();
        }

        [Fact]
        public async Task ObterTodos_ComListaVazia_DeveRetornarOkComListaVazia()
        {
            // Arrange
            var ordensVazia = new List<OrdemServicoResponse>();
            _ordemServicoController.ObterTodos().Returns(Task.FromResult<IEnumerable<OrdemServicoResponse>>(ordensVazia));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterTodos();

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            var okResult = resultado as OkObjectResult;
            var lista = okResult!.Value as IEnumerable<OrdemServicoResponse>;
            lista.Should().BeEmpty();
        }

        [Fact]
        public async Task ObterPorId_ComIdExistente_DeveRetornarOkComOrdem()
        {
            // Arrange
            var id = Guid.NewGuid();
            var ordem = new OrdemServicoResponse { Id = id, Status = StatusOrdemServico.Recebida };
            
            _ordemServicoController.ObterPorId(id).Returns(Task.FromResult<OrdemServicoResponse?>(ordem));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterPorId(id);

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            var okResult = resultado as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(ordem);
            await _ordemServicoController.Received(1).ObterPorId(id);
        }

        [Fact]
        public async Task ObterPorId_ComIdInexistente_DeveRetornarNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            _ordemServicoController.ObterPorId(id).Returns(Task.FromResult<OrdemServicoResponse?>(null));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterPorId(id);

            // Assert
            resultado.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = resultado as NotFoundObjectResult;
            notFoundResult!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task ObterPorStatus_ComStatusValido_DeveRetornarOkComOrdens()
        {
            // Arrange
            var status = StatusOrdemServico.EmExecucao;
            var ordens = new List<OrdemServicoResponse>
            {
                new OrdemServicoResponse { Id = Guid.NewGuid(), Status = status },
                new OrdemServicoResponse { Id = Guid.NewGuid(), Status = status }
            };

            _ordemServicoController.ObterPorStatus(status).Returns(Task.FromResult<IEnumerable<OrdemServicoResponse>>(ordens));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterPorStatus(status);

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            var okResult = resultado as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(ordens);
            await _ordemServicoController.Received(1).ObterPorStatus(status);
        }

        [Fact]
        public async Task ObterPorStatus_SemOrdensComStatus_DeveRetornarNotFound()
        {
            // Arrange
            var status = StatusOrdemServico.Cancelada;
            var ordensVazia = new List<OrdemServicoResponse>();

            _ordemServicoController.ObterPorStatus(status).Returns(Task.FromResult<IEnumerable<OrdemServicoResponse>>(ordensVazia));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterPorStatus(status);

            // Assert
            resultado.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task ObterPorStatus_ComResultadoNull_DeveRetornarNotFound()
        {
            // Arrange
            var status = StatusOrdemServico.Finalizada;
            _ordemServicoController.ObterPorStatus(status).Returns(Task.FromResult<IEnumerable<OrdemServicoResponse>>(null!));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterPorStatus(status);

            // Assert
            resultado.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Criar_ComDadosValidos_DeveRetornarCreated()
        {
            // Arrange
            var request = new CadastrarOrdemServicoRequest
            {
                ClienteId = Guid.NewGuid(),
                ServicoId = Guid.NewGuid(),
                VeiculoId = Guid.NewGuid(),
                Descricao = "Troca de óleo"
            };

            var ordemCriada = new OrdemServicoResponse
            {
                Id = Guid.NewGuid(),
                Status = StatusOrdemServico.Recebida
            };

            _ordemServicoController.Cadastrar(request).Returns(Task.FromResult(ordemCriada));
            var controller = CriarController();

            // Act
            var resultado = await controller.Criar(request);

            // Assert
            resultado.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = resultado as CreatedAtActionResult;
            createdResult!.ActionName.Should().Be(nameof(OrdemServicoController.ObterPorId));
            createdResult.RouteValues.Should().ContainKey("id");
            createdResult.Value.Should().BeEquivalentTo(ordemCriada);
            await _ordemServicoController.Received(1).Cadastrar(request);
        }

        [Fact]
        public async Task Criar_ComModelStateInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            var request = new CadastrarOrdemServicoRequest();
            var controller = CriarController();
            controller.ModelState.AddModelError("ClienteId", "ClienteId é obrigatório");

            // Act
            var resultado = await controller.Criar(request);

            // Assert
            resultado.Should().BeOfType<BadRequestObjectResult>();
            await _ordemServicoController.DidNotReceive().Cadastrar(Arg.Any<CadastrarOrdemServicoRequest>());
        }

        [Fact]
        public async Task Atualizar_ComDadosValidos_DeveRetornarOkComOrdemAtualizada()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new AtualizarOrdemServicoRequest
            {
                Descricao = "Descrição atualizada",
                Status = StatusOrdemServico.EmExecucao
            };

            var ordemAtualizada = new OrdemServicoResponse
            {
                Id = id,
                Status = StatusOrdemServico.Finalizada
            };

            _ordemServicoController.Atualizar(id, request).Returns(Task.FromResult(ordemAtualizada));
            var controller = CriarController();
            var resultado = await controller.Atualizar(id, request);

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            var okResult = resultado as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(ordemAtualizada);
            await _ordemServicoController.Received(1).Atualizar(id, request);
        }

        [Fact]
        public async Task Atualizar_ComModelStateInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new AtualizarOrdemServicoRequest();
            var controller = CriarController();
            controller.ModelState.AddModelError("Status", "Status inválido");

            // Act
            var resultado = await controller.Atualizar(id, request);

            // Assert
            resultado.Should().BeOfType<BadRequestObjectResult>();
            await _ordemServicoController.DidNotReceive().Atualizar(Arg.Any<Guid>(), Arg.Any<AtualizarOrdemServicoRequest>());
        }

        // TODO: Implementar teste quando interface estiver correta

        [Fact]
        public async Task AdicionarInsumosOS_ComDadosValidos_DeveRetornarOkComInsumos()
        {
            // Arrange
            var ordemServicoId = Guid.NewGuid();
            var requests = new List<CadastrarInsumoOSRequest>
            {
                new CadastrarInsumoOSRequest { EstoqueId = Guid.NewGuid(), Quantidade = 2 }
            };

            var expectedInsumos = new List<InsumoOSResponse>
            {
                new InsumoOSResponse { OrdemServicoId = ordemServicoId, EstoqueId = requests[0].EstoqueId, Quantidade = 2 }
            };

            _insumoOSController.CadastrarInsumos(ordemServicoId, requests).Returns(expectedInsumos);
            var controller = CriarController();

            // Act
            var resultado = await controller.AdicionarInsumosOS(ordemServicoId, requests);

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            var okResult = resultado as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedInsumos);

            await _insumoOSController.Received(1).CadastrarInsumos(ordemServicoId, requests);
        }

        [Fact]
        public async Task AdicionarInsumosOS_ComModelStateInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            var ordemServicoId = Guid.NewGuid();
            var requests = new List<CadastrarInsumoOSRequest>();
            var controller = CriarController();
            controller.ModelState.AddModelError("EstoqueId", "EstoqueId é obrigatório");

            // Act
            var resultado = await controller.AdicionarInsumosOS(ordemServicoId, requests);

            // Assert
            resultado.Should().BeOfType<BadRequestObjectResult>();
            await _insumoOSController.DidNotReceive().CadastrarInsumos(Arg.Any<Guid>(), Arg.Any<List<CadastrarInsumoOSRequest>>());
        }

        [Fact]
        public async Task AceitarOrcamento_ComIdValido_DeveRetornarNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            _ordemServicoController.AceitarOrcamento(id).Returns(Task.CompletedTask);
            var controller = CriarController();

            // Act
            var resultado = await controller.AceitarOrcamento(id);

            // Assert
            resultado.Should().BeOfType<NoContentResult>();
            await _ordemServicoController.Received(1).AceitarOrcamento(id);
        }

        [Fact]
        public async Task RecusarOrcamento_ComIdValido_DeveRetornarNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            _ordemServicoController.RecusarOrcamento(id).Returns(Task.CompletedTask);
            var controller = CriarController();

            // Act
            var resultado = await controller.RecusarOrcamento(id);

            // Assert
            resultado.Should().BeOfType<NoContentResult>();
            await _ordemServicoController.Received(1).RecusarOrcamento(id);
        }

        [Theory]
        [InlineData(StatusOrdemServico.Recebida)]
        [InlineData(StatusOrdemServico.EmDiagnostico)]
        [InlineData(StatusOrdemServico.AguardandoAprovacao)]
        [InlineData(StatusOrdemServico.EmExecucao)]
        [InlineData(StatusOrdemServico.Finalizada)]
        [InlineData(StatusOrdemServico.Cancelada)]
        public async Task ObterPorStatus_ComDiferentesStatus_DeveChamarControllerComStatusCorreto(StatusOrdemServico status)
        {
            // Arrange
            var ordens = new List<OrdemServicoResponse>
            {
                new OrdemServicoResponse { Id = Guid.NewGuid(), Status = status }
            };

            _ordemServicoController.ObterPorStatus(status).Returns(Task.FromResult<IEnumerable<OrdemServicoResponse>>(ordens));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterPorStatus(status);

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            await _ordemServicoController.Received(1).ObterPorStatus(status);
        }

        [Fact]
        public async Task Criar_DeveRetornarCreatedComLocationHeader()
        {
            // Arrange
            var request = new CadastrarOrdemServicoRequest
            {
                ClienteId = Guid.NewGuid(),
                ServicoId = Guid.NewGuid(),
                VeiculoId = Guid.NewGuid()
            };

            var ordemId = Guid.NewGuid();
            var ordemCriada = new OrdemServicoResponse { Id = ordemId };

            _ordemServicoController.Cadastrar(request).Returns(Task.FromResult(ordemCriada));
            var controller = CriarController();

            // Act
            var resultado = await controller.Criar(request);

            // Assert
            var createdResult = resultado as CreatedAtActionResult;
            createdResult!.RouteValues!["id"].Should().Be(ordemId);
        }

        // TODO: Corrigir namespace - temporariamente comentado
        // [Fact]
        // public async Task AdicionarInsumosOS_ComMultiplosInsumos_DeveProcessarTodos() { }
    }
}
