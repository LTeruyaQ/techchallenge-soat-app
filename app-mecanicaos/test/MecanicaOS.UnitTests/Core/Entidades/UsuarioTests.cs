using Core.Entidades;
using Core.Enumeradores;
using MecanicaOS.UnitTests.Fixtures;

namespace MecanicaOS.UnitTests.Core.Entidades
{
    /// <summary>
    /// Testes unitários para a entidade Usuario
    /// </summary>
    public class UsuarioTests
    {
        /// <summary>
        /// Verifica se um usuário administrador criado com dados válidos tem todas as propriedades preenchidas corretamente
        /// </summary>
        [Fact]
        public void Usuario_QuandoCriadoComoAdministrador_DeveSerValido()
        {
            // Arrange & Act
            var usuario = UsuarioFixture.CriarUsuarioAdministrador();

            // Assert
            usuario.Should().NotBeNull("a entidade não deve ser nula");
            usuario.Email.Should().Be("admin@teste.com", "o email deve corresponder ao valor definido");
            usuario.Email.Should().Be("admin@teste.com", "o email deve corresponder ao valor definido");
            usuario.Senha.Should().Be("senha123", "a senha deve corresponder ao valor definido");
            usuario.TipoUsuario.Should().Be(TipoUsuario.Admin, "o tipo de usuário deve ser Admin");
            usuario.RecebeAlertaEstoque.Should().BeTrue("o administrador deve receber alertas de estoque");
            usuario.ClienteId.Should().BeNull("um administrador não deve ter ClienteId");
        }

        /// <summary>
        /// Verifica se um usuário cliente criado com dados válidos tem todas as propriedades preenchidas corretamente
        /// </summary>
        [Fact]
        public void Usuario_QuandoCriadoComoCliente_DeveSerValido()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            
            // Act
            var usuario = UsuarioFixture.CriarUsuarioCliente(clienteId);

            // Assert
            usuario.Should().NotBeNull("a entidade não deve ser nula");
            usuario.Email.Should().Be("cliente@teste.com", "o email deve corresponder ao valor definido");
            usuario.Email.Should().Be("cliente@teste.com", "o email deve corresponder ao valor definido");
            usuario.TipoUsuario.Should().Be(TipoUsuario.Cliente, "o tipo de usuário deve ser Cliente");
            usuario.ClienteId.Should().Be(clienteId, "o ClienteId deve corresponder ao valor definido");
        }

        /// <summary>
        /// Verifica se a propriedade Ativo é definida como true por padrão
        /// </summary>
        [Fact]
        public void Usuario_QuandoCriado_DeveEstarAtivoPorPadrao()
        {
            // Arrange & Act
            var usuario = new Usuario();

            // Assert
            usuario.Ativo.Should().BeTrue("um usuário deve estar ativo por padrão");
        }

        /// <summary>
        /// Verifica se a propriedade Id é gerada automaticamente
        /// </summary>
        [Fact]
        public void Usuario_QuandoCriado_DeveGerarIdAutomaticamente()
        {
            // Arrange & Act
            var usuario = new Usuario();

            // Assert
            usuario.Id.Should().NotBeEmpty("o Id deve ser gerado automaticamente");
        }

        /// <summary>
        /// Verifica se a propriedade DataCadastro é preenchida automaticamente
        /// </summary>
        [Fact]
        public void Usuario_QuandoCriado_DevePreencherDataCadastro()
        {
            // Arrange & Act
            var usuario = new Usuario();
            var agora = DateTime.UtcNow;

            // Assert
            usuario.DataCadastro.Should().BeCloseTo(agora, TimeSpan.FromSeconds(1), 
                "a data de cadastro deve ser próxima à data atual");
        }

        /// <summary>
        /// Verifica se a propriedade DataAtualizacao é null no construtor e preenchida ao chamar MarcarComoAtualizada
        /// </summary>
        [Fact]
        public void Usuario_QuandoCriado_DataAtualizacaoDeveSerNull()
        {
            // Arrange & Act
            var usuario = new Usuario();

            // Assert
            usuario.DataAtualizacao.Should().BeNull("a data de atualização deve ser null no construtor");
            
            // Act - Marcar como atualizada
            usuario.MarcarComoAtualizada();
            var agora = DateTime.UtcNow;
            
            // Assert
            usuario.DataAtualizacao.Should().NotBeNull("a data de atualização deve ser preenchida após MarcarComoAtualizada");
            usuario.DataAtualizacao.Value.Should().BeCloseTo(agora, TimeSpan.FromSeconds(1), 
                "a data de atualização deve ser próxima à data atual");
        }

        /// <summary>
        /// Verifica se dois usuários com o mesmo Id são considerados iguais (comportamento da classe Entidade base)
        /// </summary>
        [Fact]
        public void Usuario_ComMesmoId_DevemSerConsideradosIguais()
        {
            // Arrange
            var id = Guid.NewGuid();
            var usuario1 = new Usuario { Id = id, Email = "teste1@teste.com" };
            var usuario2 = new Usuario { Id = id, Email = "teste2@teste.com" };

            // Act & Assert
            usuario1.Equals(usuario2).Should().BeTrue("usuários com o mesmo Id devem ser considerados iguais");
        }

