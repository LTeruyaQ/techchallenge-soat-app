using Adapters.Gateways;
using Core.DTOs.Entidades.Usuarios;
using Core.Entidades;
using Core.Enumeradores;
using Core.Especificacoes.Usuario;
using Core.Interfaces.Repositorios;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace MecanicaOS.UnitTests.Adapters.Gateways
{
    public class UsuarioGatewayTests
    {
        private readonly IRepositorio<UsuarioEntityDto> _repositorio;
        private readonly UsuarioGateway _gateway;

        public UsuarioGatewayTests()
        {
            _repositorio = Substitute.For<IRepositorio<UsuarioEntityDto>>();
            _gateway = new UsuarioGateway(_repositorio);
        }

        [Fact]
        public async Task CadastrarAsync_DeveConverterParaDtoERetornarUsuarioConvertido()
        {
            // Arrange
            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Email = "admin@teste.com",
                Senha = "SenhaHasheada",
                TipoUsuario = TipoUsuario.Admin,
                RecebeAlertaEstoque = true,
                Ativo = true,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            var dtoRetornado = new UsuarioEntityDto
            {
                Id = usuario.Id,
                Email = usuario.Email,
                Senha = usuario.Senha,
                TipoUsuario = usuario.TipoUsuario,
                RecebeAlertaEstoque = usuario.RecebeAlertaEstoque,
                Ativo = usuario.Ativo,
                DataCadastro = usuario.DataCadastro,
                DataAtualizacao = usuario.DataAtualizacao
            };

            _repositorio.CadastrarAsync(Arg.Any<UsuarioEntityDto>()).Returns(Task.FromResult(dtoRetornado));

            // Act
            var resultado = await _gateway.CadastrarAsync(usuario);

            // Assert
            await _repositorio.Received(1).CadastrarAsync(Arg.Any<UsuarioEntityDto>());
            resultado.Should().NotBeNull();
            resultado.Id.Should().Be(usuario.Id);
            resultado.Email.Should().Be(usuario.Email);
            resultado.TipoUsuario.Should().Be(usuario.TipoUsuario);
        }

        [Fact]
        public async Task EditarAsync_DeveConverterParaDtoEChamarRepositorio()
        {
            // Arrange
            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Email = "adminatualizado@teste.com",
                Senha = "NovaSenhaHasheada",
                TipoUsuario = TipoUsuario.Admin,
                RecebeAlertaEstoque = false,
                Ativo = true,
                DataCadastro = DateTime.Now.AddDays(-10),
                DataAtualizacao = DateTime.Now
            };

            // Act
            await _gateway.EditarAsync(usuario);

            // Assert
            await _repositorio.Received(1).EditarAsync(Arg.Any<UsuarioEntityDto>());
        }

        [Fact]
        public async Task DeletarAsync_DeveConverterParaDtoEChamarRepositorio()
        {
            // Arrange
            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Email = "admin@teste.com",
                Senha = "SenhaHasheada",
                TipoUsuario = TipoUsuario.Admin,
                DataCadastro = DateTime.Now
            };

            // Act
            await _gateway.DeletarAsync(usuario);

            // Assert
            await _repositorio.Received(1).DeletarAsync(Arg.Any<UsuarioEntityDto>());
        }

        [Fact]
        public async Task ObterPorIdAsync_ComIdExistente_DeveRetornarUsuarioConvertido()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new UsuarioEntityDto
            {
                Id = id,
                Email = "admin@teste.com",
                Senha = "SenhaHasheada",
                TipoUsuario = TipoUsuario.Admin,
                RecebeAlertaEstoque = true,
                Ativo = true,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            _repositorio.ObterPorIdSemRastreamentoAsync(id).Returns(Task.FromResult<UsuarioEntityDto?>(dto));

            // Act
            var resultado = await _gateway.ObterPorIdAsync(id);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(dto.Id);
            resultado.Email.Should().Be(dto.Email);
            resultado.TipoUsuario.Should().Be(dto.TipoUsuario);
            resultado.RecebeAlertaEstoque.Should().Be(dto.RecebeAlertaEstoque);
        }

        [Fact]
        public async Task ObterPorIdAsync_ComIdInexistente_DeveRetornarNull()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repositorio.ObterPorIdSemRastreamentoAsync(id).Returns(Task.FromResult<UsuarioEntityDto?>(null));

            // Act
            var resultado = await _gateway.ObterPorIdAsync(id);

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task ObterTodosAsync_DeveRetornarTodosUsuariosConvertidos()
        {
            // Arrange
            var dtos = new List<UsuarioEntityDto>
            {
                new UsuarioEntityDto
                {
                    Id = Guid.NewGuid(),
                    Email = "admin@teste.com",
                    Senha = "SenhaHasheada1",
                    TipoUsuario = TipoUsuario.Admin,
                    RecebeAlertaEstoque = true,
                    DataCadastro = DateTime.Now
                },
                new UsuarioEntityDto
                {
                    Id = Guid.NewGuid(),
                    Email = "cliente@teste.com",
                    Senha = "SenhaHasheada2",
                    TipoUsuario = TipoUsuario.Cliente,
                    RecebeAlertaEstoque = false,
                    DataCadastro = DateTime.Now
                }
            };

            _repositorio.ObterTodosAsync().Returns(Task.FromResult<IEnumerable<UsuarioEntityDto>>(dtos));

            // Act
            var resultado = await _gateway.ObterTodosAsync();

            // Assert
            resultado.Should().HaveCount(2);
            resultado.First().Email.Should().Be("admin@teste.com");
            resultado.Last().Email.Should().Be("cliente@teste.com");
        }

        [Fact]
        public async Task ObterPorEmailAsync_ComEmailExistente_DeveRetornarUsuario()
        {
            // Arrange
            var email = "admin@teste.com";
            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Email = email,
                Senha = "SenhaHasheada",
                TipoUsuario = TipoUsuario.Admin,
                DataCadastro = DateTime.Now
            };

            _repositorio.ObterUmProjetadoSemRastreamentoAsync<Usuario>(Arg.Any<ObterUsuarioPorEmailEspecificacao>())
                .Returns(Task.FromResult<Usuario?>(usuario));

            // Act
            var resultado = await _gateway.ObterPorEmailAsync(email);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Email.Should().Be(email);
        }

        [Fact]
        public async Task ObterUsuarioParaAlertaEstoqueAsync_DeveChamarRepositorioComEspecificacao()
        {
            // Arrange
            var usuarios = new List<Usuario>
            {
                new Usuario
                {
                    Id = Guid.NewGuid(),
                    Email = "admin@teste.com",
                    TipoUsuario = TipoUsuario.Admin,
                    RecebeAlertaEstoque = true,
                    DataCadastro = DateTime.Now
                }
            };

            _repositorio.ListarProjetadoAsync<Usuario>(Arg.Any<ObterUsuarioParaAlertaEstoqueEspecificacao>())
                .Returns(Task.FromResult<IEnumerable<Usuario>>(usuarios));

            // Act
            var resultado = await _gateway.ObterUsuarioParaAlertaEstoqueAsync();

            // Assert
            resultado.Should().HaveCount(1);
            resultado.First().RecebeAlertaEstoque.Should().BeTrue();
        }

        [Fact]
        public void ToDto_DeveConverterUsuarioParaDto()
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
                ClienteId = Guid.NewGuid(),
                Ativo = true,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            // Act
            var dto = UsuarioGateway.ToDto(usuario);

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(usuario.Id);
            dto.Email.Should().Be(usuario.Email);
            dto.Senha.Should().Be(usuario.Senha);
            dto.DataUltimoAcesso.Should().Be(usuario.DataUltimoAcesso);
            dto.TipoUsuario.Should().Be(usuario.TipoUsuario);
            dto.RecebeAlertaEstoque.Should().Be(usuario.RecebeAlertaEstoque);
            dto.ClienteId.Should().Be(usuario.ClienteId);
            dto.Ativo.Should().Be(usuario.Ativo);
            dto.DataCadastro.Should().Be(usuario.DataCadastro);
            dto.DataAtualizacao.Should().Be(usuario.DataAtualizacao);
        }

        [Fact]
        public void FromDto_DeveConverterDtoParaUsuario()
        {
            // Arrange
            var dto = new UsuarioEntityDto
            {
                Id = Guid.NewGuid(),
                Email = "admin@teste.com",
                Senha = "SenhaHasheada",
                DataUltimoAcesso = DateTime.Now.AddDays(-1),
                TipoUsuario = TipoUsuario.Admin,
                RecebeAlertaEstoque = true,
                ClienteId = Guid.NewGuid(),
                Ativo = true,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            // Act
            var usuario = UsuarioGateway.FromDto(dto);

            // Assert
            usuario.Should().NotBeNull();
            usuario.Id.Should().Be(dto.Id);
            usuario.Email.Should().Be(dto.Email);
            usuario.Senha.Should().Be(dto.Senha);
            usuario.DataUltimoAcesso.Should().Be(dto.DataUltimoAcesso);
            usuario.TipoUsuario.Should().Be(dto.TipoUsuario);
            usuario.RecebeAlertaEstoque.Should().Be(dto.RecebeAlertaEstoque);
            usuario.ClienteId.Should().Be(dto.ClienteId);
            usuario.Ativo.Should().Be(dto.Ativo);
            usuario.DataCadastro.Should().Be(dto.DataCadastro);
            usuario.DataAtualizacao.Should().Be(dto.DataAtualizacao);
        }
    }
}
