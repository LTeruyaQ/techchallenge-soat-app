using API.Controllers;
using Core.DTOs.Requests.Autenticacao;
using Core.DTOs.Requests.Usuario;
using Core.DTOs.Responses.Autenticacao;
using Core.DTOs.Responses.Usuario;
using Core.DTOs.UseCases.Autenticacao;
using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.root;
using Core.Interfaces.UseCases;
using Core.Interfaces.Controllers;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using NSubstitute;

namespace MecanicaOS.UnitTests.API.Controllers
{
    /// <summary>
    /// Testes unitários para o AutenticacaoController
    /// </summary>
    public class AutenticacaoControllerTests
    {
        /// <summary>
        /// Verifica se o método Login retorna Ok com token quando as credenciais são válidas
        /// </summary>
        [Fact]
        public async Task Login_QuandoCredenciaisValidas_DeveRetornarOkComToken()
        {
            // Arrange
            var autenticacaoUseCasesMock = Substitute.For<IAutenticacaoUseCases>();
            
            var request = new AutenticacaoRequest
            {
                Email = "usuario@teste.com",
                Senha = "senha123"
            };
            
            var autenticacaoDto = new AutenticacaoDto
            {
                Token = "token-jwt-valido",
                Usuario = new Usuario { Email = "usuario@teste.com" },
                Permissoes = new List<string> { "Admin" }
            };
            
            autenticacaoUseCasesMock.AutenticarUseCaseAsync(Arg.Any<AutenticacaoUseCaseDto>())
                .Returns(autenticacaoDto);
            
            var autenticacaoControllerMock = Substitute.For<IAutenticacaoController>();
            var usuarioControllerMock = Substitute.For<IUsuarioController>();
            var compositionRootMock = Substitute.For<ICompositionRoot>();
            
            compositionRootMock.CriarAutenticacaoController().Returns(autenticacaoControllerMock);
            compositionRootMock.CriarUsuarioController().Returns(usuarioControllerMock);
            
            autenticacaoControllerMock.AutenticarAsync(Arg.Any<AutenticacaoRequest>())
                .Returns(new AutenticacaoResponse
                {
                    Token = "token-jwt-valido"
                });
                
            var controller = new AutenticacaoController(compositionRootMock);
            
            // Act
            var resultado = await controller.Login(request);
            
            // Assert
            resultado.Should().BeOfType<OkObjectResult>("deve retornar Ok (200)");
            
            var okResult = resultado as OkObjectResult;
            var response = okResult.Value as AutenticacaoResponse;
            
            response.Should().NotBeNull("a resposta não deve ser nula");
            response.Token.Should().Be("token-jwt-valido", "o token deve corresponder ao gerado");
            
            await autenticacaoControllerMock.Received(1).AutenticarAsync(
                Arg.Is<AutenticacaoRequest>(r => r.Email == "usuario@teste.com" && r.Senha == "senha123"));
        }

        [Fact]
        public void ValidarToken_DeveRetornarOk()
        {
            // Arrange
            var compositionRootMock = Substitute.For<ICompositionRoot>();
            var autenticacaoControllerMock = Substitute.For<IAutenticacaoController>();
            var usuarioControllerMock = Substitute.For<IUsuarioController>();

            compositionRootMock.CriarAutenticacaoController().Returns(autenticacaoControllerMock);
            compositionRootMock.CriarUsuarioController().Returns(usuarioControllerMock);

            var controller = new AutenticacaoController(compositionRootMock);

            // Act
            var resultado = controller.ValidarToken();

            // Assert
            resultado.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Registrar_ComDadosValidos_DeveRetornarCreatedAtAction()
        {
            // Arrange
            var request = new CadastrarUsuarioRequest
            {
                Email = "novo@teste.com",
                Senha = "Senha@123",
                TipoUsuario = global::Core.Enumeradores.TipoUsuario.Admin
            };

            var usuarioResponse = new UsuarioResponse
            {
                Id = Guid.NewGuid(),
                Email = "novo@teste.com",
                Senha = "Senha@123",
                TipoUsuario = global::Core.Enumeradores.TipoUsuario.Admin,
                Ativo = true,
                RecebeAlertaEstoque = false,
                DataCadastro = DateTime.UtcNow
            };

            var compositionRootMock = Substitute.For<ICompositionRoot>();
            var autenticacaoControllerMock = Substitute.For<IAutenticacaoController>();
            var usuarioControllerMock = Substitute.For<IUsuarioController>();

            compositionRootMock.CriarAutenticacaoController().Returns(autenticacaoControllerMock);
            compositionRootMock.CriarUsuarioController().Returns(usuarioControllerMock);

            usuarioControllerMock.CadastrarAsync(request).Returns(usuarioResponse);

            var controller = new AutenticacaoController(compositionRootMock);

            // Act
            var resultado = await controller.Registrar(request);

            // Assert
            resultado.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = resultado as CreatedAtActionResult;
            createdResult!.ActionName.Should().Be("Login");
            createdResult.Value.Should().Be(usuarioResponse);

            await usuarioControllerMock.Received(1).CadastrarAsync(request);
        }

    }
}
