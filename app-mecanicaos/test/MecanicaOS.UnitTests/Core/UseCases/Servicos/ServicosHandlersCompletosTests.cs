using Core.DTOs.UseCases.Servico;
using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.UseCases.Servicos.CadastrarServico;
using Core.UseCases.Servicos.DeletarServico;
using Core.UseCases.Servicos.EditarServico;
using Core.UseCases.Servicos.ObterServico;
using Core.UseCases.Servicos.ObterServicoPorNome;
using Core.UseCases.Servicos.ObterServicosDisponiveis;
using Core.UseCases.Servicos.ObterTodosServicos;

namespace MecanicaOS.UnitTests.Core.UseCases.Servicos
{
    /// <summary>
    /// Testes completos para todos os handlers de Serviços
    /// ROI CRÍTICO: Catálogo de serviços é base para orçamentos e ordens.
    /// Importância: Precificação correta impacta receita da oficina.
    /// </summary>
    public class ServicosHandlersCompletosTests
    {
        private readonly IServicoGateway _servicoGateway;
        private readonly ILogGateway<CadastrarServicoHandler> _logGateway;
        private readonly IUnidadeDeTrabalhoGateway _udtGateway;
        private readonly IUsuarioLogadoServicoGateway _usuarioLogadoGateway;

        public ServicosHandlersCompletosTests()
        {
            _servicoGateway = Substitute.For<IServicoGateway>();
            _logGateway = Substitute.For<ILogGateway<CadastrarServicoHandler>>();
            _udtGateway = Substitute.For<IUnidadeDeTrabalhoGateway>();
            _usuarioLogadoGateway = Substitute.For<IUsuarioLogadoServicoGateway>();
            _udtGateway.Commit().Returns(Task.FromResult(true));
        }

        #region CadastrarServicoHandler

