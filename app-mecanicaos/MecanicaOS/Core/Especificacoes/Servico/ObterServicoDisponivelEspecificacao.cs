using Core.DTOs.Entidades.Servico;
using Core.Especificacoes.Base;
using System.Linq.Expressions;

namespace Core.Especificacoes.Servico
{
    public class ObterServicoDisponivelEspecificacao : EspecificacaoBase<ServicoEntityDto>
    {
        public ObterServicoDisponivelEspecificacao()
        {
            DefinirProjecao(s => new Entidades.Servico()
            {
                Id = s.Id,
                Ativo = s.Ativo,
                DataCadastro = s.DataCadastro,
                DataAtualizacao = s.DataAtualizacao,
                Nome = s.Nome,
                Descricao = s.Descricao,
                Disponivel = s.Disponivel,
                Valor = s.Valor
            });
        }

        public override Expression<Func<ServicoEntityDto, bool>> Expressao => s => s.Disponivel;
    }
}
