using Adapters.Presenters;
using Core.DTOs.Requests.Veiculo;
using Core.DTOs.Responses.Veiculo;
using Core.DTOs.UseCases.Veiculo;
using Core.Interfaces.Controllers;
using Core.Interfaces.Presenters;
using Core.Interfaces.root;
using Core.Interfaces.UseCases;

namespace Adapters.Controllers
{
    public class VeiculoController : IVeiculoController
    {
        private readonly IVeiculoUseCases _veiculoUseCases;
        private readonly IVeiculoPresenter _veiculoPresenter;
        private readonly IClienteUseCases _clienteUseCases;

        public VeiculoController(ICompositionRoot compositionRoot)
        {
            _veiculoUseCases = compositionRoot.CriarVeiculoUseCases();
            _clienteUseCases = compositionRoot.CriarClienteUseCases();
            _veiculoPresenter = new VeiculoPresenter();
        }

        public async Task<IEnumerable<VeiculoResponse>> ObterTodos()
        {
            return _veiculoPresenter.ParaResponse(await _veiculoUseCases.ObterTodosUseCaseAsync());
        }

        public async Task<VeiculoResponse> ObterPorId(Guid id)
        {
            return _veiculoPresenter.ParaResponse(await _veiculoUseCases.ObterPorIdUseCaseAsync(id));
        }

        public async Task<VeiculoResponse> ObterPorPlaca(string placa)
        {
            return _veiculoPresenter.ParaResponse(await _veiculoUseCases.ObterPorPlacaUseCaseAsync(placa));
        }

        public async Task<IEnumerable<VeiculoResponse>> ObterPorCliente(Guid clienteId)
        {
            return _veiculoPresenter.ParaResponse(await _veiculoUseCases.ObterPorClienteUseCaseAsync(clienteId));
        }

        public async Task<VeiculoResponse> Cadastrar(CadastrarVeiculoRequest request)
        {
            _ = await _clienteUseCases.ObterPorIdUseCaseAsync(request.ClienteId);
            var useCaseDto = MapearParaCadastrarVeiculoUseCaseDto(request);
            var resultado = await _veiculoUseCases.CadastrarUseCaseAsync(useCaseDto);
            return _veiculoPresenter.ParaResponse(resultado);
        }

        internal CadastrarVeiculoUseCaseDto MapearParaCadastrarVeiculoUseCaseDto(CadastrarVeiculoRequest request)
        {
            if (request is null)
                return null;

            return new CadastrarVeiculoUseCaseDto
            {
                Placa = request.Placa,
                Marca = request.Marca,
                Modelo = request.Modelo,
                Cor = request.Cor,
                Ano = request.Ano,
                Anotacoes = request.Anotacoes,
                ClienteId = request.ClienteId
            };
        }

        public async Task<VeiculoResponse> Atualizar(Guid id, AtualizarVeiculoRequest request)
        {
            var useCaseDto = MapearParaAtualizarVeiculoUseCaseDto(request);
            var resultado = await _veiculoUseCases.AtualizarUseCaseAsync(id, useCaseDto);
            return _veiculoPresenter.ParaResponse(resultado);
        }

        internal AtualizarVeiculoUseCaseDto MapearParaAtualizarVeiculoUseCaseDto(AtualizarVeiculoRequest request)
        {
            if (request is null)
                return null;

            return new AtualizarVeiculoUseCaseDto
            {
                Placa = request.Placa,
                Marca = request.Marca,
                Modelo = request.Modelo,
                Cor = request.Cor,
                Ano = request.Ano,
                Anotacoes = request.Anotacoes,
                ClienteId = request.ClienteId
            };
        }

        public async Task<bool> Deletar(Guid id)
        {
            return await _veiculoUseCases.DeletarUseCaseAsync(id);
        }
    }
}
