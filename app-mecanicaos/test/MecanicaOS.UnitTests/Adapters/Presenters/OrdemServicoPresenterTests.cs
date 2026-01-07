using Adapters.Presenters;
using Core.DTOs.Requests.OrdemServico;
using Core.DTOs.Requests.OrdemServico.InsumoOS;
using Core.Entidades;
using Core.Enumeradores;
using FluentAssertions;
using Xunit;

namespace MecanicaOS.UnitTests.Adapters.Presenters
{
    public class OrdemServicoPresenterTests
    {
        private readonly OrdemServicoPresenter _presenter;

        public OrdemServicoPresenterTests()
        {
            _presenter = new OrdemServicoPresenter();
        }

        [Fact]
        public void ParaResponse_ComOrdemServicoValida_DeveConverterCorretamente()
        {
            // Arrange
            var ordemServico = new OrdemServico
            {
                Id = Guid.NewGuid(),
                ClienteId = Guid.NewGuid(),
                VeiculoId = Guid.NewGuid(),
                ServicoId = Guid.NewGuid(),
                Orcamento = 500.00m,
                Descricao = "Manutenção preventiva",
                Status = StatusOrdemServico.AguardandoAprovacao,
                DataEnvioOrcamento = DateTime.Now,
                DataCadastro = DateTime.Now
            };

            // Act
            var response = _presenter.ParaResponse(ordemServico);

            // Assert
            response.Should().NotBeNull();
            response!.Id.Should().Be(ordemServico.Id);
            response.ClienteId.Should().Be(ordemServico.ClienteId);
            response.VeiculoId.Should().Be(ordemServico.VeiculoId);
            response.ServicoId.Should().Be(ordemServico.ServicoId);
            response.Orcamento.Should().Be((double?)ordemServico.Orcamento);
            response.Descricao.Should().Be(ordemServico.Descricao);
            response.Status.Should().Be(ordemServico.Status);
            response.DataEnvioOrcamento.Should().Be(ordemServico.DataEnvioOrcamento);
        }

        [Fact]
        public void ParaResponse_ComOrdemServicoComInsumos_DeveIncluirInsumos()
        {
            // Arrange
            var ordemServico = new OrdemServico
            {
                Id = Guid.NewGuid(),
                ClienteId = Guid.NewGuid(),
                VeiculoId = Guid.NewGuid(),
                ServicoId = Guid.NewGuid(),
                Status = StatusOrdemServico.EmExecucao,
                DataCadastro = DateTime.Now,
                InsumosOS = new List<InsumoOS>
                {
                    new InsumoOS
                    {
                        OrdemServicoId = Guid.NewGuid(),
                        EstoqueId = Guid.NewGuid(),
                        Quantidade = 2
                    },
                    new InsumoOS
                    {
                        OrdemServicoId = Guid.NewGuid(),
                        EstoqueId = Guid.NewGuid(),
                        Quantidade = 1
                    }
                }
            };

            // Act
            var response = _presenter.ParaResponse(ordemServico);

            // Assert
            response.Should().NotBeNull();
            response!.Insumos.Should().HaveCount(2);
            response.Insumos!.First().Quantidade.Should().Be(2);
            response.Insumos.Last().Quantidade.Should().Be(1);
        }

        [Fact]
        public void ParaResponse_ComOrdemServicoNula_DeveRetornarNull()
        {
            // Arrange
            OrdemServico ordemServico = null!;

            // Act
            var response = _presenter.ParaResponse(ordemServico);

            // Assert
            response.Should().BeNull();
        }

