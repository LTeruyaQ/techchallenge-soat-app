using Core.DTOs.UseCases.Usuario;
using Core.Entidades;
using Core.Enumeradores;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.Interfaces.Servicos;
using Core.UseCases.Usuarios.AtualizarUsuario;
using Core.UseCases.Usuarios.CadastrarUsuario;
using Core.UseCases.Usuarios.DeletarUsuario;
using Core.UseCases.Usuarios.ObterTodosUsuarios;
using Core.UseCases.Usuarios.ObterUsuario;
using Core.UseCases.Usuarios.ObterUsuarioPorEmail;

namespace MecanicaOS.UnitTests.Core.UseCases.Usuarios
{
    /// <summary>
    /// Testes para handlers de Usuários
    /// Importância CRÍTICA: Valida lógica de negócio de gestão de usuários.
    /// Usuários são fundamentais para segurança e autenticação do sistema.
    /// ROI ALTO: Testa ~600 linhas de código crítico para segurança.
    /// </summary>
    public class UsuariosHandlersTests
    {
        private readonly IUsuarioGateway _usuarioGateway;
        private readonly ILogGateway<CadastrarUsuarioHandler> _logGateway;
        private readonly IUnidadeDeTrabalhoGateway _udtGateway;
        private readonly IUsuarioLogadoServicoGateway _usuarioLogadoGateway;
        private readonly IServicoSenha _servicoSenha;

        public UsuariosHandlersTests()
        {
            _usuarioGateway = Substitute.For<IUsuarioGateway>();
            _logGateway = Substitute.For<ILogGateway<CadastrarUsuarioHandler>>();
            _udtGateway = Substitute.For<IUnidadeDeTrabalhoGateway>();
            _usuarioLogadoGateway = Substitute.For<IUsuarioLogadoServicoGateway>();
            _servicoSenha = Substitute.For<IServicoSenha>();

            // Configurar commit para retornar true por padrão
            _udtGateway.Commit().Returns(Task.FromResult(true));
        }

        #region CadastrarUsuarioHandler

        /// <summary>
        /// Verifica se CadastrarUsuario cria usuário Admin corretamente
        /// Importância: Valida criação de usuários administrativos
        /// </summary>
        [Fact]
        public async Task CadastrarUsuario_ComUsuarioAdmin_DeveCadastrarComSucesso()
        {
            // Arrange
            var handler = new CadastrarUsuarioHandler(
                _usuarioGateway, _logGateway, _udtGateway, _usuarioLogadoGateway, _servicoSenha);

            var request = new CadastrarUsuarioUseCaseDto
            {
                Email = "admin@teste.com",
                Senha = "senha123",
                TipoUsuario = TipoUsuario.Admin,
                RecebeAlertaEstoque = true,
                ClienteId = null
            };

            _usuarioGateway.ObterPorEmailAsync(request.Email).Returns(Task.FromResult<Usuario?>(null));
            _servicoSenha.CriptografarSenha(request.Senha).Returns("senha_criptografada");
            _usuarioGateway.CadastrarAsync(Arg.Any<Usuario>()).Returns(Task.FromResult(new Usuario()));

            // Act
            var resultado = await handler.Handle(request);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Email.Should().Be(request.Email);
            resultado.TipoUsuario.Should().Be(TipoUsuario.Admin);
            resultado.RecebeAlertaEstoque.Should().BeTrue();
            resultado.Senha.Should().BeEmpty("senha não deve ser retornada");

            await _usuarioGateway.Received(1).CadastrarAsync(Arg.Any<Usuario>());
            _servicoSenha.Received(1).CriptografarSenha(request.Senha);
        }

