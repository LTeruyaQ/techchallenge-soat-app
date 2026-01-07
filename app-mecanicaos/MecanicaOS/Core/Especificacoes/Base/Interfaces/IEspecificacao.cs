using System.Linq.Expressions;

namespace Core.Especificacoes.Base.Interfaces;

public interface IEspecificacao<T>
{
    Expression<Func<T, bool>> Expressao { get; }

    HashSet<string> Inclusoes { get; }
    bool UsarProjecao { get; }
    int Pagina { get; }
    int Tamanho { get; }
    object? ObterProjecao();
}