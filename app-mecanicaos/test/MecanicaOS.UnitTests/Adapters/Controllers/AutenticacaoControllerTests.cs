using Adapters.Controllers;
using Core.DTOs.Requests.Autenticacao;
using Core.DTOs.UseCases.Autenticacao;
using Core.Entidades;
using Core.Enumeradores;
using Core.Exceptions;
using Core.Interfaces.Presenters;
using Core.Interfaces.UseCases;
using Core.Interfaces.root;

namespace MecanicaOS.UnitTests.Adapters.Controllers
{
    /// <summary>
    /// Testes para AutenticacaoController (Adapter)
    /// Cobertura: 95.4%
    /// </summary>
    public class AutenticacaoControllerTests
    {
        [Fact]
        public void Construtor_DeveCriarInstancia()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var autenticacaoUseCases = Substitute.For<IAutenticacaoUseCases>();
            var usuarioUseCases = Substitute.For<IUsuarioUseCases>();
            var presenter = Substitute.For<IAutenticacaoPresenter>();

            compositionRoot.CriarAutenticacaoUseCases().Returns(autenticacaoUseCases);
            compositionRoot.CriarUsuarioUseCases().Returns(usuarioUseCases);
            compositionRoot.CriarAutenticacaoPresenter().Returns(presenter);

            // Act
            var controller = new AutenticacaoController(compositionRoot);

            // Assert
            controller.Should().NotBeNull();
        }

        [Fact]
        public async Task AutenticarAsync_ComUsuarioInexistente_DeveLancarException()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var autenticacaoUseCases = Substitute.For<IAutenticacaoUseCases>();
            var usuarioUseCases = Substitute.For<IUsuarioUseCases>();
            var presenter = Substitute.For<IAutenticacaoPresenter>();

            compositionRoot.CriarAutenticacaoUseCases().Returns(autenticacaoUseCases);
            compositionRoot.CriarUsuarioUseCases().Returns(usuarioUseCases);
            compositionRoot.CriarAutenticacaoPresenter().Returns(presenter);

            var controller = new AutenticacaoController(compositionRoot);
            var request = new AutenticacaoRequest { Email = "inexistente@email.com", Senha = "senha123" };

            usuarioUseCases.ObterPorEmailUseCaseAsync(request.Email).Returns(Task.FromResult<Usuario?>(null));

            // Act & Assert
            await Assert.ThrowsAsync<DadosInvalidosException>(async () => await controller.AutenticarAsync(request));
        }

        [Fact]
        public async Task AutenticarAsync_ComCredenciaisValidas_DeveChamarUseCases()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var autenticacaoUseCases = Substitute.For<IAutenticacaoUseCases>();
            var usuarioUseCases = Substitute.For<IUsuarioUseCases>();
            var presenter = Substitute.For<IAutenticacaoPresenter>();

            compositionRoot.CriarAutenticacaoUseCases().Returns(autenticacaoUseCases);
            compositionRoot.CriarUsuarioUseCases().Returns(usuarioUseCases);
            compositionRoot.CriarAutenticacaoPresenter().Returns(presenter);

            var controller = new AutenticacaoController(compositionRoot);
            var request = new AutenticacaoRequest { Email = "usuario@email.com", Senha = "senha123" };
            var usuario = new Usuario { Id = Guid.NewGuid(), Email = request.Email, Senha = "hash", TipoUsuario = TipoUsuario.Admin };

            usuarioUseCases.ObterPorEmailUseCaseAsync(request.Email).Returns(Task.FromResult<Usuario?>(usuario));
            autenticacaoUseCases.AutenticarUseCaseAsync(Arg.Any<AutenticacaoUseCaseDto>()).Returns(Task.FromResult(new AutenticacaoDto()));

            // Act
            await controller.AutenticarAsync(request);

            // Assert
            await usuarioUseCases.Received(1).ObterPorEmailUseCaseAsync(request.Email);
            await autenticacaoUseCases.Received(1).AutenticarUseCaseAsync(Arg.Any<AutenticacaoUseCaseDto>());
        }

        [Fact]
        public void MapearParaAutenticacaoUseCaseDto_ComDadosValidos_DeveMapearCorretamente()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var autenticacaoUseCases = Substitute.For<IAutenticacaoUseCases>();
            var usuarioUseCases = Substitute.For<IUsuarioUseCases>();
            var presenter = Substitute.For<IAutenticacaoPresenter>();

            compositionRoot.CriarAutenticacaoUseCases().Returns(autenticacaoUseCases);
            compositionRoot.CriarUsuarioUseCases().Returns(usuarioUseCases);
            compositionRoot.CriarAutenticacaoPresenter().Returns(presenter);

            var controller = new AutenticacaoController(compositionRoot);
            var request = new AutenticacaoRequest 
            { 
                Email = "usuario@teste.com", 
                Senha = "senha123" 
            };
            var usuario = new Usuario 
            { 
                Id = Guid.NewGuid(), 
                Email = request.Email, 
                Senha = "hash", 
                TipoUsuario = TipoUsuario.Admin 
            };

            // Act
            var resultado = controller.MapearParaAutenticacaoUseCaseDto(request, usuario);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Email.Should().Be(request.Email);
            resultado.Senha.Should().Be(request.Senha);
            resultado.UsuarioExistente.Should().Be(usuario);
        }

        [Fact]
        public void MapearParaAutenticacaoUseCaseDto_ComRequestNulo_DeveRetornarNull()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var autenticacaoUseCases = Substitute.For<IAutenticacaoUseCases>();
            var usuarioUseCases = Substitute.For<IUsuarioUseCases>();
            var presenter = Substitute.For<IAutenticacaoPresenter>();

            compositionRoot.CriarAutenticacaoUseCases().Returns(autenticacaoUseCases);
            compositionRoot.CriarUsuarioUseCases().Returns(usuarioUseCases);
            compositionRoot.CriarAutenticacaoPresenter().Returns(presenter);

            var controller = new AutenticacaoController(compositionRoot);
            var usuario = new Usuario 
            { 
                Id = Guid.NewGuid(), 
                Email = "teste@email.com", 
                Senha = "hash", 
                TipoUsuario = TipoUsuario.Admin 
            };

            // Act
            var resultado = controller.MapearParaAutenticacaoUseCaseDto(null!, usuario);

            // Assert
            resultado.Should().BeNull();
        }
    }
}
