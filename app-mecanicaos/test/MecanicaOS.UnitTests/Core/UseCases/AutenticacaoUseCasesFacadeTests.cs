using Core.DTOs.UseCases.Autenticacao;
using Core.Entidades;
using Core.Interfaces.Handlers.Autenticacao;
using Core.UseCases.Autenticacao;

namespace MecanicaOS.UnitTests.Core.UseCases
{
    /// <summary>
    /// Testes para AutenticacaoUseCasesFacade
    /// Importância: Valida delegação correta para handler de Autenticação
    /// </summary>
    public class AutenticacaoUseCasesFacadeTests
    {
        [Fact]
        public async Task AutenticarUseCaseAsync_DeveDelegarParaHandler()
        {
            // Arrange
            var handlerMock = Substitute.For<IAutenticarUsuarioHandler>();
            var request = new AutenticacaoUseCaseDto 
            { 
                Email = "usuario@teste.com", 
                Senha = "senha123" 
            };
            
            var autenticacaoDto = new AutenticacaoDto
            {
                Token = "token-jwt-valido",
                Usuario = new Usuario 
                { 
                    Id = Guid.NewGuid(), 
                    Email = request.Email 
                }
            };
            
            handlerMock.Handle(request).Returns(autenticacaoDto);
            
            var facade = new AutenticacaoUseCasesFacade(handlerMock);
            
            // Act
            var resultado = await facade.AutenticarUseCaseAsync(request);
            
            // Assert
            resultado.Should().Be(autenticacaoDto);
            await handlerMock.Received(1).Handle(request);
        }
    }
}
