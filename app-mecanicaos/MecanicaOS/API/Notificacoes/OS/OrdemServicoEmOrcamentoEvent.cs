using MediatR;

namespace API.Notificacoes.OS;

public class OrdemServicoEmOrcamentoEvent(Guid ordemServicoId) : INotification
{
    public Guid OrdemServicoId { get; } = ordemServicoId;
}