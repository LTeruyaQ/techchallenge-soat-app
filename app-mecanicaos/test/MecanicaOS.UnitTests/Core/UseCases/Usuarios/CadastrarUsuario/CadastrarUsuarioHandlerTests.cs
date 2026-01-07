using Core.DTOs.UseCases.Usuario;
using Core.Entidades;
using Core.Enumeradores;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.UseCases.Usuarios.CadastrarUsuario;
using MecanicaOS.UnitTests.Fixtures;

namespace MecanicaOS.UnitTests.Core.UseCases.Usuarios.CadastrarUsuario
{
    /// <summary>
    /// Testes unitários para o handler CadastrarUsuarioHandler
    /// </summary>
    public class CadastrarUsuarioHandlerTests
    {
        /// <summary>
        /// Verifica se o handler cadastra um usuário administrador com sucesso
        /// </summary>
        [Fact]
        public async Task Handle_ComUsuarioAdministrador_DeveCadastrarUsuario()
        {
            // Arrange
            var usuarioGatewayMock = UsuarioHandlerFixture.CriarUsuarioGatewayMock();
            var unidadeDeTrabalhoMock = UsuarioHandlerFixture.CriarUnidadeDeTrabalhMock();
            var servicoSenhaMock = UsuarioHandlerFixture.CriarServicoSenhaMock();
            
            var usuario = UsuarioHandlerFixture.CriarUsuario();
            var usuarioDto = new CadastrarUsuarioUseCaseDto
            {
                Email = usuario.Email,
                Senha = "senha123",
                TipoUsuario = TipoUsuario.Admin,
                RecebeAlertaEstoque = false,
                ClienteId = null
            };
            
            usuarioGatewayMock.ObterPorEmailAsync(Arg.Any<string>()).Returns((Usuario?)null);
            usuarioGatewayMock.CadastrarAsync(Arg.Any<Usuario>()).Returns(usuario);
            unidadeDeTrabalhoMock.Commit().Returns(true);
            
            var logGatewayMock = Substitute.For<ILogGateway<CadastrarUsuarioHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new CadastrarUsuarioHandler(
                usuarioGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock,
                servicoSenhaMock);
            
            // Act
            var resultado = await handler.Handle(usuarioDto);
            
            // Assert
            resultado.Should().NotBeNull("o resultado não deve ser nulo");
            resultado.Id.Should().NotBeEmpty("o ID deve ser gerado");
            resultado.Email.Should().Be(usuario.Email, "o email deve corresponder ao usuário cadastrado");
            resultado.Senha.Should().BeEmpty("a senha não deve ser retornada");
            resultado.TipoUsuario.Should().Be(TipoUsuario.Admin, "o tipo de usuário deve ser Admin");
            
            await usuarioGatewayMock.Received(1).ObterPorEmailAsync(Arg.Is<string>(e => e == usuarioDto.Email));
            await usuarioGatewayMock.Received(1).CadastrarAsync(Arg.Any<Usuario>());
            await unidadeDeTrabalhoMock.Received(1).Commit();
            servicoSenhaMock.Received(1).CriptografarSenha(Arg.Is<string>(s => s == usuarioDto.Senha));
        }

