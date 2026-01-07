using Core.DTOs.Responses.OrdemServico;
using Core.Interfaces.Controllers;
using Core.Interfaces.root;
using Core.Interfaces.Servicos;
using MediatR;
using System.Text;

namespace API.Notificacoes.OS;

public class OrdemServicoFinalizadaHandler : INotificationHandler<OrdemServicoFinalizadaEvent>
{
    private readonly ILogServico<OrdemServicoFinalizadaHandler> _logServico;
    private readonly IServicoEmail _emailServico;
    private readonly IOrdemServicoController _ordemServicoController;

    public OrdemServicoFinalizadaHandler(ICompositionRoot compositionRoot)
    {
        _logServico = compositionRoot.CriarLogService<OrdemServicoFinalizadaHandler>();
        _emailServico = compositionRoot.CriarServicoEmail();
        _ordemServicoController = compositionRoot.CriarOrdemServicoController();
    }

    public async Task Handle(OrdemServicoFinalizadaEvent notification, CancellationToken cancellationToken)
    {
        var metodo = nameof(Handle);

        try
        {
            _logServico.LogInicio(metodo, notification.OrdemServicoId);

            var osDto = await _ordemServicoController.ObterPorId(notification.OrdemServicoId);

            if (osDto is null) return;

            string conteudo = await GerarConteudoEmailAsync(osDto);

            await _emailServico.EnviarAsync(
                [osDto.Cliente.Contato.Email],
                "Serviço Finalizado",
                conteudo);

            _logServico.LogFim(metodo);
        }
        catch (Exception e)
        {
            _logServico.LogErro(metodo, e);

            throw;
        }
    }

    private static async Task<string> GerarConteudoEmailAsync(OrdemServicoResponse os)
    {
        const string templateFileName = "EmailOrdemServicoFinalizada.html";

        string templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", templateFileName);
        string template = await File.ReadAllTextAsync(templatePath, Encoding.UTF8);

        template = template
            .Replace("{{NOME_CLIENTE}}", os.Cliente.Nome)
            .Replace("{{NOME_SERVICO}}", os.Servico.Nome)
            .Replace("{{MODELO_VEICULO}}", os.Veiculo.Modelo)
            .Replace("{{PLACA_VEICULO}}", os.Veiculo.Placa);

        return template;
    }
}