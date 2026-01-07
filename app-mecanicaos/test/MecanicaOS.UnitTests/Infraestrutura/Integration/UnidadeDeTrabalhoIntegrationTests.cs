using Core.DTOs.Entidades.Cliente;
using Core.Enumeradores;
using Infraestrutura.Dados.UdT;
using Infraestrutura.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace MecanicaOS.UnitTests.Infraestrutura.Integration
{
    /// <summary>
    /// Testes de integração para UnidadeDeTrabalho
    /// Importância CRÍTICA: Valida padrão Unit of Work com transações.
    /// Garante que múltiplas operações sejam commitadas atomicamente.
    /// ROI ALTO: Testa ~100 linhas de código crítico para integridade de dados.
    /// </summary>
    public class UnidadeDeTrabalhoIntegrationTests : IntegrationTestBase
    {
        /// <summary>
        /// Verifica se Commit persiste alterações no banco
        /// Importância: Valida que SaveChanges funciona corretamente
        /// </summary>
        [Fact]
        public async Task Commit_ComAlteracoesPendentes_DeveRetornarTrue()
        {
            // Arrange
            var udt = new UnidadeDeTrabalho(Context);
            var repositorio = new Repositorio<ClienteEntityDto>(Context);
            
            var cliente = new ClienteEntityDto
            {
                Id = Guid.NewGuid(),
                Nome = "Cliente Teste",
                Documento = "12345678900",
                DataNascimento = "1990-01-01",
                TipoCliente = TipoCliente.PessoaFisica,
                Ativo = true,
                DataCadastro = DateTime.UtcNow
            };
            
            await repositorio.CadastrarAsync(cliente);
            
            // Act
            var resultado = await udt.Commit();
            
            // Assert
            resultado.Should().BeTrue("deve retornar true quando há alterações");
            
            var clienteSalvo = await Context.Clientes.FindAsync(cliente.Id);
            clienteSalvo.Should().NotBeNull("cliente deve estar persistido no banco");
        }

        /// <summary>
        /// Verifica se Commit sem alterações retorna false
        /// Importância: Valida comportamento quando não há mudanças
        /// </summary>
        [Fact]
        public async Task Commit_SemAlteracoesPendentes_DeveRetornarFalse()
        {
            // Arrange
            var udt = new UnidadeDeTrabalho(Context);
            
            // Act
            var resultado = await udt.Commit();
            
            // Assert
            resultado.Should().BeFalse("deve retornar false quando não há alterações");
        }

        /// <summary>
        /// Verifica se Commit persiste múltiplas operações atomicamente
        /// Importância: Valida atomicidade de transações
        /// </summary>
        [Fact]
        public async Task Commit_ComMultiplasOperacoes_DevePersistirTodasAtomicamente()
        {
            // Arrange
            var udt = new UnidadeDeTrabalho(Context);
            var repositorio = new Repositorio<ClienteEntityDto>(Context);
            
            var cliente1 = new ClienteEntityDto
            {
                Id = Guid.NewGuid(),
                Nome = "Cliente 1",
                Documento = "11111111111",
                DataNascimento = "1990-01-01",
                TipoCliente = TipoCliente.PessoaFisica,
                Ativo = true,
                DataCadastro = DateTime.UtcNow
            };
            
            var cliente2 = new ClienteEntityDto
            {
                Id = Guid.NewGuid(),
                Nome = "Cliente 2",
                Documento = "22222222222",
                DataNascimento = "1990-01-01",
                TipoCliente = TipoCliente.PessoaFisica,
                Ativo = true,
                DataCadastro = DateTime.UtcNow
            };
            
            await repositorio.CadastrarAsync(cliente1);
            await repositorio.CadastrarAsync(cliente2);
            
            // Act
            var resultado = await udt.Commit();
            
            // Assert
            resultado.Should().BeTrue();
            
            var clientesSalvos = await Context.Clientes.ToListAsync();
            clientesSalvos.Should().HaveCount(2, "ambos os clientes devem estar persistidos");
        }

        /// <summary>
        /// Verifica se Commit persiste operações de UPDATE
        /// Importância: Valida que atualizações são commitadas
        /// </summary>
        [Fact]
        public async Task Commit_ComOperacaoDeUpdate_DevePersistirAlteracao()
        {
            // Arrange
            var udt = new UnidadeDeTrabalho(Context);
            var repositorio = new Repositorio<ClienteEntityDto>(Context);
            
            var cliente = new ClienteEntityDto
            {
                Id = Guid.NewGuid(),
                Nome = "Nome Original",
                Documento = "33333333333",
                DataNascimento = "1990-01-01",
                TipoCliente = TipoCliente.PessoaFisica,
                Ativo = true,
                DataCadastro = DateTime.UtcNow
            };
            
            await repositorio.CadastrarAsync(cliente);
            await udt.Commit();
            
            // Detach para simular nova requisição
            Context.Entry(cliente).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            
            // Act
            cliente.Nome = "Nome Atualizado";
            await repositorio.EditarAsync(cliente);
            var resultado = await udt.Commit();
            
            // Assert
            resultado.Should().BeTrue();
            
            var clienteAtualizado = await Context.Clientes.FindAsync(cliente.Id);
            clienteAtualizado!.Nome.Should().Be("Nome Atualizado");
        }

        /// <summary>
        /// Verifica se Commit persiste operações de DELETE
        /// Importância: Valida que deleções são commitadas
        /// </summary>
        [Fact]
        public async Task Commit_ComOperacaoDeDelete_DevePersistirRemocao()
        {
            // Arrange
            var udt = new UnidadeDeTrabalho(Context);
            var repositorio = new Repositorio<ClienteEntityDto>(Context);
            
            var cliente = new ClienteEntityDto
            {
                Id = Guid.NewGuid(),
                Nome = "Cliente Para Deletar",
                Documento = "44444444444",
                DataNascimento = "1990-01-01",
                TipoCliente = TipoCliente.PessoaFisica,
                Ativo = true,
                DataCadastro = DateTime.UtcNow
            };
            
            await repositorio.CadastrarAsync(cliente);
            await udt.Commit();
            
            // Act
            await repositorio.DeletarAsync(cliente);
            var resultado = await udt.Commit();
            
            // Assert
            resultado.Should().BeTrue();
            
            var clienteDeletado = await Context.Clientes.FindAsync(cliente.Id);
            clienteDeletado.Should().BeNull("cliente deve ter sido removido");
        }
    }
}
