using Core.DTOs.UseCases.Autenticacao;
using Core.Entidades;
using Core.Enumeradores;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.UseCases.Autenticacao.AutenticarUsuario;

namespace MecanicaOS.UnitTests.Core.UseCases.Autenticacao.AutenticarUsuario
{
    /// <summary>
    /// Testes para AutenticarUsuarioHandler
    /// ROI CRÍTICO: Autenticação é a porta de entrada do sistema - segurança máxima necessária.
    /// Importância: Valida credenciais, gera tokens JWT e define permissões de acesso.
    /// </summary>
    public class AutenticarUsuarioHandlerTests
    {
        private readonly ISegurancaGateway _segurancaGateway;
        private readonly ILogGateway<AutenticarUsuarioHandler> _logGateway;
        private readonly IUnidadeDeTrabalhoGateway _udtGateway;
        private readonly IUsuarioLogadoServicoGateway _usuarioLogadoGateway;

        public AutenticarUsuarioHandlerTests()
        {
            _segurancaGateway = Substitute.For<ISegurancaGateway>();
            _logGateway = Substitute.For<ILogGateway<AutenticarUsuarioHandler>>();
            _udtGateway = Substitute.For<IUnidadeDeTrabalhoGateway>();
            _usuarioLogadoGateway = Substitute.For<IUsuarioLogadoServicoGateway>();
        }

        /// <summary>
        /// Verifica se autentica usuário Admin com credenciais válidas
        /// Importância: CRÍTICA - Valida login de administradores
        /// Contribuição: Garante acesso seguro ao sistema
        /// </summary>
        [Fact]
        public async Task Handle_ComUsuarioAdminValido_DeveRetornarTokenComPermissaoAdministrador()
        {
            // Arrange
            var handler = new AutenticarUsuarioHandler(_segurancaGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Email = "admin@teste.com",
                Senha = "senhaHash",
                TipoUsuario = TipoUsuario.Admin,
                Ativo = true
            };
            var request = new AutenticacaoUseCaseDto
            {
                Email = "admin@teste.com",
                Senha = "senha123",
                UsuarioExistente = usuario
            };

            _segurancaGateway.VerificarSenha(request.Senha, usuario.Senha).Returns(true);
            _segurancaGateway.GerarToken(
                Arg.Any<Guid>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Guid?>(),
                Arg.Any<IEnumerable<string>>()
            ).Returns("token-jwt-admin");

            // Act
            var resultado = await handler.Handle(request);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Token.Should().Be("token-jwt-admin");
            resultado.Usuario.Should().Be(usuario);
            resultado.Permissoes.Should().Contain("administrador");
            resultado.Permissoes.Should().HaveCount(1);
        }

        /// <summary>
        /// Verifica se autentica usuário Cliente com credenciais válidas
        /// Importância: CRÍTICA - Valida login de clientes
        /// Contribuição: Garante acesso seguro de clientes
        /// </summary>
        [Fact]
        public async Task Handle_ComUsuarioClienteValido_DeveRetornarTokenComPermissaoCliente()
        {
            // Arrange
            var handler = new AutenticarUsuarioHandler(_segurancaGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Email = "cliente@teste.com",
                Senha = "senhaHash",
                TipoUsuario = TipoUsuario.Cliente,
                Ativo = true,
                ClienteId = Guid.NewGuid()
            };
            var request = new AutenticacaoUseCaseDto
            {
                Email = "cliente@teste.com",
                Senha = "senha123",
                UsuarioExistente = usuario
            };

            _segurancaGateway.VerificarSenha(request.Senha, usuario.Senha).Returns(true);
            _segurancaGateway.GerarToken(
                Arg.Any<Guid>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Guid?>(),
                Arg.Any<IEnumerable<string>>()
            ).Returns("token-jwt-cliente");

            // Act
            var resultado = await handler.Handle(request);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Token.Should().Be("token-jwt-cliente");
            resultado.Usuario.Should().Be(usuario);
            resultado.Permissoes.Should().Contain("cliente");
            resultado.Permissoes.Should().HaveCount(1);
        }

