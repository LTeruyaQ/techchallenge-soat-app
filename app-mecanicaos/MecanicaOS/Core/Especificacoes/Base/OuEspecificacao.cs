using Core.DTOs.Entidades.Autenticacao;
using Core.Especificacoes.Base.Interfaces;
using System.Linq.Expressions;

namespace Core.Especificacoes.Base
{
    public class OuEspecificacao<T> : EspecificacaoBase<T> where T : EntityDto
    {
        public IEspecificacao<T> Esquerda { get; }
        public IEspecificacao<T> Direita { get; }

        public OuEspecificacao(IEspecificacao<T> esquerda, IEspecificacao<T> direita)
        {
            Esquerda = esquerda ?? throw new ArgumentNullException(nameof(esquerda));
            Direita = direita ?? throw new ArgumentNullException(nameof(direita));

            if (esquerda.Inclusoes != null)
                AplicarInclusoes(esquerda);

            if (direita.Inclusoes != null)
                AplicarInclusoes(direita);
        }

        private void AplicarInclusoes(IEspecificacao<T> esquerda)
        {
            foreach (var inclusao in esquerda.Inclusoes)
            {
                Inclusoes.Add(inclusao);
            }
        }

        public override Expression<Func<T, bool>> Expressao
        {
            get
            {
                var param = Expression.Parameter(typeof(T), "x");

                var esquerdaBody = Expression.Invoke(Esquerda.Expressao, param);
                var direitaBody = Expression.Invoke(Direita.Expressao, param);

                var body = Expression.OrElse(esquerdaBody, direitaBody);

                return Expression.Lambda<Func<T, bool>>(body, param);
            }
        }
    }
}