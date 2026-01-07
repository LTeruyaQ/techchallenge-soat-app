using Adapters.Gateways;
using Core.DTOs.Entidades.Servico;
using Core.Entidades;
using Core.Especificacoes.Base.Interfaces;
using Core.Especificacoes.Servico;
using Core.Interfaces.Repositorios;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace MecanicaOS.UnitTests.Adapters.Gateways
{
    public class ServicoGatewayTests
    {
        private readonly IRepositorio<ServicoEntityDto> _repositorio;
        private readonly ServicoGateway _gateway;

        public ServicoGatewayTests()
        {
            _repositorio = Substitute.For<IRepositorio<ServicoEntityDto>>();
            _gateway = new ServicoGateway(_repositorio);
        }

        [Fact]
        public async Task CadastrarAsync_DeveConverterParaDtoERetornarServicoConvertido()
        {
            // Arrange
            var servico = new Servico
            {
                Id = Guid.NewGuid(),
                Nome = "Troca de Óleo",
                Descricao = "Troca de óleo do motor",
                Valor = 150.00m,
                Disponivel = true,
                Ativo = true,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            var dtoRetornado = new ServicoEntityDto
            {
                Id = servico.Id,
                Nome = servico.Nome,
                Descricao = servico.Descricao,
                Valor = servico.Valor,
                Disponivel = servico.Disponivel,
                Ativo = servico.Ativo,
                DataCadastro = servico.DataCadastro,
                DataAtualizacao = servico.DataAtualizacao
            };

            _repositorio.CadastrarAsync(Arg.Any<ServicoEntityDto>()).Returns(Task.FromResult(dtoRetornado));

            // Act
            var resultado = await _gateway.CadastrarAsync(servico);

            // Assert
            await _repositorio.Received(1).CadastrarAsync(Arg.Any<ServicoEntityDto>());
            resultado.Should().NotBeNull();
            resultado.Id.Should().Be(servico.Id);
            resultado.Nome.Should().Be(servico.Nome);
            resultado.Valor.Should().Be(servico.Valor);
        }

        [Fact]
        public async Task EditarAsync_DeveConverterParaDtoEChamarRepositorio()
        {
            // Arrange
            var servico = new Servico
            {
                Id = Guid.NewGuid(),
                Nome = "Troca de Óleo Premium",
                Descricao = "Troca de óleo sintético",
                Valor = 200.00m,
                Disponivel = false,
                Ativo = true,
                DataCadastro = DateTime.Now.AddDays(-10),
                DataAtualizacao = DateTime.Now
            };

            // Act
            await _gateway.EditarAsync(servico);

            // Assert
            await _repositorio.Received(1).EditarAsync(Arg.Any<ServicoEntityDto>());
        }

        [Fact]
        public async Task DeletarAsync_DeveConverterParaDtoEChamarRepositorio()
        {
            // Arrange
            var servico = new Servico
            {
                Id = Guid.NewGuid(),
                Nome = "Troca de Óleo",
                Descricao = "Troca de óleo do motor",
                Valor = 150.00m,
                Disponivel = true,
                DataCadastro = DateTime.Now
            };

            // Act
            await _gateway.DeletarAsync(servico);

            // Assert
            await _repositorio.Received(1).DeletarAsync(Arg.Any<ServicoEntityDto>());
        }

        [Fact]
        public async Task ObterPorIdAsync_ComIdExistente_DeveRetornarServicoConvertido()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new ServicoEntityDto
            {
                Id = id,
                Nome = "Troca de Óleo",
                Descricao = "Troca de óleo do motor",
                Valor = 150.00m,
                Disponivel = true,
                Ativo = true,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now
            };

            _repositorio.ObterPorIdSemRastreamentoAsync(id).Returns(Task.FromResult<ServicoEntityDto?>(dto));

            // Act
            var resultado = await _gateway.ObterPorIdAsync(id);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(dto.Id);
            resultado.Nome.Should().Be(dto.Nome);
            resultado.Descricao.Should().Be(dto.Descricao);
            resultado.Valor.Should().Be(dto.Valor);
            resultado.Disponivel.Should().Be(dto.Disponivel);
        }

        [Fact]
        public async Task ObterPorIdAsync_ComIdInexistente_DeveRetornarNull()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repositorio.ObterPorIdSemRastreamentoAsync(id).Returns(Task.FromResult<ServicoEntityDto?>(null));

            // Act
            var resultado = await _gateway.ObterPorIdAsync(id);

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task ObterTodosAsync_DeveRetornarTodosServicosConvertidos()
        {
            // Arrange
            var dtos = new List<ServicoEntityDto>
            {
                new ServicoEntityDto
                {
                    Id = Guid.NewGuid(),
                    Nome = "Troca de Óleo",
                    Descricao = "Troca de óleo do motor",
                    Valor = 150.00m,
                    Disponivel = true,
                    DataCadastro = DateTime.Now
                },
                new ServicoEntityDto
                {
                    Id = Guid.NewGuid(),
                    Nome = "Alinhamento",
                    Descricao = "Alinhamento e balanceamento",
                    Valor = 80.00m,
                    Disponivel = true,
                    DataCadastro = DateTime.Now
                }
            };

            _repositorio.ObterTodosAsync().Returns(Task.FromResult<IEnumerable<ServicoEntityDto>>(dtos));

            // Act
            var resultado = await _gateway.ObterTodosAsync();

            // Assert
            resultado.Should().HaveCount(2);
            resultado.First().Nome.Should().Be("Troca de Óleo");
            resultado.Last().Nome.Should().Be("Alinhamento");
        }

        [Fact]
        public async Task ObterServicoDisponivelAsync_DeveChamarRepositorioComEspecificacao()
        {
            // Arrange
            var servicosDisponiveis = new List<Servico>
            {
                new Servico
                {
                    Id = Guid.NewGuid(),
                    Nome = "Troca de Óleo",
                    Descricao = "Troca de óleo do motor",
                    Valor = 150.00m,
                    Disponivel = true,
                    DataCadastro = DateTime.Now
                }
            };

            _repositorio.ListarProjetadoAsync<Servico>(Arg.Any<ObterServicoDisponivelEspecificacao>())
                .Returns(Task.FromResult<IEnumerable<Servico>>(servicosDisponiveis));

            // Act
            var resultado = await _gateway.ObterServicoDisponivelAsync();

            // Assert
            resultado.Should().HaveCount(1);
            resultado.First().Disponivel.Should().BeTrue();
        }

        [Fact]
        public async Task ObterServicosDisponiveisPorNomeAsync_DeveChamarRepositorioComEspecificacaoComposta()
        {
            // Arrange
            var nome = "Troca de Óleo";
            var servico = new Servico
            {
                Id = Guid.NewGuid(),
                Nome = nome,
                Descricao = "Troca de óleo do motor",
                Valor = 150.00m,
                Disponivel = true,
                DataCadastro = DateTime.Now
            };

            _repositorio.ObterUmProjetadoSemRastreamentoAsync<Servico>(Arg.Any<IEspecificacao<ServicoEntityDto>>())
                .Returns(Task.FromResult<Servico?>(servico));

            // Act
            var resultado = await _gateway.ObterServicosDisponiveisPorNomeAsync(nome);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Nome.Should().Be(nome);
            resultado.Disponivel.Should().BeTrue();
        }

    }
}