        /// <summary>
        /// Verifica se dois usuários com emails diferentes são considerados diferentes
        /// </summary>
        [Fact]
        public void Usuario_ComEmailsDiferentes_DevemSerConsideradosDiferentes()
        {
            // Arrange
            var usuario1 = new Usuario { Email = "teste1@teste.com" };
            var usuario2 = new Usuario { Email = "teste2@teste.com" };

            // Act & Assert
            usuario1.Equals(usuario2).Should().BeFalse("usuários com emails diferentes devem ser considerados diferentes");
        }

        /// <summary>
        /// Verifica se a propriedade UltimoAcesso pode ser atualizada
        /// </summary>
        [Fact]
        public void Usuario_QuandoAtualizaUltimoAcesso_DeveAlterarUltimoAcesso()
        {
            // Arrange
            var usuario = UsuarioFixture.CriarUsuarioAdministrador();
            var novoAcesso = DateTime.Now;
            
            // Act
            usuario.DataUltimoAcesso = novoAcesso;
            
            // Assert
            usuario.DataUltimoAcesso.Should().Be(novoAcesso, "o último acesso deve ser atualizado");
        }

        /// <summary>
        /// Verifica se a propriedade RecebeAlertaEstoque pode ser atualizada
        /// </summary>
        [Fact]
        public void Usuario_QuandoAtualizaRecebeAlertaEstoque_DeveAlterarRecebeAlertaEstoque()
        {
            // Arrange
            var usuario = UsuarioFixture.CriarUsuarioAdministrador();
            
            // Act
            usuario.RecebeAlertaEstoque = false;
            
            // Assert
            usuario.RecebeAlertaEstoque.Should().BeFalse("a opção de receber alertas de estoque deve ser atualizada");
        }

        /// <summary>
        /// Verifica se a propriedade Ativo pode ser atualizada
        /// </summary>
        [Fact]
        public void Usuario_QuandoAtualizaAtivo_DeveAlterarAtivo()
        {
            // Arrange
            var usuario = UsuarioFixture.CriarUsuarioAdministrador();
            
            // Act
            usuario.Ativo = false;
            
            // Assert
            usuario.Ativo.Should().BeFalse("o status ativo deve ser atualizado");
        }

        [Fact]
        public void Usuario_Atualizar_ComTodosParametros_DeveAtualizarTodasPropriedades()
        {
            // Arrange
            var usuario = UsuarioFixture.CriarUsuarioAdministrador();
            var novoEmail = "novo@teste.com";
            var novaSenha = "novaSenha123";
            var novaData = DateTime.UtcNow.AddDays(-1);
            var novoTipo = TipoUsuario.Cliente;
            var novoRecebeAlerta = false;
            var novoAtivo = false;

            // Act
            usuario.Atualizar(novoEmail, novaSenha, novaData, novoTipo, novoRecebeAlerta, novoAtivo);

            // Assert
            usuario.Email.Should().Be(novoEmail);
            usuario.Senha.Should().Be(novaSenha);
            usuario.DataUltimoAcesso.Should().Be(novaData);
            usuario.TipoUsuario.Should().Be(novoTipo);
            usuario.RecebeAlertaEstoque.Should().Be(novoRecebeAlerta);
            usuario.Ativo.Should().Be(novoAtivo);
        }

        [Fact]
        public void Usuario_Atualizar_ComParametrosNulos_NaoDeveAlterarPropriedades()
        {
            // Arrange
            var usuario = UsuarioFixture.CriarUsuarioAdministrador();
            var emailOriginal = usuario.Email;
            var senhaOriginal = usuario.Senha;
            var tipoOriginal = usuario.TipoUsuario;

            // Act
            usuario.Atualizar();

            // Assert
            usuario.Email.Should().Be(emailOriginal);
            usuario.Senha.Should().Be(senhaOriginal);
            usuario.TipoUsuario.Should().Be(tipoOriginal);
        }

        [Fact]
        public void Usuario_AtualizarUltimoAcesso_SemParametro_DeveDefinirDataAtual()
        {
            // Arrange
            var usuario = UsuarioFixture.CriarUsuarioAdministrador();
            var antes = DateTime.UtcNow;

            // Act
            usuario.AtualizarUltimoAcesso();

            // Assert
            usuario.DataUltimoAcesso.Should().NotBeNull();
            usuario.DataUltimoAcesso.Value.Should().BeCloseTo(antes, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Usuario_AtualizarUltimoAcesso_ComParametro_DeveDefinirDataEspecifica()
        {
            // Arrange
            var usuario = UsuarioFixture.CriarUsuarioAdministrador();
            var dataEspecifica = new DateTime(2024, 1, 15, 10, 30, 0);

            // Act
            usuario.AtualizarUltimoAcesso(dataEspecifica);

            // Assert
            usuario.DataUltimoAcesso.Should().Be(dataEspecifica);
        }

        [Fact]
        public void Usuario_ConstrutorComParametros_DeveCriarUsuarioCorretamente()
        {
            // Arrange
            var email = "teste@teste.com";
            var senha = "senha123";
            var tipo = TipoUsuario.Cliente;
            var clienteId = Guid.NewGuid();

            // Act
            var usuario = new Usuario(email, senha, tipo, clienteId);

            // Assert
            usuario.Email.Should().Be(email);
            usuario.Senha.Should().Be(senha);
            usuario.TipoUsuario.Should().Be(tipo);
            usuario.ClienteId.Should().Be(clienteId);
        }
    }
}
