using Core.DTOs.Entidades.Usuarios;
using Core.DTOs.UseCases.Usuario;
using Core.Entidades;
using Core.Enumeradores;
using Core.Interfaces.Gateways;
using Core.Interfaces.Repositorios;
using Core.Interfaces.Servicos;

namespace MecanicaOS.UnitTests.Fixtures
{
    /// <summary>
    /// Fixture para testes de handlers de Usuário
    /// </summary>
    public static class UsuarioHandlerFixture
    {
        /// <summary>
        /// Cria um mock de IUsuarioGateway para testes
        /// </summary>
        public static IUsuarioGateway CriarUsuarioGatewayMock()
        {
            return Substitute.For<IUsuarioGateway>();
        }

        /// <summary>
        /// Cria um mock de IRepositorio<UsuarioEntityDto> para testes
        /// </summary>
        public static IRepositorio<UsuarioEntityDto> CriarUsuarioRepositorioMock()
        {
            return Substitute.For<IRepositorio<UsuarioEntityDto>>();
        }

        /// <summary>
        /// Cria um mock de IUnidadeDeTrabalhoGateway para testes
        /// </summary>
        public static IUnidadeDeTrabalhoGateway CriarUnidadeDeTrabalhMock()
        {
            return Substitute.For<IUnidadeDeTrabalhoGateway>();
        }

        /// <summary>
        /// Cria um mock de IServicoSenha para testes
        /// </summary>
        public static IServicoSenha CriarServicoSenhaMock()
        {
            var servicoSenhaMock = Substitute.For<IServicoSenha>();
            servicoSenhaMock.CriptografarSenha(Arg.Any<string>()).Returns("senha_criptografada");
            servicoSenhaMock.VerificarSenha(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
            return servicoSenhaMock;
        }

        /// <summary>
        /// Cria um usuário válido para testes
        /// </summary>
        public static Usuario CriarUsuario()
        {
            return new Usuario
            {
                Id = Guid.NewGuid(),
                Email = "usuario@teste.com",
                Senha = "senha_criptografada",
                TipoUsuario = TipoUsuario.Admin,
                RecebeAlertaEstoque = false,
                Ativo = true,
                DataCadastro = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Cria um DTO de cadastro de usuário para testes
        /// </summary>
        public static CadastrarUsuarioUseCaseDto CriarCadastrarUsuarioUseCaseDto()
        {
            return new CadastrarUsuarioUseCaseDto
            {
                Email = "usuario@teste.com",
                Senha = "senha123",
                TipoUsuario = TipoUsuario.Admin,
                RecebeAlertaEstoque = false,
                ClienteId = null
            };
        }

        /// <summary>
        /// Cria um DTO de atualização de usuário para testes
        /// </summary>
        public static AtualizarUsuarioUseCaseDto CriarAtualizarUsuarioUseCaseDto()
        {
            return new AtualizarUsuarioUseCaseDto
            {
                Email = "usuario.atualizado@teste.com",
                Senha = "novaSenha123",
                TipoUsuario = TipoUsuario.Admin,
                RecebeAlertaEstoque = true
            };
        }

        /// <summary>
        /// Cria um gateway real de usuário para testes de integração
        /// </summary>
        public static IUsuarioGateway CriarUsuarioGatewayReal(IRepositorio<UsuarioEntityDto> repositorio)
        {
            return new global::Adapters.Gateways.UsuarioGateway(repositorio);
        }
    }
}
