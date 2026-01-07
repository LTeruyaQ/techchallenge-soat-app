using Core.DTOs.Entidades.Cliente;
using Core.Entidades;
using Core.Especificacoes.Cliente;
using Core.Interfaces.Gateways;
using Core.Interfaces.Repositorios;

namespace Adapters.Gateways
{
    public class ClienteGateway : IClienteGateway
    {
        private readonly IRepositorio<ClienteEntityDto> _repositorioCliente;

        public ClienteGateway(IRepositorio<ClienteEntityDto> repositorioCliente)
        {
            _repositorioCliente = repositorioCliente;
        }

        public async Task<Cliente> CadastrarAsync(Cliente cliente)
        {
            var response = await _repositorioCliente.CadastrarAsync(ToDto(cliente));
            return FromDto(response);
        }

        public async Task DeletarAsync(Cliente cliente)
        {
            await _repositorioCliente.DeletarAsync(ToDto(cliente));
        }

        public async Task EditarAsync(Cliente cliente)
        {
            await _repositorioCliente.EditarAsync(ToDto(cliente));
        }

        public async Task<Cliente?> ObterClienteComVeiculoPorIdAsync(Guid clienteId)
        {
            var especificacao = new ObterClienteComVeiculoPorIdEspecificacao(clienteId);
            return await _repositorioCliente.ObterUmProjetadoSemRastreamentoAsync<Cliente>(especificacao);
        }

        public async Task<Cliente?> ObterClientePorDocumentoAsync(string documento)
        {
            var especificacao = new ObterClientePorDocumento(documento);
            return await _repositorioCliente.ObterUmProjetadoSemRastreamentoAsync<Cliente>(especificacao);
        }

        public async Task<IEnumerable<Cliente>> ObterClientePorNomeAsync(string nome)
        {
            var especificacao = new ObterClientePorNomeEspecificacao(nome);
            return await _repositorioCliente.ListarProjetadoAsync<Cliente>(especificacao);
        }

        public async Task<Cliente?> ObterPorIdAsync(Guid id)
        {
            var especificacao = new ObterClienteCompletoPorIdEspecificacao(id);
            var cliente = await _repositorioCliente.ObterUmProjetadoSemRastreamentoAsync<Cliente>(especificacao);
            return cliente;
        }

        public async Task<IEnumerable<Cliente>> ObterTodosClientesAsync()
        {
            var especificacao = new ObterTodosClienteCompletoEspecificacao();
            return await _repositorioCliente.ListarProjetadoAsync<Cliente>(especificacao);
        }

        public static ClienteEntityDto ToDto(Cliente cliente)
        {
            return new ClienteEntityDto
            {
                Id = cliente.Id,
                Ativo = cliente.Ativo,
                DataCadastro = cliente.DataCadastro,
                DataAtualizacao = cliente.DataAtualizacao,
                Nome = cliente.Nome,
                Documento = cliente.Documento,
                Sexo = cliente.Sexo,
                DataNascimento = cliente.DataNascimento,
                TipoCliente = cliente.TipoCliente,
                Contato = cliente.Contato != null ? new ContatoEntityDto
                {
                    Id = cliente.Contato.Id,
                    Ativo = cliente.Contato.Ativo,
                    DataCadastro = cliente.Contato.DataCadastro,
                    DataAtualizacao = cliente.Contato.DataAtualizacao,
                    Email = cliente.Contato.Email,
                    Telefone = cliente.Contato.Telefone,
                    IdCliente = cliente.Id
                } : null,
                Endereco = cliente.Endereco != null ? new EnderecoEntityDto
                {
                    Id = cliente.Endereco.Id,
                    Ativo = cliente.Endereco.Ativo,
                    DataCadastro = cliente.Endereco.DataCadastro,
                    DataAtualizacao = cliente.Endereco.DataAtualizacao,
                    Rua = cliente.Endereco.Rua,
                    Numero = cliente.Endereco.Numero,
                    Complemento = cliente.Endereco.Complemento,
                    Bairro = cliente.Endereco.Bairro,
                    Cidade = cliente.Endereco.Cidade,
                    CEP = cliente.Endereco.CEP,
                    IdCliente = cliente.Id
                } : null
            };
        }

        public static Cliente? FromDto(ClienteEntityDto? dto)
        {
            if (dto == null) return null;
            
            return new Cliente
            {
                Id = dto.Id,
                Ativo = dto.Ativo,
                DataCadastro = dto.DataCadastro,
                DataAtualizacao = dto.DataAtualizacao,
                Nome = dto.Nome,
                Documento = dto.Documento,
                Sexo = dto.Sexo,
                DataNascimento = dto.DataNascimento,
                TipoCliente = dto.TipoCliente,
                Contato = dto.Contato != null ? new Contato
                {
                    Id = dto.Contato.Id,
                    Ativo = dto.Contato.Ativo,
                    DataCadastro = dto.Contato.DataCadastro,
                    DataAtualizacao = dto.Contato.DataAtualizacao,
                    Email = dto.Contato.Email,
                    Telefone = dto.Contato.Telefone,
                    IdCliente = dto.Id
                } : null,
                Endereco = dto.Endereco != null ? new Endereco
                {
                    Id = dto.Endereco.Id,
                    Ativo = dto.Endereco.Ativo,
                    DataCadastro = dto.Endereco.DataCadastro,
                    DataAtualizacao = dto.Endereco.DataAtualizacao,
                    Rua = dto.Endereco.Rua,
                    Numero = dto.Endereco.Numero,
                    Complemento = dto.Endereco.Complemento,
                    Bairro = dto.Endereco.Bairro,
                    Cidade = dto.Endereco.Cidade,
                    CEP = dto.Endereco.CEP,
                    IdCliente = dto.Id
                } : null,
                Veiculos = dto.Veiculos?.Select(v => new Veiculo
                {
                    Id = v.Id,
                    Ativo = v.Ativo,
                    DataCadastro = v.DataCadastro,
                    DataAtualizacao = v.DataAtualizacao,
                    Marca = v.Marca,
                    Modelo = v.Modelo,
                    Ano = v.Ano,
                    Placa = v.Placa,
                    ClienteId = v.ClienteId
                }) ?? Enumerable.Empty<Veiculo>()
            };
        }
    }
}
