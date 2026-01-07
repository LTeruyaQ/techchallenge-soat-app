using Core.DTOs.Entidades.Cliente;
using Core.Entidades;
using Core.Interfaces.Gateways;
using Core.Interfaces.Repositorios;

namespace Adapters.Gateways
{
    public class EnderecoGateway : IEnderecoGateway
    {
        private readonly IRepositorio<EnderecoEntityDto> _repositorioEndereco;

        public EnderecoGateway(IRepositorio<EnderecoEntityDto> repositorioEndereco)
        {
            _repositorioEndereco = repositorioEndereco;
        }

        public async Task CadastrarAsync(Endereco endereco)
        {
            await _repositorioEndereco.CadastrarAsync(ToDto(endereco));
        }

        public async Task EditarAsync(Endereco endereco)
        {
            await _repositorioEndereco.EditarAsync(ToDto(endereco));
        }

        public async Task<Endereco?> ObterPorIdAsync(Guid enderecoId)
        {
            var enderecoDto = await _repositorioEndereco.ObterPorIdAsync(enderecoId);
            return enderecoDto != null ? FromDto(enderecoDto) : null;
        }

        public static EnderecoEntityDto ToDto(Endereco endereco)
        {
            return new EnderecoEntityDto
            {
                Id = endereco.Id,
                Ativo = endereco.Ativo,
                DataCadastro = endereco.DataCadastro,
                DataAtualizacao = endereco.DataAtualizacao,
                Bairro = endereco.Bairro,
                CEP = endereco.CEP,
                Cidade = endereco.Cidade,
                Complemento = endereco.Complemento,
                Numero = endereco.Numero,
                Rua = endereco.Rua,
                IdCliente = endereco.IdCliente
            };
        }

        public static Endereco FromDto(EnderecoEntityDto dto)
        {
            return new Endereco
            {
                Id = dto.Id,
                Ativo = dto.Ativo,
                DataCadastro = dto.DataCadastro,
                DataAtualizacao = dto.DataAtualizacao,
                Bairro = dto.Bairro,
                CEP = dto.CEP,
                Cidade = dto.Cidade,
                Complemento = dto.Complemento,
                Numero = dto.Numero,
                Rua = dto.Rua,
                IdCliente = dto.IdCliente
            };
        }
    }
}
