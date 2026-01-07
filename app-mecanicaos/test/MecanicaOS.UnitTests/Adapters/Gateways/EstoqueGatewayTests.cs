using Adapters.Gateways;
using Core.DTOs.Entidades.Estoque;
using Core.Entidades;
using Core.Especificacoes.Estoque;
using Core.Interfaces.Repositorios;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace MecanicaOS.UnitTests.Adapters.Gateways
{
    public class EstoqueGatewayTests
    {
        private readonly IRepositorio<EstoqueEntityDto> _repositorio;
        private readonly EstoqueGateway _gateway;

        public EstoqueGatewayTests()
        {
            _repositorio = Substitute.For<IRepositorio<EstoqueEntityDto>>();
            _gateway = new EstoqueGateway(_repositorio);
        }

        [Fact]
        public async Task CadastrarAsync_DeveConverterParaDtoEChamarRepositorio()
        {
            // Arrange
            var estoque = new Estoque
            {
                Id = Guid.NewGuid(),
                Insumo = "Óleo 10W-40",
                Descricao = "Óleo sintético",
                Preco = 50.00m,
                QuantidadeDisponivel = 10,
                QuantidadeMinima = 5,
                Ativo = true,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            EstoqueEntityDto? dtoCapturado = null;
            await _repositorio.CadastrarAsync(Arg.Do<EstoqueEntityDto>(dto => dtoCapturado = dto));

            // Act
            await _gateway.CadastrarAsync(estoque);

            // Assert
            await _repositorio.Received(1).CadastrarAsync(Arg.Any<EstoqueEntityDto>());
            dtoCapturado.Should().NotBeNull();
            dtoCapturado!.Id.Should().Be(estoque.Id);
            dtoCapturado.Insumo.Should().Be(estoque.Insumo);
            dtoCapturado.Descricao.Should().Be(estoque.Descricao);
            dtoCapturado.Preco.Should().Be(estoque.Preco);
            dtoCapturado.QuantidadeDisponivel.Should().Be(estoque.QuantidadeDisponivel);
            dtoCapturado.QuantidadeMinima.Should().Be(estoque.QuantidadeMinima);
            dtoCapturado.Ativo.Should().Be(estoque.Ativo);
            dtoCapturado.DataCadastro.Should().Be(estoque.DataCadastro);
            dtoCapturado.DataAtualizacao.Should().Be(estoque.DataAtualizacao);
        }

        [Fact]
        public async Task EditarAsync_DeveConverterParaDtoEChamarRepositorio()
        {
            // Arrange
            var estoque = new Estoque
            {
                Id = Guid.NewGuid(),
                Insumo = "Óleo Premium",
                Descricao = "Óleo sintético premium",
                Preco = 75.00m,
                QuantidadeDisponivel = 15,
                QuantidadeMinima = 8,
                Ativo = true,
                DataCadastro = DateTime.Now.AddDays(-10),
                DataAtualizacao = DateTime.Now
            };

            EstoqueEntityDto? dtoCapturado = null;
            await _repositorio.EditarAsync(Arg.Do<EstoqueEntityDto>(dto => dtoCapturado = dto));

            // Act
            await _gateway.EditarAsync(estoque);

            // Assert
            await _repositorio.Received(1).EditarAsync(Arg.Any<EstoqueEntityDto>());
            dtoCapturado.Should().NotBeNull();
            dtoCapturado!.Id.Should().Be(estoque.Id);
            dtoCapturado.Insumo.Should().Be(estoque.Insumo);
        }

        [Fact]
        public async Task DeletarAsync_DeveConverterParaDtoEChamarRepositorio()
        {
            // Arrange
            var estoque = new Estoque
            {
                Id = Guid.NewGuid(),
                Insumo = "Óleo 10W-40",
                Descricao = "Óleo sintético",
                Preco = 50.00m,
                QuantidadeDisponivel = 10,
                QuantidadeMinima = 5,
                DataCadastro = DateTime.Now
            };

            // Act
            await _gateway.DeletarAsync(estoque);

            // Assert
            await _repositorio.Received(1).DeletarAsync(Arg.Any<EstoqueEntityDto>());
        }

        [Fact]
        public async Task ObterPorIdAsync_ComIdExistente_DeveRetornarEstoqueConvertido()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new EstoqueEntityDto
            {
                Id = id,
                Insumo = "Óleo 10W-40",
                Descricao = "Óleo sintético",
                Preco = 50.00m,
                QuantidadeDisponivel = 10,
                QuantidadeMinima = 5,
                Ativo = true,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            _repositorio.ObterPorIdSemRastreamentoAsync(id).Returns(Task.FromResult<EstoqueEntityDto?>(dto));

            // Act
            var resultado = await _gateway.ObterPorIdAsync(id);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(dto.Id);
            resultado.Insumo.Should().Be(dto.Insumo);
            resultado.Descricao.Should().Be(dto.Descricao);
            resultado.Preco.Should().Be(dto.Preco);
            resultado.QuantidadeDisponivel.Should().Be(dto.QuantidadeDisponivel);
            resultado.QuantidadeMinima.Should().Be(dto.QuantidadeMinima);
        }

        [Fact]
        public async Task ObterPorIdAsync_ComIdInexistente_DeveRetornarNull()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repositorio.ObterPorIdSemRastreamentoAsync(id).Returns(Task.FromResult<EstoqueEntityDto?>(null));

            // Act
            var resultado = await _gateway.ObterPorIdAsync(id);

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task ObterTodosAsync_DeveRetornarTodosEstoquesConvertidos()
        {
            // Arrange
            var dtos = new List<EstoqueEntityDto>
            {
                new EstoqueEntityDto
                {
                    Id = Guid.NewGuid(),
                    Insumo = "Óleo 10W-40",
                    Descricao = "Óleo sintético",
                    Preco = 50.00m,
                    QuantidadeDisponivel = 10,
                    QuantidadeMinima = 5,
                    DataCadastro = DateTime.Now
                },
                new EstoqueEntityDto
                {
                    Id = Guid.NewGuid(),
                    Insumo = "Filtro de Óleo",
                    Descricao = "Filtro de óleo do motor",
                    Preco = 25.00m,
                    QuantidadeDisponivel = 20,
                    QuantidadeMinima = 10,
                    DataCadastro = DateTime.Now
                }
            };

            _repositorio.ObterTodosAsync().Returns(Task.FromResult<IEnumerable<EstoqueEntityDto>>(dtos));

            // Act
            var resultado = await _gateway.ObterTodosAsync();

            // Assert
            resultado.Should().HaveCount(2);
            resultado.First().Insumo.Should().Be("Óleo 10W-40");
            resultado.Last().Insumo.Should().Be("Filtro de Óleo");
        }

        [Fact]
        public async Task ObterEstoqueCriticoAsync_DeveChamarRepositorioComEspecificacao()
        {
            // Arrange
            var estoquesCriticos = new List<Estoque>
            {
                new Estoque
                {
                    Id = Guid.NewGuid(),
                    Insumo = "Óleo 10W-40",
                    QuantidadeDisponivel = 3,
                    QuantidadeMinima = 5,
                    DataCadastro = DateTime.Now
                }
            };

            _repositorio.ListarProjetadoAsync<Estoque>(Arg.Any<ObterEstoqueCriticoEspecificacao>())
                .Returns(Task.FromResult<IEnumerable<Estoque>>(estoquesCriticos));

            // Act
            var resultado = await _gateway.ObterEstoqueCriticoAsync();

            // Assert
            resultado.Should().HaveCount(1);
            resultado.First().QuantidadeDisponivel.Should().BeLessThan(resultado.First().QuantidadeMinima);
        }

        [Fact]
        public void ToDto_DeveConverterEstoqueParaDto()
        {
            // Arrange
            var estoque = new Estoque
            {
                Id = Guid.NewGuid(),
                Insumo = "Óleo 10W-40",
                Descricao = "Óleo sintético",
                Preco = 50.00m,
                QuantidadeDisponivel = 10,
                QuantidadeMinima = 5,
                Ativo = true,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            // Act
            var dto = EstoqueGateway.ToDto(estoque);

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(estoque.Id);
            dto.Insumo.Should().Be(estoque.Insumo);
            dto.Descricao.Should().Be(estoque.Descricao);
            dto.Preco.Should().Be(estoque.Preco);
            dto.QuantidadeDisponivel.Should().Be(estoque.QuantidadeDisponivel);
            dto.QuantidadeMinima.Should().Be(estoque.QuantidadeMinima);
            dto.Ativo.Should().Be(estoque.Ativo);
            dto.DataCadastro.Should().Be(estoque.DataCadastro);
            dto.DataAtualizacao.Should().Be(estoque.DataAtualizacao);
        }

        [Fact]
        public void FromDto_DeveConverterDtoParaEstoque()
        {
            // Arrange
            var dto = new EstoqueEntityDto
            {
                Id = Guid.NewGuid(),
                Insumo = "Óleo 10W-40",
                Descricao = "Óleo sintético",
                Preco = 50.00m,
                QuantidadeDisponivel = 10,
                QuantidadeMinima = 5,
                Ativo = true,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            // Act
            var estoque = EstoqueGateway.FromDto(dto);

            // Assert
            estoque.Should().NotBeNull();
            estoque.Id.Should().Be(dto.Id);
            estoque.Insumo.Should().Be(dto.Insumo);
            estoque.Descricao.Should().Be(dto.Descricao);
            estoque.Preco.Should().Be(dto.Preco);
            estoque.QuantidadeDisponivel.Should().Be(dto.QuantidadeDisponivel);
            estoque.QuantidadeMinima.Should().Be(dto.QuantidadeMinima);
            estoque.Ativo.Should().Be(dto.Ativo);
            estoque.DataCadastro.Should().Be(dto.DataCadastro);
            estoque.DataAtualizacao.Should().Be(dto.DataAtualizacao);
        }
    }
}
