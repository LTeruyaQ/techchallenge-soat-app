using Core.Interfaces.Gateways;
using Core.Interfaces.Jobs;

namespace Adapters.Gateways
{
    public class VerificarEstoqueJobGateway : IVerificarEstoqueJobGateway
    {
        private readonly IVerificarEstoqueJob _verificarEstoqueJob;

        public VerificarEstoqueJobGateway(IVerificarEstoqueJob verificarEstoqueJob)
        {
            _verificarEstoqueJob = verificarEstoqueJob;
        }

        public async Task VerificarEstoqueAsync()
        {
            await _verificarEstoqueJob.ExecutarAsync();
        }
    }
}
