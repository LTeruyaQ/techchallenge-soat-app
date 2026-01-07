using Core.DTOs.Requests.OrdemServico.InsumoOS;
using Core.DTOs.UseCases.OrdemServico.InsumoOS;
using Core.Entidades;
using Core.Interfaces.Handlers.InsumosOS;
using Core.Interfaces.UseCases;

namespace Core.UseCases.InsumosOS
{
    public class InsumoOSUseCasesFacade : IInsumoOSUseCases
    {
        private readonly ICadastrarInsumosHandler _cadastrarInsumosHandler;

        public InsumoOSUseCasesFacade(
            ICadastrarInsumosHandler cadastrarInsumosHandler)
        {
            _cadastrarInsumosHandler = cadastrarInsumosHandler ?? throw new ArgumentNullException(nameof(cadastrarInsumosHandler));
        }

        public async Task<List<InsumoOS>> CadastrarInsumosUseCaseAsync(Guid ordemServicoId, List<CadastrarInsumoOSUseCaseDto> request)
        {
            return await _cadastrarInsumosHandler.Handle(ordemServicoId, request);
        }

        public async Task<bool> DevolverInsumosAoEstoqueUseCaseAsync(IEnumerable<DevolverInsumoOSRequest> insumosOS)
        {
            // A lógica foi movida para o controller
            return await Task.FromResult(true);
        }
    }
}
