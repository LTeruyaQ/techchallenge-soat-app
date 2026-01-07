using Core.DTOs.Entidades.Servico;
using Core.Entidades;
using Core.Especificacoes.Base.Extensoes;
using Core.Especificacoes.Servico;
using Core.Interfaces.Gateways;
using Core.Interfaces.Repositorios;

namespace Adapters.Gateways
{
    public class ServicoGateway : IServicoGateway
    {
        private readonly IRepositorio<ServicoEntityDto> _repositorioServico;

        public ServicoGateway(IRepositorio<ServicoEntityDto> repositorioServico)
        {
            _repositorioServico = repositorioServico;
        }

        public async Task<Servico> CadastrarAsync(Servico servico)
        {
            var dto = await _repositorioServico.CadastrarAsync(ToDto(servico));
            return FromDto(dto);
        }

        public async Task DeletarAsync(Servico servico)
        {
            await _repositorioServico.DeletarAsync(ToDto(servico));
        }

        public async Task EditarAsync(Servico servico)
        {
            await _repositorioServico.EditarAsync(ToDto(servico));
        }

        public async Task<Servico?> ObterPorIdAsync(Guid id)
        {
            var dto = await _repositorioServico.ObterPorIdSemRastreamentoAsync(id);
            return dto != null ? FromDto(dto) : null;
        }

        public async Task<IEnumerable<Servico>> ObterServicoDisponivelAsync()
        {
            ObterServicoDisponivelEspecificacao filtro = new();
            return await _repositorioServico.ListarProjetadoAsync<Servico>(filtro);
        }

        public async Task<Servico?> ObterServicosDisponiveisPorNomeAsync(string nome)
        {
            var especificacao = new ObterServicoPorNomeEspecificacao(nome)
                .E(new ObterServicoDisponivelEspecificacao());

            return await _repositorioServico.ObterUmProjetadoSemRastreamentoAsync<Servico>(especificacao);
        }

        public async Task<IEnumerable<Servico>> ObterTodosAsync()
        {
            var dtos = await _repositorioServico.ObterTodosAsync();
            return dtos.Select(FromDto);
        }

        private static ServicoEntityDto ToDto(Servico servico)
        {
            return new ServicoEntityDto
            {
                Id = servico.Id,
                Ativo = servico.Ativo,
                DataCadastro = servico.DataCadastro,
                DataAtualizacao = servico.DataAtualizacao,
                Nome = servico.Nome,
                Descricao = servico.Descricao,
                Valor = servico.Valor,
                Disponivel = servico.Disponivel
            };
        }

        private static Servico FromDto(ServicoEntityDto dto)
        {
            return new Servico
            {
                Id = dto.Id,
                Ativo = dto.Ativo,
                DataCadastro = dto.DataCadastro,
                DataAtualizacao = dto.DataAtualizacao,
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                Valor = dto.Valor,
                Disponivel = dto.Disponivel
            };
        }
    }
}
