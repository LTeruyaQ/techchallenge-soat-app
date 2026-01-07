using Adapters.Presenters;
using Core.DTOs.Requests.Cliente;
using Core.DTOs.Responses.Cliente;
using Core.DTOs.UseCases.Cliente;
using Core.Interfaces.Controllers;
using Core.Interfaces.Presenters;
using Core.Interfaces.root;
using Core.Interfaces.UseCases;

namespace Adapters.Controllers
{
    public class ClienteController : IClienteController
    {
        private readonly IClienteUseCases _clienteUseCases;
        private readonly IClientePresenter _clientePresenter;

        public ClienteController(ICompositionRoot compositionRoot)
        {
            _clienteUseCases = compositionRoot.CriarClienteUseCases();
            _clientePresenter = new ClientePresenter();
        }

        public async Task<IEnumerable<ClienteResponse>> ObterTodos()
        {
            return _clientePresenter.ParaResponse(await _clienteUseCases.ObterTodosUseCaseAsync());
        }

        public async Task<ClienteResponse> ObterPorId(Guid id)
        {
            return _clientePresenter.ParaResponse(await _clienteUseCases.ObterPorIdUseCaseAsync(id));
        }

        public async Task<ClienteResponse> ObterPorDocumento(string documento)
        {
            return _clientePresenter.ParaResponse(await _clienteUseCases.ObterPorDocumentoUseCaseAsync(documento));
        }

        public async Task<IEnumerable<ClienteResponse>> ObterPorNome(string nome)
        {
            return _clientePresenter.ParaResponse(await _clienteUseCases.ObterPorNomeUseCaseAsync(nome));
        }

        public async Task<ClienteResponse> Cadastrar(CadastrarClienteRequest request)
        {
            var useCaseDto = MapearParaCadastrarClienteUseCaseDto(request);
            var resultado = await _clienteUseCases.CadastrarUseCaseAsync(useCaseDto);
            return _clientePresenter.ParaResponse(resultado);
        }

        internal CadastrarClienteUseCaseDto MapearParaCadastrarClienteUseCaseDto(CadastrarClienteRequest request)
        {
            if (request is null)
                return null;

            return new CadastrarClienteUseCaseDto
            {
                Nome = request.Nome,
                Sexo = request.Sexo,
                Documento = request.Documento,
                DataNascimento = request.DataNascimento,
                TipoCliente = request.TipoCliente,
                Rua = request.Rua,
                Bairro = request.Bairro,
                Cidade = request.Cidade,
                Numero = request.Numero,
                CEP = request.CEP,
                Complemento = request.Complemento,
                Email = request.Email,
                Telefone = request.Telefone
            };
        }

        public async Task<ClienteResponse> Atualizar(Guid id, AtualizarClienteRequest request)
        {
            var useCaseDto = MapearParaAtualizarClienteUseCaseDto(request);
            var resultado = await _clienteUseCases.AtualizarUseCaseAsync(id, useCaseDto);
            return _clientePresenter.ParaResponse(resultado);
        }

        internal AtualizarClienteUseCaseDto MapearParaAtualizarClienteUseCaseDto(AtualizarClienteRequest request)
        {
            if (request is null)
                return null;

            return new AtualizarClienteUseCaseDto
            {
                Id = request.Id,
                Nome = request.Nome,
                Sexo = request.Sexo,
                Documento = request.Documento,
                DataNascimento = request.DataNascimento,
                TipoCliente = request.TipoCliente,
                EnderecoId = request.EnderecoId,
                Rua = request.Rua,
                Bairro = request.Bairro,
                Cidade = request.Cidade,
                Numero = request.Numero,
                CEP = request.CEP,
                Complemento = request.Complemento,
                ContatoId = request.ContatoId,
                Email = request.Email,
                Telefone = request.Telefone
            };
        }

        public async Task<bool> Remover(Guid id)
        {
            return await _clienteUseCases.RemoverUseCaseAsync(id);
        }
    }
}
