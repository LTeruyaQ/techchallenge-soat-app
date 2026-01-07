using Adapters.Gateways;
using Core.DTOs.Entidades.Cliente;
using Core.Entidades;
using Core.Interfaces.Repositorios;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace MecanicaOS.UnitTests.Adapters.Gateways
{
    public class ContatoGatewayTests
    {
        private readonly IRepositorio<ContatoEntityDto> _repositorio;
        private readonly ContatoGateway _gateway;

        public ContatoGatewayTests()
        {
            _repositorio = Substitute.For<IRepositorio<ContatoEntityDto>>();
            _gateway = new ContatoGateway(_repositorio);
        }

        [Fact]
        public async Task CadastrarAsync_DeveConverterParaDtoEChamarRepositorio()
        {
            // Arrange
            var contato = new Contato
            {
                Id = Guid.NewGuid(),
                Email = "teste@teste.com",
                Telefone = "(11) 98765-4321",
                IdCliente = Guid.NewGuid(),
                Ativo = true,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            ContatoEntityDto? dtoCapturado = null;
            await _repositorio.CadastrarAsync(Arg.Do<ContatoEntityDto>(dto => dtoCapturado = dto));

            // Act
            await _gateway.CadastrarAsync(contato);

            // Assert
            await _repositorio.Received(1).CadastrarAsync(Arg.Any<ContatoEntityDto>());
            dtoCapturado.Should().NotBeNull();
            dtoCapturado!.Id.Should().Be(contato.Id);
            dtoCapturado.Email.Should().Be(contato.Email);
            dtoCapturado.Telefone.Should().Be(contato.Telefone);
            dtoCapturado.IdCliente.Should().Be(contato.IdCliente);
            dtoCapturado.Ativo.Should().Be(contato.Ativo);
            dtoCapturado.DataCadastro.Should().Be(contato.DataCadastro);
            dtoCapturado.DataAtualizacao.Should().Be(contato.DataAtualizacao);
        }

        [Fact]
        public async Task EditarAsync_DeveConverterParaDtoEChamarRepositorio()
        {
            // Arrange
            var contato = new Contato
            {
                Id = Guid.NewGuid(),
                Email = "atualizado@teste.com",
                Telefone = "(11) 99999-8888",
                IdCliente = Guid.NewGuid(),
                Ativo = true,
                DataCadastro = DateTime.Now.AddDays(-10),
                DataAtualizacao = DateTime.Now
            };

            ContatoEntityDto? dtoCapturado = null;
            await _repositorio.EditarAsync(Arg.Do<ContatoEntityDto>(dto => dtoCapturado = dto));

            // Act
            await _gateway.EditarAsync(contato);

            // Assert
            await _repositorio.Received(1).EditarAsync(Arg.Any<ContatoEntityDto>());
            dtoCapturado.Should().NotBeNull();
            dtoCapturado!.Id.Should().Be(contato.Id);
            dtoCapturado.Email.Should().Be(contato.Email);
            dtoCapturado.Telefone.Should().Be(contato.Telefone);
            dtoCapturado.IdCliente.Should().Be(contato.IdCliente);
        }

        [Fact]
        public async Task ObterPorIdAsync_ComIdExistente_DeveRetornarContatoConvertido()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new ContatoEntityDto
            {
                Id = id,
                Email = "teste@teste.com",
                Telefone = "(11) 98765-4321",
                IdCliente = Guid.NewGuid(),
                Ativo = true,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            _repositorio.ObterPorIdAsync(id).Returns(Task.FromResult<ContatoEntityDto?>(dto));

            // Act
            var resultado = await _gateway.ObterPorIdAsync(id);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(dto.Id);
            resultado.Email.Should().Be(dto.Email);
            resultado.Telefone.Should().Be(dto.Telefone);
            resultado.IdCliente.Should().Be(dto.IdCliente);
            resultado.Ativo.Should().Be(dto.Ativo);
            resultado.DataCadastro.Should().Be(dto.DataCadastro);
            resultado.DataAtualizacao.Should().Be(dto.DataAtualizacao);
        }

        [Fact]
        public async Task ObterPorIdAsync_ComIdInexistente_DeveRetornarNull()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repositorio.ObterPorIdAsync(id).Returns(Task.FromResult<ContatoEntityDto?>(null));

            // Act
            var resultado = await _gateway.ObterPorIdAsync(id);

            // Assert
            resultado.Should().BeNull();
            await _repositorio.Received(1).ObterPorIdAsync(id);
        }

        [Fact]
        public void ToDto_DeveConverterContatoParaDto()
        {
            // Arrange
            var contato = new Contato
            {
                Id = Guid.NewGuid(),
                Email = "teste@teste.com",
                Telefone = "(11) 98765-4321",
                IdCliente = Guid.NewGuid(),
                Ativo = true,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            // Act
            var dto = ContatoGateway.ToDto(contato);

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(contato.Id);
            dto.Email.Should().Be(contato.Email);
            dto.Telefone.Should().Be(contato.Telefone);
            dto.IdCliente.Should().Be(contato.IdCliente);
            dto.Ativo.Should().Be(contato.Ativo);
            dto.DataCadastro.Should().Be(contato.DataCadastro);
            dto.DataAtualizacao.Should().Be(contato.DataAtualizacao);
        }

        [Fact]
        public void FromDto_DeveConverterDtoParaContato()
        {
            // Arrange
            var dto = new ContatoEntityDto
            {
                Id = Guid.NewGuid(),
                Email = "teste@teste.com",
                Telefone = "(11) 98765-4321",
                IdCliente = Guid.NewGuid(),
                Ativo = true,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            // Act
            var contato = ContatoGateway.FromDto(dto);

            // Assert
            contato.Should().NotBeNull();
            contato.Id.Should().Be(dto.Id);
            contato.Email.Should().Be(dto.Email);
            contato.Telefone.Should().Be(dto.Telefone);
            contato.IdCliente.Should().Be(dto.IdCliente);
            contato.Ativo.Should().Be(dto.Ativo);
            contato.DataCadastro.Should().Be(dto.DataCadastro);
            contato.DataAtualizacao.Should().Be(dto.DataAtualizacao);
        }
    }
}
