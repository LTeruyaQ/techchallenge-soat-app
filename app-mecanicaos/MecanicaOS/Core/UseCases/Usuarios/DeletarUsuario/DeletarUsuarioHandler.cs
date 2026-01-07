using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.Usuarios;
using Core.UseCases.Abstrato;

namespace Core.UseCases.Usuarios.DeletarUsuario
{
    public class DeletarUsuarioHandler : UseCasesHandlerAbstrato<DeletarUsuarioHandler>, IDeletarUsuarioHandler
    {
        private readonly IUsuarioGateway _usuarioGateway;

        public DeletarUsuarioHandler(
            IUsuarioGateway usuarioGateway,
            ILogGateway<DeletarUsuarioHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _usuarioGateway = usuarioGateway ?? throw new ArgumentNullException(nameof(usuarioGateway));
        }

        public async Task<bool> Handle(Guid id)
        {
            var metodo = nameof(Handle);

            try
            {
                LogInicio(metodo, id);

                var usuario = await _usuarioGateway.ObterPorIdAsync(id)
                    ?? throw new DadosNaoEncontradosException("Usuário não encontrado");

                await _usuarioGateway.DeletarAsync(usuario);
                var sucesso = await Commit();

                if (!sucesso)
                    throw new PersistirDadosException("Erro ao deletar usuário");

                LogFim(metodo, sucesso);

                return sucesso;
            }
            catch (Exception e)
            {
                LogErro(metodo, e);
                throw;
            }
        }
    }
}
