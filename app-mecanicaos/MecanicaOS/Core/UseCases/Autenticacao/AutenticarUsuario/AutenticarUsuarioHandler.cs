using Core.DTOs.UseCases.Autenticacao;
using Core.Entidades;
using Core.Enumeradores;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.Autenticacao;
using Core.UseCases.Abstrato;

namespace Core.UseCases.Autenticacao.AutenticarUsuario
{
    public class AutenticarUsuarioHandler : UseCasesHandlerAbstrato<AutenticarUsuarioHandler>, IAutenticarUsuarioHandler
    {
        private readonly ISegurancaGateway _segurancaGateway;

        public AutenticarUsuarioHandler(
            ISegurancaGateway segurancaGateway,
            ILogGateway<AutenticarUsuarioHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _segurancaGateway = segurancaGateway ?? throw new ArgumentNullException(nameof(segurancaGateway));
        }

        public async Task<AutenticacaoDto> Handle(AutenticacaoUseCaseDto request)
        {
            string metodo = nameof(Handle);

            try
            {
                LogInicio(metodo, request);

                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Senha))
                    throw new DadosInvalidosException("Email e senha são obrigatórios");

                if (request.UsuarioExistente == null)
                    throw new DadosInvalidosException("Usuário ou senha inválidos");

                if (!_segurancaGateway.VerificarSenha(request.Senha, request.UsuarioExistente.Senha))
                    throw new DadosInvalidosException("Usuário ou senha inválidos");

                var permissoes = ObterPermissoesDoUsuario(request.UsuarioExistente);
                var token = _segurancaGateway.GerarToken(request.UsuarioExistente.Id, request.UsuarioExistente.Email, request.UsuarioExistente.TipoUsuario.ToString(), null, permissoes);

                var autenticacaoDto = new AutenticacaoDto
                {
                    Token = token,
                    Usuario = request.UsuarioExistente,
                    Permissoes = permissoes.ToList()
                };

                LogFim(metodo, autenticacaoDto);
                return autenticacaoDto;
            }
            catch (Exception ex)
            {
                LogErro(metodo, ex);
                throw;
            }
        }

        private IEnumerable<string> ObterPermissoesDoUsuario(Usuario usuario)
        {
            var permissoes = new List<string>();

            switch (usuario.TipoUsuario)
            {
                case TipoUsuario.Admin:
                    permissoes.Add("administrador");
                    break;

                case TipoUsuario.Cliente:
                    permissoes.Add("cliente");
                    break;
            }

            return permissoes;
        }
    }
}
