using Core.DTOs.Entidades.Usuarios;
using Core.Entidades;
using Core.Enumeradores;
using Core.Interfaces.Repositorios;
using Infraestrutura.Autenticacao;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace MecanicaOS.UnitTests.Infraestrutura.Autenticacao
{
    /// <summary>
    /// Testes para UsuarioLogadoServico
    /// Importância CRÍTICA: Valida extração de informações do usuário autenticado via JWT.
    /// Garante que claims sejam lidas corretamente e que o contexto HTTP funcione adequadamente.
    /// </summary>
    public class UsuarioLogadoServicoTests
    {
        /// <summary>
        /// Verifica se o serviço identifica usuário não autenticado
        /// Importância: Valida comportamento quando não há autenticação
        /// </summary>
        [Fact]
        public void EstaAutenticado_SemUsuario_DeveRetornarFalse()
        {
            // Arrange
            var httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
            var repositorioMock = Substitute.For<IRepositorio<UsuarioEntityDto>>();
            
            httpContextAccessorMock.HttpContext.Returns((HttpContext?)null);
            
            var servico = new UsuarioLogadoServico(httpContextAccessorMock, repositorioMock);
            
            // Act
            var resultado = servico.EstaAutenticado;
            
            // Assert
            resultado.Should().BeFalse("não deve estar autenticado sem contexto HTTP");
        }

        /// <summary>
        /// Verifica se o serviço extrai UsuarioId do token JWT
        /// Importância: Valida extração de ID do usuário das claims
        /// </summary>
        [Fact]
        public void UsuarioId_ComClaimNameIdentifier_DeveRetornarId()
        {
            // Arrange
            var httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
            var repositorioMock = Substitute.For<IRepositorio<UsuarioEntityDto>>();
            
            var usuarioId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString())
            };
            
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            
            httpContextAccessorMock.HttpContext.Returns(httpContext);
            
            var servico = new UsuarioLogadoServico(httpContextAccessorMock, repositorioMock);
            
            // Act
            var resultado = servico.UsuarioId;
            
            // Assert
            resultado.Should().Be(usuarioId);
        }

        /// <summary>
        /// Verifica se o serviço extrai Email do token JWT
        /// Importância: Valida extração de email das claims
        /// </summary>
        [Fact]
        public void Email_ComClaimEmail_DeveRetornarEmail()
        {
            // Arrange
            var httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
            var repositorioMock = Substitute.For<IRepositorio<UsuarioEntityDto>>();
            
            var email = "usuario@teste.com";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, email)
            };
            
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            
            httpContextAccessorMock.HttpContext.Returns(httpContext);
            
            var servico = new UsuarioLogadoServico(httpContextAccessorMock, repositorioMock);
            
            // Act
            var resultado = servico.Email;
            
            // Assert
            resultado.Should().Be(email);
        }

        /// <summary>
        /// Verifica se o serviço extrai TipoUsuario do token JWT
        /// Importância: Valida extração de tipo de usuário das claims
        /// </summary>
        [Fact]
        public void TipoUsuario_ComClaimTipoUsuario_DeveRetornarTipo()
        {
            // Arrange
            var httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
            var repositorioMock = Substitute.For<IRepositorio<UsuarioEntityDto>>();
            
            var claims = new List<Claim>
            {
                new Claim("tipo_usuario", "Admin")
            };
            
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            
            httpContextAccessorMock.HttpContext.Returns(httpContext);
            
            var servico = new UsuarioLogadoServico(httpContextAccessorMock, repositorioMock);
            
            // Act
            var resultado = servico.TipoUsuario;
            
            // Assert
            resultado.Should().Be(TipoUsuario.Admin);
        }

        /// <summary>
        /// Verifica se o serviço valida role do usuário
        /// Importância: Valida autorização baseada em roles
        /// </summary>
        [Fact]
        public void EstaNaRole_ComRoleValida_DeveRetornarTrue()
        {
            // Arrange
            var httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
            var repositorioMock = Substitute.For<IRepositorio<UsuarioEntityDto>>();
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, "Admin")
            };
            
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            
            httpContextAccessorMock.HttpContext.Returns(httpContext);
            
            var servico = new UsuarioLogadoServico(httpContextAccessorMock, repositorioMock);
            
            // Act
            var resultado = servico.EstaNaRole("Admin");
            
            // Assert
            resultado.Should().BeTrue("o usuário está na role Admin");
        }

        /// <summary>
        /// Verifica se o serviço valida permissão do usuário
        /// Importância: Valida autorização baseada em permissões
        /// </summary>
        [Fact]
        public void PossuiPermissao_ComPermissaoValida_DeveRetornarTrue()
        {
            // Arrange
            var httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
            var repositorioMock = Substitute.For<IRepositorio<UsuarioEntityDto>>();
            
            var claims = new List<Claim>
            {
                new Claim("permissao", "criar_cliente")
            };
            
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            
            httpContextAccessorMock.HttpContext.Returns(httpContext);
            
            var servico = new UsuarioLogadoServico(httpContextAccessorMock, repositorioMock);
            
            // Act
            var resultado = servico.PossuiPermissao("criar_cliente");
            
            // Assert
            resultado.Should().BeTrue("o usuário possui a permissão");
        }

        /// <summary>
        /// Verifica se o serviço retorna todas as claims
        /// Importância: Valida acesso completo às claims do token
        /// </summary>
        [Fact]
        public void ObterTodasClaims_ComUsuarioAutenticado_DeveRetornarClaims()
        {
            // Arrange
            var httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
            var repositorioMock = Substitute.For<IRepositorio<UsuarioEntityDto>>();
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, "usuario@teste.com"),
                new Claim("tipo_usuario", "Admin")
            };
            
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            
            httpContextAccessorMock.HttpContext.Returns(httpContext);
            
            var servico = new UsuarioLogadoServico(httpContextAccessorMock, repositorioMock);
            
            // Act
            var resultado = servico.ObterTodasClaims();
            
            // Assert
            resultado.Should().HaveCount(3);
            resultado.Should().Contain(c => c.Type == ClaimTypes.Email);
        }

        /// <summary>
        /// Verifica se o serviço retorna null quando usuário não está autenticado
        /// Importância: Valida comportamento seguro sem autenticação
        /// </summary>
        [Fact]
        public void ObterUsuarioLogado_SemAutenticacao_DeveRetornarNull()
        {
            // Arrange
            var httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
            var repositorioMock = Substitute.For<IRepositorio<UsuarioEntityDto>>();
            
            httpContextAccessorMock.HttpContext.Returns((HttpContext?)null);
            
            var servico = new UsuarioLogadoServico(httpContextAccessorMock, repositorioMock);
            
            // Act
            var resultado = servico.ObterUsuarioLogado();
            
            // Assert
            resultado.Should().BeNull("não há usuário autenticado");
        }

        /// <summary>
        /// Verifica se o serviço retorna null quando claim de ID é inválida
        /// Importância: ALTA - Valida tratamento de claims malformadas
        /// Contribuição: Previne erros com tokens inválidos
        /// </summary>
        [Fact]
        public void UsuarioId_ComClaimIdInvalida_DeveRetornarNull()
        {
            // Arrange
            var httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
            var repositorioMock = Substitute.For<IRepositorio<UsuarioEntityDto>>();
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "id-invalido")
            };
            
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            
            httpContextAccessorMock.HttpContext.Returns(httpContext);
            
            var servico = new UsuarioLogadoServico(httpContextAccessorMock, repositorioMock);
            
            // Act
            var resultado = servico.UsuarioId;
            
            // Assert
            resultado.Should().BeNull();
        }

        /// <summary>
        /// Verifica se o serviço usa claim alternativa 'sub' para ID
        /// Importância: ALTA - Valida compatibilidade com diferentes formatos de JWT
        /// Contribuição: Garante suporte a tokens de diferentes provedores
        /// </summary>
        [Fact]
        public void UsuarioId_ComClaimSub_DeveRetornarId()
        {
            // Arrange
            var httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
            var repositorioMock = Substitute.For<IRepositorio<UsuarioEntityDto>>();
            
            var usuarioId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim("sub", usuarioId.ToString())
            };
            
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            
            httpContextAccessorMock.HttpContext.Returns(httpContext);
            
            var servico = new UsuarioLogadoServico(httpContextAccessorMock, repositorioMock);
            
            // Act
            var resultado = servico.UsuarioId;
            
            // Assert
            resultado.Should().Be(usuarioId);
        }

        /// <summary>
        /// Verifica se o serviço retorna nome vazio quando não há claim
        /// Importância: MÉDIA - Valida comportamento padrão
        /// Contribuição: Previne NullReferenceException
        /// </summary>
        [Fact]
        public void Nome_SemClaimNome_DeveRetornarVazio()
        {
            // Arrange
            var httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
            var repositorioMock = Substitute.For<IRepositorio<UsuarioEntityDto>>();
            
            var claims = new List<Claim>();
            
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            
            httpContextAccessorMock.HttpContext.Returns(httpContext);
            
            var servico = new UsuarioLogadoServico(httpContextAccessorMock, repositorioMock);
            
            // Act
            var resultado = servico.Nome;
            
            // Assert
            resultado.Should().BeEmpty();
        }

        /// <summary>
        /// Verifica se EstaNaRole retorna false quando usuário não tem role
        /// Importância: ALTA - Valida autorização negativa
        /// Contribuição: Garante segurança ao negar acesso sem role
        /// </summary>
        [Fact]
        public void EstaNaRole_SemRole_DeveRetornarFalse()
        {
            // Arrange
            var httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
            var repositorioMock = Substitute.For<IRepositorio<UsuarioEntityDto>>();
            
            var claims = new List<Claim>();
            
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            
            httpContextAccessorMock.HttpContext.Returns(httpContext);
            
            var servico = new UsuarioLogadoServico(httpContextAccessorMock, repositorioMock);
            
            // Act
            var resultado = servico.EstaNaRole("Admin");
            
            // Assert
            resultado.Should().BeFalse();
        }

        /// <summary>
        /// Verifica se PossuiPermissao retorna false quando usuário não tem permissão
        /// Importância: ALTA - Valida autorização negativa
        /// Contribuição: Garante segurança ao negar acesso sem permissão
        /// </summary>
        [Fact]
        public void PossuiPermissao_SemPermissao_DeveRetornarFalse()
        {
            // Arrange
            var httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
            var repositorioMock = Substitute.For<IRepositorio<UsuarioEntityDto>>();
            
            var claims = new List<Claim>();
            
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            
            httpContextAccessorMock.HttpContext.Returns(httpContext);
            
            var servico = new UsuarioLogadoServico(httpContextAccessorMock, repositorioMock);
            
            // Act
            var resultado = servico.PossuiPermissao("criar_cliente");
            
            // Assert
            resultado.Should().BeFalse();
        }

        /// <summary>
        /// Verifica se ObterTodasClaims retorna vazio quando não há usuário
        /// Importância: MÉDIA - Valida comportamento sem autenticação
        /// Contribuição: Previne NullReferenceException
        /// </summary>
        [Fact]
        public void ObterTodasClaims_SemUsuario_DeveRetornarVazio()
        {
            // Arrange
            var httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
            var repositorioMock = Substitute.For<IRepositorio<UsuarioEntityDto>>();
            
            httpContextAccessorMock.HttpContext.Returns((HttpContext?)null);
            
            var servico = new UsuarioLogadoServico(httpContextAccessorMock, repositorioMock);
            
            // Act
            var resultado = servico.ObterTodasClaims();
            
            // Assert
            resultado.Should().BeEmpty();
        }

        /// <summary>
        /// Verifica se UsuarioId usa claim alternativa 'id' quando 'sub' e 'NameIdentifier' não existem
        /// </summary>
        [Fact]
        public void UsuarioId_ComClaimId_DeveRetornarId()
        {
            // Arrange
            var httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
            var repositorioMock = Substitute.For<IRepositorio<UsuarioEntityDto>>();
            
            var usuarioId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim("id", usuarioId.ToString())
            };
            
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            
            httpContextAccessorMock.HttpContext.Returns(httpContext);
            
            var servico = new UsuarioLogadoServico(httpContextAccessorMock, repositorioMock);
            
            // Act
            var resultado = servico.UsuarioId;
            
            // Assert
            resultado.Should().Be(usuarioId);
        }

        /// <summary>
        /// Verifica se TipoUsuario retorna null quando claim tem valor inválido
        /// </summary>
        [Fact]
        public void TipoUsuario_ComClaimInvalida_DeveRetornarNull()
        {
            // Arrange
            var httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
            var repositorioMock = Substitute.For<IRepositorio<UsuarioEntityDto>>();
            
            var claims = new List<Claim>
            {
                new Claim("tipo_usuario", "TipoInvalido")
            };
            
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            
            httpContextAccessorMock.HttpContext.Returns(httpContext);
            
            var servico = new UsuarioLogadoServico(httpContextAccessorMock, repositorioMock);
            
            // Act
            var resultado = servico.TipoUsuario;
            
            // Assert
            resultado.Should().BeNull();
        }

        /// <summary>
        /// Verifica se TipoUsuario retorna null quando usuário não está autenticado
        /// </summary>
        [Fact]
        public void TipoUsuario_SemAutenticacao_DeveRetornarNull()
        {
            // Arrange
            var httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
            var repositorioMock = Substitute.For<IRepositorio<UsuarioEntityDto>>();
            
            httpContextAccessorMock.HttpContext.Returns((HttpContext?)null);
            
            var servico = new UsuarioLogadoServico(httpContextAccessorMock, repositorioMock);
            
            // Act
            var resultado = servico.TipoUsuario;
            
            // Assert
            resultado.Should().BeNull();
        }

        /// <summary>
        /// Verifica se UsuarioId retorna null quando não há claim de ID
        /// </summary>
        [Fact]
        public void UsuarioId_SemClaimId_DeveRetornarNull()
        {
            // Arrange
            var httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
            var repositorioMock = Substitute.For<IRepositorio<UsuarioEntityDto>>();
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, "teste@teste.com")
            };
            
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            
            httpContextAccessorMock.HttpContext.Returns(httpContext);
            
            var servico = new UsuarioLogadoServico(httpContextAccessorMock, repositorioMock);
            
            // Act
            var resultado = servico.UsuarioId;
            
            // Assert
            resultado.Should().BeNull();
        }

        /// <summary>
        /// Verifica se ObterUsuarioLogado retorna usuário quando autenticado e existe no repositório
        /// </summary>
        [Fact]
        public void ObterUsuarioLogado_ComUsuarioAutenticadoEExistente_DeveRetornarUsuario()
        {
            // Arrange
            var httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
            var repositorioMock = Substitute.For<IRepositorio<UsuarioEntityDto>>();
            
            var usuarioId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString())
            };
            
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            
            httpContextAccessorMock.HttpContext.Returns(httpContext);
            
            var usuarioDto = new UsuarioEntityDto
            {
                Id = usuarioId,
                Email = "teste@teste.com",
                Senha = "senha_hash",
                TipoUsuario = TipoUsuario.Admin,
                Ativo = true,
                DataCadastro = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow,
                RecebeAlertaEstoque = true,
                ClienteId = null
            };
            
            repositorioMock.ObterPorIdAsync(usuarioId).Returns(Task.FromResult<UsuarioEntityDto?>(usuarioDto));
            
            var servico = new UsuarioLogadoServico(httpContextAccessorMock, repositorioMock);
            
            // Act
            var resultado = servico.ObterUsuarioLogado();
            
            // Assert
            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(usuarioId);
            resultado.Email.Should().Be("teste@teste.com");
            resultado.TipoUsuario.Should().Be(TipoUsuario.Admin);
        }

        /// <summary>
        /// Verifica se ObterUsuarioLogado retorna null quando usuário não existe no repositório
        /// </summary>
        [Fact]
        public void ObterUsuarioLogado_ComUsuarioAutenticadoMasInexistenteNoRepo_DeveRetornarNull()
        {
            // Arrange
            var httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
            var repositorioMock = Substitute.For<IRepositorio<UsuarioEntityDto>>();
            
            var usuarioId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString())
            };
            
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            
            httpContextAccessorMock.HttpContext.Returns(httpContext);
            repositorioMock.ObterPorIdAsync(usuarioId).Returns(Task.FromResult<UsuarioEntityDto?>(null));
            
            var servico = new UsuarioLogadoServico(httpContextAccessorMock, repositorioMock);
            
            // Act
            var resultado = servico.ObterUsuarioLogado();
            
            // Assert
            resultado.Should().BeNull();
        }

        /// <summary>
        /// Verifica se UsuarioId retorna null quando usuário não está autenticado
        /// </summary>
        [Fact]
        public void UsuarioId_SemAutenticacao_DeveRetornarNull()
        {
            // Arrange
            var httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
            var repositorioMock = Substitute.For<IRepositorio<UsuarioEntityDto>>();
            
            httpContextAccessorMock.HttpContext.Returns((HttpContext?)null);
            
            var servico = new UsuarioLogadoServico(httpContextAccessorMock, repositorioMock);
            
            // Act
            var resultado = servico.UsuarioId;
            
            // Assert
            resultado.Should().BeNull();
        }

        /// <summary>
        /// Verifica se TipoUsuario retorna null quando não há claim tipo_usuario
        /// </summary>
        [Fact]
        public void TipoUsuario_SemClaimTipoUsuario_DeveRetornarNull()
        {
            // Arrange
            var httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
            var repositorioMock = Substitute.For<IRepositorio<UsuarioEntityDto>>();
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, "teste@teste.com")
            };
            
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            
            httpContextAccessorMock.HttpContext.Returns(httpContext);
            
            var servico = new UsuarioLogadoServico(httpContextAccessorMock, repositorioMock);
            
            // Act
            var resultado = servico.TipoUsuario;
            
            // Assert
            resultado.Should().BeNull();
        }
    }
}
