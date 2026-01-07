using Adapters.Presenters;
using Core.DTOs.UseCases.Autenticacao;
using FluentAssertions;

namespace MecanicaOS.UnitTests.Adapters.Presenters
{
    /// <summary>
    /// Testes para AutenticacaoPresenter
    /// 
    /// IMPORTÂNCIA: Presenter responsável por converter DTOs de autenticação para responses da API.
    /// Garante que tokens JWT sejam formatados corretamente para o cliente.
    /// 
    /// COBERTURA: Valida conversão de AutenticacaoDto para AutenticacaoResponse.
    /// Testa cenários de sucesso e edge cases (null, valores vazios).
    /// </summary>
    public class AutenticacaoPresenterTests
    {
        [Fact]
        public void ParaResponse_ComDtoValido_DeveConverterCorretamente()
        {
            // Arrange
            var presenter = new AutenticacaoPresenter();
            var dto = new AutenticacaoDto
            {
                Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.test.token"
            };

            // Act
            var response = presenter.ParaResponse(dto);

            // Assert
            response.Should().NotBeNull();
            response!.Token.Should().Be(dto.Token);
        }

        [Fact]
        public void ParaResponse_ComDtoNull_DeveRetornarNull()
        {
            // Arrange
            var presenter = new AutenticacaoPresenter();

            // Act
            var response = presenter.ParaResponse(null!);

            // Assert
            response.Should().BeNull();
        }

        [Fact]
        public void ParaResponse_ComTokenVazio_DeveConverterComTokenVazio()
        {
            // Arrange
            var presenter = new AutenticacaoPresenter();
            var dto = new AutenticacaoDto
            {
                Token = string.Empty
            };

            // Act
            var response = presenter.ParaResponse(dto);

            // Assert
            response.Should().NotBeNull();
            response!.Token.Should().BeEmpty();
        }

        [Fact]
        public void ParaResponse_ComTokenLongo_DeveConverterCorretamente()
        {
            // Arrange
            var presenter = new AutenticacaoPresenter();
            var tokenLongo = new string('a', 1000);
            var dto = new AutenticacaoDto
            {
                Token = tokenLongo
            };

            // Act
            var response = presenter.ParaResponse(dto);

            // Assert
            response.Should().NotBeNull();
            response!.Token.Should().Be(tokenLongo);
            response.Token.Length.Should().Be(1000);
        }

        [Fact]
        public void ParaResponse_ComTokenComCaracteresEspeciais_DeveConverterCorretamente()
        {
            // Arrange
            var presenter = new AutenticacaoPresenter();
            var dto = new AutenticacaoDto
            {
                Token = "token.com-caracteres_especiais/+=123"
            };

            // Act
            var response = presenter.ParaResponse(dto);

            // Assert
            response.Should().NotBeNull();
            response!.Token.Should().Be(dto.Token);
        }

        [Fact]
        public void ParaResponse_MultiplasChamadas_DeveRetornarInstanciasIndependentes()
        {
            // Arrange
            var presenter = new AutenticacaoPresenter();
            var dto1 = new AutenticacaoDto { Token = "token1" };
            var dto2 = new AutenticacaoDto { Token = "token2" };

            // Act
            var response1 = presenter.ParaResponse(dto1);
            var response2 = presenter.ParaResponse(dto2);

            // Assert
            response1.Should().NotBeSameAs(response2);
            response1!.Token.Should().Be("token1");
            response2!.Token.Should().Be("token2");
        }

        [Fact]
        public void Presenter_DeveImplementarInterface()
        {
            // Arrange & Act
            var presenter = new AutenticacaoPresenter();

            // Assert
            presenter.Should().BeAssignableTo<global::Core.Interfaces.Presenters.IAutenticacaoPresenter>();
        }
    }
}
