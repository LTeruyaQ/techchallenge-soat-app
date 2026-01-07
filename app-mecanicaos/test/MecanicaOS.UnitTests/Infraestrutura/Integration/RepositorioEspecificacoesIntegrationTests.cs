using Core.DTOs.Entidades.Cliente;
using Core.Entidades;
using Core.Enumeradores;
using Core.Especificacoes.Base;
using Core.Especificacoes.Base.Interfaces;
using Core.Especificacoes.Cliente;
using Infraestrutura.Repositorios;
using System.Linq.Expressions;

namespace MecanicaOS.UnitTests.Infraestrutura.Integration
{
    /// <summary>
    /// Testes de integração para Repositorio<T> com Especificações
    /// Importância CRÍTICA: Valida queries complexas com filtros, ordenação e projeção.
    /// Garante que o padrão Specification funcione corretamente com EF Core.
    /// ROI ALTO: Testa ~150 linhas de código crítico para queries complexas.
    /// </summary>
    public class RepositorioEspecificacoesIntegrationTests : IntegrationTestBase
    {
        /// <summary>
        /// Verifica se ListarAsync com especificação filtra corretamente
        /// Importância: Valida queries com filtros
        /// </summary>
        [Fact]
        public async Task ListarAsync_ComEspecificacao_DeveFiltrarCorretamente()
        {
            // Arrange
            var repositorio = new Repositorio<ClienteEntityDto>(Context);
            
            var cliente = new ClienteEntityDto
            {
                Id = Guid.NewGuid(),
                Nome = "Cliente Teste",
                Documento = "11111111111",
                DataNascimento = "1990-01-01",
                TipoCliente = TipoCliente.PessoaFisica,
                Ativo = true,
                DataCadastro = DateTime.UtcNow
            };
            
            await repositorio.CadastrarAsync(cliente);
            await Context.SaveChangesAsync();
            
            var especificacao = new ObterClienteCompletoPorIdEspecificacao(cliente.Id);
            
            // Act
            var resultado = await repositorio.ListarAsync(especificacao);
            
            // Assert
            resultado.Should().HaveCount(1);
            resultado.First().Id.Should().Be(cliente.Id);
        }

        /// <summary>
        /// Verifica se ListarSemRastreamentoAsync com especificação funciona
        /// Importância: Valida queries sem tracking com filtros
        /// </summary>
        [Fact]
        public async Task ListarSemRastreamentoAsync_ComEspecificacao_DeveFiltrarSemTracking()
        {
            // Arrange
            var repositorio = new Repositorio<ClienteEntityDto>(Context);
            
            var cliente = new ClienteEntityDto
            {
                Id = Guid.NewGuid(),
                Nome = "Cliente Teste",
                Documento = "33333333333",
                DataNascimento = "1990-01-01",
                TipoCliente = TipoCliente.PessoaFisica,
                Ativo = true,
                DataCadastro = DateTime.UtcNow
            };
            
            await repositorio.CadastrarAsync(cliente);
            await Context.SaveChangesAsync();
            
            var especificacao = new ObterClienteCompletoPorIdEspecificacao(cliente.Id);
            
            // Act
            var resultado = await repositorio.ListarSemRastreamentoAsync(especificacao);
            
            // Assert
            resultado.Should().HaveCount(1);
            Context.Entry(resultado.First()).State.Should().Be(Microsoft.EntityFrameworkCore.EntityState.Detached);
        }

        /// <summary>
        /// Verifica se ObterUmAsync com especificação retorna entidade única
        /// Importância: Valida busca de entidade única com filtro
        /// </summary>
        [Fact]
        public async Task ObterUmAsync_ComEspecificacao_DeveRetornarEntidadeUnica()
        {
            // Arrange
            var repositorio = new Repositorio<ClienteEntityDto>(Context);
            
            var cliente = new ClienteEntityDto
            {
                Id = Guid.NewGuid(),
                Nome = "Cliente Único",
                Documento = "44444444444",
                DataNascimento = "1990-01-01",
                TipoCliente = TipoCliente.PessoaFisica,
                Ativo = true,
                DataCadastro = DateTime.UtcNow
            };
            
            await repositorio.CadastrarAsync(cliente);
            await Context.SaveChangesAsync();
            
            var especificacao = new ObterClienteCompletoPorIdEspecificacao(cliente.Id);
            
            // Act
            var resultado = await repositorio.ObterUmAsync(especificacao);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado!.Documento.Should().Be("44444444444");
        }

        /// <summary>
        /// Verifica se ObterUmSemRastreamentoAsync funciona corretamente
        /// Importância: Valida busca única sem tracking
        /// </summary>
        [Fact]
        public async Task ObterUmSemRastreamentoAsync_ComEspecificacao_DeveRetornarSemTracking()
        {
            // Arrange
            var repositorio = new Repositorio<ClienteEntityDto>(Context);
            
            var cliente = new ClienteEntityDto
            {
                Id = Guid.NewGuid(),
                Nome = "Cliente NoTrack",
                Documento = "55555555555",
                DataNascimento = "1990-01-01",
                TipoCliente = TipoCliente.PessoaFisica,
                Ativo = true,
                DataCadastro = DateTime.UtcNow
            };
            
            await repositorio.CadastrarAsync(cliente);
            await Context.SaveChangesAsync();
            
            var especificacao = new ObterClienteCompletoPorIdEspecificacao(cliente.Id);
            
            // Act
            var resultado = await repositorio.ObterUmSemRastreamentoAsync(especificacao);
            
            // Assert
            resultado.Should().NotBeNull();
            Context.Entry(resultado!).State.Should().Be(Microsoft.EntityFrameworkCore.EntityState.Detached);
        }

    }
}
