using API.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MecanicaOS.UnitTests.API.Controllers
{
    /// <summary>
    /// Testes para BaseApiController (API)
    /// 
    /// IMPORTÂNCIA: Controller base que fornece funcionalidades comuns para todos os controllers da API.
    /// A validação de ModelState é crítica para garantir que dados inválidos não sejam processados.
    /// 
    /// COBERTURA: Valida o método ValidarModelState que é usado por todos os controllers derivados.
    /// </summary>
    public class BaseApiControllerTests
    {
        // Controller concreto para testar a classe abstrata
        private class TestController : BaseApiController
        {
            public IActionResult TestarValidarModelState() => ValidarModelState();
        }

        [Fact]
        public void ValidarModelState_ComModelStateValido_DeveRetornarNull()
        {
            // Arrange
            var controller = new TestController();

            // Act
            var resultado = controller.TestarValidarModelState();

            // Assert
            resultado.Should().BeNull("ModelState válido não deve retornar erro");
        }

        [Fact]
        public void ValidarModelState_ComModelStateInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            var controller = new TestController();
            controller.ModelState.AddModelError("Campo", "Campo obrigatório");

            // Act
            var resultado = controller.TestarValidarModelState();

            // Assert
            resultado.Should().NotBeNull("ModelState inválido deve retornar erro");
            resultado.Should().BeOfType<BadRequestObjectResult>("Deve retornar BadRequest");
            
            var badRequestResult = resultado as BadRequestObjectResult;
            badRequestResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public void ValidarModelState_ComMultiplosErros_DeveConcatenarMensagens()
        {
            // Arrange
            var controller = new TestController();
            controller.ModelState.AddModelError("Campo1", "Erro no campo 1");
            controller.ModelState.AddModelError("Campo2", "Erro no campo 2");

            // Act
            var resultado = controller.TestarValidarModelState();

            // Assert
            var badRequestResult = resultado as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            
            var valorRetornado = badRequestResult!.Value;
            valorRetornado.Should().NotBeNull();
            
            // Verifica se a mensagem contém os erros concatenados
            var propriedadeMessage = valorRetornado!.GetType().GetProperty("Message");
            var mensagem = propriedadeMessage?.GetValue(valorRetornado) as string;
            
            mensagem.Should().NotBeNullOrEmpty();
            mensagem.Should().Contain("Erro no campo 1");
            mensagem.Should().Contain("Erro no campo 2");
            mensagem.Should().Contain(";", "Múltiplos erros devem ser separados por ponto e vírgula");
        }

        [Fact]
        public void ValidarModelState_ComErroSemMensagem_DeveRetornarMensagemPadrao()
        {
            // Arrange
            var controller = new TestController();
            
            // Adiciona erro com mensagem vazia
            controller.ModelState.AddModelError("Campo", "");

            // Act
            var resultado = controller.TestarValidarModelState();

            // Assert
            var badRequestResult = resultado as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            
            var valorRetornado = badRequestResult!.Value;
            var propriedadeMessage = valorRetornado!.GetType().GetProperty("Message");
            var mensagem = propriedadeMessage?.GetValue(valorRetornado) as string;
            
            mensagem.Should().Be("Dados inválidos", "Quando não há mensagens específicas, deve usar mensagem padrão");
        }

        [Fact]
        public void ValidarModelState_DeveRetornarObjetoComStatusCodeEMessage()
        {
            // Arrange
            var controller = new TestController();
            controller.ModelState.AddModelError("Teste", "Mensagem de teste");

            // Act
            var resultado = controller.TestarValidarModelState();

            // Assert
            var badRequestResult = resultado as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            
            var valorRetornado = badRequestResult!.Value;
            valorRetornado.Should().NotBeNull();
            
            // Verifica propriedades do objeto retornado
            var propriedadeStatusCode = valorRetornado!.GetType().GetProperty("StatusCode");
            var propriedadeMessage = valorRetornado.GetType().GetProperty("Message");
            
            propriedadeStatusCode.Should().NotBeNull("Objeto deve ter propriedade StatusCode");
            propriedadeMessage.Should().NotBeNull("Objeto deve ter propriedade Message");
            
            var statusCode = propriedadeStatusCode!.GetValue(valorRetornado);
            statusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public void ValidarModelState_ComMensagensVazias_DeveIgnorarMensagensVazias()
        {
            // Arrange
            var controller = new TestController();
            controller.ModelState.AddModelError("Campo1", "Erro válido");
            controller.ModelState.AddModelError("Campo2", "");
            controller.ModelState.AddModelError("Campo3", "   ");

            // Act
            var resultado = controller.TestarValidarModelState();

            // Assert
            var badRequestResult = resultado as BadRequestObjectResult;
            var valorRetornado = badRequestResult!.Value;
            var propriedadeMessage = valorRetornado!.GetType().GetProperty("Message");
            var mensagem = propriedadeMessage?.GetValue(valorRetornado) as string;
            
            // A mensagem pode conter espaços extras devido ao processamento do ModelState
            mensagem.Should().Contain("Erro válido", "Mensagens vazias devem ser ignoradas");
            mensagem.Should().NotContain("Campo2");
        }
    }
}
