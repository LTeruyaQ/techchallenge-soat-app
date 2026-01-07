using API.Notificacoes.OS;
using Core.DTOs.Requests.OrdemServico.InsumoOS;
using Core.DTOs.Responses.OrdemServico;
using Core.DTOs.Responses.OrdemServico.InsumoOrdemServico;
using Core.Interfaces.Controllers;
using Core.Interfaces.root;
using Core.Interfaces.Servicos;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace MecanicaOS.UnitTests.API.Notificacoes.OS
{
    /// <summary>
    /// Testes para OrdemServicoCanceladaHandler
    /// 
    /// IMPORTÂNCIA: Handler crítico que processa eventos de cancelamento de OS.
    /// Responsável por devolver insumos ao estoque quando uma OS é cancelada.
    /// 
    /// COBERTURA: Testa todos os cenários do handler de notificação.
    /// Valida integração com controllers e devolução de insumos.
    /// </summary>
    public class OrdemServicoCanceladaHandlerTests
    {
        private readonly ICompositionRoot _compositionRoot;
        private readonly IOrdemServicoController _ordemServicoController;
        private readonly IInsumoOSController _insumosOSController;
        private readonly ILogServico<OrdemServicoCanceladaHandler> _logServico;
        private readonly OrdemServicoCanceladaHandler _handler;

        public OrdemServicoCanceladaHandlerTests()
        {
            _compositionRoot = Substitute.For<ICompositionRoot>();
            _ordemServicoController = Substitute.For<IOrdemServicoController>();
            _insumosOSController = Substitute.For<IInsumoOSController>();
            _logServico = Substitute.For<ILogServico<OrdemServicoCanceladaHandler>>();

            _compositionRoot.CriarOrdemServicoController().Returns(_ordemServicoController);
            _compositionRoot.CriarInsumoOSController().Returns(_insumosOSController);
            _compositionRoot.CriarLogService<OrdemServicoCanceladaHandler>().Returns(_logServico);

            _handler = new OrdemServicoCanceladaHandler(_compositionRoot);
        }

        [Fact]
        public void Construtor_DeveCriarInstanciaComDependencias()
        {
            // Arrange
            var compositionRoot = Substitute.For<ICompositionRoot>();
            var ordemServicoController = Substitute.For<IOrdemServicoController>();
            var insumosOSController = Substitute.For<IInsumoOSController>();
            var logServico = Substitute.For<ILogServico<OrdemServicoCanceladaHandler>>();

            compositionRoot.CriarOrdemServicoController().Returns(ordemServicoController);
            compositionRoot.CriarInsumoOSController().Returns(insumosOSController);
            compositionRoot.CriarLogService<OrdemServicoCanceladaHandler>().Returns(logServico);

            // Act
            var handler = new OrdemServicoCanceladaHandler(compositionRoot);

            // Assert
            handler.Should().NotBeNull();
            compositionRoot.Received(1).CriarOrdemServicoController();
            compositionRoot.Received(1).CriarInsumoOSController();
            compositionRoot.Received(1).CriarLogService<OrdemServicoCanceladaHandler>();
        }

        [Fact]
        public async Task Handle_ComOrdemServicoComInsumos_DeveDevolverInsumosAoEstoque()
        {
            // Arrange
            var ordemServicoId = Guid.NewGuid();
            var notification = new OrdemServicoCanceladaEvent(ordemServicoId);
            var cancellationToken = CancellationToken.None;

            var ordemServico = CriarOrdemServicoComInsumos(ordemServicoId);
            _ordemServicoController.ObterPorId(ordemServicoId).Returns(ordemServico);

            // Act
            await _handler.Handle(notification, cancellationToken);

            // Assert
            _logServico.Received(1).LogInicio(nameof(_handler.Handle), ordemServicoId);
            await _ordemServicoController.Received(1).ObterPorId(ordemServicoId);
            await _insumosOSController.Received(1).DevolverInsumosAoEstoque(
                Arg.Is<IEnumerable<DevolverInsumoOSRequest>>(requests => 
                    requests.Any(r => r.EstoqueId == ordemServico.Insumos!.First().EstoqueId)));
            _logServico.Received(1).LogFim(nameof(_handler.Handle), null);
        }

        [Fact]
        public async Task Handle_ComOrdemServicoSemInsumos_NaoDeveDevolverInsumos()
        {
            // Arrange
            var ordemServicoId = Guid.NewGuid();
            var notification = new OrdemServicoCanceladaEvent(ordemServicoId);
            var cancellationToken = CancellationToken.None;

            var ordemServico = CriarOrdemServicoSemInsumos(ordemServicoId);
            _ordemServicoController.ObterPorId(ordemServicoId).Returns(ordemServico);

            // Act
            await _handler.Handle(notification, cancellationToken);

            // Assert
            _logServico.Received(1).LogInicio(nameof(_handler.Handle), ordemServicoId);
            await _ordemServicoController.Received(1).ObterPorId(ordemServicoId);
            await _insumosOSController.DidNotReceive().DevolverInsumosAoEstoque(Arg.Any<IEnumerable<DevolverInsumoOSRequest>>());
            _logServico.Received(1).LogFim(nameof(_handler.Handle), null);
        }

        [Fact]
        public async Task Handle_ComInsumosNull_NaoDeveDevolverInsumos()
        {
            // Arrange
            var ordemServicoId = Guid.NewGuid();
            var notification = new OrdemServicoCanceladaEvent(ordemServicoId);
            var cancellationToken = CancellationToken.None;

            var ordemServico = CriarOrdemServicoComInsumosNull(ordemServicoId);
            _ordemServicoController.ObterPorId(ordemServicoId).Returns(ordemServico);

            // Act
            await _handler.Handle(notification, cancellationToken);

            // Assert
            _logServico.Received(1).LogInicio(nameof(_handler.Handle), ordemServicoId);
            await _ordemServicoController.Received(1).ObterPorId(ordemServicoId);
            await _insumosOSController.DidNotReceive().DevolverInsumosAoEstoque(Arg.Any<IEnumerable<DevolverInsumoOSRequest>>());
            _logServico.Received(1).LogFim(nameof(_handler.Handle), null);
        }

        [Fact]
        public async Task Handle_ComExcecaoNoObterOrdemServico_DeveLogarErroEReLancar()
        {
            // Arrange
            var ordemServicoId = Guid.NewGuid();
            var notification = new OrdemServicoCanceladaEvent(ordemServicoId);
            var cancellationToken = CancellationToken.None;

            var exception = new Exception("Erro ao obter ordem de serviço");
            _ordemServicoController.ObterPorId(ordemServicoId)
                .Throws(exception);

            // Act
            var act = async () => await _handler.Handle(notification, cancellationToken);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Erro ao obter ordem de serviço");
            
            _logServico.Received(1).LogErro(nameof(_handler.Handle), exception);
        }

        [Fact]
        public async Task Handle_ComExcecaoNaDevolucaoInsumos_DeveLogarErroEReLancar()
        {
            // Arrange
            var ordemServicoId = Guid.NewGuid();
            var notification = new OrdemServicoCanceladaEvent(ordemServicoId);
            var cancellationToken = CancellationToken.None;

            var ordemServico = CriarOrdemServicoComInsumos(ordemServicoId);
            _ordemServicoController.ObterPorId(ordemServicoId).Returns(ordemServico);

            var exception = new Exception("Erro ao devolver insumos");
            _insumosOSController.DevolverInsumosAoEstoque(Arg.Any<IEnumerable<DevolverInsumoOSRequest>>())
                .Throws(exception);

            // Act
            var act = async () => await _handler.Handle(notification, cancellationToken);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Erro ao devolver insumos");
            
            _logServico.Received(1).LogErro(nameof(_handler.Handle), exception);
        }

        [Fact]
        public async Task Handle_ComMultiplosInsumos_DeveDevolverTodosCorretamente()
        {
            // Arrange
            var ordemServicoId = Guid.NewGuid();
            var notification = new OrdemServicoCanceladaEvent(ordemServicoId);
            var cancellationToken = CancellationToken.None;

            var ordemServico = CriarOrdemServicoComMultiplosInsumos(ordemServicoId);
            _ordemServicoController.ObterPorId(ordemServicoId).Returns(ordemServico);

            // Act
            await _handler.Handle(notification, cancellationToken);

            // Assert
            _logServico.Received(1).LogInicio(nameof(_handler.Handle), ordemServicoId);
            await _insumosOSController.Received(1).DevolverInsumosAoEstoque(
                Arg.Is<IEnumerable<DevolverInsumoOSRequest>>(requests => 
                    requests.Count() == 3)); // Deve ter 3 insumos
            _logServico.Received(1).LogFim(nameof(_handler.Handle), null);
        }

        private static OrdemServicoResponse CriarOrdemServicoComInsumos(Guid ordemServicoId)
        {
            return new OrdemServicoResponse
            {
                Id = ordemServicoId,
                Insumos = new List<InsumoOSResponse>
                {
                    new InsumoOSResponse
                    {
                        EstoqueId = Guid.NewGuid(),
                        Quantidade = 2
                    }
                }
            };
        }

        private static OrdemServicoResponse CriarOrdemServicoSemInsumos(Guid ordemServicoId)
        {
            return new OrdemServicoResponse
            {
                Id = ordemServicoId,
                Insumos = new List<InsumoOSResponse>() // Lista vazia
            };
        }

        private static OrdemServicoResponse CriarOrdemServicoComInsumosNull(Guid ordemServicoId)
        {
            return new OrdemServicoResponse
            {
                Id = ordemServicoId,
                Insumos = null // Insumos null
            };
        }

        private static OrdemServicoResponse CriarOrdemServicoComMultiplosInsumos(Guid ordemServicoId)
        {
            return new OrdemServicoResponse
            {
                Id = ordemServicoId,
                Insumos = new List<InsumoOSResponse>
                {
                    new InsumoOSResponse { EstoqueId = Guid.NewGuid(), Quantidade = 1 },
                    new InsumoOSResponse { EstoqueId = Guid.NewGuid(), Quantidade = 2 },
                    new InsumoOSResponse { EstoqueId = Guid.NewGuid(), Quantidade = 3 }
                }
            };
        }
    }
}
