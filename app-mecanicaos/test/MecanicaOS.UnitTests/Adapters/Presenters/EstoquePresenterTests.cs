using Adapters.Presenters;
using Core.DTOs.Requests.Estoque;
using Core.Entidades;
using FluentAssertions;
using Xunit;

namespace MecanicaOS.UnitTests.Adapters.Presenters
{
    public class EstoquePresenterTests
    {
        private readonly EstoquePresenter _presenter;

        public EstoquePresenterTests()
        {
            _presenter = new EstoquePresenter();
        }

        [Fact]
        public void ParaResponse_ComEstoqueValido_DeveConverterCorretamente()
        {
            // Arrange
            var estoque = new Estoque
            {
                Id = Guid.NewGuid(),
                Insumo = "Óleo 10W-40",
                Descricao = "Óleo sintético para motor",
                Preco = 50.00m,
                QuantidadeDisponivel = 10,
                QuantidadeMinima = 5,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now.AddDays(1)
            };

            // Act
            var response = _presenter.ParaResponse(estoque);

            // Assert
            response.Should().NotBeNull();
            response!.Id.Should().Be(estoque.Id);
            response.Insumo.Should().Be(estoque.Insumo);
            response.Descricao.Should().Be(estoque.Descricao);
            response.Preco.Should().Be((double)estoque.Preco);
            response.QuantidadeDisponivel.Should().Be(estoque.QuantidadeDisponivel);
            response.QuantidadeMinima.Should().Be(estoque.QuantidadeMinima);
            response.DataCadastro.Should().Be(estoque.DataCadastro);
            response.DataAtualizacao.Should().Be(estoque.DataAtualizacao);
        }

        [Fact]
        public void ParaResponse_ComEstoqueNulo_DeveRetornarNull()
        {
            // Arrange
            Estoque estoque = null!;

            // Act
            var response = _presenter.ParaResponse(estoque);

            // Assert
            response.Should().BeNull();
        }

        [Fact]
        public void ParaResponse_ComListaDeEstoques_DeveConverterTodos()
        {
            // Arrange
            var estoques = new List<Estoque>
            {
                new Estoque
                {
                    Id = Guid.NewGuid(),
                    Insumo = "Óleo 10W-40",
                    Descricao = "Óleo sintético",
                    Preco = 50.00m,
                    QuantidadeDisponivel = 10,
                    QuantidadeMinima = 5,
                    DataCadastro = DateTime.Now
                },
                new Estoque
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

            // Act
            var responses = _presenter.ParaResponse(estoques);

            // Assert
            responses.Should().HaveCount(2);
            responses.First()!.Insumo.Should().Be("Óleo 10W-40");
            responses.Last()!.Insumo.Should().Be("Filtro de Óleo");
        }

        [Fact]
        public void ParaResponse_ComListaNula_DeveRetornarListaVazia()
        {
            // Arrange
            IEnumerable<Estoque> estoques = null!;

            // Act
            var responses = _presenter.ParaResponse(estoques);

            // Assert
            responses.Should().NotBeNull();
            responses.Should().BeEmpty();
        }

        [Fact]
        public void ParaUseCaseDto_ComCadastrarEstoqueRequest_DeveConverterCorretamente()
        {
            // Arrange
            var request = new CadastrarEstoqueRequest
            {
                Insumo = "Óleo 10W-40",
                Descricao = "Óleo sintético para motor",
                Preco = 50.00,
                QuantidadeDisponivel = 10,
                QuantidadeMinima = 5
            };

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().NotBeNull();
            dto!.Insumo.Should().Be(request.Insumo);
            dto.Descricao.Should().Be(request.Descricao);
            dto.Preco.Should().Be((decimal)request.Preco);
            dto.QuantidadeDisponivel.Should().Be(request.QuantidadeDisponivel);
            dto.QuantidadeMinima.Should().Be(request.QuantidadeMinima);
        }

        [Fact]
        public void ParaUseCaseDto_ComCadastrarEstoqueRequestNulo_DeveRetornarNull()
        {
            // Arrange
            CadastrarEstoqueRequest request = null!;

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().BeNull();
        }

        [Fact]
        public void ParaUseCaseDto_ComAtualizarEstoqueRequest_DeveConverterCorretamente()
        {
            // Arrange
            var request = new AtualizarEstoqueRequest
            {
                Insumo = "Óleo Premium",
                Descricao = "Óleo sintético premium",
                Preco = 75.50m,
                QuantidadeDisponivel = 15,
                QuantidadeMinima = 8
            };

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().NotBeNull();
            dto!.Insumo.Should().Be(request.Insumo);
            dto.Descricao.Should().Be(request.Descricao);
            dto.Preco.Should().Be(request.Preco);
            dto.QuantidadeDisponivel.Should().Be(request.QuantidadeDisponivel);
            dto.QuantidadeMinima.Should().Be(request.QuantidadeMinima);
        }

        [Fact]
        public void ParaUseCaseDto_ComAtualizarEstoqueRequestNulo_DeveRetornarNull()
        {
            // Arrange
            AtualizarEstoqueRequest request = null!;

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().BeNull();
        }

        [Fact]
        public void ParaUseCaseDto_ComAtualizarEstoqueRequest_ComValoresNulos_DeveConverterCorretamente()
        {
            // Arrange
            var request = new AtualizarEstoqueRequest
            {
                Insumo = null,
                Descricao = null,
                Preco = null,
                QuantidadeDisponivel = null,
                QuantidadeMinima = null
            };

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().NotBeNull();
            dto!.Insumo.Should().BeNull();
            dto.Descricao.Should().BeNull();
            dto.Preco.Should().BeNull();
            dto.QuantidadeDisponivel.Should().BeNull();
            dto.QuantidadeMinima.Should().BeNull();
        }
    }
}
