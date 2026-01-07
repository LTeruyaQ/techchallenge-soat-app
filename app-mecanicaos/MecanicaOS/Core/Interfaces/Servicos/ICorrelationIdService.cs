namespace Core.Interfaces.Servicos
{
    public interface IIdCorrelacionalService
    {
        string GetCorrelationId();
        void SetCorrelationId(Guid? correlationId = null);
    }
}
