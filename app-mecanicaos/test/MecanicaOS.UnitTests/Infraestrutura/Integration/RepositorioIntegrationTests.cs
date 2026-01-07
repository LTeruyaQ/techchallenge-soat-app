using Core.DTOs.Entidades.Cliente;
using Core.Enumeradores;
using Infraestrutura.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace MecanicaOS.UnitTests.Infraestrutura.Integration
{
    /// <summary>
    /// Testes de integração para Repositorio<T>
    /// Importância CRÍTICA: Valida operações CRUD com EF Core InMemory.
    /// Garante que o repositório genérico funcione corretamente com banco de dados.
    /// ROI ALTO: Testa ~300 linhas de código de infraestrutura crítica.
    /// </summary>
    public class RepositorioIntegrationTests : IntegrationTestBase
    {
        /// <summary>
        /// Verifica se o repositório cadastra entidade corretamente
        /// Importância: Valida operação CREATE do CRUD
        /// </summary>
        [Fact]
        public async Task CadastrarAsync_ComEntidadeValida_DeveCadastrarNoBanco()
        {
            // Arrange
            var repositorio = new Repositorio<ClienteEntityDto>(Context);
            var cliente = new ClienteEntityDto
            {
                Id = Guid.NewGuid(),
                Nome = "João Silva",
                Documento = "12345678900",
                DataNascimento = "1990-01-01",
                TipoCliente = TipoCliente.PessoaFisica,
                Ativo = true,
                DataCadastro = DateTime.UtcNow
            };
            
            // Act
            var resultado = await repositorio.CadastrarAsync(cliente);
            await Context.SaveChangesAsync();
            
            // Assert
            resultado.Should().NotBeNull();
            resultado.Id.Should().Be(cliente.Id);
            
            var clienteSalvo = await Context.Clientes.FindAsync(cliente.Id);
            clienteSalvo.Should().NotBeNull();
            clienteSalvo!.Nome.Should().Be("João Silva");
        }

        /// <summary>
        /// Verifica se o repositório cadastra várias entidades em lote
        /// Importância: Valida operação em lote (performance)
        /// </summary>
        [Fact]
        public async Task CadastrarVariosAsync_ComVariasEntidades_DeveCadastrarTodasNoBanco()
        {
            // Arrange
            var repositorio = new Repositorio<ClienteEntityDto>(Context);
            var clientes = new List<ClienteEntityDto>
            {
                new ClienteEntityDto
                {
                    Id = Guid.NewGuid(),
                    Nome = "Cliente 1",
                    Documento = "11111111111",
                    DataNascimento = "1990-01-01",
                    TipoCliente = TipoCliente.PessoaFisica,
                    Ativo = true,
                    DataCadastro = DateTime.UtcNow
                },
                new ClienteEntityDto
                {
                    Id = Guid.NewGuid(),
                    Nome = "Cliente 2",
                    Documento = "22222222222",
                    DataNascimento = "1990-01-01",
                    TipoCliente = TipoCliente.PessoaFisica,
                    Ativo = true,
                    DataCadastro = DateTime.UtcNow
                }
            };
            
            // Act
            var resultado = await repositorio.CadastrarVariosAsync(clientes);
            await Context.SaveChangesAsync();
            
            // Assert
            resultado.Should().HaveCount(2);
            
            var clientesSalvos = await Context.Clientes.ToListAsync();
            clientesSalvos.Should().HaveCount(2);
        }

        /// <summary>
        /// Verifica se o repositório obtém entidade por ID
        /// Importância: Valida operação READ do CRUD
        /// </summary>
        [Fact]
        public async Task ObterPorIdAsync_ComIdExistente_DeveRetornarEntidade()
        {
            // Arrange
            var repositorio = new Repositorio<ClienteEntityDto>(Context);
            var cliente = new ClienteEntityDto
            {
                Id = Guid.NewGuid(),
                Nome = "Maria Santos",
                Documento = "98765432100",
                DataNascimento = "1990-01-01",
                TipoCliente = TipoCliente.PessoaFisica,
                Ativo = true,
                DataCadastro = DateTime.UtcNow
            };
            
            await repositorio.CadastrarAsync(cliente);
            await Context.SaveChangesAsync();
            
            // Act
            var resultado = await repositorio.ObterPorIdAsync(cliente.Id);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado!.Nome.Should().Be("Maria Santos");
        }

        /// <summary>
        /// Verifica se o repositório retorna null para ID inexistente
        /// Importância: Valida comportamento quando entidade não existe
        /// </summary>
        [Fact]
        public async Task ObterPorIdAsync_ComIdInexistente_DeveRetornarNull()
        {
            // Arrange
            var repositorio = new Repositorio<ClienteEntityDto>(Context);
            var idInexistente = Guid.NewGuid();
            
            // Act
            var resultado = await repositorio.ObterPorIdAsync(idInexistente);
            
            // Assert
            resultado.Should().BeNull();
        }

        /// <summary>
        /// Verifica se o repositório obtém entidade sem rastreamento
        /// Importância: Valida queries de leitura sem tracking (performance)
        /// </summary>
        [Fact]
        public async Task ObterPorIdSemRastreamentoAsync_DeveRetornarEntidadeSemTracking()
        {
            // Arrange
            var repositorio = new Repositorio<ClienteEntityDto>(Context);
            var cliente = new ClienteEntityDto
            {
                Id = Guid.NewGuid(),
                Nome = "Pedro Oliveira",
                Documento = "55555555555",
                DataNascimento = "1990-01-01",
                TipoCliente = TipoCliente.PessoaFisica,
                Ativo = true,
                DataCadastro = DateTime.UtcNow
            };
            
            await repositorio.CadastrarAsync(cliente);
            await Context.SaveChangesAsync();
            
            // Act
            var resultado = await repositorio.ObterPorIdSemRastreamentoAsync(cliente.Id);
            
            // Assert
            resultado.Should().NotBeNull();
            Context.Entry(resultado!).State.Should().Be(Microsoft.EntityFrameworkCore.EntityState.Detached,
                "entidade não deve estar sendo rastreada");
        }

        /// <summary>
        /// Verifica se o repositório obtém todas as entidades
        /// Importância: Valida listagem completa
        /// </summary>
        [Fact]
        public async Task ObterTodosAsync_DeveRetornarTodasEntidades()
        {
            // Arrange
            var repositorio = new Repositorio<ClienteEntityDto>(Context);
            var clientes = new List<ClienteEntityDto>
            {
                new ClienteEntityDto
                {
                    Id = Guid.NewGuid(),
                    Nome = "Cliente A",
                    Documento = "11111111111",
                    DataNascimento = "1990-01-01",
                    TipoCliente = TipoCliente.PessoaFisica,
                    Ativo = true,
                    DataCadastro = DateTime.UtcNow
                },
                new ClienteEntityDto
                {
                    Id = Guid.NewGuid(),
                    Nome = "Cliente B",
                    Documento = "22222222222",
                    DataNascimento = "1990-01-01",
                    TipoCliente = TipoCliente.PessoaFisica,
                    Ativo = true,
                    DataCadastro = DateTime.UtcNow
                }
            };
            
            await repositorio.CadastrarVariosAsync(clientes);
            await Context.SaveChangesAsync();
            
            // Act
            var resultado = await repositorio.ObterTodosAsync();
            
            // Assert
            resultado.Should().HaveCount(2);
        }

        /// <summary>
        /// Verifica se o repositório edita entidade corretamente
        /// Importância: Valida operação UPDATE do CRUD
        /// </summary>
        [Fact]
        public async Task EditarAsync_ComEntidadeExistente_DeveAtualizarNoBanco()
        {
            // Arrange
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
            await Context.SaveChangesAsync();
            
            // Detach para simular nova requisição
            Context.Entry(cliente).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            
            // Act
            cliente.Nome = "Nome Atualizado";
            await repositorio.EditarAsync(cliente);
            await Context.SaveChangesAsync();
            
            // Assert
            var clienteAtualizado = await Context.Clientes.FindAsync(cliente.Id);
            clienteAtualizado!.Nome.Should().Be("Nome Atualizado");
        }

        /// <summary>
        /// Verifica se o repositório deleta entidade corretamente
        /// Importância: Valida operação DELETE do CRUD
        /// </summary>
        [Fact]
        public async Task DeletarAsync_ComEntidadeExistente_DeveRemoverDoBanco()
        {
            // Arrange
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
            await Context.SaveChangesAsync();
            
            // Act
            await repositorio.DeletarAsync(cliente);
            await Context.SaveChangesAsync();
            
            // Assert
            var clienteDeletado = await Context.Clientes.FindAsync(cliente.Id);
            clienteDeletado.Should().BeNull("entidade deve ter sido removida");
        }

        /// <summary>
        /// Verifica se o repositório deleta logicamente (soft delete)
        /// Importância: Valida deleção lógica preservando dados
        /// </summary>
        [Fact]
        public async Task DeletarLogicamenteAsync_ComEntidadeExistente_DeveMarcarComoInativo()
        {
            // Arrange
            var repositorio = new Repositorio<ClienteEntityDto>(Context);
            var cliente = new ClienteEntityDto
            {
                Id = Guid.NewGuid(),
                Nome = "Cliente Para Inativar",
                Documento = "66666666666",
                DataNascimento = "1990-01-01",
                TipoCliente = TipoCliente.PessoaFisica,
                Ativo = true,
                DataCadastro = DateTime.UtcNow
            };
            
            await repositorio.CadastrarAsync(cliente);
            await Context.SaveChangesAsync();
            
            // Act
            await repositorio.DeletarLogicamenteAsync(cliente);
            await Context.SaveChangesAsync();
            
            // Assert
            var clienteInativado = await Context.Clientes.FindAsync(cliente.Id);
            clienteInativado.Should().NotBeNull();
            clienteInativado!.Ativo.Should().BeFalse("entidade deve estar inativa");
        }

        /// <summary>
        /// Verifica se o repositório edita várias entidades em lote
        /// Importância: Valida operação UPDATE em lote (performance)
        /// </summary>
        [Fact]
        public async Task EditarVariosAsync_ComVariasEntidades_DeveAtualizarTodasNoBanco()
        {
            // Arrange
            var repositorio = new Repositorio<ClienteEntityDto>(Context);
            var clientes = new List<ClienteEntityDto>
            {
                new ClienteEntityDto
                {
                    Id = Guid.NewGuid(),
                    Nome = "Cliente X",
                    Documento = "77777777777",
                    DataNascimento = "1990-01-01",
                    TipoCliente = TipoCliente.PessoaFisica,
                    Ativo = true,
                    DataCadastro = DateTime.UtcNow
                },
                new ClienteEntityDto
                {
                    Id = Guid.NewGuid(),
                    Nome = "Cliente Y",
                    Documento = "88888888888",
                    DataNascimento = "1990-01-01",
                    TipoCliente = TipoCliente.PessoaFisica,
                    Ativo = true,
                    DataCadastro = DateTime.UtcNow
                }
            };
            
            await repositorio.CadastrarVariosAsync(clientes);
            await Context.SaveChangesAsync();
            
            // Detach para simular nova requisição
            foreach (var c in clientes)
            {
                Context.Entry(c).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }
            
            // Act
            clientes[0].Nome = "Cliente X Atualizado";
            clientes[1].Nome = "Cliente Y Atualizado";
            await repositorio.EditarVariosAsync(clientes);
            await Context.SaveChangesAsync();
            
            // Assert
            var clientesAtualizados = await Context.Clientes
                .Where(c => c.Id == clientes[0].Id || c.Id == clientes[1].Id)
                .ToListAsync();
            
            clientesAtualizados.Should().HaveCount(2);
            clientesAtualizados.Should().Contain(c => c.Nome == "Cliente X Atualizado");
            clientesAtualizados.Should().Contain(c => c.Nome == "Cliente Y Atualizado");
        }

        /// <summary>
        /// Verifica se o repositório deleta várias entidades em lote
        /// Importância: Valida operação DELETE em lote
        /// </summary>
        [Fact]
        public async Task DeletarVariosAsync_ComVariasEntidades_DeveRemoverTodasDoBanco()
        {
            // Arrange
            var repositorio = new Repositorio<ClienteEntityDto>(Context);
            var clientes = new List<ClienteEntityDto>
            {
                new ClienteEntityDto
                {
                    Id = Guid.NewGuid(),
                    Nome = "Cliente Delete 1",
                    Documento = "99999999991",
                    DataNascimento = "1990-01-01",
                    TipoCliente = TipoCliente.PessoaFisica,
                    Ativo = true,
                    DataCadastro = DateTime.UtcNow
                },
                new ClienteEntityDto
                {
                    Id = Guid.NewGuid(),
                    Nome = "Cliente Delete 2",
                    Documento = "99999999992",
                    DataNascimento = "1990-01-01",
                    TipoCliente = TipoCliente.PessoaFisica,
                    Ativo = true,
                    DataCadastro = DateTime.UtcNow
                }
            };
            
            await repositorio.CadastrarVariosAsync(clientes);
            await Context.SaveChangesAsync();
            
            // Act
            await repositorio.DeletarVariosAsync(clientes);
            await Context.SaveChangesAsync();
            
            // Assert
            var clientesRestantes = await Context.Clientes
                .Where(c => c.Id == clientes[0].Id || c.Id == clientes[1].Id)
                .ToListAsync();
            
            clientesRestantes.Should().BeEmpty("todas as entidades devem ter sido removidas");
        }

        /// <summary>
        /// Verifica se CadastrarVariosAsync com lista vazia não gera erro
        /// Importância: Valida tratamento de edge case
        /// </summary>
        [Fact]
        public async Task CadastrarVariosAsync_ComListaVazia_DeveRetornarListaVazia()
        {
            // Arrange
            var repositorio = new Repositorio<ClienteEntityDto>(Context);
            var clientesVazio = new List<ClienteEntityDto>();
            
            // Act
            var resultado = await repositorio.CadastrarVariosAsync(clientesVazio);
            
            // Assert
            resultado.Should().BeEmpty();
        }

        /// <summary>
        /// Verifica se EditarVariosAsync com lista vazia não gera erro
        /// Importância: Valida tratamento de edge case
        /// </summary>
        [Fact]
        public async Task EditarVariosAsync_ComListaVazia_NaoDeveLancarExcecao()
        {
            // Arrange
            var repositorio = new Repositorio<ClienteEntityDto>(Context);
            var clientesVazio = new List<ClienteEntityDto>();
            
            // Act
            Func<Task> act = async () => await repositorio.EditarVariosAsync(clientesVazio);
            
            // Assert
            await act.Should().NotThrowAsync("não deve lançar exceção com lista vazia");
        }
    }
}
