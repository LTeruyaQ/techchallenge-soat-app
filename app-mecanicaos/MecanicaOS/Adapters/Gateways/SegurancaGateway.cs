using Core.Interfaces.Gateways;
using Core.Interfaces.Servicos;

namespace Adapters.Gateways
{
    public class SegurancaGateway : ISegurancaGateway
    {
        private readonly IServicoSenha _servicoSenha;
        private readonly IServicoJwt _servicoJwt;

        public SegurancaGateway(IServicoSenha servicoSenha, IServicoJwt servicoJwt)
        {
            _servicoSenha = servicoSenha ?? throw new ArgumentNullException(nameof(servicoSenha));
            _servicoJwt = servicoJwt ?? throw new ArgumentNullException(nameof(servicoJwt));
        }

        public bool VerificarSenha(string senhaPlano, string hashArmazenado)
        {
            return _servicoSenha.VerificarSenha(senhaPlano, hashArmazenado);
        }

        public string GerarToken(Guid usuarioId, string email, string tipoUsuario, Guid? clienteId, IEnumerable<string> permissoes)
        {
            return _servicoJwt.GerarToken(usuarioId, email, tipoUsuario, null, permissoes);
        }
    }
}
