using Core.DTOs.Entidades.Estoque;
using Core.Entidades;
using Core.Especificacoes.Estoque;
using Core.Interfaces.Gateways;
using Core.Interfaces.Repositorios;

namespace Adapters.Gateways
{
    public class AlertaEstoqueGateway : IAlertaEstoqueGateway
    {
        private readonly IRepositorio<AlertaEstoqueEntityDto> _repositorioAlertaEstoque;

        public AlertaEstoqueGateway(IRepositorio<AlertaEstoqueEntityDto> repositorioAlertaEstoque)
        {
            _repositorioAlertaEstoque = repositorioAlertaEstoque;
        }

        public async Task CadastrarVariosAsync(IEnumerable<AlertaEstoque> alertas)
        {
            await _repositorioAlertaEstoque.CadastrarVariosAsync(alertas.Select(ToDto));
        }

        public static AlertaEstoqueEntityDto ToDto(AlertaEstoque alerta)
        {
            return new AlertaEstoqueEntityDto
            {
                Id = alerta.Id,
                Ativo = alerta.Ativo,
                DataCadastro = alerta.DataCadastro,
                DataAtualizacao = alerta.DataAtualizacao,
                EstoqueId = alerta.EstoqueId,
                Estoque = alerta.Estoque == null ? null : new EstoqueEntityDto
                {
                    Id = alerta.Estoque.Id,
                    Ativo = alerta.Estoque.Ativo,
                    DataCadastro = alerta.Estoque.DataCadastro,
                    DataAtualizacao = alerta.Estoque.DataAtualizacao,
                    QuantidadeDisponivel = alerta.Estoque.QuantidadeDisponivel,
                    QuantidadeMinima = alerta.Estoque.QuantidadeMinima,
                    Descricao = alerta.Estoque.Descricao,
                    Insumo = alerta.Estoque.Insumo,
                    Preco = alerta.Estoque.Preco
                }
            };
        }

        public async Task<IEnumerable<AlertaEstoque>> ObterAlertaDoDiaPorEstoqueAsync(Guid insumoId, DateTime dataAtual)
        {
            return await _repositorioAlertaEstoque.ListarProjetadoAsync<AlertaEstoque>(
                new ObterAlertaDoDiaPorEstoqueEspecificacao(
                    insumoId,
                    dataAtual));
        }
    }
}
