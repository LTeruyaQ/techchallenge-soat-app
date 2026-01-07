using Core.Entidades;
using Core.Enumeradores;
using MecanicaOS.UnitTests.Fixtures;

namespace MecanicaOS.UnitTests.Core.Entidades
{
    /// <summary>
    /// Testes unitários para a entidade Cliente
    /// </summary>
    public class ClienteTests
    {
        /// <summary>
        /// Verifica se um cliente criado com dados válidos tem todas as propriedades preenchidas corretamente
        /// </summary>
        [Fact]
        public void Cliente_QuandoCriadoComDadosValidos_DeveSerValido()
        {
            // Arrange & Act
            var cliente = ClienteFixture.CriarClienteValido();

            // Assert
            cliente.Should().NotBeNull("a entidade não deve ser nula");
            cliente.Nome.Should().Be("Cliente Teste", "o nome deve corresponder ao valor definido");
            cliente.TipoCliente.Should().Be(TipoCliente.PessoaFisica, "o tipo de cliente deve ser pessoa física");
            cliente.Documento.Should().Be("12345678900", "o documento deve corresponder ao CPF definido");
            cliente.Contato.Should().NotBeNull("o contato não deve ser nulo");
            cliente.Endereco.Should().NotBeNull("o endereço não deve ser nulo");
        }

        /// <summary>
        /// Verifica se um cliente pessoa jurídica é criado corretamente
        /// </summary>
        [Fact]
        public void Cliente_QuandoCriadoComoPessoaJuridica_DeveSerValido()
        {
            // Arrange & Act
            var cliente = ClienteFixture.CriarClientePessoaJuridica();

            // Assert
            cliente.Should().NotBeNull("a entidade não deve ser nula");
            cliente.Nome.Should().Be("Empresa Teste LTDA", "o nome deve corresponder ao valor definido");
            cliente.TipoCliente.Should().Be(TipoCliente.PessoaJuridico, "o tipo de cliente deve ser pessoa jurídica");
            cliente.Documento.Should().Be("12345678000199", "o documento deve corresponder ao CNPJ definido");
        }

        /// <summary>
        /// Verifica se a propriedade Ativo é definida como true por padrão
        /// </summary>
        [Fact]
        public void Cliente_QuandoCriado_DeveEstarAtivoPorPadrao()
        {
            // Arrange & Act
            var cliente = new Cliente();

            // Assert
            cliente.Ativo.Should().BeTrue("um cliente deve estar ativo por padrão");
        }

        /// <summary>
        /// Verifica se a propriedade Id é gerada automaticamente
        /// </summary>
        [Fact]
        public void Cliente_QuandoCriado_DeveGerarIdAutomaticamente()
        {
            // Arrange & Act
            var cliente = new Cliente();

            // Assert
            cliente.Id.Should().NotBeEmpty("o Id deve ser gerado automaticamente");
        }

        /// <summary>
        /// Verifica se a propriedade DataCadastro é preenchida automaticamente
        /// </summary>
        [Fact]
        public void Cliente_QuandoCriado_DevePreencherDataCadastro()
        {
            // Arrange & Act
            var cliente = new Cliente();
            var agora = DateTime.UtcNow;

            // Assert
            cliente.DataCadastro.Should().BeCloseTo(agora, TimeSpan.FromSeconds(1), 
                "a data de cadastro deve ser próxima à data atual");
        }

        /// <summary>
        /// Verifica se a propriedade DataAtualizacao é null no construtor e preenchida ao chamar MarcarComoAtualizada
        /// </summary>
        [Fact]
        public void Cliente_QuandoCriado_DataAtualizacaoDeveSerNull()
        {
            // Arrange & Act
            var cliente = new Cliente();

            // Assert
            cliente.DataAtualizacao.Should().BeNull("a data de atualização deve ser null no construtor");
            
            // Act - Marcar como atualizada
            cliente.MarcarComoAtualizada();
            var agora = DateTime.UtcNow;
            
            // Assert
            cliente.DataAtualizacao.Should().NotBeNull("a data de atualização deve ser preenchida após MarcarComoAtualizada");
            cliente.DataAtualizacao.Value.Should().BeCloseTo(agora, TimeSpan.FromSeconds(1), 
                "a data de atualização deve ser próxima à data atual");
        }

        /// <summary>
        /// Verifica se dois clientes com o mesmo Id são considerados iguais (comportamento da classe Entidade base)
        /// </summary>
        [Fact]
        public void Cliente_ComMesmoId_DevemSerConsideradosIguais()
        {
            // Arrange
            var id = Guid.NewGuid();
            var cliente1 = new Cliente { Id = id, Documento = "12345678900" };
            var cliente2 = new Cliente { Id = id, Documento = "98765432100" };

            // Act & Assert
            cliente1.Equals(cliente2).Should().BeTrue("clientes com o mesmo Id devem ser considerados iguais");
        }

        /// <summary>
        /// Verifica se dois clientes com documentos diferentes são considerados diferentes
        /// </summary>
        [Fact]
        public void Cliente_ComDocumentosDiferentes_DevemSerConsideradosDiferentes()
        {
            // Arrange
            var cliente1 = new Cliente { Documento = "12345678900" };
            var cliente2 = new Cliente { Documento = "98765432100" };

            // Act & Assert
            cliente1.Equals(cliente2).Should().BeFalse("clientes com documentos diferentes devem ser considerados diferentes");
        }
    }
}
