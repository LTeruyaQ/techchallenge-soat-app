using API.Jobs;
using Core.DTOs.Requests.OrdemServico.InsumoOS;
using Core.DTOs.Responses.OrdemServico;
using Core.DTOs.Responses.OrdemServico.InsumoOrdemServico;
using Core.Interfaces.Controllers;
using Core.Interfaces.root;
using Core.Interfaces.Servicos;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace MecanicaOS.UnitTests.API.Jobs
{
    /// <summary>
    /// Testes para VerificarOrcamentoExpiradoJob após migração para Clean Architecture
    /// Testa integração com Controllers ao invés de repositórios diretos
    /// </summary>
    public class VerificarOrcamentoExpiradoJobTests
    {
        private readonly ICompositionRoot _compositionRoot;
        private readonly IOrdemServicoController _ordemServicoController;
        private readonly IInsumoOSController _insumoOSController;
        private readonly ILogServico<VerificarOrcamentoExpiradoJob> _logServico;

        public VerificarOrcamentoExpiradoJobTests()
        {
            _compositionRoot = Substitute.For<ICompositionRoot>();
            _ordemServicoController = Substitute.For<IOrdemServicoController>();
            _insumoOSController = Substitute.For<IInsumoOSController>();
            _logServico = Substitute.For<ILogServico<VerificarOrcamentoExpiradoJob>>();

            _compositionRoot.CriarOrdemServicoController().Returns(_ordemServicoController);
            _compositionRoot.CriarInsumoOSController().Returns(_insumoOSController);
            _compositionRoot.CriarLogService<VerificarOrcamentoExpiradoJob>().Returns(_logServico);
        }

        private VerificarOrcamentoExpiradoJob CriarJob()
        {
            return new VerificarOrcamentoExpiradoJob(_compositionRoot);
        }

        [Fact]
        public void Construtor_DeveCriarInstanciaComDependencias()
        {
            // Arrange & Act
            var job = CriarJob();

            // Assert
            job.Should().NotBeNull();
            _compositionRoot.Received(1).CriarOrdemServicoController();
            _compositionRoot.Received(1).CriarInsumoOSController();
            _compositionRoot.Received(1).CriarLogService<VerificarOrcamentoExpiradoJob>();
        }

        [Fact]
        public async Task ExecutarAsync_SemOrcamentosExpirados_NaoDeveDevolverInsumos()
        {
            // Arrange
            var ordensVazias = new List<OrdemServicoResponse>();
            
            _ordemServicoController.ObterOrcamentosExpirados()
                .Returns(Task.FromResult<IEnumerable<OrdemServicoResponse>>(ordensVazias));

            var job = CriarJob();

            // Act
            await job.ExecutarAsync();

            // Assert
            _logServico.Received(1).LogInicio(nameof(job.ExecutarAsync));
            _logServico.Received(1).LogFim(nameof(job.ExecutarAsync));
            await _insumoOSController.DidNotReceive().DevolverInsumosAoEstoque(
                Arg.Any<IEnumerable<DevolverInsumoOSRequest>>());
        }

        [Fact]
        public async Task ExecutarAsync_ComOrcamentosExpirados_DeveDevolverInsumosAoEstoque()
        {
            // Arrange
            var ordensExpiradas = new List<OrdemServicoResponse>
            {
                new OrdemServicoResponse
                {
                    Id = Guid.NewGuid(),
                    Insumos = new List<InsumoOSResponse>
                    {
                        new InsumoOSResponse { EstoqueId = Guid.NewGuid(), Quantidade = 5 },
                        new InsumoOSResponse { EstoqueId = Guid.NewGuid(), Quantidade = 3 }
                    }
                },
                new OrdemServicoResponse
                {
                    Id = Guid.NewGuid(),
                    Insumos = new List<InsumoOSResponse>
                    {
                        new InsumoOSResponse { EstoqueId = Guid.NewGuid(), Quantidade = 2 }
                    }
                }
            };

            _ordemServicoController.ObterOrcamentosExpirados()
                .Returns(Task.FromResult<IEnumerable<OrdemServicoResponse>>(ordensExpiradas));

            var job = CriarJob();

            // Act
            await job.ExecutarAsync();

            // Assert
            _logServico.Received(1).LogInicio(nameof(job.ExecutarAsync));
            _logServico.Received(1).LogFim(nameof(job.ExecutarAsync));
            _logServico.Received(1).LogInicio("Processadas 2 ordens com orçamento expirado");
            
            await _ordemServicoController.Received(1).ObterOrcamentosExpirados();
            await _insumoOSController.Received(2).DevolverInsumosAoEstoque(
                Arg.Any<IEnumerable<DevolverInsumoOSRequest>>());
        }

        [Fact]
        public async Task ExecutarAsync_ComOrdemSemInsumos_NaoDeveDevolverInsumos()
        {
            // Arrange
            var ordensExpiradas = new List<OrdemServicoResponse>
            {
                new OrdemServicoResponse
                {
                    Id = Guid.NewGuid(),
                    Insumos = null // Sem insumos
                },
                new OrdemServicoResponse
                {
                    Id = Guid.NewGuid(),
                    Insumos = new List<InsumoOSResponse>() // Lista vazia
                }
            };

            _ordemServicoController.ObterOrcamentosExpirados()
                .Returns(Task.FromResult<IEnumerable<OrdemServicoResponse>>(ordensExpiradas));

            var job = CriarJob();

            // Act
            await job.ExecutarAsync();

            // Assert
            _logServico.Received(1).LogInicio(nameof(job.ExecutarAsync));
            _logServico.Received(1).LogFim(nameof(job.ExecutarAsync));
            _logServico.Received(1).LogInicio("Processadas 2 ordens com orçamento expirado");
            
            await _insumoOSController.DidNotReceive().DevolverInsumosAoEstoque(
                Arg.Any<IEnumerable<DevolverInsumoOSRequest>>());
        }

        [Fact]
        public async Task ExecutarAsync_ComExcecaoNoObterOrcamentos_DeveLogarErroEReLancar()
        {
            // Arrange
            var exception = new Exception("Erro ao obter orçamentos expirados");
            _ordemServicoController.ObterOrcamentosExpirados()
                .Throws(exception);

            var job = CriarJob();

            // Act
            var act = async () => await job.ExecutarAsync();

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Erro ao obter orçamentos expirados");
            
            _logServico.Received(1).LogErro(nameof(job.ExecutarAsync), Arg.Any<Exception>());
        }

        [Fact]
        public async Task ExecutarAsync_ComExcecaoNaDevolucaoInsumos_DeveLogarErroEReLancar()
        {
            // Arrange
            var ordensExpiradas = new List<OrdemServicoResponse>
            {
                new OrdemServicoResponse
                {
                    Id = Guid.NewGuid(),
                    Insumos = new List<InsumoOSResponse>
                    {
                        new InsumoOSResponse { EstoqueId = Guid.NewGuid(), Quantidade = 5 }
                    }
                }
            };

            _ordemServicoController.ObterOrcamentosExpirados()
                .Returns(Task.FromResult<IEnumerable<OrdemServicoResponse>>(ordensExpiradas));

            var devolverException = new Exception("Erro ao devolver insumos");
            _insumoOSController.DevolverInsumosAoEstoque(Arg.Any<IEnumerable<DevolverInsumoOSRequest>>())
                .Throws(devolverException);

            var job = CriarJob();

            // Act
            var act = async () => await job.ExecutarAsync();

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Erro ao devolver insumos");
            
            _logServico.Received(1).LogErro(nameof(job.ExecutarAsync), Arg.Any<Exception>());
        }

        [Fact]
        public async Task ExecutarAsync_ComMultiplasOrdensComInsumos_DeveProcessarTodasCorretamente()
        {
            // Arrange
            var ordensExpiradas = new List<OrdemServicoResponse>
            {
                new OrdemServicoResponse
                {
                    Id = Guid.NewGuid(),
                    Insumos = new List<InsumoOSResponse>
                    {
                        new InsumoOSResponse { EstoqueId = Guid.NewGuid(), Quantidade = 10 }
                    }
                },
                new OrdemServicoResponse
                {
                    Id = Guid.NewGuid(),
                    Insumos = new List<InsumoOSResponse>
                    {
                        new InsumoOSResponse { EstoqueId = Guid.NewGuid(), Quantidade = 7 },
                        new InsumoOSResponse { EstoqueId = Guid.NewGuid(), Quantidade = 4 }
                    }
                },
                new OrdemServicoResponse
                {
                    Id = Guid.NewGuid(),
                    Insumos = null // Esta não deve causar erro
                }
            };

            _ordemServicoController.ObterOrcamentosExpirados()
                .Returns(Task.FromResult<IEnumerable<OrdemServicoResponse>>(ordensExpiradas));

            var job = CriarJob();

            // Act
            await job.ExecutarAsync();

            // Assert
            _logServico.Received(1).LogInicio(nameof(job.ExecutarAsync));
            _logServico.Received(1).LogFim(nameof(job.ExecutarAsync));
            _logServico.Received(1).LogInicio("Processadas 3 ordens com orçamento expirado");
            
            // Deve processar apenas as 2 ordens com insumos
            await _insumoOSController.Received(2).DevolverInsumosAoEstoque(
                Arg.Any<IEnumerable<DevolverInsumoOSRequest>>());
        }
    }
}
