using Core.DTOs.Requests.OrdemServico.InsumoOS;
using Core.Interfaces.Controllers;
using Core.Interfaces.root;
using Core.Interfaces.Servicos;
using MediatR;

namespace API.Notificacoes.OS;

public class OrdemServicoCanceladaHandler : INotificationHandler<OrdemServicoCanceladaEvent>
{
    private readonly IOrdemServicoController _ordemServicoController;
    private readonly IInsumoOSController _insumosOSController;
    private readonly ILogServico<OrdemServicoCanceladaHandler> _logServico;

    public OrdemServicoCanceladaHandler(ICompositionRoot compositionRoot)
    {
        _ordemServicoController = compositionRoot.CriarOrdemServicoController();
        _insumosOSController = compositionRoot.CriarInsumoOSController();
        _logServico = compositionRoot.CriarLogService<OrdemServicoCanceladaHandler>();
    }

    public async Task Handle(OrdemServicoCanceladaEvent notification, CancellationToken cancellationToken)
    {
        var metodo = nameof(Handle);

        try
        {
            _logServico.LogInicio(metodo, notification.OrdemServicoId);

            var os = await _ordemServicoController.ObterPorId(notification.OrdemServicoId);

            var insumosOSDto = os.Insumos;
            if (insumosOSDto == null || !insumosOSDto.Any())
            {
                _logServico.LogFim(metodo, null);
                return;
            }

            await _insumosOSController.DevolverInsumosAoEstoque(os.Insumos.Select(i => new DevolverInsumoOSRequest()
            {
                EstoqueId = i.EstoqueId,
                Quantidade = i.Quantidade
            }));

            _logServico.LogFim(metodo, null);
        }
        catch (Exception e)
        {
            _logServico.LogErro(metodo, e);

            throw;
        }
    }
}
