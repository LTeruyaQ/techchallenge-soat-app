using Core.Interfaces.Servicos;

namespace API.Middlewares
{
    public class IdCorrelacionalLogMiddleware : IMiddleware
    {
        private const string CorrelationIdHeader = "X-Correlation-ID";

        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var correlationIdDemoAPILogService = context.RequestServices.GetRequiredService<IIdCorrelacionalService>();

            if (correlationIdDemoAPILogService is not null)
            {
                if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId) &&
                    Guid.TryParse(correlationId, out var correlationIdGuid))
                {
                    correlationIdDemoAPILogService.SetCorrelationId(correlationIdGuid);
                }

                context.Response.OnStarting(() =>
                {
                    if (!context.Response.Headers.ContainsKey(CorrelationIdHeader))
                    {
                        context.Response.Headers.Append(CorrelationIdHeader, correlationIdDemoAPILogService.GetCorrelationId());
                    }
                    return Task.CompletedTask;
                });
            }

            return next(context);
        }
    }
}