        [Fact]
        public void ParaResponse_ComListaDeOrdensServico_DeveConverterTodas()
        {
            // Arrange
            var ordensServico = new List<OrdemServico>
            {
                new OrdemServico
                {
                    Id = Guid.NewGuid(),
                    ClienteId = Guid.NewGuid(),
                    VeiculoId = Guid.NewGuid(),
                    ServicoId = Guid.NewGuid(),
                    Status = StatusOrdemServico.AguardandoAprovacao,
                    DataCadastro = DateTime.Now
                },
                new OrdemServico
                {
                    Id = Guid.NewGuid(),
                    ClienteId = Guid.NewGuid(),
                    VeiculoId = Guid.NewGuid(),
                    ServicoId = Guid.NewGuid(),
                    Status = StatusOrdemServico.EmExecucao,
                    DataCadastro = DateTime.Now
                }
            };

            // Act
            var responses = _presenter.ParaResponse(ordensServico);

            // Assert
            responses.Should().HaveCount(2);
            responses.First()!.Status.Should().Be(StatusOrdemServico.AguardandoAprovacao);
            responses.Last()!.Status.Should().Be(StatusOrdemServico.EmExecucao);
        }

        [Fact]
        public void ParaResponse_ComListaNula_DeveRetornarListaVazia()
        {
            // Arrange
            IEnumerable<OrdemServico> ordensServico = null!;

            // Act
            var responses = _presenter.ParaResponse(ordensServico);

            // Assert
            responses.Should().NotBeNull();
            responses.Should().BeEmpty();
        }

        [Fact]
        public void ParaUseCaseDto_ComCadastrarOrdemServicoRequest_DeveConverterCorretamente()
        {
            // Arrange
            var request = new CadastrarOrdemServicoRequest
            {
                ClienteId = Guid.NewGuid(),
                VeiculoId = Guid.NewGuid(),
                ServicoId = Guid.NewGuid(),
                Descricao = "Manutenção preventiva completa"
            };

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().NotBeNull();
            dto!.ClienteId.Should().Be(request.ClienteId);
            dto.VeiculoId.Should().Be(request.VeiculoId);
            dto.ServicoId.Should().Be(request.ServicoId);
            dto.Descricao.Should().Be(request.Descricao);
        }

        [Fact]
        public void ParaUseCaseDto_ComCadastrarOrdemServicoRequestNulo_DeveRetornarNull()
        {
            // Arrange
            CadastrarOrdemServicoRequest request = null!;

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().BeNull();
        }

        [Fact]
        public void ParaUseCaseDto_ComAtualizarOrdemServicoRequest_DeveConverterCorretamente()
        {
            // Arrange
            var request = new AtualizarOrdemServicoRequest
            {
                ClienteId = Guid.NewGuid(),
                VeiculoId = Guid.NewGuid(),
                ServicoId = Guid.NewGuid(),
                Descricao = "Descrição atualizada",
                Status = StatusOrdemServico.EmExecucao
            };

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().NotBeNull();
            dto!.ClienteId.Should().Be(request.ClienteId);
            dto.VeiculoId.Should().Be(request.VeiculoId);
            dto.ServicoId.Should().Be(request.ServicoId);
            dto.Descricao.Should().Be(request.Descricao);
            dto.Status.Should().Be(request.Status);
        }

        [Fact]
        public void ParaUseCaseDto_ComAtualizarOrdemServicoRequestNulo_DeveRetornarNull()
        {
            // Arrange
            AtualizarOrdemServicoRequest request = null!;

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().BeNull();
        }

        [Fact]
        public void ParaUseCaseDto_ComCadastrarInsumoOSRequest_DeveConverterCorretamente()
        {
            // Arrange
            var request = new CadastrarInsumoOSRequest
            {
                EstoqueId = Guid.NewGuid(),
                Quantidade = 5
            };

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().NotBeNull();
            dto!.EstoqueId.Should().Be(request.EstoqueId);
            dto.Quantidade.Should().Be(request.Quantidade);
        }

        [Fact]
        public void ParaUseCaseDto_ComCadastrarInsumoOSRequestNulo_DeveRetornarNull()
        {
            // Arrange
            CadastrarInsumoOSRequest request = null!;

            // Act
            var dto = _presenter.ParaUseCaseDto(request);

            // Assert
            dto.Should().BeNull();
        }
    }
}
