using Adapters.Presenters;
using Core.DTOs.Requests.OrdemServico;
using Core.DTOs.Responses.OrdemServico;
using Core.DTOs.UseCases.OrdemServico;
using Core.Enumeradores;
using Core.Exceptions;
using Core.Interfaces.Controllers;
using Core.Interfaces.Presenters;
using Core.Interfaces.root;
using Core.Interfaces.UseCases;

namespace Adapters.Controllers
{
    public class OrdemServicoController : IOrdemServicoController
    {
        private readonly IOrdemServicoUseCases _ordemServicoUseCases;
        private readonly IOrcamentoUseCases _orcamentoUseCases;
        private readonly IClienteUseCases _clienteUseCases;
        private readonly IServicoUseCases _servicoUseCases;
        private readonly IVeiculoUseCases _veiculoUseCases;
        private readonly IOrdemServicoPresenter _ordemServicoPresenter;

        public OrdemServicoController(ICompositionRoot compositionRoot)
        {
            _ordemServicoUseCases = compositionRoot.CriarOrdemServicoUseCases();
            _ordemServicoPresenter = new OrdemServicoPresenter();
            _orcamentoUseCases = compositionRoot.CriarOrcamentoUseCases();
            _clienteUseCases = compositionRoot.CriarClienteUseCases();
            _servicoUseCases = compositionRoot.CriarServicoUseCases();
            _veiculoUseCases = compositionRoot.CriarVeiculoUseCases();
        }

        public async Task<IEnumerable<OrdemServicoResponse>> ObterTodos()
        {
            return _ordemServicoPresenter.ParaResponse(await _ordemServicoUseCases.ObterTodosUseCaseAsync())
                .Where(response => response != null)!;
        }

        public async Task<OrdemServicoResponse> ObterPorId(Guid id)
        {
            var ordemServico = await _ordemServicoUseCases.ObterPorIdUseCaseAsync(id);

            var response = _ordemServicoPresenter.ParaResponse(ordemServico) ??
                throw new InvalidOperationException("A ordem de serviço não pode ser nula.");

            return response;
        }

        public async Task<IEnumerable<OrdemServicoResponse>> ObterPorStatus(StatusOrdemServico status)
        {
            return _ordemServicoPresenter.ParaResponse(await _ordemServicoUseCases.ObterPorStatusUseCaseAsync(status))
                .Where(response => response != null)!;
        }

        public async Task CalcularOrcamentoAsync(Guid id)
        {
            var ordemServico = await _ordemServicoUseCases.ObterPorIdUseCaseAsync(id);

            if (ordemServico != null)
            {
                var orcamento = _orcamentoUseCases.GerarOrcamentoUseCase(ordemServico);
                await _ordemServicoUseCases.AtualizarUseCaseAsync(id, new() { Orcamento = orcamento });
            }
        }

        public async Task<OrdemServicoResponse> Cadastrar(CadastrarOrdemServicoRequest request)
        {
            var useCaseDto = MapearParaCadastrarOrdemServicoUseCaseDto(request);

            var veiculo = await _veiculoUseCases.ObterPorIdUseCaseAsync(request.VeiculoId)
                ?? throw new DadosNaoEncontradosException("Veículo não encontrado");

            var cliente = await _clienteUseCases.ObterPorIdUseCaseAsync(request.ClienteId)
                ?? throw new DadosNaoEncontradosException("Cliente não encontrado");

            var servico = await _servicoUseCases.ObterServicoPorIdUseCaseAsync(request.ServicoId)
                ?? throw new DadosNaoEncontradosException("Serviço não encontrado");

            useCaseDto.Veiculo = veiculo;
            useCaseDto.Cliente = cliente;
            useCaseDto.Servico = servico;

            var resultado = await _ordemServicoUseCases.CadastrarUseCaseAsync(useCaseDto);
            var response = _ordemServicoPresenter.ParaResponse(resultado) ??
                throw new InvalidOperationException("O resultado do cadastro não pode ser nulo.");

            return response;
        }

        internal CadastrarOrdemServicoUseCaseDto MapearParaCadastrarOrdemServicoUseCaseDto(CadastrarOrdemServicoRequest request)
        {
            if (request is null)
                return null;

            return new CadastrarOrdemServicoUseCaseDto
            {
                ClienteId = request.ClienteId,
                VeiculoId = request.VeiculoId,
                ServicoId = request.ServicoId,
                Descricao = request.Descricao
            };
        }

        public async Task<OrdemServicoResponse> Atualizar(Guid id, AtualizarOrdemServicoRequest request)
        {
            var useCaseDto = MapearParaAtualizarOrdemServicoUseCaseDto(request);
            var resultado = await _ordemServicoUseCases.AtualizarUseCaseAsync(id, useCaseDto);
            var response = _ordemServicoPresenter.ParaResponse(resultado);
            if (response == null)
                throw new InvalidOperationException("O resultado da atualização não pode ser nulo.");
            return response;
        }

        internal AtualizarOrdemServicoUseCaseDto MapearParaAtualizarOrdemServicoUseCaseDto(AtualizarOrdemServicoRequest request)
        {
            if (request is null)
                return null;

            return new AtualizarOrdemServicoUseCaseDto
            {
                Descricao = request.Descricao,
                Status = request.Status,
                DataEnvioOrcamento = request.DataEnvioOrcamento
            };
        }

        public async Task AceitarOrcamento(Guid id)
        {
            await _ordemServicoUseCases.AceitarOrcamentoUseCaseAsync(id);

            await _ordemServicoUseCases.AtualizarUseCaseAsync(id, new AtualizarOrdemServicoUseCaseDto
            {
                Status = StatusOrdemServico.EmExecucao,
            });
        }

        public async Task RecusarOrcamento(Guid id)
        {
            await _ordemServicoUseCases.RecusarOrcamentoUseCaseAsync(id);
        }

        public async Task<IEnumerable<OrdemServicoResponse>> ObterOrcamentosExpirados()
        {
            var ordens = await _ordemServicoUseCases.ObterOrcamentosExpiradosUseCaseAsync();
            return _ordemServicoPresenter.ParaResponse(ordens)
                .Where(response => response != null)!;
        }

        public async Task<IEnumerable<OrdemServicoResponse>> ListarOrdensServicoAtivas()
        {
            var ordensServico = await _ordemServicoUseCases.ListarOrdensServicoAtivasUseCaseAsync();
            return _ordemServicoPresenter.ParaResponse(ordensServico)
                .Where(response => response != null)!;
        }
    }
}
