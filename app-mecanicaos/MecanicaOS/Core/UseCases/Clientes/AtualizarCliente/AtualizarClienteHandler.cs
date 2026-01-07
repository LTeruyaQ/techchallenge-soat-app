using Core.DTOs.UseCases.Cliente;
using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.Clientes;
using Core.UseCases.Abstrato;

namespace Core.UseCases.Clientes.AtualizarCliente
{
    public class AtualizarClienteHandler : UseCasesHandlerAbstrato<AtualizarClienteHandler>, IAtualizarClienteHandler
    {
        private readonly IClienteGateway _clienteGateway;
        private readonly IEnderecoGateway _enderecoGateway;
        private readonly IContatoGateway _contatoGateway;

        public AtualizarClienteHandler(
            IClienteGateway clienteGateway,
            IEnderecoGateway enderecoGateway,
            IContatoGateway contatoGateway,
            ILogGateway<AtualizarClienteHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _clienteGateway = clienteGateway ?? throw new ArgumentNullException(nameof(clienteGateway));
            _contatoGateway = contatoGateway ?? throw new ArgumentNullException(nameof(contatoGateway));
            _enderecoGateway = enderecoGateway ?? throw new ArgumentNullException(nameof(enderecoGateway));
        }

        public async Task<Cliente> Handle(Guid id, AtualizarClienteUseCaseDto request)
        {
            string metodo = nameof(Handle);

            try
            {
                LogInicio(metodo, new { id, request });

                var cliente = await _clienteGateway.ObterPorIdAsync(id)
                    ?? throw new DadosNaoEncontradosException("cliente não encontrado");

                cliente.Atualizar(request.Nome, request.Sexo, request.TipoCliente, request.DataNascimento);

                await _clienteGateway.EditarAsync(cliente);

                if (!request.EnderecoId.Equals(Guid.Empty))
                    await AtualizarEnderecoCliente(request);

                if (!request.ContatoId.Equals(Guid.Empty))
                    await AtualizarContatoCliente(request);

                if (!await Commit())
                    throw new PersistirDadosException("Erro ao atualizar cliente");

                LogFim(metodo, cliente);

                return cliente;
            }
            catch (Exception e)
            {
                LogErro(metodo, e);
                throw;
            }
        }

        private async Task AtualizarEnderecoCliente(AtualizarClienteUseCaseDto enderecoCliente)
        {
            if (enderecoCliente.EnderecoId.Equals(Guid.Empty))
                throw new DadosInvalidosException("Deve ser informado o id do endereço a ser editado");

            if (await _enderecoGateway.ObterPorIdAsync(enderecoCliente.EnderecoId) is Endereco endereco)
            {
                endereco.Bairro = enderecoCliente.Bairro;
                endereco.Numero = enderecoCliente.Numero;
                endereco.CEP = enderecoCliente.CEP;
                endereco.Cidade = enderecoCliente.Cidade;
                endereco.Complemento = enderecoCliente.Complemento;
                endereco.Rua = enderecoCliente.Rua;
                endereco.MarcarComoAtualizada();

                await _enderecoGateway.EditarAsync(endereco);
            }
            else
            {
                throw new DadosNaoEncontradosException("Endereço inexistente");
            }
        }

        private async Task AtualizarContatoCliente(AtualizarClienteUseCaseDto contatoCliente)
        {
            if (contatoCliente.ContatoId.Equals(Guid.Empty))
                throw new Exception("Contato inexistente");

            if (await _contatoGateway.ObterPorIdAsync(contatoCliente.ContatoId) is Contato contato)
            {
                contato.Telefone = contatoCliente.Telefone ?? string.Empty;
                contato.IdCliente = contatoCliente.Id ?? Guid.Empty;
                contato.Email = contatoCliente.Email ?? string.Empty;
                contato.MarcarComoAtualizada();

                await _contatoGateway.EditarAsync(contato);
            }
        }
    }
}
