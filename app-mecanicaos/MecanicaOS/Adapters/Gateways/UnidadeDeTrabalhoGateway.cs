using Core.Interfaces.Gateways;
using Core.Interfaces.Repositorios;

namespace Adapters.Gateways
{
    public class UnidadeDeTrabalhoGateway : IUnidadeDeTrabalhoGateway
    {
        private readonly IUnidadeDeTrabalho _unidadeDeTrabalho;

        public UnidadeDeTrabalhoGateway(IUnidadeDeTrabalho unidadeDeTrabalho)
        {
            _unidadeDeTrabalho = unidadeDeTrabalho ?? throw new ArgumentNullException(nameof(unidadeDeTrabalho));
        }

        public async Task<bool> Commit()
        {
            return await _unidadeDeTrabalho.Commit();
        }
    }
}
