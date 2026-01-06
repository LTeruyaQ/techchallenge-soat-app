using Core.Interfaces.Servicos;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Infraestrutura.Servicos;

public class ServicoEmail : IServicoEmail
{
    private readonly string _apiKey;

    public ServicoEmail(IConfiguration configuration)
    {
        _apiKey = configuration["SENDGRID_APIKEY"]!;
    }

    public async Task EnviarAsync(IEnumerable<string> emailsDestino, string assunto, string conteudo)
    {
        var client = new SendGridClient(_apiKey);

        var remetente = new EmailAddress("mecanicaosbr@gmail.com", "MecanicaOS");

        var destinatarios = emailsDestino
        .Select(email => new EmailAddress(email))
        .ToList();

        var msg = MailHelper.CreateSingleEmailToMultipleRecipients(
            from: remetente,
            tos: destinatarios,
            subject: assunto,
            plainTextContent: null,
            htmlContent: conteudo,
            showAllRecipients: false
        );

        var response = await client.SendEmailAsync(msg);
    }
}