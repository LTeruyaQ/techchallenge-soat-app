using Core.DTOs.Entidades.Autenticacao;
using Core.Especificacoes.Base.Interfaces;
using System.Linq.Expressions;

namespace Core.Especificacoes.Base
{
    public abstract class EspecificacaoBase<T> : IEspecificacao<T> where T : EntityDto
    {
        private int _pagina = 0;
        public int Pagina => _pagina;


        private int _tamanho = 0;
        public int Tamanho => _tamanho;


        private readonly HashSet<string> _inclusoes = new();

        public abstract Expression<Func<T, bool>> Expressao { get; }

        public HashSet<string> Inclusoes => _inclusoes;

        private Expression<Func<T, object>>? _projecao;
        public bool UsarProjecao => _projecao != null;

        public object? ObterProjecao() => _projecao;

        protected void DefinirProjecao(Expression<Func<T, object>> projecao)
        {
            _projecao = projecao ?? throw new ArgumentNullException(nameof(projecao));
        }

        protected void AdicionarInclusao<TProp>(Expression<Func<T, TProp>> navegacao)
        {
            if (navegacao == null) throw new ArgumentNullException(nameof(navegacao));
            _inclusoes.Add(AuxiliarExpressao.ObterCaminho(navegacao));
        }

        protected void AdicionarInclusao<TColecao, TProp>(
            Expression<Func<T, IEnumerable<TColecao>>> colecao,
            Expression<Func<TColecao, TProp>> navegacao)
        {
            if (colecao == null) throw new ArgumentNullException(nameof(colecao));
            if (navegacao == null) throw new ArgumentNullException(nameof(navegacao));

            var caminhoColecao = AuxiliarExpressao.ObterCaminho(colecao);
            var caminhoNavegacao = AuxiliarExpressao.ObterCaminho(navegacao);
            _inclusoes.Add($"{caminhoColecao}.{caminhoNavegacao}");
        }

        protected void AdicionarPaginacao(int pagina, int tamanho)
        {
            _pagina = pagina;
            _tamanho = tamanho;
        }
    }
}