        /// <summary>
        /// Verifica se CadastrarUsuario lança exceção para email já cadastrado
        /// Importância: Valida regra de negócio de unicidade de email
        /// </summary>
        [Fact]
        public async Task CadastrarUsuario_ComEmailJaCadastrado_DeveLancarExcecao()
        {
            // Arrange
            var handler = new CadastrarUsuarioHandler(
                _usuarioGateway, _logGateway, _udtGateway, _usuarioLogadoGateway, _servicoSenha);

            var request = new CadastrarUsuarioUseCaseDto
            {
                Email = "existente@teste.com",
                Senha = "senha123",
                TipoUsuario = TipoUsuario.Admin
            };

            _usuarioGateway.ObterPorEmailAsync(request.Email).Returns(Task.FromResult<Usuario?>(new Usuario()));

            // Act & Assert
            await handler.Invoking(h => h.Handle(request))
                .Should().ThrowAsync<DadosJaCadastradosException>()
                .WithMessage("Usuário já cadastrado");
        }

        /// <summary>
        /// Verifica se CadastrarUsuario lança exceção quando commit falha
        /// Importância: Valida tratamento de erro de persistência
        /// </summary>
        [Fact]
        public async Task CadastrarUsuario_ComFalhaNoCommit_DeveLancarExcecao()
        {
            // Arrange
            var handler = new CadastrarUsuarioHandler(
                _usuarioGateway, _logGateway, _udtGateway, _usuarioLogadoGateway, _servicoSenha);

            var request = new CadastrarUsuarioUseCaseDto
            {
                Email = "novo@teste.com",
                Senha = "senha123",
                TipoUsuario = TipoUsuario.Admin
            };

            _usuarioGateway.ObterPorEmailAsync(request.Email).Returns(Task.FromResult<Usuario?>(null));
            _servicoSenha.CriptografarSenha(request.Senha).Returns("senha_criptografada");
            _usuarioGateway.CadastrarAsync(Arg.Any<Usuario>()).Returns(Task.FromResult(new Usuario()));
            _udtGateway.Commit().Returns(Task.FromResult(false)); // Simula falha

            // Act & Assert
            await handler.Invoking(h => h.Handle(request))
                .Should().ThrowAsync<PersistirDadosException>()
                .WithMessage("Erro ao cadastrar usuário");
        }

        #endregion

        #region AtualizarUsuarioHandler

        /// <summary>
        /// Verifica se AtualizarUsuario atualiza dados corretamente
        /// Importância: Valida atualização de usuários
        /// </summary>
        [Fact]
        public async Task AtualizarUsuario_ComDadosValidos_DeveAtualizarComSucesso()
        {
            // Arrange
            var logGatewayAtualizar = Substitute.For<ILogGateway<AtualizarUsuarioHandler>>();
            var handler = new AtualizarUsuarioHandler(
                _usuarioGateway, _servicoSenha, logGatewayAtualizar, _udtGateway, _usuarioLogadoGateway);

            var usuarioId = Guid.NewGuid();
            var usuarioExistente = new Usuario
            {
                Id = usuarioId,
                Email = "usuario@teste.com",
                Senha = "senha_antiga",
                TipoUsuario = TipoUsuario.Admin,
                RecebeAlertaEstoque = false
            };

            var request = new AtualizarUsuarioUseCaseDto
            {
                Email = "novo@teste.com",
                Senha = "nova_senha",
                TipoUsuario = TipoUsuario.Cliente,
                RecebeAlertaEstoque = true
            };

            _usuarioGateway.ObterPorIdAsync(usuarioId).Returns(Task.FromResult<Usuario?>(usuarioExistente));
            _servicoSenha.CriptografarSenha(request.Senha).Returns("nova_senha_criptografada");
            _usuarioGateway.EditarAsync(Arg.Any<Usuario>()).Returns(Task.FromResult(new Usuario()));

            // Act
            var resultado = await handler.Handle(usuarioId, request);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Senha.Should().BeEmpty("senha não deve ser retornada");

            await _usuarioGateway.Received(1).EditarAsync(Arg.Is<Usuario>(u =>
                u.Id == usuarioId &&
                u.Email == request.Email &&
                u.TipoUsuario == request.TipoUsuario &&
                u.RecebeAlertaEstoque == true));
        }