        /// <summary>
        /// Verifica se CadastrarServico cadastra serviço com sucesso
        /// Importância: ALTA - Cadastro de serviços é operação fundamental
        /// Contribuição: Garante que novos serviços podem ser adicionados ao catálogo
        /// </summary>
        [Fact]
        public async Task CadastrarServico_ComDadosValidos_DeveCadastrarComSucesso()
        {
            // Arrange
            var handler = new CadastrarServicoHandler(_servicoGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var request = new CadastrarServicoUseCaseDto
            {
                Nome = "Troca de Óleo",
                Descricao = "Troca de óleo do motor",
                Valor = 150.00m,
                Disponivel = true
            };

            _servicoGateway.CadastrarAsync(Arg.Any<Servico>()).Returns(Task.FromResult(new Servico
            {
                Id = Guid.NewGuid(),
                Nome = request.Nome,
                Descricao = request.Descricao,
                Valor = request.Valor,
                Disponivel = request.Disponivel
            }));

            // Act
            var resultado = await handler.Handle(request);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Nome.Should().Be("Troca de Óleo");
            resultado.Valor.Should().Be(150.00m);
            await _servicoGateway.Received(1).CadastrarAsync(Arg.Any<Servico>());
            await _udtGateway.Received(1).Commit();
        }

        /// <summary>
        /// Verifica se CadastrarServico lança exceção quando commit falha
        /// Importância: ALTA - Valida tratamento de erro de persistência
        /// Contribuição: Garante que falhas são tratadas adequadamente
        /// </summary>
        [Fact]
        public async Task CadastrarServico_ComFalhaNoCommit_DeveLancarExcecao()
        {
            // Arrange
            var handler = new CadastrarServicoHandler(_servicoGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var request = new CadastrarServicoUseCaseDto
            {
                Nome = "Alinhamento",
                Descricao = "Alinhamento de rodas",
                Valor = 80.00m,
                Disponivel = true
            };

            _servicoGateway.CadastrarAsync(Arg.Any<Servico>()).Returns(Task.FromResult(new Servico
            {
                Nome = "Teste",
                Descricao = "Teste"
            }));
            _udtGateway.Commit().Returns(Task.FromResult(false));

            // Act & Assert
            await handler.Invoking(h => h.Handle(request))
                .Should().ThrowAsync<PersistirDadosException>()
                .WithMessage("Erro ao cadastrar serviço");
        }

        #endregion

        #region EditarServicoHandler

        /// <summary>
        /// Verifica se EditarServico atualiza serviço com sucesso
        /// Importância: ALTA - Atualização de preços é operação frequente
        /// Contribuição: Garante que serviços podem ser ajustados conforme necessário
        /// </summary>
        [Fact]
        public async Task EditarServico_ComDadosValidos_DeveAtualizarComSucesso()
        {
            // Arrange
            var logGatewayEditar = Substitute.For<ILogGateway<EditarServicoHandler>>();
            var handler = new EditarServicoHandler(_servicoGateway, logGatewayEditar, _udtGateway, _usuarioLogadoGateway);
            var servicoId = Guid.NewGuid();
            var servicoExistente = new Servico
            {
                Id = servicoId,
                Nome = "Troca de Óleo",
                Descricao = "Troca de óleo do motor",
                Valor = 150.00m,
                Disponivel = true
            };
            var request = new EditarServicoUseCaseDto
            {
                Nome = "Troca de Óleo",
                Descricao = "Troca de óleo do motor",
                Valor = 180.00m,
                Disponivel = true
            };

            _servicoGateway.ObterPorIdAsync(servicoId).Returns(Task.FromResult<Servico?>(servicoExistente));
            _servicoGateway.EditarAsync(Arg.Any<Servico>()).Returns(Task.CompletedTask);

            // Act
            var resultado = await handler.Handle(servicoId, request);

            // Assert
            resultado.Should().NotBeNull();
            await _servicoGateway.Received(1).EditarAsync(Arg.Any<Servico>());
            await _udtGateway.Received(1).Commit();
        }

        /// <summary>
        /// Verifica se EditarServico lança exceção para serviço inexistente
        /// Importância: ALTA - Previne atualização de registros inválidos
        /// Contribuição: Garante integridade dos dados
        /// </summary>
        [Fact]
        public async Task EditarServico_ComServicoInexistente_DeveLancarExcecao()
        {
            // Arrange
            var logGatewayEditar = Substitute.For<ILogGateway<EditarServicoHandler>>();
            var handler = new EditarServicoHandler(_servicoGateway, logGatewayEditar, _udtGateway, _usuarioLogadoGateway);
            var servicoId = Guid.NewGuid();
            var request = new EditarServicoUseCaseDto 
            { 
                Nome = "Teste",
                Descricao = "Teste",
                Valor = 200.00m,
                Disponivel = true
            };

            _servicoGateway.ObterPorIdAsync(servicoId).Returns(Task.FromResult<Servico?>(null));

            // Act & Assert
            await handler.Invoking(h => h.Handle(servicoId, request))
                .Should().ThrowAsync<DadosNaoEncontradosException>()
                .WithMessage("Serviço não encontrado");
        }

        #endregion

        #region DeletarServicoHandler

        /// <summary>
        /// Verifica se DeletarServico remove serviço com sucesso
        /// Importância: MÉDIA - Exclusão de serviços obsoletos
        /// Contribuição: Permite limpeza do catálogo
        /// </summary>
        [Fact]
        public async Task DeletarServico_ComServicoExistente_DeveDeletarComSucesso()
        {
            // Arrange
            var logGatewayDeletar = Substitute.For<ILogGateway<DeletarServicoHandler>>();
            var handler = new DeletarServicoHandler(_servicoGateway, logGatewayDeletar, _udtGateway, _usuarioLogadoGateway);
            var servicoId = Guid.NewGuid();
            var servico = new Servico
            {
                Id = servicoId,
                Nome = "Serviço Obsoleto",
                Descricao = "Descontinuado",
                Valor = 50.00m,
                Disponivel = false
            };

            _servicoGateway.ObterPorIdAsync(servicoId).Returns(Task.FromResult<Servico?>(servico));
            _servicoGateway.DeletarAsync(Arg.Any<Servico>()).Returns(Task.CompletedTask);

            // Act
            await handler.Handle(servicoId);

            // Assert
            await _servicoGateway.Received(1).DeletarAsync(Arg.Any<Servico>());
            await _udtGateway.Received(1).Commit();
        }

        /// <summary>
        /// Verifica se DeletarServico lança exceção para serviço inexistente
        /// Importância: ALTA - Previne exclusão de registros inválidos
        /// Contribuição: Garante integridade
        /// </summary>
        [Fact]
        public async Task DeletarServico_ComServicoInexistente_DeveLancarExcecao()
        {
            // Arrange
            var logGatewayDeletar = Substitute.For<ILogGateway<DeletarServicoHandler>>();
            var handler = new DeletarServicoHandler(_servicoGateway, logGatewayDeletar, _udtGateway, _usuarioLogadoGateway);
            var servicoId = Guid.NewGuid();

            _servicoGateway.ObterPorIdAsync(servicoId).Returns(Task.FromResult<Servico?>(null));

            // Act & Assert
            await handler.Invoking(h => h.Handle(servicoId))
                .Should().ThrowAsync<DadosNaoEncontradosException>()
                .WithMessage("Serviço não encontrado");
        }

        #endregion

        #region ObterServicoHandler

        /// <summary>
        /// Verifica se ObterServico retorna serviço existente
        /// Importância: ALTA - Consulta de serviço é operação frequente
        /// Contribuição: Permite verificar detalhes de serviços
        /// </summary>
        [Fact]
        public async Task ObterServico_ComIdExistente_DeveRetornarServico()
        {
            // Arrange
            var logGatewayObter = Substitute.For<ILogGateway<ObterServicoHandler>>();
            var handler = new ObterServicoHandler(_servicoGateway, logGatewayObter, _udtGateway, _usuarioLogadoGateway);
            var servicoId = Guid.NewGuid();
            var servico = new Servico
            {
                Id = servicoId,
                Nome = "Troca de Óleo",
                Descricao = "Troca de óleo do motor",
                Valor = 150.00m,
                Disponivel = true
            };

            _servicoGateway.ObterPorIdAsync(servicoId).Returns(Task.FromResult<Servico?>(servico));

            // Act
            var resultado = await handler.Handle(servicoId);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(servicoId);
            resultado.Nome.Should().Be("Troca de Óleo");
            resultado.Valor.Should().Be(150.00m);
        }

        /// <summary>
        /// Verifica se ObterServico lança exceção para ID inexistente
        /// Importância: MÉDIA - Comportamento esperado
        /// Contribuição: Permite tratamento adequado de serviço não encontrado
        /// </summary>
        [Fact]
        public async Task ObterServico_ComIdInexistente_DeveLancarExcecao()
        {
            // Arrange
            var logGatewayObter = Substitute.For<ILogGateway<ObterServicoHandler>>();
            var handler = new ObterServicoHandler(_servicoGateway, logGatewayObter, _udtGateway, _usuarioLogadoGateway);
            var servicoId = Guid.NewGuid();

            _servicoGateway.ObterPorIdAsync(servicoId).Returns(Task.FromResult<Servico?>(null));

            // Act & Assert
            await handler.Invoking(h => h.Handle(servicoId))
                .Should().ThrowAsync<DadosNaoEncontradosException>()
                .WithMessage("Serviço não encontrado");
        }

        #endregion

        #region ObterTodosServicosHandler

        /// <summary>
        /// Verifica se ObterTodosServicos retorna lista completa
        /// Importância: ALTA - Listagem de serviços é operação comum
        /// Contribuição: Permite visualização completa do catálogo
        /// </summary>
        [Fact]
        public async Task ObterTodosServicos_ComVariosServicos_DeveRetornarListaCompleta()
        {
            // Arrange
            var logGatewayObterTodos = Substitute.For<ILogGateway<ObterTodosServicosHandler>>();
            var handler = new ObterTodosServicosHandler(_servicoGateway, logGatewayObterTodos, _udtGateway, _usuarioLogadoGateway);
            var servicos = new List<Servico>
            {
                new Servico { Id = Guid.NewGuid(), Nome = "Troca de Óleo", Descricao = "Óleo", Valor = 150m, Disponivel = true },
                new Servico { Id = Guid.NewGuid(), Nome = "Alinhamento", Descricao = "Alinhamento", Valor = 80m, Disponivel = true },
                new Servico { Id = Guid.NewGuid(), Nome = "Balanceamento", Descricao = "Balanceamento", Valor = 60m, Disponivel = false }
            };

            _servicoGateway.ObterTodosAsync().Returns(Task.FromResult<IEnumerable<Servico>>(servicos));

            // Act
            var resultado = await handler.Handle();

            // Assert
            resultado.Should().HaveCount(3);
            resultado.Should().Contain(s => s.Nome == "Troca de Óleo");
            resultado.Should().Contain(s => s.Nome == "Alinhamento");
            resultado.Should().Contain(s => s.Nome == "Balanceamento");
        }

        /// <summary>
        /// Verifica se ObterTodosServicos retorna lista vazia quando não há serviços
        /// Importância: MÉDIA - Comportamento esperado sem dados
        /// Contribuição: Previne erros em telas de listagem
        /// </summary>
        [Fact]
        public async Task ObterTodosServicos_SemServicos_DeveRetornarListaVazia()
        {
            // Arrange
            var logGatewayObterTodos = Substitute.For<ILogGateway<ObterTodosServicosHandler>>();
            var handler = new ObterTodosServicosHandler(_servicoGateway, logGatewayObterTodos, _udtGateway, _usuarioLogadoGateway);

            _servicoGateway.ObterTodosAsync().Returns(Task.FromResult<IEnumerable<Servico>>(new List<Servico>()));

            // Act
            var resultado = await handler.Handle();

            // Assert
            resultado.Should().BeEmpty();
        }

        #endregion

        #region ObterServicoPorNomeHandler

        /// <summary>
        /// Verifica se ObterServicoPorNome retorna serviço existente
        /// Importância: ALTA - Busca por nome é operação comum
        /// Contribuição: Facilita localização de serviços
        /// </summary>
        [Fact]
        public async Task ObterServicoPorNome_ComNomeExistente_DeveRetornarServico()
        {
            // Arrange
            var logGatewayObterPorNome = Substitute.For<ILogGateway<ObterServicoPorNomeHandler>>();
            var handler = new ObterServicoPorNomeHandler(_servicoGateway, logGatewayObterPorNome, _udtGateway, _usuarioLogadoGateway);
            var nome = "Troca de Óleo";
            var servico = new Servico
            {
                Id = Guid.NewGuid(),
                Nome = nome,
                Descricao = "Troca de óleo do motor",
                Valor = 150.00m,
                Disponivel = true
            };

            _servicoGateway.ObterServicosDisponiveisPorNomeAsync(nome).Returns(Task.FromResult<Servico?>(servico));

            // Act
            var resultado = await handler.Handle(nome);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Nome.Should().Be(nome);
            resultado.Disponivel.Should().BeTrue();
        }

        /// <summary>
        /// Verifica se ObterServicoPorNome retorna null para nome inexistente
        /// Importância: MÉDIA - Comportamento esperado
        /// Contribuição: Permite tratamento adequado de serviço não encontrado
        /// </summary>
        [Fact]
        public async Task ObterServicoPorNome_ComNomeInexistente_DeveRetornarNull()
        {
            // Arrange
            var logGatewayObterPorNome = Substitute.For<ILogGateway<ObterServicoPorNomeHandler>>();
            var handler = new ObterServicoPorNomeHandler(_servicoGateway, logGatewayObterPorNome, _udtGateway, _usuarioLogadoGateway);
            var nome = "Serviço Inexistente";

            _servicoGateway.ObterServicosDisponiveisPorNomeAsync(nome).Returns(Task.FromResult<Servico?>(null));

            // Act
            var resultado = await handler.Handle(nome);

            // Assert
            resultado.Should().BeNull();
        }

        #endregion

        #region ObterServicosDisponiveisHandler

        /// <summary>
        /// Verifica se ObterServicosDisponiveis retorna apenas serviços disponíveis
        /// Importância: CRÍTICA - Apenas serviços disponíveis devem ser oferecidos
        /// Contribuição: Garante que clientes veem apenas serviços ativos
        /// </summary>
        [Fact]
        public async Task ObterServicosDisponiveis_ComServicosDisponiveis_DeveRetornarApenasOsDisponiveis()
        {
            // Arrange
            var logGatewayObterDisponiveis = Substitute.For<ILogGateway<ObterServicosDisponiveisHandler>>();
            var handler = new ObterServicosDisponiveisHandler(_servicoGateway, logGatewayObterDisponiveis, _udtGateway, _usuarioLogadoGateway);
            var servicosDisponiveis = new List<Servico>
            {
                new Servico { Id = Guid.NewGuid(), Nome = "Troca de Óleo", Descricao = "Óleo", Valor = 150m, Disponivel = true },
                new Servico { Id = Guid.NewGuid(), Nome = "Alinhamento", Descricao = "Alinhamento", Valor = 80m, Disponivel = true }
            };

            _servicoGateway.ObterServicoDisponivelAsync().Returns(Task.FromResult<IEnumerable<Servico>>(servicosDisponiveis));

            // Act
            var resultado = await handler.Handle();

            // Assert
            resultado.Should().HaveCount(2);
            resultado.Should().AllSatisfy(s => s.Disponivel.Should().BeTrue());
        }

        /// <summary>
        /// Verifica se ObterServicosDisponiveis retorna vazio quando não há disponíveis
        /// Importância: MÉDIA - Comportamento esperado quando não há serviços ativos
        /// Contribuição: Permite identificar quando não há serviços para oferecer
        /// </summary>
        [Fact]
        public async Task ObterServicosDisponiveis_SemServicosDisponiveis_DeveRetornarListaVazia()
        {
            // Arrange
            var logGatewayObterDisponiveis = Substitute.For<ILogGateway<ObterServicosDisponiveisHandler>>();
            var handler = new ObterServicosDisponiveisHandler(_servicoGateway, logGatewayObterDisponiveis, _udtGateway, _usuarioLogadoGateway);

            _servicoGateway.ObterServicoDisponivelAsync().Returns(Task.FromResult<IEnumerable<Servico>>(new List<Servico>()));

            // Act
            var resultado = await handler.Handle();

            // Assert
            resultado.Should().BeEmpty();
        }

        #endregion
    }
}
