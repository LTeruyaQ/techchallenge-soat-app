using Core.DTOs.Entidades.Cliente;
using Core.Entidades;
using Core.Enumeradores;
using Core.Especificacoes.Base;
using Core.Especificacoes.Base.Interfaces;
using Core.Especificacoes.Cliente;
using Infraestrutura.Dados.Especificacoes;
using System.Linq.Expressions;

namespace MecanicaOS.UnitTests.Infraestrutura.Especificacoes
{
    /// <summary>
    /// Testes para AvaliadorDeEspecificacao
    /// Importância CRÍTICA: Valida a tradução de especificações em queries EF Core.
    /// Este componente é responsável por aplicar filtros, includes e projeções.
    /// ROI ALTO: Testa ~72 linhas de código crítico para queries complexas.
    /// </summary>
    public class AvaliadorDeEspecificacaoTests
    {
        /// <summary>
        /// Especificação simples para testes
        /// </summary>
        private class ClienteAtivoEspecificacao : EspecificacaoBase<ClienteEntityDto>
        {
            public override Expression<Func<ClienteEntityDto, bool>> Expressao => c => c.Ativo;
        }

        /// <summary>
        /// Especificação com paginação
        /// </summary>
        private class ClientePaginadoEspecificacao : EspecificacaoBase<ClienteEntityDto>
        {
            public ClientePaginadoEspecificacao(int pagina, int tamanho)
            {
                AdicionarPaginacao(pagina, tamanho);
            }

            public override Expression<Func<ClienteEntityDto, bool>> Expressao => c => c.Ativo;
        }

        /// <summary>
        /// Verifica se ObterConsulta aplica filtro Where corretamente
        /// Importância: Valida que expressões são traduzidas para SQL
        /// </summary>
        [Fact]
        public void ObterConsulta_ComExpressao_DeveAplicarWhere()
        {
            // Arrange
            var avaliador = new AvaliadorDeEspecificacao<ClienteEntityDto>();
            var clientes = new List<ClienteEntityDto>
            {
                new ClienteEntityDto { Id = Guid.NewGuid(), Nome = "Cliente 1", Ativo = true, Documento = "123", DataNascimento = "1990-01-01", TipoCliente = TipoCliente.PessoaFisica },
                new ClienteEntityDto { Id = Guid.NewGuid(), Nome = "Cliente 2", Ativo = false, Documento = "456", DataNascimento = "1990-01-01", TipoCliente = TipoCliente.PessoaFisica }
            }.AsQueryable();

            var especificacao = new ClienteAtivoEspecificacao();

            // Act
            var resultado = avaliador.ObterConsulta(clientes, especificacao);

            // Assert
            resultado.Should().HaveCount(1);
            resultado.First().Ativo.Should().BeTrue();
        }

        /// <summary>
        /// Verifica se ObterConsulta funciona sem expressão (retorna todos)
        /// Importância: Valida comportamento quando não há filtro
        /// </summary>
        [Fact]
        public void ObterConsulta_SemExpressao_DeveRetornarTodos()
        {
            // Arrange
            var avaliador = new AvaliadorDeEspecificacao<ClienteEntityDto>();
            var clientes = new List<ClienteEntityDto>
            {
                new ClienteEntityDto { Id = Guid.NewGuid(), Nome = "Cliente 1", Ativo = true, Documento = "123", DataNascimento = "1990-01-01", TipoCliente = TipoCliente.PessoaFisica },
                new ClienteEntityDto { Id = Guid.NewGuid(), Nome = "Cliente 2", Ativo = false, Documento = "456", DataNascimento = "1990-01-01", TipoCliente = TipoCliente.PessoaFisica }
            }.AsQueryable();

            // Especificação sem expressão (null)
            var especificacao = new ObterClienteCompletoPorIdEspecificacao(Guid.Empty);
            
            // Forçar expressão null para testar o comportamento
            var especificacaoMock = Substitute.For<IEspecificacao<ClienteEntityDto>>();
            especificacaoMock.Expressao.Returns((Expression<Func<ClienteEntityDto, bool>>?)null);
            especificacaoMock.Inclusoes.Returns(new HashSet<string>());

            // Act
            var resultado = avaliador.ObterConsulta(clientes, especificacaoMock);

            // Assert
            resultado.Should().HaveCount(2, "sem filtro deve retornar todos");
        }

        /// <summary>
        /// Verifica se AplicarPaginacao funciona corretamente
        /// Importância: Valida Skip/Take para paginação
        /// </summary>
        [Fact]
        public void AplicarPaginacao_ComTamanhoDefinido_DeveAplicarSkipETake()
        {
            // Arrange
            var avaliador = new AvaliadorDeEspecificacao<ClienteEntityDto>();
            var clientes = Enumerable.Range(1, 10).Select(i => new ClienteEntityDto
            {
                Id = Guid.NewGuid(),
                Nome = $"Cliente {i}",
                Ativo = true,
                Documento = $"{i}",
                DataNascimento = "1990-01-01",
                TipoCliente = TipoCliente.PessoaFisica
            }).AsQueryable();

            var especificacao = new ClientePaginadoEspecificacao(pagina: 1, tamanho: 3); // Página 1, 3 itens

            // Act
            var resultado = avaliador.AplicarPaginacao(clientes, especificacao);

            // Assert
            resultado.Should().HaveCount(3, "deve retornar 3 itens");
            resultado.First().Nome.Should().Be("Cliente 4", "deve pular os 3 primeiros (página 1)");
        }

