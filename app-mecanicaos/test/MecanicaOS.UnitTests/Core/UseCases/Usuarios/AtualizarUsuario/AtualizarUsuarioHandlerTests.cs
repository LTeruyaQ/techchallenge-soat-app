using Core.DTOs.UseCases.Usuario;
using Core.Entidades;
using Core.Enumeradores;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.UseCases.Usuarios.AtualizarUsuario;
using MecanicaOS.UnitTests.Fixtures;

namespace MecanicaOS.UnitTests.Core.UseCases.Usuarios.AtualizarUsuario
{
    /// <summary>
    /// Testes unitários para o handler AtualizarUsuarioHandler
    /// </summary>
    public class AtualizarUsuarioHandlerTests
    {
        /// <summary>
        /// Verifica se o handler atualiza um usuário com sucesso
        /// </summary>
        [Fact]
        public async Task Handle_ComDadosValidos_DeveAtualizarUsuario()
        {
            // Arrange
            var usuarioGatewayMock = UsuarioHandlerFixture.CriarUsuarioGatewayMock();
            var unidadeDeTrabalhoMock = UsuarioHandlerFixture.CriarUnidadeDeTrabalhMock();
            var servicoSenhaMock = UsuarioHandlerFixture.CriarServicoSenhaMock();
            
            var usuario = UsuarioHandlerFixture.CriarUsuario();
            var usuarioDto = UsuarioHandlerFixture.CriarAtualizarUsuarioUseCaseDto();
            
            usuarioGatewayMock.ObterPorIdAsync(usuario.Id).Returns(usuario);
            unidadeDeTrabalhoMock.Commit().Returns(true);
            
            var logGatewayMock = Substitute.For<ILogGateway<AtualizarUsuarioHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new AtualizarUsuarioHandler(
                usuarioGatewayMock,
                servicoSenhaMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            var resultado = await handler.Handle(usuario.Id, usuarioDto);
            
            // Assert
            resultado.Should().NotBeNull("o resultado não deve ser nulo");
            resultado.Id.Should().Be(usuario.Id, "o ID deve ser preservado");
            resultado.Email.Should().Be(usuarioDto.Email, "o email deve ser atualizado");
            resultado.Senha.Should().BeEmpty("a senha não deve ser retornada");
            
            await usuarioGatewayMock.Received(1).ObterPorIdAsync(usuario.Id);
            await usuarioGatewayMock.Received(1).EditarAsync(Arg.Any<Usuario>());
            await unidadeDeTrabalhoMock.Received(1).Commit();
        }

        /// <summary>
        /// Verifica se o handler lança exceção quando usuário não é encontrado
        /// </summary>
        [Fact]
        public async Task Handle_ComUsuarioInexistente_DeveLancarDadosNaoEncontradosException()
        {
            // Arrange
            var usuarioGatewayMock = UsuarioHandlerFixture.CriarUsuarioGatewayMock();
            var unidadeDeTrabalhoMock = UsuarioHandlerFixture.CriarUnidadeDeTrabalhMock();
            var servicoSenhaMock = UsuarioHandlerFixture.CriarServicoSenhaMock();
            
            var usuarioId = Guid.NewGuid();
            var usuarioDto = UsuarioHandlerFixture.CriarAtualizarUsuarioUseCaseDto();
            
            usuarioGatewayMock.ObterPorIdAsync(usuarioId).Returns((Usuario?)null);
            
            var logGatewayMock = Substitute.For<ILogGateway<AtualizarUsuarioHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new AtualizarUsuarioHandler(
                usuarioGatewayMock,
                servicoSenhaMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            Func<Task> act = async () => await handler.Handle(usuarioId, usuarioDto);
            
            // Assert
            await act.Should().ThrowAsync<DadosNaoEncontradosException>()
                .WithMessage("Usuário não encontrado");
            
            await usuarioGatewayMock.Received(1).ObterPorIdAsync(usuarioId);
            await usuarioGatewayMock.DidNotReceive().EditarAsync(Arg.Any<Usuario>());
        }

        /// <summary>
        /// Verifica se o handler mantém a senha existente quando não é fornecida nova senha
        /// </summary>
        [Fact]
        public async Task Handle_SemNovaSenha_DeveManterSenhaExistente()
        {
            // Arrange
            var usuarioGatewayMock = UsuarioHandlerFixture.CriarUsuarioGatewayMock();
            var unidadeDeTrabalhoMock = UsuarioHandlerFixture.CriarUnidadeDeTrabalhMock();
            var servicoSenhaMock = UsuarioHandlerFixture.CriarServicoSenhaMock();
            
            var usuario = UsuarioHandlerFixture.CriarUsuario();
            var senhaOriginal = usuario.Senha;
            var usuarioDto = new AtualizarUsuarioUseCaseDto
            {
                Email = "novo@teste.com",
                Senha = null, // Sem nova senha
                TipoUsuario = TipoUsuario.Admin,
                RecebeAlertaEstoque = true
            };
            
            usuarioGatewayMock.ObterPorIdAsync(usuario.Id).Returns(usuario);
            unidadeDeTrabalhoMock.Commit().Returns(true);
            
            var logGatewayMock = Substitute.For<ILogGateway<AtualizarUsuarioHandler>>();
            var usuarioLogadoServicoGatewayMock = Substitute.For<IUsuarioLogadoServicoGateway>();
            
            var handler = new AtualizarUsuarioHandler(
                usuarioGatewayMock,
                servicoSenhaMock,
                logGatewayMock,
                unidadeDeTrabalhoMock,
                usuarioLogadoServicoGatewayMock);
            
            // Act
            await handler.Handle(usuario.Id, usuarioDto);
            
            // Assert
            servicoSenhaMock.DidNotReceive().CriptografarSenha(Arg.Any<string>());
        }
    }
}
