using Adapters.Presenters;
using Core.DTOs.Requests.Usuario;
using Core.Entidades;
using Core.Enumeradores;
using FluentAssertions;
using Xunit;

namespace MecanicaOS.UnitTests.Adapters.Presenters
{
    public class UsuarioPresenterTests
    {
        private readonly UsuarioPresenter _presenter;

        public UsuarioPresenterTests()
        {
            _presenter = new UsuarioPresenter();
        }

        [Fact]
        public void ParaResponse_ComUsuarioValido_DeveConverterCorretamente()
        {
            // Arrange
            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Email = "admin@teste.com",
                Senha = "SenhaHasheada",
                DataUltimoAcesso = DateTime.Now.AddDays(-1),
                TipoUsuario = TipoUsuario.Admin,
                RecebeAlertaEstoque = true,
                Ativo = true,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now.AddDays(1)
            };

            // Act
            var response = _presenter.ParaResponse(usuario);

            // Assert
            response.Should().NotBeNull();
            response!.Id.Should().Be(usuario.Id);
            response.Email.Should().Be(usuario.Email);
            response.Senha.Should().Be(usuario.Senha);
            response.DataUltimoAcesso.Should().Be(usuario.DataUltimoAcesso);
            response.TipoUsuario.Should().Be(usuario.TipoUsuario);
            response.RecebeAlertaEstoque.Should().Be(usuario.RecebeAlertaEstoque);
            response.Ativo.Should().Be(usuario.Ativo);
            response.DataCadastro.Should().Be(usuario.DataCadastro);
            response.DataAtualizacao.Should().Be(usuario.DataAtualizacao);
        }

        [Fact]
        public void ParaResponse_ComUsuarioNulo_DeveRetornarNull()
        {
            // Arrange
            Usuario usuario = null!;

            // Act
            var response = _presenter.ParaResponse(usuario);

            // Assert
            response.Should().BeNull();
        }

        [Fact]
        public void ParaResponse_ComListaDeUsuarios_DeveConverterTodos()
        {
            // Arrange
            var usuarios = new List<Usuario>
            {
                new Usuario
                {
                    Id = Guid.NewGuid(),
                    Email = "admin@teste.com",
                    Senha = "SenhaHasheada1",
                    TipoUsuario = TipoUsuario.Admin,
                    RecebeAlertaEstoque = true,
                    DataCadastro = DateTime.Now
                },
                new Usuario
                {
                    Id = Guid.NewGuid(),
                    Email = "cliente@teste.com",
                    Senha = "SenhaHasheada2",
                    TipoUsuario = TipoUsuario.Cliente,
                    RecebeAlertaEstoque = false,
                    DataCadastro = DateTime.Now
                }
            };

            // Act
            var responses = _presenter.ParaResponse(usuarios);

            // Assert
            responses.Should().HaveCount(2);
            responses.First()!.Email.Should().Be("admin@teste.com");
            responses.Last()!.Email.Should().Be("cliente@teste.com");
        }

        [Fact]
        public void ParaResponse_ComListaNula_DeveRetornarListaVazia()
        {
            // Arrange
            IEnumerable<Usuario> usuarios = null!;

            // Act
            var responses = _presenter.ParaResponse(usuarios);

            // Assert
            responses.Should().NotBeNull();
            responses.Should().BeEmpty();
        }

        [Fact]
        public void ParaUseCaseDto_ComCadastrarUsuarioRequest_DeveConverterCorretamente()
        {
            // Arrange
            var request = new CadastrarUsuarioRequest
            {
                Email = "novoadmin@teste.com",
                Senha = "Senha@123",
                TipoUsuario = TipoUsuario.Admin,
                RecebeAlertaEstoque = true,
                Documento = "12345678900"
            };

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().NotBeNull();
            dto!.Email.Should().Be(request.Email);
            dto.Senha.Should().Be(request.Senha);
            dto.TipoUsuario.Should().Be(request.TipoUsuario);
            dto.RecebeAlertaEstoque.Should().Be(request.RecebeAlertaEstoque);
            dto.Documento.Should().Be(request.Documento);
        }

        [Fact]
        public void ParaUseCaseDto_ComCadastrarUsuarioRequestNulo_DeveRetornarNull()
        {
            // Arrange
            CadastrarUsuarioRequest request = null!;

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().BeNull();
        }

        [Fact]
        public void ParaUseCaseDto_ComAtualizarUsuarioRequest_DeveConverterCorretamente()
        {
            // Arrange
            var request = new AtualizarUsuarioRequest
            {
                Email = "adminatualizado@teste.com",
                Senha = "NovaSenha@123",
                DataUltimoAcesso = DateTime.Now,
                TipoUsuario = TipoUsuario.Admin,
                RecebeAlertaEstoque = false
            };

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().NotBeNull();
            dto!.Email.Should().Be(request.Email);
            dto.Senha.Should().Be(request.Senha);
            dto.DataUltimoAcesso.Should().Be(request.DataUltimoAcesso);
            dto.TipoUsuario.Should().Be(request.TipoUsuario);
            dto.RecebeAlertaEstoque.Should().Be(request.RecebeAlertaEstoque);
        }

        [Fact]
        public void ParaUseCaseDto_ComAtualizarUsuarioRequestNulo_DeveRetornarNull()
        {
            // Arrange
            AtualizarUsuarioRequest request = null!;

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().BeNull();
        }

        [Fact]
        public void ParaUseCaseDto_ComAtualizarUsuarioRequest_ComValoresNulos_DeveConverterCorretamente()
        {
            // Arrange
            var request = new AtualizarUsuarioRequest
            {
                Email = null,
                Senha = null,
                DataUltimoAcesso = null,
                TipoUsuario = null,
                RecebeAlertaEstoque = null
            };

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().NotBeNull();
            dto!.Email.Should().BeNull();
            dto.Senha.Should().BeNull();
            dto.DataUltimoAcesso.Should().BeNull();
            dto.TipoUsuario.Should().BeNull();
            dto.RecebeAlertaEstoque.Should().BeNull();
        }
    }
}