        /// <summary>
        /// Verifica se AplicarPaginacao sem tamanho retorna todos
        /// Importância: Valida comportamento quando não há paginação
        /// </summary>
        [Fact]
        public void AplicarPaginacao_SemTamanho_DeveRetornarTodos()
        {
            // Arrange
            var avaliador = new AvaliadorDeEspecificacao<ClienteEntityDto>();
            var clientes = Enumerable.Range(1, 10).Select(i => new ClienteEntityDto
            {
                Id = Guid.NewGuid(),
                Nome = $"Cliente {i}",
                Ativo = true,
                Documento = $"{i}",
                DataNascimento = "1990-01-01",
                TipoCliente = TipoCliente.PessoaFisica
            }).AsQueryable();

            var especificacao = new ClienteAtivoEspecificacao(); // Sem paginação (tamanho = 0)

            // Act
            var resultado = avaliador.AplicarPaginacao(clientes, especificacao);

            // Assert
            resultado.Should().HaveCount(10, "sem paginação deve retornar todos");
        }

        /// <summary>
        /// Verifica se AplicarProjecao lança exceção quando não há projeção
        /// Importância: Valida tratamento de erro
        /// </summary>
        [Fact]
        public void AplicarProjecao_SemProjecaoDefinida_DeveLancarExcecao()
        {
            // Arrange
            var avaliador = new AvaliadorDeEspecificacao<ClienteEntityDto>();
            var clientes = new List<ClienteEntityDto>
            {
                new ClienteEntityDto { Id = Guid.NewGuid(), Nome = "Cliente 1", Ativo = true, Documento = "123", DataNascimento = "1990-01-01", TipoCliente = TipoCliente.PessoaFisica }
            }.AsQueryable();

            var especificacao = new ClienteAtivoEspecificacao(); // Sem projeção

            // Act & Assert
            var act = () => avaliador.AplicarProjecao<string>(clientes, especificacao);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("A especificação não contém uma projeção definida.");
        }

        /// <summary>
        /// Verifica se AplicarProjecao lança exceção com tipo incompatível
        /// Importância: Valida tratamento de erro para projeções inválidas
        /// </summary>
        [Fact]
        public void AplicarProjecao_ComTipoIncompativel_DeveLancarExcecao()
        {
            // Arrange
            var avaliador = new AvaliadorDeEspecificacao<ClienteEntityDto>();
            var clientes = new List<ClienteEntityDto>
            {
                new ClienteEntityDto 
                { 
                    Id = Guid.NewGuid(), 
                    Nome = "Cliente Teste", 
                    Ativo = true, 
                    Documento = "123", 
                    DataNascimento = "1990-01-01", 
                    TipoCliente = TipoCliente.PessoaFisica 
                }
            }.AsQueryable();

            // Criar mock de especificação com projeção incompatível
            var especificacaoMock = Substitute.For<IEspecificacao<ClienteEntityDto>>();
            especificacaoMock.UsarProjecao.Returns(true);
            especificacaoMock.ObterProjecao().Returns((Expression<Func<ClienteEntityDto, string>>?)null);

            // Act & Assert
            var act = () => avaliador.AplicarProjecao<int>(clientes, especificacaoMock);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Tipo de projeção incompatível.");
        }

        /// <summary>
        /// Verifica se AplicarPaginacao na primeira página funciona
        /// Importância: Valida caso comum de paginação (página 0)
        /// </summary>
        [Fact]
        public void AplicarPaginacao_PrimeiraPagina_DeveRetornarPrimeirosItens()
        {
            // Arrange
            var avaliador = new AvaliadorDeEspecificacao<ClienteEntityDto>();
            var clientes = Enumerable.Range(1, 10).Select(i => new ClienteEntityDto
            {
                Id = Guid.NewGuid(),
                Nome = $"Cliente {i}",
                Ativo = true,
                Documento = $"{i}",
                DataNascimento = "1990-01-01",
                TipoCliente = TipoCliente.PessoaFisica
            }).AsQueryable();

            var especificacao = new ClientePaginadoEspecificacao(pagina: 0, tamanho: 5); // Primeira página

            // Act
            var resultado = avaliador.AplicarPaginacao(clientes, especificacao);

            // Assert
            resultado.Should().HaveCount(5);
            resultado.First().Nome.Should().Be("Cliente 1");
            resultado.Last().Nome.Should().Be("Cliente 5");
        }
    }
}
