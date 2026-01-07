using API.Jobs;
using Hangfire;

namespace API
{
    //TODO: ver para onde essa classe vai
    public class RecurringJobs
    {
        private readonly IRecurringJobManager _recurringJobManager;

        public RecurringJobs(IRecurringJobManager recurringJobManager)
        {
            _recurringJobManager = recurringJobManager;
        }

        public void ScheduleJobs()
        {
            _recurringJobManager.AddOrUpdate<VerificarEstoqueJob>(
                recurringJobId: "verificar-estoque",
                methodCall: job => job.ExecutarAsync(),
                cronExpression: Cron.Hourly(),
                options: new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.Local
                }
            );

            _recurringJobManager.AddOrUpdate<VerificarOrcamentoExpiradoJob>(
                recurringJobId: "verificar-orcamento-expirado",
                methodCall: job => job.ExecutarAsync(),
                cronExpression: Cron.Hourly(),
                options: new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.Local
                }
            );
        }
    }
}