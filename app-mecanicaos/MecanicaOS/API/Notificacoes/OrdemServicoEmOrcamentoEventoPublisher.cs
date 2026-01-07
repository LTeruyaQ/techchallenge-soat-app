using API.Notificacoes.OS;
using Core.Entidades;
using Core.Enumeradores;
using Core.Interfaces.Eventos;
using MediatR;

namespace API.Notificacoes
{
    public class OrdemServicoEmOrcamentoEventoPublisher : IEventoPublisher
    {
        private readonly IMediator _mediator;

        public StatusOrdemServico Status => StatusOrdemServico.EmDiagnostico;

        public OrdemServicoEmOrcamentoEventoPublisher(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task PublicarAsync(OrdemServico ordemServico)
        {
            OrdemServicoEmOrcamentoEvent evento = new(ordemServico.Id);
            await _mediator.Publish(evento);
        }
    }
}
