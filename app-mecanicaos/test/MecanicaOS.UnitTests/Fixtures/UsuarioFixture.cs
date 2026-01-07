using Core.Entidades;
using Core.Enumeradores;

namespace MecanicaOS.UnitTests.Fixtures
{
    /// <summary>
    /// Fixture para criação de objetos Usuario para testes
    /// </summary>
    public static class UsuarioFixture
    {
        /// <summary>
        /// Cria um usuário administrador válido para testes
        /// </summary>
        /// <returns>Uma instância de Usuario com tipo Administrador</returns>
        public static Usuario CriarUsuarioAdministrador()
        {
            return new Usuario
            {
                Id = Guid.NewGuid(),
                Email = "admin@teste.com",
                Senha = "senha123",
                TipoUsuario = TipoUsuario.Admin,
                RecebeAlertaEstoque = true,
                DataUltimoAcesso = DateTime.Now.AddDays(-1),
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now,
                Ativo = true
            };
        }

        /// <summary>
        /// Cria um usuário cliente válido para testes
        /// </summary>
        /// <param name="clienteId">ID do cliente vinculado ao usuário</param>
        /// <returns>Uma instância de Usuario com tipo Cliente</returns>
        public static Usuario CriarUsuarioCliente(Guid? clienteId = null)
        {
            return new Usuario
            {
                Id = Guid.NewGuid(),
                Email = "cliente@teste.com",
                Senha = "senha123",
                TipoUsuario = TipoUsuario.Cliente,
                ClienteId = clienteId ?? Guid.NewGuid(),
                RecebeAlertaEstoque = false,
                DataUltimoAcesso = DateTime.Now.AddDays(-2),
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now,
                Ativo = true
            };
        }

        /// <summary>
        /// Cria um usuário inativo para testes
        /// </summary>
        /// <returns>Uma instância de Usuario com Ativo = false</returns>
        public static Usuario CriarUsuarioInativo()
        {
            return new Usuario
            {
                Id = Guid.NewGuid(),
                Email = "inativo@teste.com",
                Senha = "senha123",
                TipoUsuario = TipoUsuario.Admin,
                RecebeAlertaEstoque = false,
                DataUltimoAcesso = DateTime.Now.AddDays(-30),
                DataCadastro = DateTime.Now.AddDays(-60),
                DataAtualizacao = DateTime.Now.AddDays(-30),
                Ativo = false
            };
        }

        /// <summary>
        /// Cria uma lista de usuários para testes
        /// </summary>
        /// <param name="quantidade">Quantidade de usuários a serem criados</param>
        /// <returns>Lista de usuários</returns>
        public static List<Usuario> CriarListaUsuarios(int quantidade = 3)
        {
            var usuarios = new List<Usuario>();
            
            for (int i = 0; i < quantidade; i++)
            {
                var tipoUsuario = i % 2 == 0 ? TipoUsuario.Admin : TipoUsuario.Cliente;
                var usuario = new Usuario
                {
                    Id = Guid.NewGuid(),
                    Email = $"usuario{i}@teste.com",
                    Senha = "senha123",
                    TipoUsuario = tipoUsuario,
                    ClienteId = tipoUsuario == TipoUsuario.Cliente ? Guid.NewGuid() : null,
                    RecebeAlertaEstoque = i % 2 == 0,
                    DataUltimoAcesso = DateTime.Now.AddDays(-i),
                    DataCadastro = DateTime.Now,
                    DataAtualizacao = DateTime.Now,
                    Ativo = true
                };
                
                usuarios.Add(usuario);
            }
            
            return usuarios;
        }
    }
}