        /// <summary>
        /// Verifica se AtualizarUsuario lança exceção para usuário inexistente
        /// Importância: Valida tratamento de erro
        /// </summary>
        [Fact]
        public async Task AtualizarUsuario_ComUsuarioInexistente_DeveLancarExcecao()
        {
            // Arrange
            var logGatewayAtualizar = Substitute.For<ILogGateway<AtualizarUsuarioHandler>>();
            var handler = new AtualizarUsuarioHandler(
                _usuarioGateway, _servicoSenha, logGatewayAtualizar, _udtGateway, _usuarioLogadoGateway);

            var usuarioId = Guid.NewGuid();
            var request = new AtualizarUsuarioUseCaseDto
            {
                Email = "novo@teste.com",
                TipoUsuario = TipoUsuario.Admin
            };

            _usuarioGateway.ObterPorIdAsync(usuarioId).Returns(Task.FromResult<Usuario?>(null));

            // Act & Assert
            await handler.Invoking(h => h.Handle(usuarioId, request))
                .Should().ThrowAsync<DadosNaoEncontradosException>()
                .WithMessage("Usuário não encontrado");
        }

        #endregion

        #region DeletarUsuarioHandler

        /// <summary>
        /// Verifica se DeletarUsuario remove usuário corretamente
        /// Importância: Valida exclusão de usuários
        /// </summary>
        [Fact]
        public async Task DeletarUsuario_ComUsuarioExistente_DeveDeletarComSucesso()
        {
            // Arrange
            var logGatewayDeletar = Substitute.For<ILogGateway<DeletarUsuarioHandler>>();
            var handler = new DeletarUsuarioHandler(
                _usuarioGateway, logGatewayDeletar, _udtGateway, _usuarioLogadoGateway);

            var usuarioId = Guid.NewGuid();
            var usuario = new Usuario { Id = usuarioId, Email = "usuario@teste.com" };

            _usuarioGateway.ObterPorIdAsync(usuarioId).Returns(Task.FromResult<Usuario?>(usuario));
            _usuarioGateway.DeletarAsync(Arg.Any<Usuario>()).Returns(Task.CompletedTask);

            // Act
            var resultado = await handler.Handle(usuarioId);

            // Assert
            resultado.Should().BeTrue();
            await _usuarioGateway.Received(1).DeletarAsync(Arg.Is<Usuario>(u => u.Id == usuarioId));
        }

        /// <summary>
        /// Verifica se DeletarUsuario lança exceção para usuário inexistente
        /// Importância: Valida tratamento de erro
        /// </summary>
        [Fact]
        public async Task DeletarUsuario_ComUsuarioInexistente_DeveLancarExcecao()
        {
            // Arrange
            var logGatewayDeletar = Substitute.For<ILogGateway<DeletarUsuarioHandler>>();
            var handler = new DeletarUsuarioHandler(
                _usuarioGateway, logGatewayDeletar, _udtGateway, _usuarioLogadoGateway);

            var usuarioId = Guid.NewGuid();
            _usuarioGateway.ObterPorIdAsync(usuarioId).Returns(Task.FromResult<Usuario?>(null));

            // Act & Assert
            await handler.Invoking(h => h.Handle(usuarioId))
                .Should().ThrowAsync<DadosNaoEncontradosException>()
                .WithMessage("Usuário não encontrado");
        }

        #endregion

        #region ObterUsuarioHandler

