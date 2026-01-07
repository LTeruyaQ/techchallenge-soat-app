namespace Core.Interfaces.Handlers.AlertasEstoque
{
    public interface IVerificarAlertaEnviadoHojeHandler
    {
        Task<bool> Handle(Guid estoqueId);
    }
}
