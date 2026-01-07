using API.Controllers;
using Core.DTOs.Requests.Veiculo;
using Core.DTOs.Responses.Veiculo;
using Core.Interfaces.Controllers;
using Core.Interfaces.root;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace MecanicaOS.UnitTests.API.Controllers
{
    /// <summary>
    /// Testes para VeiculoController (API)
    /// 
    /// IMPORTÂNCIA: Controller para gestão de veículos dos clientes.
    /// Essencial para rastreamento de histórico de manutenção por veículo.
    /// 
    /// COBERTURA: Valida todos os 7 endpoints HTTP:
    /// - GET /veiculo - Listar todos os veículos
    /// - GET /veiculo/{id} - Obter veículo por ID
    /// - GET /veiculo/placa/{placa} - Obter veículo por placa
    /// - GET /veiculo/cliente/{clienteId} - Obter veículos de um cliente
    /// - POST /veiculo - Cadastrar novo veículo
    /// - PUT /veiculo/{id} - Atualizar veículo
    /// - DELETE /veiculo/{id} - Remover veículo
    /// 
    /// REGRAS DE NEGÓCIO:
    /// - Todos os endpoints requerem autenticação
    /// - Validação de ModelState em operações de escrita
    /// - Logging de todas as operações
    /// - Retorno 404 quando veículo não encontrado (ObterPorId e ObterPorPlaca)
    /// </summary>
    public class VeiculoApiControllerTests
    {
        private readonly ICompositionRoot _compositionRoot;
        private readonly IVeiculoController _veiculoController;
        private readonly ILogger<VeiculoController> _logger;

        public VeiculoApiControllerTests()
        {
            _compositionRoot = Substitute.For<ICompositionRoot>();
            _veiculoController = Substitute.For<IVeiculoController>();
            _logger = Substitute.For<ILogger<VeiculoController>>();
            _compositionRoot.CriarVeiculoController().Returns(_veiculoController);
        }

        private VeiculoController CriarController()
        {
            return new VeiculoController(_compositionRoot, _logger);
        }

        [Fact]
        public async Task Cadastrar_ComRequestValido_DeveRetornarCreatedAtAction()
        {
            // Arrange
            var request = new CadastrarVeiculoRequest
            {
                Placa = "ABC1234",
                Modelo = "Civic",
                Marca = "Honda",
                Ano = "2020",
                ClienteId = Guid.NewGuid()
            };

            var veiculoCriado = new VeiculoResponse
            {
                Id = Guid.NewGuid(),
                Placa = request.Placa,
                Modelo = request.Modelo,
                Marca = request.Marca,
                Ano = request.Ano
            };

            _veiculoController.Cadastrar(request).Returns(Task.FromResult(veiculoCriado));
            var controller = CriarController();

            // Act
            var resultado = await controller.Cadastrar(request);

            // Assert
            resultado.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = resultado as CreatedAtActionResult;
            createdResult!.ActionName.Should().Be(nameof(VeiculoController.ObterPorId));
            createdResult.RouteValues!["id"].Should().Be(veiculoCriado.Id);
            createdResult.Value.Should().BeEquivalentTo(veiculoCriado);
            await _veiculoController.Received(1).Cadastrar(request);
        }

        [Fact]
        public async Task Cadastrar_ComModelStateInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            var request = new CadastrarVeiculoRequest();
            var controller = CriarController();
            controller.ModelState.AddModelError("Placa", "O campo Placa é obrigatório");

            // Act
            var resultado = await controller.Cadastrar(request);

            // Assert
            resultado.Should().BeOfType<BadRequestObjectResult>();
            await _veiculoController.DidNotReceive().Cadastrar(Arg.Any<CadastrarVeiculoRequest>());
        }

        [Fact]
        public async Task Deletar_ComSucesso_DeveRetornarNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            _veiculoController.Deletar(id).Returns(Task.FromResult(true));
            var controller = CriarController();

            // Act
            var resultado = await controller.Deletar(id);

            // Assert
            resultado.Should().BeOfType<NoContentResult>();
            await _veiculoController.Received(1).Deletar(id);
        }

        [Fact]
        public async Task Deletar_ComFalha_DeveRetornarInternalServerError()
        {
            // Arrange
            var id = Guid.NewGuid();
            _veiculoController.Deletar(id).Returns(Task.FromResult(false));
            var controller = CriarController();

            // Act
            var resultado = await controller.Deletar(id);

            // Assert
            resultado.Should().BeOfType<ObjectResult>();
            var objectResult = resultado as ObjectResult;
            objectResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            await _veiculoController.Received(1).Deletar(id);
        }

        [Fact]
        public async Task Editar_ComRequestValido_DeveRetornarOkComVeiculoAtualizado()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new AtualizarVeiculoRequest
            {
                Placa = "XYZ5678",
                Modelo = "Corolla",
                Marca = "Toyota",
                Ano = "2021"
            };

            var veiculoAtualizado = new VeiculoResponse
            {
                Id = id,
                Placa = request.Placa,
                Modelo = request.Modelo,
                Marca = request.Marca,
                Ano = request.Ano
            };

            _veiculoController.Atualizar(id, request).Returns(Task.FromResult(veiculoAtualizado));
            var controller = CriarController();

            // Act
            var resultado = await controller.Editar(id, request);

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            var okResult = resultado as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(veiculoAtualizado);
            await _veiculoController.Received(1).Atualizar(id, request);
        }

        [Fact]
        public async Task Editar_ComModelStateInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new AtualizarVeiculoRequest();
            var controller = CriarController();
            controller.ModelState.AddModelError("Placa", "O campo Placa é obrigatório");

            // Act
            var resultado = await controller.Editar(id, request);

            // Assert
            resultado.Should().BeOfType<BadRequestObjectResult>();
            await _veiculoController.DidNotReceive().Atualizar(Arg.Any<Guid>(), Arg.Any<AtualizarVeiculoRequest>());
        }

        [Fact]
        public async Task ObterPorCliente_ComClienteValido_DeveRetornarOkComListaDeVeiculos()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var veiculos = new List<VeiculoResponse>
            {
                new VeiculoResponse { Id = Guid.NewGuid(), Placa = "ABC1234", Modelo = "Civic" },
                new VeiculoResponse { Id = Guid.NewGuid(), Placa = "XYZ5678", Modelo = "Corolla" }
            };

            _veiculoController.ObterPorCliente(clienteId).Returns(Task.FromResult<IEnumerable<VeiculoResponse>>(veiculos));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterPorCliente(clienteId);

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            var okResult = resultado as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(veiculos);
            await _veiculoController.Received(1).ObterPorCliente(clienteId);
        }

        [Fact]
        public async Task ObterPorId_ComIdValido_DeveRetornarOkComVeiculo()
        {
            // Arrange
            var id = Guid.NewGuid();
            var veiculo = new VeiculoResponse
            {
                Id = id,
                Placa = "ABC1234",
                Modelo = "Civic",
                Marca = "Honda",
                Ano = "2020"
            };

            _veiculoController.ObterPorId(id).Returns(Task.FromResult(veiculo));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterPorId(id);

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            var okResult = resultado as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(veiculo);
            await _veiculoController.Received(1).ObterPorId(id);
        }

        [Fact]
        public async Task ObterPorId_ComVeiculoNaoEncontrado_DeveRetornarNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            _veiculoController.ObterPorId(id).Returns(Task.FromResult<VeiculoResponse>(null!));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterPorId(id);

            // Assert
            resultado.Should().BeOfType<NotFoundObjectResult>();
            await _veiculoController.Received(1).ObterPorId(id);
        }

        [Fact]
        public async Task ObterPorPlaca_ComPlacaValida_DeveRetornarOkComVeiculo()
        {
            // Arrange
            var placa = "ABC1234";
            var veiculo = new VeiculoResponse
            {
                Id = Guid.NewGuid(),
                Placa = placa,
                Modelo = "Civic",
                Marca = "Honda",
                Ano = "2020"
            };

            _veiculoController.ObterPorPlaca(placa).Returns(Task.FromResult(veiculo));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterPorPlaca(placa);

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            var okResult = resultado as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(veiculo);
            await _veiculoController.Received(1).ObterPorPlaca(placa);
        }

        [Fact]
        public async Task ObterPorPlaca_ComVeiculoNaoEncontrado_DeveRetornarNotFound()
        {
            // Arrange
            var placa = "XYZ9999";
            _veiculoController.ObterPorPlaca(placa).Returns(Task.FromResult<VeiculoResponse>(null!));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterPorPlaca(placa);

            // Assert
            resultado.Should().BeOfType<NotFoundObjectResult>();
            await _veiculoController.Received(1).ObterPorPlaca(placa);
        }

        [Fact]
        public async Task ObterTodos_DeveRetornarOkComListaDeVeiculos()
        {
            // Arrange
            var veiculos = new List<VeiculoResponse>
            {
                new VeiculoResponse { Id = Guid.NewGuid(), Placa = "ABC1234", Modelo = "Civic" },
                new VeiculoResponse { Id = Guid.NewGuid(), Placa = "XYZ5678", Modelo = "Corolla" },
                new VeiculoResponse { Id = Guid.NewGuid(), Placa = "DEF9012", Modelo = "Gol" }
            };

            _veiculoController.ObterTodos().Returns(Task.FromResult<IEnumerable<VeiculoResponse>>(veiculos));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterTodos();

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            var okResult = resultado as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(veiculos);
            await _veiculoController.Received(1).ObterTodos();
        }

        [Fact]
        public async Task ObterTodos_ComListaVazia_DeveRetornarOkComListaVazia()
        {
            // Arrange
            _veiculoController.ObterTodos().Returns(Task.FromResult<IEnumerable<VeiculoResponse>>(new List<VeiculoResponse>()));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterTodos();

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            var okResult = resultado as OkObjectResult;
            var lista = okResult!.Value as IEnumerable<VeiculoResponse>;
            lista.Should().BeEmpty();
        }

        [Fact]
        public void Controller_DeveHerdarDeBaseApiController()
        {
            // Arrange & Act
            var controller = CriarController();

            // Assert
            controller.Should().BeAssignableTo<BaseApiController>();
        }

    }
}