        /// <summary>
        /// Verifica se ObterUsuario retorna usuário sem senha
        /// Importância: Valida segurança (senha não deve ser retornada)
        /// </summary>
        [Fact]
        public async Task ObterUsuario_ComUsuarioExistente_DeveRetornarSemSenha()
        {
            // Arrange
            var logGatewayObter = Substitute.For<ILogGateway<ObterUsuarioHandler>>();
            var handler = new ObterUsuarioHandler(
                _usuarioGateway, logGatewayObter, _udtGateway, _usuarioLogadoGateway);

            var usuarioId = Guid.NewGuid();
            var usuario = new Usuario
            {
                Id = usuarioId,
                Email = "usuario@teste.com",
                Senha = "senha_criptografada",
                TipoUsuario = TipoUsuario.Admin
            };

            _usuarioGateway.ObterPorIdAsync(usuarioId).Returns(Task.FromResult<Usuario?>(usuario));

            // Act
            var resultado = await handler.Handle(usuarioId);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(usuarioId);
            resultado.Email.Should().Be("usuario@teste.com");
            resultado.Senha.Should().BeEmpty("senha não deve ser retornada");
        }

        #endregion

        #region ObterTodosUsuariosHandler

        /// <summary>
        /// Verifica se ObterTodosUsuarios retorna lista sem senhas
        /// Importância: Valida segurança em listagens
        /// </summary>
        [Fact]
        public async Task ObterTodosUsuarios_DeveRetornarListaSemSenhas()
        {
            // Arrange
            var logGatewayObterTodos = Substitute.For<ILogGateway<ObterTodosUsuariosHandler>>();
            var handler = new ObterTodosUsuariosHandler(
                _usuarioGateway, logGatewayObterTodos, _udtGateway, _usuarioLogadoGateway);

            var usuarios = new List<Usuario>
            {
                new Usuario { Id = Guid.NewGuid(), Email = "usuario1@teste.com", Senha = "senha1" },
                new Usuario { Id = Guid.NewGuid(), Email = "usuario2@teste.com", Senha = "senha2" }
            };

            _usuarioGateway.ObterTodosAsync().Returns(Task.FromResult<IEnumerable<Usuario>>(usuarios));

            // Act
            var resultado = await handler.Handle();

            // Assert
            resultado.Should().HaveCount(2);
            resultado.Should().AllSatisfy(u => u.Senha.Should().BeEmpty("senhas não devem ser retornadas"));
        }

        #endregion

        #region ObterUsuarioPorEmailHandler

        /// <summary>
        /// Verifica se ObterUsuarioPorEmail retorna usuário corretamente
        /// Importância: Valida busca por email (usado em autenticação)
        /// </summary>
        [Fact]
        public async Task ObterUsuarioPorEmail_ComEmailExistente_DeveRetornarUsuario()
        {
            // Arrange
            var logGatewayObterPorEmail = Substitute.For<ILogGateway<ObterUsuarioPorEmailHandler>>();
            var handler = new ObterUsuarioPorEmailHandler(
                _usuarioGateway, logGatewayObterPorEmail, _udtGateway, _usuarioLogadoGateway);

            var email = "usuario@teste.com";
            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Email = email,
                Senha = "senha_criptografada",
                TipoUsuario = TipoUsuario.Admin
            };

            _usuarioGateway.ObterPorEmailAsync(email).Returns(Task.FromResult<Usuario?>(usuario));

            // Act
            var resultado = await handler.Handle(email);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Email.Should().Be(email);
            // ObterUsuarioPorEmail não limpa a senha (usado internamente para autenticação)
        }

        /// <summary>
        /// Verifica se ObterUsuarioPorEmail retorna null para email inexistente
        /// Importância: Valida comportamento esperado
        /// </summary>
        [Fact]
        public async Task ObterUsuarioPorEmail_ComEmailInexistente_DeveRetornarNull()
        {
            // Arrange
            var logGatewayObterPorEmail = Substitute.For<ILogGateway<ObterUsuarioPorEmailHandler>>();
            var handler = new ObterUsuarioPorEmailHandler(
                _usuarioGateway, logGatewayObterPorEmail, _udtGateway, _usuarioLogadoGateway);

            var email = "inexistente@teste.com";
            _usuarioGateway.ObterPorEmailAsync(email).Returns(Task.FromResult<Usuario?>(null));

            // Act
            var resultado = await handler.Handle(email);

            // Assert
            resultado.Should().BeNull();
        }

        #endregion
    }
}
