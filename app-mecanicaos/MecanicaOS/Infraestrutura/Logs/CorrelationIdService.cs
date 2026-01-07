using Core.Interfaces.Servicos;

namespace Infraestrutura.Logs
{
    public class IdCorrelacionalService : IIdCorrelacionalService
    {
        private string _correlationId = Guid.NewGuid().ToString();

        public string GetCorrelationId()
        {
            return _correlationId;
        }

        public void SetCorrelationId(Guid? correlationId = null)
        {
            if (correlationId.HasValue && correlationId != Guid.Empty)
            {
                _correlationId = correlationId.Value.ToString();
            }
        }
    }
}