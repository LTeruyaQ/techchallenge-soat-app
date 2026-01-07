using MediatR;

namespace API.Notificacoes.OS;

public class OrdemServicoCanceladaEvent(Guid ordemServicoId) : INotification
{
    public Guid OrdemServicoId { get; } = ordemServicoId;
}