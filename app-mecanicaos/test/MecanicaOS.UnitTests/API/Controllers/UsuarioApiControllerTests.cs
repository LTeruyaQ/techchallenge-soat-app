using API.Controllers;
using Core.DTOs.Requests.Usuario;
using Core.DTOs.Responses.Usuario;
using Core.Enumeradores;
using Core.Interfaces.Controllers;
using Core.Interfaces.root;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace MecanicaOS.UnitTests.API.Controllers
{
    /// <summary>
    /// Testes para UsuarioController (API)
    /// 
    /// IMPORTÂNCIA: Controller para gestão de usuários do sistema.
    /// Essencial para controle de acesso e autenticação.
    /// 
    /// COBERTURA: Valida todos os 5 endpoints HTTP:
    /// - GET /usuario - Listar todos os usuários (Admin)
    /// - GET /usuario/{id} - Obter usuário por ID (Admin)
    /// - GET /usuario/email/{email} - Obter usuário por email (Admin)
    /// - POST /usuario - Cadastrar novo usuário (Admin)
    /// - PUT /usuario/{id} - Atualizar usuário (Admin)
    /// - DELETE /usuario/{id} - Remover usuário (Admin)
    /// 
    /// REGRAS DE NEGÓCIO:
    /// - Todos os endpoints requerem role Admin
    /// - Validação de ModelState em operações de escrita
    /// - Retorno 404 quando usuário não encontrado
    /// - Senhas nunca são retornadas nas respostas
    /// </summary>
    public class UsuarioApiControllerTests
    {
        private readonly ICompositionRoot _compositionRoot;
        private readonly IUsuarioController _usuarioController;

        public UsuarioApiControllerTests()
        {
            _compositionRoot = Substitute.For<ICompositionRoot>();
            _usuarioController = Substitute.For<IUsuarioController>();
            _compositionRoot.CriarUsuarioController().Returns(_usuarioController);
        }

        private UsuarioController CriarController()
        {
            return new UsuarioController(_compositionRoot);
        }

        [Fact]
        public async Task ObterTodos_DeveRetornarOkComListaDeUsuarios()
        {
            // Arrange
            var usuarios = new List<UsuarioResponse>
            {
                new UsuarioResponse { Id = Guid.NewGuid(), Email = "admin@teste.com", TipoUsuario = TipoUsuario.Admin },
                new UsuarioResponse { Id = Guid.NewGuid(), Email = "cliente@teste.com", TipoUsuario = TipoUsuario.Cliente }
            };

            _usuarioController.ObterTodosAsync().Returns(Task.FromResult<IEnumerable<UsuarioResponse>>(usuarios));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterTodos();

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            var okResult = resultado as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(usuarios);
            await _usuarioController.Received(1).ObterTodosAsync();
        }

        [Fact]
        public async Task ObterTodos_ComListaVazia_DeveRetornarOkComListaVazia()
        {
            // Arrange
            var usuariosVazios = new List<UsuarioResponse>();
            _usuarioController.ObterTodosAsync().Returns(Task.FromResult<IEnumerable<UsuarioResponse>>(usuariosVazios));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterTodos();

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            var okResult = resultado as OkObjectResult;
            var lista = okResult!.Value as IEnumerable<UsuarioResponse>;
            lista.Should().BeEmpty();
        }

        [Fact]
        public async Task ObterPorId_ComIdValido_DeveRetornarOkComUsuario()
        {
            // Arrange
            var id = Guid.NewGuid();
            var usuario = new UsuarioResponse
            {
                Id = id,
                Email = "admin@teste.com",
                TipoUsuario = TipoUsuario.Admin,
                RecebeAlertaEstoque = true
            };

            _usuarioController.ObterPorIdAsync(id).Returns(Task.FromResult(usuario));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterPorId(id);

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            var okResult = resultado as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(usuario);
            await _usuarioController.Received(1).ObterPorIdAsync(id);
        }

        [Fact]
        public async Task ObterPorId_ComUsuarioNaoEncontrado_DeveRetornarNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            _usuarioController.ObterPorIdAsync(id).Returns(Task.FromResult<UsuarioResponse>(null!));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterPorId(id);

            // Assert
            resultado.Should().BeOfType<NotFoundObjectResult>();
            await _usuarioController.Received(1).ObterPorIdAsync(id);
        }

        [Fact]
        public async Task ObterPorEmail_ComEmailValido_DeveRetornarOkComUsuario()
        {
            // Arrange
            var email = "admin@teste.com";
            var usuario = new UsuarioResponse
            {
                Id = Guid.NewGuid(),
                Email = email,
                TipoUsuario = TipoUsuario.Admin
            };

            _usuarioController.ObterPorEmailAsync(email).Returns(Task.FromResult(usuario));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterPorEmail(email);

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            var okResult = resultado as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(usuario);
            await _usuarioController.Received(1).ObterPorEmailAsync(email);
        }

        [Fact]
        public async Task ObterPorEmail_ComUsuarioNaoEncontrado_DeveRetornarNotFound()
        {
            // Arrange
            var email = "naoexiste@teste.com";
            _usuarioController.ObterPorEmailAsync(email).Returns(Task.FromResult<UsuarioResponse>(null!));
            var controller = CriarController();

            // Act
            var resultado = await controller.ObterPorEmail(email);

            // Assert
            resultado.Should().BeOfType<NotFoundObjectResult>();
            await _usuarioController.Received(1).ObterPorEmailAsync(email);
        }

        [Fact]
        public async Task Criar_ComRequestValido_DeveRetornarCreatedAtAction()
        {
            // Arrange
            var request = new CadastrarUsuarioRequest
            {
                Email = "novoadmin@teste.com",
                Senha = "Senha@123",
                TipoUsuario = TipoUsuario.Admin,
                RecebeAlertaEstoque = true
            };

            var usuarioCriado = new UsuarioResponse
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                TipoUsuario = request.TipoUsuario,
                RecebeAlertaEstoque = request.RecebeAlertaEstoque!.Value
            };

            _usuarioController.CadastrarAsync(request).Returns(Task.FromResult(usuarioCriado));
            var controller = CriarController();

            // Act
            var resultado = await controller.Criar(request);

            // Assert
            resultado.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = resultado as CreatedAtActionResult;
            createdResult!.ActionName.Should().Be(nameof(UsuarioController.ObterPorId));
            createdResult.RouteValues!["id"].Should().Be(usuarioCriado.Id);
            createdResult.Value.Should().BeEquivalentTo(usuarioCriado);
            await _usuarioController.Received(1).CadastrarAsync(request);
        }

        [Fact]
        public async Task Criar_ComModelStateInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            var request = new CadastrarUsuarioRequest();
            var controller = CriarController();
            controller.ModelState.AddModelError("Nome", "O campo Nome é obrigatório");

            // Act
            var resultado = await controller.Criar(request);

            // Assert
            resultado.Should().BeOfType<BadRequestObjectResult>();
            await _usuarioController.DidNotReceive().CadastrarAsync(Arg.Any<CadastrarUsuarioRequest>());
        }

        [Fact]
        public async Task Atualizar_ComRequestValido_DeveRetornarOkComUsuarioAtualizado()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new AtualizarUsuarioRequest
            {
                Email = "adminatualizado@teste.com",
                TipoUsuario = TipoUsuario.Admin,
                RecebeAlertaEstoque = false
            };

            var usuarioAtualizado = new UsuarioResponse
            {
                Id = id,
                Email = request.Email,
                TipoUsuario = request.TipoUsuario!.Value,
                RecebeAlertaEstoque = request.RecebeAlertaEstoque!.Value
            };

            _usuarioController.AtualizarAsync(id, request).Returns(Task.FromResult(usuarioAtualizado));
            var controller = CriarController();

            // Act
            var resultado = await controller.Atualizar(id, request);

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
            var okResult = resultado as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(usuarioAtualizado);
            await _usuarioController.Received(1).AtualizarAsync(id, request);
        }

        [Fact]
        public async Task Atualizar_ComModelStateInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new AtualizarUsuarioRequest();
            var controller = CriarController();
            controller.ModelState.AddModelError("Nome", "O campo Nome é obrigatório");

            // Act
            var resultado = await controller.Atualizar(id, request);

            // Assert
            resultado.Should().BeOfType<BadRequestObjectResult>();
            await _usuarioController.DidNotReceive().AtualizarAsync(Arg.Any<Guid>(), Arg.Any<AtualizarUsuarioRequest>());
        }

        [Fact]
        public async Task Deletar_ComSucesso_DeveRetornarNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            _usuarioController.DeletarAsync(id).Returns(Task.FromResult(true));
            var controller = CriarController();

            // Act
            var resultado = await controller.Deletar(id);

            // Assert
            resultado.Should().BeOfType<NoContentResult>();
            await _usuarioController.Received(1).DeletarAsync(id);
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
            var usuario = new UsuarioResponse { Id = id };
            _usuarioController.ObterPorIdAsync(id).Returns(Task.FromResult(usuario));
            var controller = CriarController();

            // Act
            await controller.ObterPorId(id);

            // Assert
            await _usuarioController.Received(1).ObterPorIdAsync(id);
        }

        [Fact]
        public async Task ObterPorEmail_DevePassarEmailCorreto()
        {
            // Arrange
            var email = "teste@teste.com";
            var usuario = new UsuarioResponse { Id = Guid.NewGuid(), Email = email };
            _usuarioController.ObterPorEmailAsync(email).Returns(Task.FromResult(usuario));
            var controller = CriarController();

            // Act
            await controller.ObterPorEmail(email);

            // Assert
            await _usuarioController.Received(1).ObterPorEmailAsync(email);
        }
    }
}
