using Core.DTOs.Entidades.Estoque;
using Core.Entidades;
using Core.Especificacoes.Estoque;
using Core.Interfaces.Gateways;
using Core.Interfaces.Repositorios;

namespace Adapters.Gateways
{
    public class EstoqueGateway : IEstoqueGateway
    {
        private readonly IRepositorio<EstoqueEntityDto> _repositorioEstoque;

        public EstoqueGateway(IRepositorio<EstoqueEntityDto> repositorioEstoque)
        {
            _repositorioEstoque = repositorioEstoque;
        }

        public async Task CadastrarAsync(Estoque estoque)
        {
            await _repositorioEstoque.CadastrarAsync(ToDto(estoque));
        }

        public async Task DeletarAsync(Estoque estoque)
        {
            await _repositorioEstoque.DeletarAsync(ToDto(estoque));
        }

        public async Task EditarAsync(Estoque estoque)
        {
            await _repositorioEstoque.EditarAsync(ToDto(estoque));
        }

        public async Task<IEnumerable<Estoque>> ObterEstoqueCriticoAsync()
        {
            var especificacao = new ObterEstoqueCriticoEspecificacao();
            return await _repositorioEstoque.ListarProjetadoAsync<Estoque>(especificacao);
        }

        public async Task<Estoque?> ObterPorIdAsync(Guid id)
        {
            var estoqueDto = await _repositorioEstoque.ObterPorIdSemRastreamentoAsync(id);
            return estoqueDto != null ? FromDto(estoqueDto) : null;
        }

        public async Task<IEnumerable<Estoque>> ObterTodosAsync()
        {
            var estoquesDto = await _repositorioEstoque.ObterTodosAsync();
            return estoquesDto.Select(FromDto);
        }

        public static EstoqueEntityDto ToDto(Estoque estoque)
        {
            return new EstoqueEntityDto
            {
                Id = estoque.Id,
                Ativo = estoque.Ativo,
                DataCadastro = estoque.DataCadastro,
                DataAtualizacao = estoque.DataAtualizacao,
                QuantidadeMinima = estoque.QuantidadeMinima,
                QuantidadeDisponivel = estoque.QuantidadeDisponivel,
                Descricao = estoque.Descricao,
                Insumo = estoque.Insumo,
                Preco = estoque.Preco
            };
        }

        public static Estoque FromDto(EstoqueEntityDto dto)
        {
            return new Estoque
            {
                Id = dto.Id,
                Ativo = dto.Ativo,
                DataCadastro = dto.DataCadastro,
                DataAtualizacao = dto.DataAtualizacao,
                QuantidadeMinima = dto.QuantidadeMinima,
                QuantidadeDisponivel = dto.QuantidadeDisponivel,
                Descricao = dto.Descricao,
                Insumo = dto.Insumo,
                Preco = dto.Preco
            };
        }
    }
}
