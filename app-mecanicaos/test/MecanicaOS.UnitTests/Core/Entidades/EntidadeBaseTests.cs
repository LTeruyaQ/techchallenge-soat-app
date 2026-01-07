using Core.Entidades;
using Core.Enumeradores;

namespace MecanicaOS.UnitTests.Core.Entidades
{
    /// <summary>
    /// Testes para métodos da classe base Entidade
    /// Importância: ALTA - Valida comportamentos fundamentais de todas as entidades
    /// </summary>
    public class EntidadeBaseTests
    {
        /// <summary>
        /// Verifica se o método Ativar() marca a entidade como ativa
        /// </summary>
        [Fact]
        public void Entidade_Ativar_DeveMudarAtivoParaTrue()
        {
            // Arrange
            var cliente = new Cliente
            {
                Nome = "Teste",
                Documento = "12345678900",
                TipoCliente = TipoCliente.PessoaFisica,
                Ativo = false
            };

            // Act
            cliente.Ativar();

            // Assert
            cliente.Ativo.Should().BeTrue("a entidade deve estar ativa após chamar Ativar()");
            cliente.DataAtualizacao.Should().NotBeNull("DataAtualizacao deve ser definida");
        }

        /// <summary>
        /// Verifica se o método Desativar() marca a entidade como inativa
        /// </summary>
        [Fact]
        public void Entidade_Desativar_DeveMudarAtivoParaFalse()
        {
            // Arrange
            var cliente = new Cliente
            {
                Nome = "Teste",
                Documento = "12345678900",
                TipoCliente = TipoCliente.PessoaFisica,
                Ativo = true
            };

            // Act
            cliente.Desativar();

            // Assert
            cliente.Ativo.Should().BeFalse("a entidade deve estar inativa após chamar Desativar()");
            cliente.DataAtualizacao.Should().NotBeNull("DataAtualizacao deve ser definida");
        }

        /// <summary>
        /// Verifica se Ativar() atualiza a DataAtualizacao
        /// </summary>
        [Fact]
        public void Entidade_Ativar_DeveAtualizarDataAtualizacao()
        {
            // Arrange
            var servico = new Servico
            {
                Nome = "Teste",
                Descricao = "Teste",
                Valor = 100m,
                Ativo = false,
                DataAtualizacao = null
            };

            // Act
            servico.Ativar();

            // Assert
            servico.DataAtualizacao.Should().NotBeNull("DataAtualizacao deve ser definida ao ativar");
            servico.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Verifica se Desativar() atualiza a DataAtualizacao
        /// </summary>
        [Fact]
        public void Entidade_Desativar_DeveAtualizarDataAtualizacao()
        {
            // Arrange
            var servico = new Servico
            {
                Nome = "Teste",
                Descricao = "Teste",
                Valor = 100m,
                Ativo = true,
                DataAtualizacao = null
            };

            // Act
            servico.Desativar();

            // Assert
            servico.DataAtualizacao.Should().NotBeNull("DataAtualizacao deve ser definida ao desativar");
            servico.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Verifica se GetHashCode() retorna o hash do Id
        /// </summary>
        [Fact]
        public void Entidade_GetHashCode_DeveRetornarHashDoId()
        {
            // Arrange
            var id = Guid.NewGuid();
            var cliente = new Cliente
            {
                Id = id,
                Nome = "Teste",
                Documento = "12345678900",
                TipoCliente = TipoCliente.PessoaFisica
            };

            // Act
            var hashCode = cliente.GetHashCode();

            // Assert
            hashCode.Should().Be(id.GetHashCode(), "o hash code deve ser baseado no Id");
        }

        /// <summary>
        /// Verifica se entidades com mesmo Id têm mesmo hash code
        /// </summary>
        [Fact]
        public void Entidade_GetHashCode_EntidadesComMesmoId_DevemTerMesmoHash()
        {
            // Arrange
            var id = Guid.NewGuid();
            var cliente1 = new Cliente
            {
                Id = id,
                Nome = "Cliente 1",
                Documento = "12345678900",
                TipoCliente = TipoCliente.PessoaFisica
            };
            var cliente2 = new Cliente
            {
                Id = id,
                Nome = "Cliente 2",
                Documento = "98765432100",
                TipoCliente = TipoCliente.PessoaFisica
            };

            // Act
            var hash1 = cliente1.GetHashCode();
            var hash2 = cliente2.GetHashCode();

            // Assert
            hash1.Should().Be(hash2, "entidades com mesmo Id devem ter mesmo hash code");
        }

        /// <summary>
        /// Verifica se entidades com Ids diferentes têm hash codes diferentes
        /// </summary>
        [Fact]
        public void Entidade_GetHashCode_EntidadesComIdsDiferentes_DevemTerHashesDiferentes()
        {
            // Arrange
            var cliente1 = new Cliente
            {
                Id = Guid.NewGuid(),
                Nome = "Cliente 1",
                Documento = "12345678900",
                TipoCliente = TipoCliente.PessoaFisica
            };
            var cliente2 = new Cliente
            {
                Id = Guid.NewGuid(),
                Nome = "Cliente 2",
                Documento = "98765432100",
                TipoCliente = TipoCliente.PessoaFisica
            };

            // Act
            var hash1 = cliente1.GetHashCode();
            var hash2 = cliente2.GetHashCode();

            // Assert
            hash1.Should().NotBe(hash2, "entidades com Ids diferentes devem ter hash codes diferentes");
        }
    }
}