        /// <summary>
        /// Verifica se o handler cadastra um usuário cliente com ClienteId
        /// </summary>
        [Fact]
        public async Task Handle_ComUsuarioCliente_DeveVincularClienteId()
        {
            // Arrange
            var usuarioGatewayMock = UsuarioHandlerFixture.CriarUsuarioGatewayMock();
            var unidadeDeTrabalhoMock = UsuarioHandlerFixture.CriarUnidadeDeTrabalhMock();
            var servicoSenhaMock = UsuarioHandlerFixture.CriarServicoSenhaMock();
            
            var clienteId = Guid.NewGuid();
            var usuario = UsuarioHandlerFixture.CriarUsuario();
            usuario.TipoUsuario = TipoUsuario.Cliente;
            usuario.ClienteId = clienteId;
            
            var usuarioDto = new CadastrarUsuarioUseCaseDto
            {
                Email = usuario.Email,
                Senha = "senha123",
                TipoUsuario = TipoUsuario.Cliente,
                RecebeAlertaEstoque = true,
                ClienteId = clienteId // Controller já resolveu
            };
            
            usuarioGatewayMock.ObterPorEmailAsync(Arg.Any<string>()).Returns((Usuario?)null);
            usuarioGatewayMock.CadastrarAsync(Arg.Any<Usuario>()).Returns(usuario);
            unidadeDeTrabalhoMock.Commit().Returns(true);
            
            var logGatewayMock = Substitute.For<ILogGateway<CadastrarUsuarioHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new CadastrarUsuarioHandler(
                usuarioGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock,
                servicoSenhaMock);
            
            // Act
            var resultado = await handler.Handle(usuarioDto);
            
            // Assert
            resultado.Should().NotBeNull("o resultado não deve ser nulo");
            resultado.ClienteId.Should().Be(clienteId, "o ClienteId deve estar vinculado");
            resultado.TipoUsuario.Should().Be(TipoUsuario.Cliente, "o tipo de usuário deve ser Cliente");
            resultado.RecebeAlertaEstoque.Should().BeTrue("deve receber alerta de estoque");
            resultado.Senha.Should().BeEmpty("a senha não deve ser retornada");
            
            await usuarioGatewayMock.Received(1).CadastrarAsync(Arg.Any<Usuario>());
        }

        /// <summary>
        /// Verifica se o handler lança exceção quando tenta cadastrar usuário com email já existente
        /// </summary>
        [Fact]
        public async Task Handle_ComEmailJaCadastrado_DeveLancarDadosJaCadastradosException()
        {
            // Arrange
            var usuarioGatewayMock = UsuarioHandlerFixture.CriarUsuarioGatewayMock();
            var unidadeDeTrabalhoMock = UsuarioHandlerFixture.CriarUnidadeDeTrabalhMock();
            var servicoSenhaMock = UsuarioHandlerFixture.CriarServicoSenhaMock();
            
            var usuarioExistente = UsuarioHandlerFixture.CriarUsuario();
            var usuarioDto = UsuarioHandlerFixture.CriarCadastrarUsuarioUseCaseDto();
            
            usuarioGatewayMock.ObterPorEmailAsync(Arg.Any<string>()).Returns(usuarioExistente);
            
            var logGatewayMock = Substitute.For<ILogGateway<CadastrarUsuarioHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new CadastrarUsuarioHandler(
                usuarioGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock,
                servicoSenhaMock);
            
            // Act
            Func<Task> act = async () => await handler.Handle(usuarioDto);
            
            // Assert
            await act.Should().ThrowAsync<DadosJaCadastradosException>()
                .WithMessage("Usuário já cadastrado");
            
            await usuarioGatewayMock.Received(1).ObterPorEmailAsync(Arg.Is<string>(e => e == usuarioDto.Email));
            await usuarioGatewayMock.DidNotReceive().CadastrarAsync(Arg.Any<Usuario>());
            await unidadeDeTrabalhoMock.DidNotReceive().Commit();
        }

