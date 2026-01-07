using Core.DTOs.UseCases.Usuario;
using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.Usuarios;
using Core.Interfaces.Servicos;
using Core.UseCases.Abstrato;

namespace Core.UseCases.Usuarios.AtualizarUsuario
{
    public class AtualizarUsuarioHandler : UseCasesHandlerAbstrato<AtualizarUsuarioHandler>, IAtualizarUsuarioHandler
    {
        private readonly IUsuarioGateway _usuarioGateway;
        private readonly IServicoSenha _servicoSenha;

        public AtualizarUsuarioHandler(
            IUsuarioGateway usuarioGateway,
            IServicoSenha servicoSenha,
            ILogGateway<AtualizarUsuarioHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _usuarioGateway = usuarioGateway ?? throw new ArgumentNullException(nameof(usuarioGateway));
            _servicoSenha = servicoSenha ?? throw new ArgumentNullException(nameof(servicoSenha));
        }

        public async Task<Usuario> Handle(Guid id, AtualizarUsuarioUseCaseDto request)
        {
            var metodo = nameof(Handle);

            try
            {
                LogInicio(metodo, new { id, request });

                Usuario usuario = await _usuarioGateway.ObterPorIdAsync(id) ?? throw new DadosNaoEncontradosException("Usuário não encontrado");

                string senhaCriptografada = !string.IsNullOrEmpty(request.Senha)
                    ? _servicoSenha.CriptografarSenha(request.Senha)
                    : usuario.Senha;

                usuario.Atualizar(
                    request.Email,
                    senhaCriptografada,
                    request.DataUltimoAcesso,
                    request.TipoUsuario,
                    request.RecebeAlertaEstoque);

                await _usuarioGateway.EditarAsync(usuario);

                if (!await Commit())
                    throw new PersistirDadosException("Erro ao atualizar usuario");

                IsNotGetSenha(usuario);

                LogFim(metodo, usuario);

                return usuario;
            }
            catch (Exception e)
            {
                LogErro(metodo, e);
                throw;
            }
        }

        private static void IsNotGetSenha(Usuario usuario)
        {
            usuario.Senha = string.Empty;
        }
    }
}
