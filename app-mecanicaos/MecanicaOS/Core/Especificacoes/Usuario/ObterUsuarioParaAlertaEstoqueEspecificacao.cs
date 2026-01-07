using Core.DTOs.Entidades.Usuarios;
using Core.Especificacoes.Base;
using System.Linq.Expressions;

namespace Core.Especificacoes.Usuario;

public class ObterUsuarioParaAlertaEstoqueEspecificacao : EspecificacaoBase<UsuarioEntityDto>
{

    public ObterUsuarioParaAlertaEstoqueEspecificacao()
    {
        DefinirProjecao(u => new Entidades.Usuario
        {
            Id = u.Id,
            DataCadastro = u.DataCadastro,
            DataAtualizacao = u.DataAtualizacao,
            Ativo = u.Ativo,
            Email = u.Email,
            Senha = u.Senha,
            TipoUsuario = u.TipoUsuario,
            DataUltimoAcesso = u.DataUltimoAcesso,
            RecebeAlertaEstoque = u.RecebeAlertaEstoque,
            ClienteId = u.ClienteId,
        });
    }

    public override Expression<Func<UsuarioEntityDto, bool>> Expressao =>
     u => u.RecebeAlertaEstoque;
}
