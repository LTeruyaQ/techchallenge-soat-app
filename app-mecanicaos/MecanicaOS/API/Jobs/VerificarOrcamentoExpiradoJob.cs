using Core.DTOs.Requests.OrdemServico.InsumoOS;
using Core.Enumeradores;
using Core.Interfaces.Controllers;
using Core.Interfaces.root;
using Core.Interfaces.Servicos;
using Hangfire;

namespace API.Jobs;

/// <summary>
/// Job para verificar orçamentos expirados e devolver insumos ao estoque.
/// Segue padrão Clean Architecture: Job → Controllers → UseCases → Gateways → Repositórios
/// </summary>
public class VerificarOrcamentoExpiradoJob
{
    private readonly ICompositionRoot _compositionRoot;
    private readonly IOrdemServicoController _ordemServicoController;
    private readonly IInsumoOSController _insumoOSController;
    private readonly ILogServico<VerificarOrcamentoExpiradoJob> _logServico;

    public VerificarOrcamentoExpiradoJob(ICompositionRoot compositionRoot)
    {
        _compositionRoot = compositionRoot;
        _ordemServicoController = _compositionRoot.CriarOrdemServicoController();
        _insumoOSController = _compositionRoot.CriarInsumoOSController();
        _logServico = _compositionRoot.CriarLogService<VerificarOrcamentoExpiradoJob>();
    }

    public async Task ExecutarAsync()
    {
        var metodo = nameof(ExecutarAsync);

        try
        {
            _logServico.LogInicio(metodo);

            var ordensComOrcamentoExpirado = await _ordemServicoController.ObterOrcamentosExpirados();

            if (ordensComOrcamentoExpirado.Any())
            {
                foreach (var ordemServico in ordensComOrcamentoExpirado)
                {
                    if (ordemServico.Insumos != null && ordemServico.Insumos.Any())
                    {
                        var devolverInsumosRequest = ordemServico.Insumos.Select(insumo =>
                            new DevolverInsumoOSRequest
                            {
                                EstoqueId = insumo.EstoqueId,
                                Quantidade = insumo.Quantidade
                            });

                        await _insumoOSController.DevolverInsumosAoEstoque(devolverInsumosRequest);
                    }

                    await _ordemServicoController.Atualizar(
                        ordemServico.Id,
                        new()
                        {
                            Status = StatusOrdemServico.OrcamentoExpirado
                        });
                }

                _logServico.LogInicio($"Processadas {ordensComOrcamentoExpirado.Count()} ordens com orçamento expirado");
            }

            _logServico.LogFim(metodo);
        }
        catch (Exception e)
        {
            _logServico.LogErro(metodo, e);
            throw;
        }
    }
}
