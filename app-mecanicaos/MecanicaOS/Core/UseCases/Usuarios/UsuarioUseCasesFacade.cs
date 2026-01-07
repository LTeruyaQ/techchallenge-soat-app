using Core.DTOs.UseCases.Usuario;
using Core.Entidades;
using Core.Interfaces.Handlers.Usuarios;
using Core.Interfaces.UseCases;

namespace Core.UseCases.Usuarios
{
    /// <summary>
    /// Facade para manter compatibilidade com a interface IUsuarioUseCases
    /// enquanto utiliza os novos casos de uso individuais
    /// </summary>
    public class UsuarioUseCasesFacade : IUsuarioUseCases
    {
        private readonly ICadastrarUsuarioHandler _cadastrarUsuarioHandler;
        private readonly IAtualizarUsuarioHandler _atualizarUsuarioHandler;
        private readonly IObterUsuarioHandler _obterUsuarioHandler;
        private readonly IObterTodosUsuariosHandler _obterTodosUsuariosHandler;
        private readonly IDeletarUsuarioHandler _deletarUsuarioHandler;
        private readonly IObterUsuarioPorEmailHandler _obterUsuarioPorEmailHandler;
        private readonly IObterUsuariosParaAlertaEstoqueHandler _obterUsuariosParaAlertaEstoqueHandler;

        public UsuarioUseCasesFacade(
            ICadastrarUsuarioHandler cadastrarUsuarioHandler,
            IAtualizarUsuarioHandler atualizarUsuarioHandler,
            IObterUsuarioHandler obterUsuarioHandler,
            IObterTodosUsuariosHandler obterTodosUsuariosHandler,
            IDeletarUsuarioHandler deletarUsuarioHandler,
            IObterUsuarioPorEmailHandler obterUsuarioPorEmailHandler,
            IObterUsuariosParaAlertaEstoqueHandler obterUsuariosParaAlertaEstoqueHandler)
        {
            _cadastrarUsuarioHandler = cadastrarUsuarioHandler ?? throw new ArgumentNullException(nameof(cadastrarUsuarioHandler));
            _atualizarUsuarioHandler = atualizarUsuarioHandler ?? throw new ArgumentNullException(nameof(atualizarUsuarioHandler));
            _obterUsuarioHandler = obterUsuarioHandler ?? throw new ArgumentNullException(nameof(obterUsuarioHandler));
            _obterTodosUsuariosHandler = obterTodosUsuariosHandler ?? throw new ArgumentNullException(nameof(obterTodosUsuariosHandler));
            _deletarUsuarioHandler = deletarUsuarioHandler ?? throw new ArgumentNullException(nameof(deletarUsuarioHandler));
            _obterUsuarioPorEmailHandler = obterUsuarioPorEmailHandler ?? throw new ArgumentNullException(nameof(obterUsuarioPorEmailHandler));
            _obterUsuariosParaAlertaEstoqueHandler = obterUsuariosParaAlertaEstoqueHandler ?? throw new ArgumentNullException(nameof(obterUsuariosParaAlertaEstoqueHandler));
        }

        public async Task<Usuario> AtualizarUseCaseAsync(Guid id, AtualizarUsuarioUseCaseDto request)
        {
            return await _atualizarUsuarioHandler.Handle(id, request);
        }

        public async Task<Usuario> CadastrarUseCaseAsync(CadastrarUsuarioUseCaseDto request)
        {
            return await _cadastrarUsuarioHandler.Handle(request);
        }

        public async Task<bool> DeletarUseCaseAsync(Guid id)
        {
            return await _deletarUsuarioHandler.Handle(id);
        }

        public async Task<Usuario?> ObterPorEmailUseCaseAsync(string email)
        {
            return await _obterUsuarioPorEmailHandler.Handle(email);
        }

        public async Task<Usuario?> ObterPorIdUseCaseAsync(Guid id)
        {
            return await _obterUsuarioHandler.Handle(id);
        }

        public async Task<IEnumerable<Usuario>> ObterTodosUseCaseAsync()
        {
            return await _obterTodosUsuariosHandler.Handle();
        }

        public async Task<IEnumerable<Usuario>> ObterUsuariosParaAlertaEstoqueUseCaseAsync()
        {
            return await _obterUsuariosParaAlertaEstoqueHandler.Handle();
        }
    }
}
