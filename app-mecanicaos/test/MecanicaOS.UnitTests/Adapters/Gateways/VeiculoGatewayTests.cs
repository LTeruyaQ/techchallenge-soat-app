using Adapters.Gateways;
using Core.DTOs.Entidades.Veiculo;
using Core.Entidades;
using Core.Especificacoes.Veiculo;
using Core.Interfaces.Repositorios;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace MecanicaOS.UnitTests.Adapters.Gateways
{
    public class VeiculoGatewayTests
    {
        private readonly IRepositorio<VeiculoEntityDto> _repositorio;
        private readonly VeiculoGateway _gateway;

        public VeiculoGatewayTests()
        {
            _repositorio = Substitute.For<IRepositorio<VeiculoEntityDto>>();
            _gateway = new VeiculoGateway(_repositorio);
        }

        [Fact]
        public async Task CadastrarAsync_DeveConverterParaDtoEChamarRepositorio()
        {
            // Arrange
            var veiculo = new Veiculo
            {
                Id = Guid.NewGuid(),
                Placa = "ABC1234",
                Marca = "Honda",
                Modelo = "Civic",
                Cor = "Preto",
                Ano = "2020",
                Anotacoes = "Veículo em bom estado",
                ClienteId = Guid.NewGuid(),
                Ativo = true,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            VeiculoEntityDto? dtoCapturado = null;
            await _repositorio.CadastrarAsync(Arg.Do<VeiculoEntityDto>(dto => dtoCapturado = dto));

            // Act
            await _gateway.CadastrarAsync(veiculo);

            // Assert
            await _repositorio.Received(1).CadastrarAsync(Arg.Any<VeiculoEntityDto>());
            dtoCapturado.Should().NotBeNull();
            dtoCapturado!.Id.Should().Be(veiculo.Id);
            dtoCapturado.Placa.Should().Be(veiculo.Placa);
            dtoCapturado.Marca.Should().Be(veiculo.Marca);
            dtoCapturado.Modelo.Should().Be(veiculo.Modelo);
            dtoCapturado.Cor.Should().Be(veiculo.Cor);
            dtoCapturado.Ano.Should().Be(veiculo.Ano);
            dtoCapturado.Anotacoes.Should().Be(veiculo.Anotacoes);
            dtoCapturado.ClienteId.Should().Be(veiculo.ClienteId);
            dtoCapturado.Ativo.Should().Be(veiculo.Ativo);
            dtoCapturado.DataCadastro.Should().Be(veiculo.DataCadastro);
            dtoCapturado.DataAtualizacao.Should().Be(veiculo.DataAtualizacao);
        }

        [Fact]
        public async Task EditarAsync_DeveConverterParaDtoEChamarRepositorio()
        {
            // Arrange
            var veiculo = new Veiculo
            {
                Id = Guid.NewGuid(),
                Placa = "XYZ5678",
                Marca = "Toyota",
                Modelo = "Corolla",
                Cor = "Branco",
                Ano = "2021",
                ClienteId = Guid.NewGuid(),
                Ativo = true,
                DataCadastro = DateTime.Now.AddDays(-10),
                DataAtualizacao = DateTime.Now
            };

            // Act
            await _gateway.EditarAsync(veiculo);

            // Assert
            await _repositorio.Received(1).EditarAsync(Arg.Any<VeiculoEntityDto>());
        }

        [Fact]
        public async Task DeletarAsync_DeveConverterParaDtoEChamarRepositorio()
        {
            // Arrange
            var veiculo = new Veiculo
            {
                Id = Guid.NewGuid(),
                Placa = "ABC1234",
                Marca = "Honda",
                Modelo = "Civic",
                Ano = "2020",
                ClienteId = Guid.NewGuid(),
                DataCadastro = DateTime.Now
            };

            // Act
            await _gateway.DeletarAsync(veiculo);

            // Assert
            await _repositorio.Received(1).DeletarAsync(Arg.Any<VeiculoEntityDto>());
        }

        [Fact]
        public async Task ObterPorIdAsync_ComIdExistente_DeveRetornarVeiculoConvertido()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new VeiculoEntityDto
            {
                Id = id,
                Placa = "ABC1234",
                Marca = "Honda",
                Modelo = "Civic",
                Cor = "Preto",
                Ano = "2020",
                Anotacoes = "Veículo em bom estado",
                ClienteId = Guid.NewGuid(),
                Ativo = true,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            _repositorio.ObterPorIdSemRastreamentoAsync(id).Returns(Task.FromResult<VeiculoEntityDto?>(dto));

            // Act
            var resultado = await _gateway.ObterPorIdAsync(id);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(dto.Id);
            resultado.Placa.Should().Be(dto.Placa);
            resultado.Marca.Should().Be(dto.Marca);
            resultado.Modelo.Should().Be(dto.Modelo);
            resultado.Cor.Should().Be(dto.Cor);
            resultado.Ano.Should().Be(dto.Ano);
        }

        [Fact]
        public async Task ObterPorIdAsync_ComIdInexistente_DeveRetornarNull()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repositorio.ObterPorIdSemRastreamentoAsync(id).Returns(Task.FromResult<VeiculoEntityDto?>(null));

            // Act
            var resultado = await _gateway.ObterPorIdAsync(id);

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task ObterTodosAsync_DeveRetornarTodosVeiculosConvertidos()
        {
            // Arrange
            var dtos = new List<VeiculoEntityDto>
            {
                new VeiculoEntityDto
                {
                    Id = Guid.NewGuid(),
                    Placa = "ABC1234",
                    Marca = "Honda",
                    Modelo = "Civic",
                    Ano = "2020",
                    ClienteId = Guid.NewGuid(),
                    DataCadastro = DateTime.Now
                },
                new VeiculoEntityDto
                {
                    Id = Guid.NewGuid(),
                    Placa = "XYZ5678",
                    Marca = "Toyota",
                    Modelo = "Corolla",
                    Ano = "2021",
                    ClienteId = Guid.NewGuid(),
                    DataCadastro = DateTime.Now
                }
            };

            _repositorio.ObterTodosAsync().Returns(Task.FromResult<IEnumerable<VeiculoEntityDto>>(dtos));

            // Act
            var resultado = await _gateway.ObterTodosAsync();

            // Assert
            resultado.Should().HaveCount(2);
            resultado.First().Placa.Should().Be("ABC1234");
            resultado.Last().Placa.Should().Be("XYZ5678");
        }

        [Fact]
        public async Task ObterVeiculoPorClienteAsync_DeveChamarRepositorioComEspecificacao()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var veiculos = new List<Veiculo>
            {
                new Veiculo
                {
                    Id = Guid.NewGuid(),
                    Placa = "ABC1234",
                    ClienteId = clienteId,
                    DataCadastro = DateTime.Now
                }
            };

            _repositorio.ListarProjetadoAsync<Veiculo>(Arg.Any<ObterVeiculoPorClienteEspecificacao>())
                .Returns(Task.FromResult<IEnumerable<Veiculo>>(veiculos));

            // Act
            var resultado = await _gateway.ObterVeiculoPorClienteAsync(clienteId);

            // Assert
            resultado.Should().HaveCount(1);
            resultado.First().ClienteId.Should().Be(clienteId);
        }

        [Fact]
        public async Task ObterVeiculoPorPlacaAsync_DeveChamarRepositorioComEspecificacao()
        {
            // Arrange
            var placa = "ABC1234";
            var veiculos = new List<Veiculo>
            {
                new Veiculo
                {
                    Id = Guid.NewGuid(),
                    Placa = placa,
                    ClienteId = Guid.NewGuid(),
                    DataCadastro = DateTime.Now
                }
            };

            _repositorio.ListarProjetadoAsync<Veiculo>(Arg.Any<ObterVeiculoPorPlacaEspecificacao>())
                .Returns(Task.FromResult<IEnumerable<Veiculo>>(veiculos));

            // Act
            var resultado = await _gateway.ObterVeiculoPorPlacaAsync(placa);

            // Assert
            resultado.Should().HaveCount(1);
            resultado.First().Placa.Should().Be(placa);
        }

        [Fact]
        public void ToDto_DeveConverterVeiculoParaDto()
        {
            // Arrange
            var veiculo = new Veiculo
            {
                Id = Guid.NewGuid(),
                Placa = "ABC1234",
                Marca = "Honda",
                Modelo = "Civic",
                Cor = "Preto",
                Ano = "2020",
                Anotacoes = "Veículo em bom estado",
                ClienteId = Guid.NewGuid(),
                Ativo = true,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            // Act
            var dto = VeiculoGateway.ToDto(veiculo);

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(veiculo.Id);
            dto.Placa.Should().Be(veiculo.Placa);
            dto.Marca.Should().Be(veiculo.Marca);
            dto.Modelo.Should().Be(veiculo.Modelo);
            dto.Cor.Should().Be(veiculo.Cor);
            dto.Ano.Should().Be(veiculo.Ano);
            dto.Anotacoes.Should().Be(veiculo.Anotacoes);
            dto.ClienteId.Should().Be(veiculo.ClienteId);
            dto.Ativo.Should().Be(veiculo.Ativo);
            dto.DataCadastro.Should().Be(veiculo.DataCadastro);
            dto.DataAtualizacao.Should().Be(veiculo.DataAtualizacao);
        }

        [Fact]
        public void FromDto_DeveConverterDtoParaVeiculo()
        {
            // Arrange
            var dto = new VeiculoEntityDto
            {
                Id = Guid.NewGuid(),
                Placa = "ABC1234",
                Marca = "Honda",
                Modelo = "Civic",
                Cor = "Preto",
                Ano = "2020",
                Anotacoes = "Veículo em bom estado",
                ClienteId = Guid.NewGuid(),
                Ativo = true,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            // Act
            var veiculo = VeiculoGateway.FromDto(dto);

            // Assert
            veiculo.Should().NotBeNull();
            veiculo.Id.Should().Be(dto.Id);
            veiculo.Placa.Should().Be(dto.Placa);
            veiculo.Marca.Should().Be(dto.Marca);
            veiculo.Modelo.Should().Be(dto.Modelo);
            veiculo.Cor.Should().Be(dto.Cor);
            veiculo.Ano.Should().Be(dto.Ano);
            veiculo.Anotacoes.Should().Be(dto.Anotacoes);
            veiculo.ClienteId.Should().Be(dto.ClienteId);
            veiculo.Ativo.Should().Be(dto.Ativo);
            veiculo.DataCadastro.Should().Be(dto.DataCadastro);
            veiculo.DataAtualizacao.Should().Be(dto.DataAtualizacao);
        }
    }
}
