namespace Core.Interfaces.Repositorios
{
    public interface IUnidadeDeTrabalho
    {
        Task<bool> Commit();
    }
}
