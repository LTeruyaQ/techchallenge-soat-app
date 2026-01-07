using Adapters.Gateways;
using Core.DTOs.UseCases.Eventos;
using Core.Entidades;
using Core.Enumeradores;
using Core.Interfaces.Eventos;
using Core.Interfaces.Gateways;

namespace MecanicaOS.UnitTests.Adapters.Gateways
{
    /// <summary>
    /// Testes para EventosGateway
    /// Importância: Valida publicação de eventos de domínio.
    /// Garante que eventos sejam publicados corretamente para o sistema de mensageria.
    /// </summary>
    public class EventosGatewayTests
    {
        /// <summary>
        /// Verifica se o gateway publica evento de ordem finalizada
        /// Importância: Valida publicação de eventos de conclusão
        /// </summary>
        [Fact]
        public async Task Publicar_OrdemServicoFinalizada_DeveDelegarParaPublisherCorreto()
        {
            //Arrange
            var publisherFinalizadaMock = Substitute.For<IEventoPublisher>();

            publisherFinalizadaMock.Status.Returns(StatusOrdemServico.Finalizada);

            var colecaoDeHandlers = new List<IEventoPublisher>
            {
                publisherFinalizadaMock,
            };

            var gateway = new EventosGateway(colecaoDeHandlers);

            var ordemServico = new OrdemServico { Id = Guid.NewGuid(), Status = StatusOrdemServico.Finalizada };

            // Act
            await gateway.Publicar(ordemServico);

            // Assert
            await publisherFinalizadaMock.Received(1).PublicarAsync(
                Arg.Is<OrdemServico>(os => os.Id == ordemServico.Id));
        }


        /// <summary>
        /// Verifica se o gateway publica evento de ordem em orçamento
        /// Importância: Valida publicação de eventos de orçamento
        /// </summary>
        [Fact]
        public async Task Publicar_OrdemServicoEmOrcamento_DevePublicarEvento()
        {
            //Arrange
            var publisherEmOrcamentoMock = Substitute.For<IEventoPublisher>();

            publisherEmOrcamentoMock.Status.Returns(StatusOrdemServico.EmDiagnostico);

            var colecaoDeHandlers = new List<IEventoPublisher>
            {
                publisherEmOrcamentoMock,
            };

            var gateway = new EventosGateway(colecaoDeHandlers);

            var ordemServico = new OrdemServico { Id = Guid.NewGuid(), Status = StatusOrdemServico.EmDiagnostico };

            // Act
            await gateway.Publicar(ordemServico);

            // Assert
            await publisherEmOrcamentoMock.Received(1).PublicarAsync(
                Arg.Is<OrdemServico>(os => os.Id == ordemServico.Id));
        }

        /// <summary>
        /// Verifica se o gateway publica evento de ordem cancelada
        /// Importância: Valida publicação de eventos de cancelamento
        /// </summary>
        [Fact]
        public async Task Publicar_OrdemServicoCancelada_DevePublicarEvento()
        {
            //Arrange
            var publisherCanceladaMock = Substitute.For<IEventoPublisher>();

            publisherCanceladaMock.Status.Returns(StatusOrdemServico.Cancelada);

            var colecaoDeHandlers = new List<IEventoPublisher>
            {
                publisherCanceladaMock,
            };

            var gateway = new EventosGateway(colecaoDeHandlers);

            var ordemServico = new OrdemServico { Id = Guid.NewGuid(), Status = StatusOrdemServico.Cancelada };

            // Act
            await gateway.Publicar(ordemServico);

            // Assert
            await publisherCanceladaMock.Received(1).PublicarAsync(
                Arg.Is<OrdemServico>(os => os.Id == ordemServico.Id));
        }
    }
}
