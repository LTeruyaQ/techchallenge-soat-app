using Core.Especificacoes.Base.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infraestrutura.Dados.Especificacoes
{
    public class AvaliadorDeEspecificacao<T> where T : class
    {
        public IQueryable<T> ObterConsulta(
            IQueryable<T> consultaInicial,
            IEspecificacao<T> especificacao)
        {
            var consulta = consultaInicial;

            if (especificacao.Expressao != null)
            {
                consulta = consulta.Where(especificacao.Expressao);
            }

            if (especificacao.Inclusoes != null && especificacao.Inclusoes.Any())
            {
                foreach (var include in especificacao.Inclusoes)
                {
                    consulta = consulta.Include(include);
                }
            }

            return consulta;
        }

        public IQueryable<TProjecao> AplicarProjecao<TProjecao>(
            IQueryable<T> consulta,
            IEspecificacao<T> especificacao)
        {
            if (!especificacao.UsarProjecao)
            {
                throw new InvalidOperationException("A especificação não contém uma projeção definida.");
            }

            var projecao = especificacao.ObterProjecao() as Expression<Func<T, TProjecao>>;
            if (projecao == null)
            {
                var projecaoObj = especificacao.ObterProjecao() as Expression<Func<T, object>>;
                if (projecaoObj == null)
                {
                    throw new InvalidOperationException("Tipo de projeção incompatível.");
                }

                var parametro = Expression.Parameter(typeof(T), "x");
                var corpo = Expression.Convert(
                    Expression.Invoke(projecaoObj, parametro),
                    typeof(TProjecao)
                );
                projecao = Expression.Lambda<Func<T, TProjecao>>(corpo, parametro);
            }

            return consulta.Select(projecao);
        }

        public IQueryable<T> AplicarPaginacao(IQueryable<T> consulta,
            IEspecificacao<T> especificacao)
        {
            if (especificacao.Tamanho > 0)
            {
                return consulta.Skip(especificacao.Tamanho * especificacao.Pagina).Take(especificacao.Tamanho);
            }

            return consulta;
        }
    }
}
