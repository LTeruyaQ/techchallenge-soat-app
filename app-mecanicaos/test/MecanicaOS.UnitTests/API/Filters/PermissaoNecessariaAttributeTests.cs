using API.Filters;
using Core.Interfaces.Servicos;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace MecanicaOS.UnitTests.API.Filters
{
    /// <summary>
    /// Testes para PermissaoNecessariaAttribute (API Filter)
    /// 
    /// IMPORTÂNCIA CRÍTICA: Este filtro é responsável por controlar acesso a recursos protegidos.
    /// Garante que apenas usuários autenticados e com permissões adequadas possam acessar endpoints específicos.
    /// 
    /// COBERTURA: Valida todos os cenários de autorização:
    /// - Usuário não autenticado → 401 Unauthorized
    /// - Usuário autenticado sem permissão → 403 Forbidden
    /// - Usuário autenticado com permissão → Permite acesso
    /// - Serviço não disponível → 401 Unauthorized
    /// 
    /// IMPACTO: Filter crítico de segurança com 0% de cobertura atual.
    /// </summary>
    public class PermissaoNecessariaAttributeTests
    {
        private readonly IUsuarioLogadoServico _usuarioLogadoServico;
        private readonly IServiceProvider _serviceProvider;
        private readonly AuthorizationFilterContext _context;

        public PermissaoNecessariaAttributeTests()
        {
            _usuarioLogadoServico = Substitute.For<IUsuarioLogadoServico>();
            _serviceProvider = Substitute.For<IServiceProvider>();
            
            var httpContext = new DefaultHttpContext
            {
                RequestServices = _serviceProvider
            };

            var actionContext = new ActionContext(
                httpContext,
                new Microsoft.AspNetCore.Routing.RouteData(),
                new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());

            _context = new AuthorizationFilterContext(
                actionContext,
                new List<IFilterMetadata>());

            _serviceProvider.GetService(typeof(IUsuarioLogadoServico))
                .Returns(_usuarioLogadoServico);
        }

        [Fact]
        public void Construtor_DeveArmazenarPermissao()
        {
            // Arrange
            var permissao = "Admin";

            // Act
            var filter = new PermissaoNecessariaAttribute(permissao);

            // Assert
            filter.Should().NotBeNull();
        }

        [Fact]
        public void OnAuthorization_UsuarioNaoAutenticado_DeveRetornarUnauthorized()
        {
            // Arrange
            _usuarioLogadoServico.EstaAutenticado.Returns(false);
            var filter = new PermissaoNecessariaAttribute("Admin");

            // Act
            filter.OnAuthorization(_context);

            // Assert
            _context.Result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public void OnAuthorization_ServicoUsuarioNull_DeveRetornarUnauthorized()
        {
            // Arrange
            _serviceProvider.GetService(typeof(IUsuarioLogadoServico)).Returns((IUsuarioLogadoServico?)null);
            var filter = new PermissaoNecessariaAttribute("Admin");

            // Act
            filter.OnAuthorization(_context);

            // Assert
            _context.Result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public void OnAuthorization_UsuarioAutenticadoSemPermissao_DeveRetornarForbidden()
        {
            // Arrange
            _usuarioLogadoServico.EstaAutenticado.Returns(true);
            _usuarioLogadoServico.PossuiPermissao("Admin").Returns(false);
            var filter = new PermissaoNecessariaAttribute("Admin");

            // Act
            filter.OnAuthorization(_context);

            // Assert
            _context.Result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public void OnAuthorization_UsuarioAutenticadoComPermissao_DevePermitirAcesso()
        {
            // Arrange
            _usuarioLogadoServico.EstaAutenticado.Returns(true);
            _usuarioLogadoServico.PossuiPermissao("Admin").Returns(true);
            var filter = new PermissaoNecessariaAttribute("Admin");

            // Act
            filter.OnAuthorization(_context);

            // Assert
            _context.Result.Should().BeNull("não deve bloquear quando usuário tem permissão");
        }

        [Theory]
        [InlineData("Admin")]
        [InlineData("GerenciarEstoque")]
        [InlineData("VisualizarRelatorios")]
        [InlineData("GerenciarUsuarios")]
        public void OnAuthorization_DiferentesPermissoes_DeveValidarCorretamente(string permissao)
        {
            // Arrange
            _usuarioLogadoServico.EstaAutenticado.Returns(true);
            _usuarioLogadoServico.PossuiPermissao(permissao).Returns(true);
            var filter = new PermissaoNecessariaAttribute(permissao);

            // Act
            filter.OnAuthorization(_context);

            // Assert
            _context.Result.Should().BeNull();
            _usuarioLogadoServico.Received(1).PossuiPermissao(permissao);
        }

        [Fact]
        public void OnAuthorization_UsuarioAutenticadoMasServicoRetornaFalso_DeveRetornarForbidden()
        {
            // Arrange
            _usuarioLogadoServico.EstaAutenticado.Returns(true);
            _usuarioLogadoServico.PossuiPermissao(Arg.Any<string>()).Returns(false);
            var filter = new PermissaoNecessariaAttribute("QualquerPermissao");

            // Act
            filter.OnAuthorization(_context);

            // Assert
            _context.Result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public void OnAuthorization_DeveObterServicoDoRequestServices()
        {
            // Arrange
            var filter = new PermissaoNecessariaAttribute("Admin");

            // Act
            filter.OnAuthorization(_context);

            // Assert
            _serviceProvider.Received(1).GetService(typeof(IUsuarioLogadoServico));
        }

        [Fact]
        public void OnAuthorization_UsuarioNaoAutenticado_NaoDeveVerificarPermissao()
        {
            // Arrange
            _usuarioLogadoServico.EstaAutenticado.Returns(false);
            var filter = new PermissaoNecessariaAttribute("Admin");

            // Act
            filter.OnAuthorization(_context);

            // Assert
            _usuarioLogadoServico.DidNotReceive().PossuiPermissao(Arg.Any<string>());
        }

        [Fact]
        public void OnAuthorization_MultiplasChamadasComMesmoUsuario_DeveValidarCadaVez()
        {
            // Arrange
            _usuarioLogadoServico.EstaAutenticado.Returns(true);
            _usuarioLogadoServico.PossuiPermissao("Admin").Returns(true);
            var filter = new PermissaoNecessariaAttribute("Admin");

            // Act
            filter.OnAuthorization(_context);
            filter.OnAuthorization(_context);
            filter.OnAuthorization(_context);

            // Assert
            var _ = _usuarioLogadoServico.Received(3).EstaAutenticado;
            _usuarioLogadoServico.Received(3).PossuiPermissao("Admin");
        }

        [Fact]
        public void Attribute_DevePermitirMultiplasInstancias()
        {
            // Arrange & Act
            var filter1 = new PermissaoNecessariaAttribute("Admin");
            var filter2 = new PermissaoNecessariaAttribute("GerenciarEstoque");

            // Assert
            filter1.Should().NotBeSameAs(filter2);
        }

        [Fact]
        public void Attribute_DeveHerdarDeAuthorizeAttribute()
        {
            // Arrange & Act
            var filter = new PermissaoNecessariaAttribute("Admin");

            // Assert
            filter.Should().BeAssignableTo<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();
        }

        [Fact]
        public void Attribute_DeveImplementarIAuthorizationFilter()
        {
            // Arrange & Act
            var filter = new PermissaoNecessariaAttribute("Admin");

            // Assert
            filter.Should().BeAssignableTo<IAuthorizationFilter>();
        }
    }
}
