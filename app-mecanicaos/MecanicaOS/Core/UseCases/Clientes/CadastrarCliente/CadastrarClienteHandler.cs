using Core.DTOs.UseCases.Cliente;
using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.Clientes;
using Core.UseCases.Abstrato;

namespace Core.UseCases.Clientes.CadastrarCliente
{
    public class CadastrarClienteHandler : UseCasesHandlerAbstrato<CadastrarClienteHandler>, ICadastrarClienteHandler
    {
        private readonly IClienteGateway _clienteGateway;
        private readonly IEnderecoGateway _enderecoGateway;
        private readonly IContatoGateway _contatoGateway;

        public CadastrarClienteHandler(
            IClienteGateway clienteGateway,
            IEnderecoGateway enderecoGateway,
            IContatoGateway contatoGateway,
            ILogGateway<CadastrarClienteHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _clienteGateway = clienteGateway ?? throw new ArgumentNullException(nameof(clienteGateway));
            _contatoGateway = contatoGateway ?? throw new ArgumentNullException(nameof(contatoGateway));
            _enderecoGateway = enderecoGateway ?? throw new ArgumentNullException(nameof(enderecoGateway));
        }

        public async Task<Cliente> Handle(CadastrarClienteUseCaseDto request)
        {
            string metodo = nameof(Handle);

            try
            {
                LogInicio(metodo, request);

                // Verificar se o cliente já existe com o documento informado
                var clienteExistente = await _clienteGateway.ObterClientePorDocumentoAsync(request.Documento);
                if (clienteExistente != null)
                {
                    throw new DadosJaCadastradosException("Cliente já cadastrado");
                }

                Cliente cliente = new()
                {
                    Nome = request.Nome,
                    Sexo = request.Sexo ?? (request.TipoCliente == Core.Enumeradores.TipoCliente.PessoaFisica ? "Não informado" : null),
                    Documento = request.Documento,
                    DataNascimento = request.DataNascimento,
                    TipoCliente = request.TipoCliente
                };

                var entityCliente = await _clienteGateway.CadastrarAsync(cliente);

                if (!entityCliente.Id.Equals(Guid.Empty))
                {
                    await CadastrarEnderecoCliente(entityCliente, request);
                    await CadastrarContatoCliente(entityCliente, request);
                }

                try
                {
                    if (!await Commit())
                        throw new PersistirDadosException("Erro ao cadastrar cliente");
                }
                catch (Exception ex)
                {
                    throw new PersistirDadosException("Erro ao cadastrar cliente", ex);
                }

                LogFim(metodo, entityCliente);

                return entityCliente;
            }
            catch (Exception e)
            {
                LogErro(metodo, e);
                throw;
            }
        }

        private async Task CadastrarEnderecoCliente(Cliente cliente, CadastrarClienteUseCaseDto enderecoCliente)
        {
            Endereco endereco = new()
            {
                Bairro = enderecoCliente.Bairro,
                Numero = enderecoCliente.Numero,
                CEP = enderecoCliente.CEP,
                Cidade = enderecoCliente.Cidade,
                Complemento = enderecoCliente.Complemento,
                Rua = enderecoCliente.Rua,
                IdCliente = cliente.Id
            };

            cliente.Endereco = endereco;

            await _enderecoGateway.CadastrarAsync(endereco);
        }

        private async Task CadastrarContatoCliente(Cliente cliente, CadastrarClienteUseCaseDto contatoCliente)
        {
            Contato contato = new()
            {
                IdCliente = cliente.Id,
                Email = contatoCliente.Email,
                Telefone = contatoCliente.Telefone
            };

            cliente.Contato = contato;

            await _contatoGateway.CadastrarAsync(contato);
        }
    }
}
