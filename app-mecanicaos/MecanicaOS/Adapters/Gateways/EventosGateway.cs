using Core.Entidades;
using Core.Enumeradores;
using Core.Interfaces.Eventos;
using Core.Interfaces.Gateways;

namespace Adapters.Gateways
{
    public class EventosGateway : IEventoGateway
    {
        private readonly Dictionary<StatusOrdemServico, IEventoPublisher> _handlers;

        public EventosGateway(IEnumerable<IEventoPublisher> handlers)
        {
            _handlers = handlers.ToDictionary(h => h.Status);

        }

        public async Task Publicar(OrdemServico ordemServico)
        {
            if (_handlers.TryGetValue(ordemServico.Status, out var handler))
                await handler.PublicarAsync(ordemServico);
        }
    }
}
