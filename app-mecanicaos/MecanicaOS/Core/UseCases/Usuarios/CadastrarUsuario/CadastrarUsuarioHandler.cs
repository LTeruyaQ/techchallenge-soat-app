using Core.DTOs.UseCases.Usuario;
using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.Usuarios;
using Core.Interfaces.Servicos;
using Core.UseCases.Abstrato;

namespace Core.UseCases.Usuarios.CadastrarUsuario
{
    public class CadastrarUsuarioHandler : UseCasesHandlerAbstrato<CadastrarUsuarioHandler>, ICadastrarUsuarioHandler
    {
        private readonly IUsuarioGateway _usuarioGateway;
        private readonly IServicoSenha _servicoSenha;

        public CadastrarUsuarioHandler(
            IUsuarioGateway usuarioGateway,
            ILogGateway<CadastrarUsuarioHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway,
            IServicoSenha servicoSenha)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _usuarioGateway = usuarioGateway ?? throw new ArgumentNullException(nameof(usuarioGateway));
            _servicoSenha = servicoSenha ?? throw new ArgumentNullException(nameof(servicoSenha));
        }

        public async Task<Usuario> Handle(CadastrarUsuarioUseCaseDto request)
        {
            var metodo = nameof(Handle);

            try
            {
                LogInicio(metodo, request);

                await VerificarUsuarioCadastradoAsync(request.Email);

                Usuario usuario = new()
                {
                    Email = request.Email,
                    TipoUsuario = request.TipoUsuario,
                    RecebeAlertaEstoque = request.RecebeAlertaEstoque.HasValue ? request.RecebeAlertaEstoque.Value : false,
                    ClienteId = request.ClienteId // Controller já resolveu o ClienteId
                };

                usuario.Senha = _servicoSenha.CriptografarSenha(request.Senha);

                var entidade = await _usuarioGateway.CadastrarAsync(usuario);

                if (!await Commit())
                    throw new PersistirDadosException("Erro ao cadastrar usuário");

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


        private async Task VerificarUsuarioCadastradoAsync(string email)
        {
            var metodo = nameof(VerificarUsuarioCadastradoAsync);

            try
            {
                LogInicio(metodo);

                var usuario = await _usuarioGateway.ObterPorEmailAsync(email);

                LogFim(metodo);

                if (usuario is not null)
                    throw new DadosJaCadastradosException("Usuário já cadastrado");
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
