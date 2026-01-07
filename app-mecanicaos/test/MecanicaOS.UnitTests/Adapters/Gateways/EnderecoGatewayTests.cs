using Adapters.Gateways;
using Core.DTOs.Entidades.Cliente;
using Core.Entidades;
using Core.Interfaces.Repositorios;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace MecanicaOS.UnitTests.Adapters.Gateways
{
    public class EnderecoGatewayTests
    {
        private readonly IRepositorio<EnderecoEntityDto> _repositorio;
        private readonly EnderecoGateway _gateway;

        public EnderecoGatewayTests()
        {
            _repositorio = Substitute.For<IRepositorio<EnderecoEntityDto>>();
            _gateway = new EnderecoGateway(_repositorio);
        }

        [Fact]
        public async Task CadastrarAsync_DeveConverterParaDtoEChamarRepositorio()
        {
            // Arrange
            var endereco = new Endereco
            {
                Id = Guid.NewGuid(),
                Rua = "Rua Teste",
                Numero = "123",
                Bairro = "Centro",
                Cidade = "São Paulo",
                CEP = "01000-000",
                Complemento = "Apto 10",
                IdCliente = Guid.NewGuid(),
                Ativo = true,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            EnderecoEntityDto? dtoCapturado = null;
            await _repositorio.CadastrarAsync(Arg.Do<EnderecoEntityDto>(dto => dtoCapturado = dto));

            // Act
            await _gateway.CadastrarAsync(endereco);

            // Assert
            await _repositorio.Received(1).CadastrarAsync(Arg.Any<EnderecoEntityDto>());
            dtoCapturado.Should().NotBeNull();
            dtoCapturado!.Id.Should().Be(endereco.Id);
            dtoCapturado.Rua.Should().Be(endereco.Rua);
            dtoCapturado.Numero.Should().Be(endereco.Numero);
            dtoCapturado.Bairro.Should().Be(endereco.Bairro);
            dtoCapturado.Cidade.Should().Be(endereco.Cidade);
            dtoCapturado.CEP.Should().Be(endereco.CEP);
            dtoCapturado.Complemento.Should().Be(endereco.Complemento);
            dtoCapturado.IdCliente.Should().Be(endereco.IdCliente);
            dtoCapturado.Ativo.Should().Be(endereco.Ativo);
            dtoCapturado.DataCadastro.Should().Be(endereco.DataCadastro);
            dtoCapturado.DataAtualizacao.Should().Be(endereco.DataAtualizacao);
        }

        [Fact]
        public async Task EditarAsync_DeveConverterParaDtoEChamarRepositorio()
        {
            // Arrange
            var endereco = new Endereco
            {
                Id = Guid.NewGuid(),
                Rua = "Rua Atualizada",
                Numero = "456",
                Bairro = "Bairro Novo",
                Cidade = "Rio de Janeiro",
                CEP = "20000-000",
                Complemento = "Casa",
                IdCliente = Guid.NewGuid(),
                Ativo = true,
                DataCadastro = DateTime.Now.AddDays(-10),
                DataAtualizacao = DateTime.Now
            };

            EnderecoEntityDto? dtoCapturado = null;
            await _repositorio.EditarAsync(Arg.Do<EnderecoEntityDto>(dto => dtoCapturado = dto));

            // Act
            await _gateway.EditarAsync(endereco);

            // Assert
            await _repositorio.Received(1).EditarAsync(Arg.Any<EnderecoEntityDto>());
            dtoCapturado.Should().NotBeNull();
            dtoCapturado!.Id.Should().Be(endereco.Id);
            dtoCapturado.Rua.Should().Be(endereco.Rua);
            dtoCapturado.Numero.Should().Be(endereco.Numero);
        }

        [Fact]
        public async Task ObterPorIdAsync_ComIdExistente_DeveRetornarEnderecoConvertido()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new EnderecoEntityDto
            {
                Id = id,
                Rua = "Rua Teste",
                Numero = "123",
                Bairro = "Centro",
                Cidade = "São Paulo",
                CEP = "01000-000",
                Complemento = "Apto 10",
                IdCliente = Guid.NewGuid(),
                Ativo = true,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            _repositorio.ObterPorIdAsync(id).Returns(Task.FromResult<EnderecoEntityDto?>(dto));

            // Act
            var resultado = await _gateway.ObterPorIdAsync(id);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(dto.Id);
            resultado.Rua.Should().Be(dto.Rua);
            resultado.Numero.Should().Be(dto.Numero);
            resultado.Bairro.Should().Be(dto.Bairro);
            resultado.Cidade.Should().Be(dto.Cidade);
            resultado.CEP.Should().Be(dto.CEP);
            resultado.Complemento.Should().Be(dto.Complemento);
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
            _repositorio.ObterPorIdAsync(id).Returns(Task.FromResult<EnderecoEntityDto?>(null));

            // Act
            var resultado = await _gateway.ObterPorIdAsync(id);

            // Assert
            resultado.Should().BeNull();
            await _repositorio.Received(1).ObterPorIdAsync(id);
        }

        [Fact]
        public void ToDto_DeveConverterEnderecoParaDto()
        {
            // Arrange
            var endereco = new Endereco
            {
                Id = Guid.NewGuid(),
                Rua = "Rua Teste",
                Numero = "123",
                Bairro = "Centro",
                Cidade = "São Paulo",
                CEP = "01000-000",
                Complemento = "Apto 10",
                IdCliente = Guid.NewGuid(),
                Ativo = true,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            // Act
            var dto = EnderecoGateway.ToDto(endereco);

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(endereco.Id);
            dto.Rua.Should().Be(endereco.Rua);
            dto.Numero.Should().Be(endereco.Numero);
            dto.Bairro.Should().Be(endereco.Bairro);
            dto.Cidade.Should().Be(endereco.Cidade);
            dto.CEP.Should().Be(endereco.CEP);
            dto.Complemento.Should().Be(endereco.Complemento);
            dto.IdCliente.Should().Be(endereco.IdCliente);
            dto.Ativo.Should().Be(endereco.Ativo);
            dto.DataCadastro.Should().Be(endereco.DataCadastro);
            dto.DataAtualizacao.Should().Be(endereco.DataAtualizacao);
        }

        [Fact]
        public void FromDto_DeveConverterDtoParaEndereco()
        {
            // Arrange
            var dto = new EnderecoEntityDto
            {
                Id = Guid.NewGuid(),
                Rua = "Rua Teste",
                Numero = "123",
                Bairro = "Centro",
                Cidade = "São Paulo",
                CEP = "01000-000",
                Complemento = "Apto 10",
                IdCliente = Guid.NewGuid(),
                Ativo = true,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            // Act
            var endereco = EnderecoGateway.FromDto(dto);

            // Assert
            endereco.Should().NotBeNull();
            endereco.Id.Should().Be(dto.Id);
            endereco.Rua.Should().Be(dto.Rua);
            endereco.Numero.Should().Be(dto.Numero);
            endereco.Bairro.Should().Be(dto.Bairro);
            endereco.Cidade.Should().Be(dto.Cidade);
            endereco.CEP.Should().Be(dto.CEP);
            endereco.Complemento.Should().Be(dto.Complemento);
            endereco.IdCliente.Should().Be(dto.IdCliente);
            endereco.Ativo.Should().Be(dto.Ativo);
            endereco.DataCadastro.Should().Be(dto.DataCadastro);
            endereco.DataAtualizacao.Should().Be(dto.DataAtualizacao);
        }
    }
}
