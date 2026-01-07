using Core.DTOs.Entidades.Usuarios;
using Core.Entidades;
using Core.Especificacoes.Usuario;
using Core.Interfaces.Gateways;
using Core.Interfaces.Repositorios;

namespace Adapters.Gateways
{
    public class UsuarioGateway : IUsuarioGateway
    {
        private readonly IRepositorio<UsuarioEntityDto> _repositorioUsuario;

        public UsuarioGateway(IRepositorio<UsuarioEntityDto> repositorioUsuario)
        {
            _repositorioUsuario = repositorioUsuario;
        }

        public async Task<Usuario> CadastrarAsync(Usuario usuario)
        {
            var dto = await _repositorioUsuario.CadastrarAsync(ToDto(usuario));
            return FromDto(dto);
        }

        public async Task DeletarAsync(Usuario usuario)
        {
            await _repositorioUsuario.DeletarAsync(ToDto(usuario));
        }

        public async Task EditarAsync(Usuario usuario)
        {
            await _repositorioUsuario.EditarAsync(ToDto(usuario));
        }

        public async Task<Usuario?> ObterPorEmailAsync(string email)
        {
            var especificacao = new ObterUsuarioPorEmailEspecificacao(email);
            return await _repositorioUsuario.ObterUmProjetadoSemRastreamentoAsync<Usuario>(especificacao);
        }

        public async Task<Usuario?> ObterPorIdAsync(Guid id)
        {
            var dto = await _repositorioUsuario.ObterPorIdSemRastreamentoAsync(id);
            return dto != null ? FromDto(dto) : null;
        }

        public async Task<IEnumerable<Usuario>> ObterTodosAsync()
        {
            var dtos = await _repositorioUsuario.ObterTodosAsync();
            return dtos.Select(FromDto);
        }

        public async Task<IEnumerable<Usuario>> ObterUsuarioParaAlertaEstoqueAsync()
        {
            var especificacao = new ObterUsuarioParaAlertaEstoqueEspecificacao();
            return await _repositorioUsuario.ListarProjetadoAsync<Usuario>(especificacao);
        }

        public static UsuarioEntityDto ToDto(Usuario usuario)
        {
            return new UsuarioEntityDto
            {
                Id = usuario.Id,
                Ativo = usuario.Ativo,
                DataCadastro = usuario.DataCadastro,
                DataAtualizacao = usuario.DataAtualizacao,
                Email = usuario.Email,
                Senha = usuario.Senha,
                DataUltimoAcesso = usuario.DataUltimoAcesso,
                TipoUsuario = usuario.TipoUsuario,
                RecebeAlertaEstoque = usuario.RecebeAlertaEstoque,
                ClienteId = usuario.ClienteId
            };
        }

        public static Usuario FromDto(UsuarioEntityDto dto)
        {
            return new Usuario
            {
                Id = dto.Id,
                Ativo = dto.Ativo,
                DataCadastro = dto.DataCadastro,
                DataAtualizacao = dto.DataAtualizacao,
                Email = dto.Email,
                Senha = dto.Senha,
                DataUltimoAcesso = dto.DataUltimoAcesso,
                TipoUsuario = dto.TipoUsuario,
                RecebeAlertaEstoque = dto.RecebeAlertaEstoque,
                ClienteId = dto.ClienteId
            };
        }
    }
}
