using API.Notificacoes.OS;
using Core.DTOs.UseCases.Eventos;
using Core.Entidades;
using Core.Enumeradores;
using Core.Interfaces.Eventos;
using MediatR;

namespace API.Notificacoes
{
    public class OrdemServicoFinalizadaEventoPublisher : IEventoPublisher
    {
        private readonly IMediator _mediator;

        public StatusOrdemServico Status => StatusOrdemServico.Finalizada;

        public OrdemServicoFinalizadaEventoPublisher(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task PublicarAsync(OrdemServico ordemServico)
        {
            OrdemServicoFinalizadaEvent evento = new(ordemServico.Id);
            await _mediator.Publish(evento);
        }
    }
}
