using Core.DTOs.Entidades.Autenticacao;
using Core.Especificacoes.Base.Interfaces;

namespace Core.Interfaces.Repositorios;

public interface IRepositorio<T> where T : EntityDto
{
    Task<T?> ObterPorIdAsync(Guid id);
    Task<T?> ObterPorIdSemRastreamentoAsync(Guid id);
    Task<T> CadastrarAsync(T entidade);
    Task<IEnumerable<T>> CadastrarVariosAsync(IEnumerable<T> entidades);
    Task EditarAsync(T novaEntidade);
    Task EditarVariosAsync(IEnumerable<T> entidades);
    Task DeletarAsync(T entidade);
    Task DeletarVariosAsync(IEnumerable<T> entidades);
    Task DeletarLogicamenteAsync(T entidade);

    Task<IEnumerable<T>> ObterTodosAsync();

    Task<TProjecao?> ObterUmProjetadoAsync<TProjecao>(IEspecificacao<T> especificacao);
    Task<TProjecao?> ObterUmProjetadoSemRastreamentoAsync<TProjecao>(IEspecificacao<T> especificacao);
    Task<IEnumerable<TProjecao>> ListarProjetadoAsync<TProjecao>(IEspecificacao<T> filtro);
    Task<IEnumerable<TProjecao>> ListarProjetadoSemRastreamentoAsync<TProjecao>(IEspecificacao<T> especificacao);

    Task<T?> ObterUmAsync(IEspecificacao<T> especificacao);
    Task<T?> ObterUmSemRastreamentoAsync(IEspecificacao<T> especificacao);
    Task<IEnumerable<T>> ListarAsync(IEspecificacao<T> filtro);
    Task<IEnumerable<T>> ListarSemRastreamentoAsync(IEspecificacao<T> especificacao);
}