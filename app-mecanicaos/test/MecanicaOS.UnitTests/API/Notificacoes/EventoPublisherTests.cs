using API.Notificacoes;
using API.Notificacoes.OS;
using Core.DTOs.UseCases.Eventos;
using Core.Entidades;
using Core.Enumeradores;
using MediatR;
using NSubstitute;
using Xunit;

namespace MecanicaOS.UnitTests.API.Notificacoes
{
    public class EventoPublisherTests
    {
        [Fact]
        public async Task Publicar_OrdemServicoFinalizadaEvent_DeveChamarMediator()
        {
            // Arrange
            var mediator = Substitute.For<IMediator>();
            var publisher = new OrdemServicoFinalizadaEventoPublisher(mediator);
            var ordemServico = new OrdemServico { Status = StatusOrdemServico.Finalizada};

            // Act
            await publisher.PublicarAsync(ordemServico);

            // Assert
            await mediator.Received(1).Publish(Arg.Any<INotification>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Publicar_OrdemServicoEmOrcamentoEvent_DeveChamarMediator()
        {
            // Arrange
            var mediator = Substitute.For<IMediator>();
            var publisher = new OrdemServicoEmOrcamentoEventoPublisher(mediator);
            var ordemServico = new OrdemServico { Status = StatusOrdemServico.EmDiagnostico };

            // Act
            await publisher.PublicarAsync(ordemServico);

            // Assert
            await mediator.Received(1).Publish(Arg.Any<INotification>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Publicar_OrdemServicoCanceladaEvent_DeveChamarMediator()
        {
            // Arrange
            var mediator = Substitute.For<IMediator>();
            var publisher = new OrdemServicoCanceladaEventoPublisher(mediator);
            var ordemServico = new OrdemServico { Status = StatusOrdemServico.Cancelada };

            // Act
            await publisher.PublicarAsync(ordemServico);

            // Assert
            await mediator.Received(1).Publish(Arg.Any<INotification>(), Arg.Any<CancellationToken>());
        }
    }
}
