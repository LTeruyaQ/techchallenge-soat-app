using Core.DTOs.UseCases.Eventos;
using Core.Entidades;

namespace Core.Interfaces.Gateways
{
    public interface IEventoGateway
    {
        Task Publicar(OrdemServico ordemServico);
    }
}
