using API.Notificacoes.OS;
using Core.Entidades;
using Core.Enumeradores;
using Core.Interfaces.Eventos;
using MediatR;

namespace API.Notificacoes
{
    public class OrdemServicoCanceladaEventoPublisher : IEventoPublisher
    {
        private readonly IMediator _mediator;

        public StatusOrdemServico Status => StatusOrdemServico.Cancelada;

        public OrdemServicoCanceladaEventoPublisher(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task PublicarAsync(OrdemServico ordemServico)
        {
            OrdemServicoCanceladaEvent evento = new(ordemServico.Id);
            await _mediator.Publish(evento);
        }
    }
}