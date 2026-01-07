using Core.Interfaces.Repositorios;

namespace Infraestrutura.Dados.UdT
{
    public class UnidadeDeTrabalho : IUnidadeDeTrabalho
    {
        private readonly MecanicaContexto _contexto;

        public UnidadeDeTrabalho(MecanicaContexto contexto)
        {
            _contexto = contexto;
        }

        public async Task<bool> Commit()
        {
            return await _contexto.SaveChangesAsync() > 0;
        }
    }
}
