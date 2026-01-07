namespace Core.Interfaces.Gateways
{
    public interface IUnidadeDeTrabalhoGateway
    {
        Task<bool> Commit();
    }
}
