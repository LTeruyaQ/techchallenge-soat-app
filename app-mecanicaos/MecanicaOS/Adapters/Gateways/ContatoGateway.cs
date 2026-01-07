using Core.DTOs.Entidades.Cliente;
using Core.Entidades;
using Core.Interfaces.Gateways;
using Core.Interfaces.Repositorios;

namespace Adapters.Gateways
{
    public class ContatoGateway : IContatoGateway
    {
        private readonly IRepositorio<ContatoEntityDto> _repositorioContato;

        public ContatoGateway(IRepositorio<ContatoEntityDto> repositorioContato)
        {
            _repositorioContato = repositorioContato;
        }

        public async Task CadastrarAsync(Contato contato)
        {
            await _repositorioContato.CadastrarAsync(ToDto(contato));
        }

        public async Task EditarAsync(Contato contato)
        {
            await _repositorioContato.EditarAsync(ToDto(contato));
        }

        public async Task<Contato?> ObterPorIdAsync(Guid contatoId)
        {
            var contatoDto = await _repositorioContato.ObterPorIdAsync(contatoId);
            return contatoDto != null ? FromDto(contatoDto) : null;
        }

        public static ContatoEntityDto ToDto(Contato contato)
        {
            return new ContatoEntityDto
            {
                Id = contato.Id,
                Ativo = contato.Ativo,
                DataCadastro = contato.DataCadastro,
                DataAtualizacao = contato.DataAtualizacao,
                Email = contato.Email,
                Telefone = contato.Telefone,
                IdCliente = contato.IdCliente
            };
        }

        public static Contato FromDto(ContatoEntityDto dto)
        {
            return new Contato
            {
                Id = dto.Id,
                Ativo = dto.Ativo,
                DataCadastro = dto.DataCadastro,
                DataAtualizacao = dto.DataAtualizacao,
                Email = dto.Email,
                Telefone = dto.Telefone,
                IdCliente = dto.IdCliente
            };
        }
    }
}
