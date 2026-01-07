using Core.DTOs.Requests.Estoque;
using Core.DTOs.Responses.Estoque;
using Core.DTOs.Responses.Usuario;
using Core.Interfaces.Controllers;
using Core.Interfaces.Jobs;
using Core.Interfaces.root;
using Core.Interfaces.Servicos;
using Hangfire;
using System.Text;

namespace API.Jobs;

/// <summary>
/// Job para verificar estoque crítico e enviar alertas por email.
/// Segue padrão Clean Architecture: Job → Controllers → UseCases → Gateways → Repositórios
/// </summary>
public class VerificarEstoqueJob : IVerificarEstoqueJob
{
    private readonly ICompositionRoot _compositionRoot;
    private readonly IEstoqueController _estoqueController;
    private readonly IUsuarioController _usuarioController;
    private readonly IAlertaEstoqueController _alertaEstoqueController;
    private readonly ILogServico<VerificarEstoqueJob> _logServico;
    private readonly IServicoEmail _servicoEmail;

    public VerificarEstoqueJob(ICompositionRoot compositionRoot)
    {
        _compositionRoot = compositionRoot;
        _estoqueController = _compositionRoot.CriarEstoqueController();
        _usuarioController = _compositionRoot.CriarUsuarioController();
        _alertaEstoqueController = _compositionRoot.CriarAlertaEstoqueController();
        _logServico = _compositionRoot.CriarLogService<VerificarEstoqueJob>();
        _servicoEmail = _compositionRoot.CriarServicoEmail();
    }

    public async Task ExecutarAsync()
    {
        var metodo = nameof(ExecutarAsync);

        try
        {
            _logServico.LogInicio(metodo);

            var estoqueCritico = await _estoqueController.ObterEstoqueCritico();

            if (estoqueCritico.Any())
            {
                var insumosParaAlerta = new List<EstoqueResponse>();
                foreach (var insumo in estoqueCritico)
                {
                    var alertaJaEnviado = await _alertaEstoqueController.VerificarAlertaEnviadoHoje(insumo.Id);
                    if (!alertaJaEnviado)
                    {
                        insumosParaAlerta.Add(insumo);
                    }
                }

                if (insumosParaAlerta.Any())
                {
                    await EnviarAlertaEstoqueAsync(insumosParaAlerta);
                    await SalvarAlertaEnviadoAsync(insumosParaAlerta);
                }
            }

            _logServico.LogFim(metodo, estoqueCritico);
        }
        catch (Exception e)
        {
            _logServico.LogErro(metodo, e);

            throw;
        }
    }

    private async Task EnviarAlertaEstoqueAsync(IEnumerable<EstoqueResponse> insumosCriticos)
    {
        var metodo = nameof(EnviarAlertaEstoqueAsync);

        _logServico.LogInicio(metodo, insumosCriticos);
        try
        {
            var usuariosAlerta = await _usuarioController.ObterUsuariosParaAlertaEstoque();

            var conteudo = await GerarConteudoEmailAsync(insumosCriticos);

            if (!string.IsNullOrEmpty(conteudo))
            {
                await _servicoEmail.EnviarAsync(
                    usuariosAlerta.Select(u => u.Email),
                    "Alerta de Estoque Baixo",
                    conteudo
                );
            }

            _logServico.LogFim(metodo);
        }
        catch (Exception e)
        {
            _logServico.LogErro(metodo, e);
            throw;
        }
    }

    private async Task<string?> GerarConteudoEmailAsync(IEnumerable<EstoqueResponse> insumosCriticos)
    {
        var metodo = nameof(GerarConteudoEmailAsync);
        _logServico.LogInicio(metodo, insumosCriticos);

        try
        {
            string templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "EmailAlertaEstoque.html");
            string template = await File.ReadAllTextAsync(templatePath, Encoding.UTF8);

            var sbItens = new StringBuilder();
            foreach (var insumo in insumosCriticos)
            {
                sbItens.AppendLine($@"
                <li>
                    <strong>Insumo:</strong> {insumo.Insumo}<br/>
                    <strong>Quantidade Atual:</strong> {insumo.QuantidadeDisponivel}<br/>
                    <strong>Quantidade Mínima:</strong> {insumo.QuantidadeMinima}
                </li>
                <br/>");
            }

            string conteudoFinal = template.Replace("{{INSUMOS}}", sbItens.ToString());

            _logServico.LogFim(metodo, conteudoFinal);
            return conteudoFinal;
        }
        catch (Exception e)
        {
            _logServico.LogErro(metodo, e);
            throw;
        }
    }

    private async Task SalvarAlertaEnviadoAsync(IEnumerable<EstoqueResponse> insumosCriticos)
    {
        var metodo = nameof(SalvarAlertaEnviadoAsync);
        _logServico.LogInicio(metodo, insumosCriticos);
        try
        {
            var alertas = insumosCriticos
                .Select(insumo => new CadastrarAlertaEstoqueRequest
                {
                    EstoqueId = insumo.Id,
                    DataEnvio = DateTime.UtcNow
                });

            await _alertaEstoqueController.CadastrarAlertas(alertas);

            _logServico.LogFim(metodo);
        }
        catch (Exception e)
        {
            _logServico.LogErro(metodo, e);
            throw;
        }
    }
}
