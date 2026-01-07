using Core.DTOs.Requests.OrdemServico;
using Core.DTOs.Responses.OrdemServico;
using Core.DTOs.Responses.OrdemServico.InsumoOrdemServico;
using Core.Enumeradores;
using Core.Interfaces.Controllers;
using Core.Interfaces.root;
using Core.Interfaces.Servicos;
using MediatR;
using System.Text;
using System.Text.RegularExpressions;

namespace API.Notificacoes.OS;

public class OrdemServicoEmOrcamentoHandler : INotificationHandler<OrdemServicoEmOrcamentoEvent>
{
    private readonly IServicoEmail _emailServico;
    private readonly ILogServico<OrdemServicoEmOrcamentoHandler> _logServico;
    private readonly IOrdemServicoController _ordemServicoController;
    public OrdemServicoEmOrcamentoHandler(ICompositionRoot compositionRoot)
    {
        _emailServico = compositionRoot.CriarServicoEmail();
        _logServico = compositionRoot.CriarLogService<OrdemServicoEmOrcamentoHandler>();
        _ordemServicoController = compositionRoot.CriarOrdemServicoController();
    }

    public async Task Handle(OrdemServicoEmOrcamentoEvent notification, CancellationToken cancellationToken)
    {
        var metodo = nameof(Handle);

        try
        {
            _logServico.LogInicio(metodo, notification.OrdemServicoId);

            await _ordemServicoController.CalcularOrcamentoAsync(notification.OrdemServicoId);

            var os = await _ordemServicoController.ObterPorId(notification.OrdemServicoId);

            await EnviarOrcamentoAsync(os);

            await _ordemServicoController.Atualizar(os.Id,
                new AtualizarOrdemServicoRequest
                {
                    Status = StatusOrdemServico.AguardandoAprovacao,
                    DataEnvioOrcamento = DateTime.Now
                });

            _logServico.LogFim(metodo);
        }
        catch (Exception e)
        {
            _logServico.LogErro(metodo, e);

            throw;
        }
    }

    private async Task EnviarOrcamentoAsync(OrdemServicoResponse os)
    {
        string conteudo = await GerarConteudoEmailAsync(os);

        await _emailServico.EnviarAsync(
            [os.Cliente!.Contato!.Email],
            "Orçamento de Serviço",
            conteudo);
    }

    private static async Task<string> GerarConteudoEmailAsync(OrdemServicoResponse os)
    {
        const string templateFileName = "EmailOrcamentoOS.html";

        string templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", templateFileName);
        string template = await File.ReadAllTextAsync(templatePath, Encoding.UTF8);

        template = template
            .Replace("{{NOME_CLIENTE}}", os.Cliente.Nome)
            .Replace("{{NOME_SERVICO}}", os.Servico.Nome)
            .Replace("{{VALOR_SERVICO}}", os.Servico.Valor.ToString("N2"))
            .Replace("{{VALOR_TOTAL}}", os.Orcamento!.Value.ToString("N2"));

        string insumosHtml = GerarHtmlInsumos(os.Insumos);
        template = Regex.Replace(template, @"{{#each INSUMOS}}(.*?){{/each}}", insumosHtml, RegexOptions.Singleline);

        return template;
    }

    private static string GerarHtmlInsumos(IEnumerable<InsumoOSResponse> insumosOS)
    {
        return string.Join(Environment.NewLine, insumosOS.Select(i =>
        {
            var descricao = i.Estoque!.Insumo;
            var quantidade = i.Quantidade;
            var precoTotal = quantidade * i.Estoque.Preco;
            return $"""
                        <tr>
                            <td>{descricao} ({quantidade} und)</td>
                            <td>R$ {precoTotal:N2}</td>
                        </tr>
                    """;
        }));
    }
}
