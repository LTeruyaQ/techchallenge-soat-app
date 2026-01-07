using Adapters.Presenters;
using Core.DTOs.Requests.Servico;
using Core.Entidades;
using FluentAssertions;
using Xunit;

namespace MecanicaOS.UnitTests.Adapters.Presenters
{
    public class ServicoPresenterTests
    {
        private readonly ServicoPresenter _presenter;

        public ServicoPresenterTests()
        {
            _presenter = new ServicoPresenter();
        }

        [Fact]
        public void ParaResponse_ComServicoValido_DeveConverterCorretamente()
        {
            // Arrange
            var servico = new Servico
            {
                Id = Guid.NewGuid(),
                Nome = "Troca de Óleo",
                Descricao = "Troca de óleo do motor",
                Valor = 150.00m,
                Disponivel = true,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now.AddDays(1)
            };

            // Act
            var response = _presenter.ParaResponse(servico);

            // Assert
            response.Should().NotBeNull();
            response.Id.Should().Be(servico.Id);
            response.Nome.Should().Be(servico.Nome);
            response.Descricao.Should().Be(servico.Descricao);
            response.Valor.Should().Be(servico.Valor);
            response.Disponivel.Should().Be(servico.Disponivel);
            response.DataCadastro.Should().Be(servico.DataCadastro);
            response.DataAtualizacao.Should().Be(servico.DataAtualizacao);
        }

        [Fact]
        public void ParaResponse_ComListaDeServicos_DeveConverterTodos()
        {
            // Arrange
            var servicos = new List<Servico>
            {
                new Servico
                {
                    Id = Guid.NewGuid(),
                    Nome = "Troca de Óleo",
                    Descricao = "Troca de óleo do motor",
                    Valor = 150.00m,
                    Disponivel = true,
                    DataCadastro = DateTime.Now
                },
                new Servico
                {
                    Id = Guid.NewGuid(),
                    Nome = "Alinhamento",
                    Descricao = "Alinhamento e balanceamento",
                    Valor = 80.00m,
                    Disponivel = true,
                    DataCadastro = DateTime.Now
                }
            };

            // Act
            var responses = _presenter.ParaResponse(servicos);

            // Assert
            responses.Should().HaveCount(2);
            responses.First().Nome.Should().Be("Troca de Óleo");
            responses.Last().Nome.Should().Be("Alinhamento");
        }

        [Fact]
        public void ParaResponse_ComListaNula_DeveRetornarListaVazia()
        {
            // Arrange
            IEnumerable<Servico> servicos = null!;

            // Act
            var responses = _presenter.ParaResponse(servicos);

            // Assert
            responses.Should().NotBeNull();
            responses.Should().BeEmpty();
        }

        [Fact]
        public void ParaResponse_ComListaVazia_DeveRetornarListaVazia()
        {
            // Arrange
            var servicos = new List<Servico>();

            // Act
            var responses = _presenter.ParaResponse(servicos);

            // Assert
            responses.Should().NotBeNull();
            responses.Should().BeEmpty();
        }

        [Fact]
        public void ParaUseCaseDto_ComCadastrarServicoRequest_DeveConverterCorretamente()
        {
            // Arrange
            var request = new CadastrarServicoRequest
            {
                Nome = "Troca de Óleo",
                Descricao = "Troca de óleo do motor",
                Valor = 150.00m,
                Disponivel = true
            };

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().NotBeNull();
            dto!.Nome.Should().Be(request.Nome);
            dto.Descricao.Should().Be(request.Descricao);
            dto.Valor.Should().Be(request.Valor);
            dto.Disponivel.Should().Be(request.Disponivel);
        }

        [Fact]
        public void ParaUseCaseDto_ComCadastrarServicoRequestNulo_DeveRetornarNull()
        {
            // Arrange
            CadastrarServicoRequest request = null!;

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().BeNull();
        }

        [Fact]
        public void ParaUseCaseDto_ComEditarServicoRequest_DeveConverterCorretamente()
        {
            // Arrange
            var request = new EditarServicoRequest
            {
                Nome = "Troca de Óleo Premium",
                Descricao = "Troca de óleo sintético",
                Valor = 200.00m,
                Disponivel = false
            };

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().NotBeNull();
            dto!.Nome.Should().Be(request.Nome);
            dto.Descricao.Should().Be(request.Descricao);
            dto.Valor.Should().Be(request.Valor);
            dto.Disponivel.Should().Be(request.Disponivel);
        }

        [Fact]
        public void ParaUseCaseDto_ComEditarServicoRequestNulo_DeveRetornarNull()
        {
            // Arrange
            EditarServicoRequest request = null!;

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().BeNull();
        }

        [Fact]
        public void ParaUseCaseDto_ComEditarServicoRequest_ComValoresNulos_DeveConverterCorretamente()
        {
            // Arrange
            var request = new EditarServicoRequest
            {
                Nome = "Serviço Teste",
                Descricao = "Descrição Teste",
                Valor = null,
                Disponivel = null
            };

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().NotBeNull();
            dto!.Nome.Should().Be(request.Nome);
            dto.Descricao.Should().Be(request.Descricao);
            dto.Valor.Should().BeNull();
            dto.Disponivel.Should().BeNull();
        }
    }
}