        /// <summary>
        /// Verifica se o handler lança exceção quando ocorre erro ao persistir
        /// </summary>
        [Fact]
        public async Task Handle_QuandoOcorreErroAoPersistir_DeveLancarPersistirDadosException()
        {
            // Arrange
            var usuarioGatewayMock = UsuarioHandlerFixture.CriarUsuarioGatewayMock();
            var unidadeDeTrabalhoMock = UsuarioHandlerFixture.CriarUnidadeDeTrabalhMock();
            var servicoSenhaMock = UsuarioHandlerFixture.CriarServicoSenhaMock();
            
            var usuario = UsuarioHandlerFixture.CriarUsuario();
            var usuarioDto = UsuarioHandlerFixture.CriarCadastrarUsuarioUseCaseDto();
            
            usuarioGatewayMock.ObterPorEmailAsync(Arg.Any<string>()).Returns((Usuario?)null);
            usuarioGatewayMock.CadastrarAsync(Arg.Any<Usuario>()).Returns(usuario);
            unidadeDeTrabalhoMock.Commit().Returns(false);
            
            var logGatewayMock = Substitute.For<ILogGateway<CadastrarUsuarioHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new CadastrarUsuarioHandler(
                usuarioGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock,
                servicoSenhaMock);
            
            // Act
            Func<Task> act = async () => await handler.Handle(usuarioDto);
            
            // Assert
            await act.Should().ThrowAsync<PersistirDadosException>()
                .WithMessage("Erro ao cadastrar usuário");
            
            await usuarioGatewayMock.Received(1).CadastrarAsync(Arg.Any<Usuario>());
            await unidadeDeTrabalhoMock.Received(1).Commit();
        }

        /// <summary>
        /// Verifica se a senha é criptografada antes de cadastrar
        /// </summary>
        [Fact]
        public async Task Handle_DeveCriptografarSenhaAntesDeCadastrar()
        {
            // Arrange
            var usuarioGatewayMock = UsuarioHandlerFixture.CriarUsuarioGatewayMock();
            var unidadeDeTrabalhoMock = UsuarioHandlerFixture.CriarUnidadeDeTrabalhMock();
            var servicoSenhaMock = UsuarioHandlerFixture.CriarServicoSenhaMock();
            
            var usuario = UsuarioHandlerFixture.CriarUsuario();
            var usuarioDto = UsuarioHandlerFixture.CriarCadastrarUsuarioUseCaseDto();
            
            usuarioGatewayMock.ObterPorEmailAsync(Arg.Any<string>()).Returns((Usuario?)null);
            usuarioGatewayMock.CadastrarAsync(Arg.Any<Usuario>()).Returns(usuario);
            unidadeDeTrabalhoMock.Commit().Returns(true);
            
            var logGatewayMock = Substitute.For<ILogGateway<CadastrarUsuarioHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new CadastrarUsuarioHandler(
                usuarioGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock,
                servicoSenhaMock);
            
            // Act
            await handler.Handle(usuarioDto);
            
            // Assert
            servicoSenhaMock.Received(1).CriptografarSenha(Arg.Is<string>(s => s == usuarioDto.Senha));
        }

        /// <summary>
        /// Verifica se a senha é removida do resultado (segurança)
        /// </summary>
        [Fact]
        public async Task Handle_DeveRemoverSenhaDoResultado()
        {
            // Arrange
            var usuarioGatewayMock = UsuarioHandlerFixture.CriarUsuarioGatewayMock();
            var unidadeDeTrabalhoMock = UsuarioHandlerFixture.CriarUnidadeDeTrabalhMock();
            var servicoSenhaMock = UsuarioHandlerFixture.CriarServicoSenhaMock();
            
            var usuario = UsuarioHandlerFixture.CriarUsuario();
            var usuarioDto = UsuarioHandlerFixture.CriarCadastrarUsuarioUseCaseDto();
            
            usuarioGatewayMock.ObterPorEmailAsync(Arg.Any<string>()).Returns((Usuario?)null);
            usuarioGatewayMock.CadastrarAsync(Arg.Any<Usuario>()).Returns(usuario);
            unidadeDeTrabalhoMock.Commit().Returns(true);
            
            var logGatewayMock = Substitute.For<ILogGateway<CadastrarUsuarioHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new CadastrarUsuarioHandler(
                usuarioGatewayMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock,
                servicoSenhaMock);
            
            // Act
            var resultado = await handler.Handle(usuarioDto);
            
            // Assert
            resultado.Senha.Should().BeEmpty("a senha não deve ser retornada por segurança");
        }
    }
}
