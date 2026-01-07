using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infraestrutura.Dados.Extensions;

public static class DatabaseExtensions
{
    /// <summary>
    /// Aplica migrações pendentes do banco de dados de forma assíncrona
    /// </summary>
    /// <typeparam name="TContext">Tipo do contexto do banco de dados</typeparam>
    /// <param name="provedorServicos">Provedor de serviços</param>
    /// <returns>Task representando a operação assíncrona</returns>
    public static async Task AplicarMigracoesAsync<TContext>(this IServiceProvider provedorServicos)
        where TContext : DbContext
    {
        using var escopo = provedorServicos.CreateScope();
        var servicos = escopo.ServiceProvider;
        var logger = servicos.GetRequiredService<ILogger<TContext>>();
        var contexto = servicos.GetRequiredService<TContext>();

        try
        {
            logger.LogInformation("Verificando migrações pendentes...");
            var migracoesPendentes = await contexto.Database.GetPendingMigrationsAsync();

            if (migracoesPendentes.Any())
            {
                logger.LogInformation("Migrações pendentes encontradas: {MigracoesPendentes}",
                    string.Join(", ", migracoesPendentes));

                logger.LogInformation("Aplicando migrações...");
                await contexto.Database.MigrateAsync();

                logger.LogInformation("Migrações aplicadas com sucesso!");
            }
            else
            {
                logger.LogInformation("Nenhuma migração pendente para aplicar.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao aplicar migrações do banco de dados");
            throw new InvalidOperationException("Não foi possível aplicar as migrações do banco de dados. Verifique os logs para mais detalhes.", ex);
        }
    }
}