        /// <summary>
        /// Verifica se rejeita autenticação com email vazio
        /// Importância: ALTA - Validação de entrada obrigatória
        /// Contribuição: Previne tentativas de login inválidas
        /// </summary>
        [Theory]
        [InlineData(null, "senha123")]
        [InlineData("", "senha123")]
        [InlineData("   ", "senha123")]
        public async Task Handle_ComEmailInvalido_DeveLancarDadosInvalidosException(string email, string senha)
        {
            // Arrange
            var handler = new AutenticarUsuarioHandler(_segurancaGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var request = new AutenticacaoUseCaseDto
            {
                Email = email,
                Senha = senha,
                UsuarioExistente = new Usuario { Id = Guid.NewGuid() }
            };

            // Act & Assert
            await handler.Invoking(h => h.Handle(request))
                .Should().ThrowAsync<DadosInvalidosException>()
                .WithMessage("Email e senha são obrigatórios");
        }

        /// <summary>
        /// Verifica se rejeita autenticação com senha vazia
        /// Importância: ALTA - Validação de entrada obrigatória
        /// Contribuição: Previne tentativas de login inválidas
        /// </summary>
        [Theory]
        [InlineData("usuario@teste.com", null)]
        [InlineData("usuario@teste.com", "")]
        [InlineData("usuario@teste.com", "   ")]
        public async Task Handle_ComSenhaInvalida_DeveLancarDadosInvalidosException(string email, string senha)
        {
            // Arrange
            var handler = new AutenticarUsuarioHandler(_segurancaGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var request = new AutenticacaoUseCaseDto
            {
                Email = email,
                Senha = senha,
                UsuarioExistente = new Usuario { Id = Guid.NewGuid() }
            };

            // Act & Assert
            await handler.Invoking(h => h.Handle(request))
                .Should().ThrowAsync<DadosInvalidosException>()
                .WithMessage("Email e senha são obrigatórios");
        }

        /// <summary>
        /// Verifica se rejeita autenticação quando usuário não existe
        /// Importância: CRÍTICA - Segurança contra ataques
        /// Contribuição: Previne enumeração de usuários
        /// </summary>
        [Fact]
        public async Task Handle_ComUsuarioInexistente_DeveLancarDadosInvalidosException()
        {
            // Arrange
            var handler = new AutenticarUsuarioHandler(_segurancaGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var request = new AutenticacaoUseCaseDto
            {
                Email = "inexistente@teste.com",
                Senha = "senha123",
                UsuarioExistente = null
            };

            // Act & Assert
            await handler.Invoking(h => h.Handle(request))
                .Should().ThrowAsync<DadosInvalidosException>()
                .WithMessage("Usuário ou senha inválidos");
        }

        /// <summary>
        /// Verifica se rejeita autenticação com senha incorreta
        /// Importância: CRÍTICA - Segurança de autenticação
        /// Contribuição: Previne acesso não autorizado
        /// </summary>
        [Fact]
        public async Task Handle_ComSenhaIncorreta_DeveLancarDadosInvalidosException()
        {
            // Arrange
            var handler = new AutenticarUsuarioHandler(_segurancaGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Email = "usuario@teste.com",
                Senha = "senhaHashCorreta",
                TipoUsuario = TipoUsuario.Admin,
                Ativo = true
            };
            var request = new AutenticacaoUseCaseDto
            {
                Email = "usuario@teste.com",
                Senha = "senhaErrada",
                UsuarioExistente = usuario
            };

            _segurancaGateway.VerificarSenha(request.Senha, usuario.Senha).Returns(false);

            // Act & Assert
            await handler.Invoking(h => h.Handle(request))
                .Should().ThrowAsync<DadosInvalidosException>()
                .WithMessage("Usuário ou senha inválidos");
        }

        /// <summary>
        /// Verifica se o token gerado contém as informações corretas
        /// Importância: ALTA - Validação de geração de token
        /// Contribuição: Garante que JWT contém dados corretos
        /// </summary>
        [Fact]
        public async Task Handle_ComCredenciaisValidas_DeveGerarTokenComInformacoesCorretas()
        {
            // Arrange
            var handler = new AutenticarUsuarioHandler(_segurancaGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var usuarioId = Guid.NewGuid();
            var usuario = new Usuario
            {
                Id = usuarioId,
                Email = "usuario@teste.com",
                Senha = "senhaHash",
                TipoUsuario = TipoUsuario.Admin,
                Ativo = true
            };
            var request = new AutenticacaoUseCaseDto
            {
                Email = "usuario@teste.com",
                Senha = "senha123",
                UsuarioExistente = usuario
            };

            _segurancaGateway.VerificarSenha(request.Senha, usuario.Senha).Returns(true);
            _segurancaGateway.GerarToken(
                Arg.Any<Guid>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Guid?>(),
                Arg.Any<IEnumerable<string>>()
            ).Returns("token-jwt");

            // Act
            var resultado = await handler.Handle(request);

            // Assert
            _segurancaGateway.Received(1).GerarToken(
                usuarioId,
                "usuario@teste.com",
                "Admin",
                null,
                Arg.Is<IEnumerable<string>>(p => p.Contains("administrador"))
            );
        }

        /// <summary>
        /// Verifica se diferentes tipos de usuário recebem permissões corretas
        /// Importância: CRÍTICA - Controle de acesso baseado em roles
        /// Contribuição: Garante segregação de permissões
        /// </summary>
        [Theory]
        [InlineData(TipoUsuario.Admin, "administrador")]
        [InlineData(TipoUsuario.Cliente, "cliente")]
        public async Task Handle_ComDiferentesTiposUsuario_DeveRetornarPermissoesCorretas(TipoUsuario tipo, string permissaoEsperada)
        {
            // Arrange
            var handler = new AutenticarUsuarioHandler(_segurancaGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Email = "usuario@teste.com",
                Senha = "senhaHash",
                TipoUsuario = tipo,
                Ativo = true
            };
            var request = new AutenticacaoUseCaseDto
            {
                Email = "usuario@teste.com",
                Senha = "senha123",
                UsuarioExistente = usuario
            };

            _segurancaGateway.VerificarSenha(request.Senha, usuario.Senha).Returns(true);
            _segurancaGateway.GerarToken(
                Arg.Any<Guid>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Guid?>(),
                Arg.Any<IEnumerable<string>>()
            ).Returns("token-jwt");

            // Act
            var resultado = await handler.Handle(request);

            // Assert
            resultado.Permissoes.Should().Contain(permissaoEsperada);
            resultado.Permissoes.Should().HaveCount(1);
        }
    }
}
