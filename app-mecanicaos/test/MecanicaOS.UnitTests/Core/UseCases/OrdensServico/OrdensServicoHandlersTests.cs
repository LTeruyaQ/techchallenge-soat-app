using Core.DTOs.UseCases.OrdemServico;
using Core.Entidades;
using Core.Enumeradores;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.UseCases.OrdensServico.AceitarOrcamento;
using Core.UseCases.OrdensServico.AtualizarOrdemServico;
using Core.UseCases.OrdensServico.CadastrarOrdemServico;
using Core.UseCases.OrdensServico.ObterOrdemServico;
using Core.UseCases.OrdensServico.ObterOrdemServicoPorStatus;
using Core.UseCases.OrdensServico.ObterTodosOrdensServico;
using Core.UseCases.OrdensServico.RecusarOrcamento;

namespace MecanicaOS.UnitTests.Core.UseCases.OrdensServico
{
    /// <summary>
    /// Testes para handlers de OrdemServico
    /// ROI CRÍTICO: OrdemServico é o core do negócio - gestão de status e orçamentos.
    /// Importância: Workflow complexo com múltiplos status e regras de transição.
    /// </summary>
    public class OrdensServicoHandlersTests
    {
        private readonly IOrdemServicoGateway _ordemServicoGateway;
        private readonly IEventoGateway _eventoGateway;
        private readonly ILogGateway<CadastrarOrdemServicoHandler> _logGateway;
        private readonly IUnidadeDeTrabalhoGateway _udtGateway;
        private readonly IUsuarioLogadoServicoGateway _usuarioLogadoGateway;

        public OrdensServicoHandlersTests()
        {
            _ordemServicoGateway = Substitute.For<IOrdemServicoGateway>();
            _eventoGateway = Substitute.For<IEventoGateway>();
            _logGateway = Substitute.For<ILogGateway<CadastrarOrdemServicoHandler>>();
            _udtGateway = Substitute.For<IUnidadeDeTrabalhoGateway>();
            _usuarioLogadoGateway = Substitute.For<IUsuarioLogadoServicoGateway>();
            _udtGateway.Commit().Returns(Task.FromResult(true));
        }

        #region CadastrarOrdemServicoHandler

        /// <summary>
        /// Verifica se CadastrarOrdemServico cadastra ordem com sucesso
        /// Importância: CRÍTICA - Cadastro de ordens é operação fundamental
        /// Contribuição: Garante que novas ordens podem ser criadas
        /// </summary>
        [Fact]
        public async Task CadastrarOrdemServico_ComDadosValidos_DeveCadastrarComSucesso()
        {
            // Arrange
            var handler = new CadastrarOrdemServicoHandler(_ordemServicoGateway, _logGateway, _udtGateway, _usuarioLogadoGateway);
            var request = new CadastrarOrdemServicoUseCaseDto
            {
                Cliente = new Cliente { Id = Guid.NewGuid(), Nome = "Cliente Teste", Documento = "123", TipoCliente = TipoCliente.PessoaFisica },
                Servico = new Servico { Id = Guid.NewGuid(), Nome = "Troca de Óleo", Descricao = "Óleo", Valor = 150m, Disponivel = true },
                VeiculoId = Guid.NewGuid(),
                Descricao = "Troca de óleo do motor"
            };

            _ordemServicoGateway.CadastrarAsync(Arg.Any<OrdemServico>()).Returns(Task.FromResult(new OrdemServico
            {
                Id = Guid.NewGuid(),
                ClienteId = request.Cliente.Id,
                ServicoId = request.Servico.Id,
                VeiculoId = request.VeiculoId,
                Descricao = request.Descricao,
                Status = StatusOrdemServico.AguardandoAprovacao
            }));

            // Act
            var resultado = await handler.Handle(request);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Status.Should().Be(StatusOrdemServico.Recebida);
            await _ordemServicoGateway.Received(1).CadastrarAsync(Arg.Any<OrdemServico>());
            await _udtGateway.Received(1).Commit();
        }

        #endregion

        #region AtualizarOrdemServicoHandler

        /// <summary>
        /// Verifica se AtualizarOrdemServico atualiza ordem com sucesso
        /// Importância: ALTA - Atualização de status é operação frequente
        /// Contribuição: Garante que ordens podem ser atualizadas
        /// </summary>
        [Fact]
        public async Task AtualizarOrdemServico_ComDadosValidos_DeveAtualizarComSucesso()
        {
            // Arrange
            var logGatewayAtualizar = Substitute.For<ILogGateway<AtualizarOrdemServicoHandler>>();
            var handler = new AtualizarOrdemServicoHandler(_ordemServicoGateway, _eventoGateway, logGatewayAtualizar, _udtGateway, _usuarioLogadoGateway);
            var ordemId = Guid.NewGuid();
            var ordemExistente = new OrdemServico
            {
                Id = ordemId,
                ClienteId = Guid.NewGuid(),
                ServicoId = Guid.NewGuid(),
                VeiculoId = Guid.NewGuid(),
                Status = StatusOrdemServico.AguardandoAprovacao,
                Descricao = "Descrição original"
            };
            var request = new AtualizarOrdemServicoUseCaseDto
            {
                Status = StatusOrdemServico.EmExecucao,
                Descricao = "Descrição atualizada"
            };

            _ordemServicoGateway.ObterPorIdAsync(ordemId).Returns(Task.FromResult<OrdemServico?>(ordemExistente));
            _ordemServicoGateway.EditarAsync(Arg.Any<OrdemServico>()).Returns(Task.CompletedTask);

            // Act
            var resultado = await handler.Handle(ordemId, request);

            // Assert
            resultado.Should().NotBeNull();
            await _ordemServicoGateway.Received(1).EditarAsync(Arg.Any<OrdemServico>());
            await _udtGateway.Received(1).Commit();
        }

        /// <summary>
        /// Verifica se AtualizarOrdemServico lança exceção para ordem inexistente
        /// Importância: ALTA - Previne atualização de registros inválidos
        /// Contribuição: Garante integridade dos dados
        /// </summary>
        [Fact]
        public async Task AtualizarOrdemServico_ComOrdemInexistente_DeveLancarExcecao()
        {
            // Arrange
            var logGatewayAtualizar = Substitute.For<ILogGateway<AtualizarOrdemServicoHandler>>();
            var handler = new AtualizarOrdemServicoHandler(_ordemServicoGateway, _eventoGateway,logGatewayAtualizar, _udtGateway, _usuarioLogadoGateway);
            var ordemId = Guid.NewGuid();
            var request = new AtualizarOrdemServicoUseCaseDto { Status = StatusOrdemServico.EmExecucao };

            _ordemServicoGateway.ObterPorIdAsync(ordemId).Returns(Task.FromResult<OrdemServico?>(null));

            // Act & Assert
            await handler.Invoking(h => h.Handle(ordemId, request))
                .Should().ThrowAsync<DadosNaoEncontradosException>()
                .WithMessage("Ordem de serviço não encontrada");
        }

        #endregion

        #region ObterOrdemServicoHandler

        /// <summary>
        /// Verifica se ObterOrdemServico retorna ordem existente
        /// Importância: ALTA - Consulta de ordem é operação frequente
        /// Contribuição: Permite verificar detalhes de ordens
        /// </summary>
        [Fact]
        public async Task ObterOrdemServico_ComIdExistente_DeveRetornarOrdem()
        {
            // Arrange
            var logGatewayObter = Substitute.For<ILogGateway<ObterOrdemServicoHandler>>();
            var handler = new ObterOrdemServicoHandler(_ordemServicoGateway, logGatewayObter, _udtGateway, _usuarioLogadoGateway);
            var ordemId = Guid.NewGuid();
            var ordem = new OrdemServico
            {
                Id = ordemId,
                ClienteId = Guid.NewGuid(),
                ServicoId = Guid.NewGuid(),
                VeiculoId = Guid.NewGuid(),
                Status = StatusOrdemServico.AguardandoAprovacao,
                Descricao = "Troca de óleo"
            };

            _ordemServicoGateway.ObterPorIdAsync(ordemId).Returns(Task.FromResult<OrdemServico?>(ordem));

            // Act
            var resultado = await handler.Handle(ordemId);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(ordemId);
            resultado.Status.Should().Be(StatusOrdemServico.AguardandoAprovacao);
        }

        /// <summary>
        /// Verifica se ObterOrdemServico retorna null para ID inexistente
        /// Importância: MÉDIA - Comportamento esperado
        /// Contribuição: Permite tratamento adequado de ordem não encontrada
        /// </summary>
        [Fact]
        public async Task ObterOrdemServico_ComIdInexistente_DeveRetornarNull()
        {
            // Arrange
            var logGatewayObter = Substitute.For<ILogGateway<ObterOrdemServicoHandler>>();
            var handler = new ObterOrdemServicoHandler(_ordemServicoGateway, logGatewayObter, _udtGateway, _usuarioLogadoGateway);
            var ordemId = Guid.NewGuid();

            _ordemServicoGateway.ObterPorIdAsync(ordemId).Returns(Task.FromResult<OrdemServico?>(null));

            // Act
            var resultado = await handler.Handle(ordemId);

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task ObterOrdemServico_QuandoGatewayLancaExcecao_DevePropagar()
        {
            // Arrange
            var logGatewayObter = Substitute.For<ILogGateway<ObterOrdemServicoHandler>>();
            var handler = new ObterOrdemServicoHandler(_ordemServicoGateway, logGatewayObter, _udtGateway, _usuarioLogadoGateway);
            var ordemId = Guid.NewGuid();
            _ordemServicoGateway.ObterPorIdAsync(ordemId).Returns(Task.FromException<OrdemServico?>(new InvalidOperationException("Erro no banco")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await handler.Handle(ordemId));
        }

        #endregion

        #region ObterTodosOrdensServicoHandler

        /// <summary>
        /// Verifica se ObterTodosOrdensServico retorna lista completa
        /// Importância: ALTA - Listagem de ordens é operação comum
        /// Contribuição: Permite visualização completa das ordens
        /// </summary>
        [Fact]
        public async Task ObterTodosOrdensServico_ComVariasOrdens_DeveRetornarListaCompleta()
        {
            // Arrange
            var logGatewayObterTodos = Substitute.For<ILogGateway<ObterTodosOrdensServicoHandler>>();
            var handler = new ObterTodosOrdensServicoHandler(_ordemServicoGateway, logGatewayObterTodos, _udtGateway, _usuarioLogadoGateway);
            var ordens = new List<OrdemServico>
            {
                new OrdemServico { Id = Guid.NewGuid(), ClienteId = Guid.NewGuid(), ServicoId = Guid.NewGuid(), VeiculoId = Guid.NewGuid(), Status = StatusOrdemServico.AguardandoAprovacao },
                new OrdemServico { Id = Guid.NewGuid(), ClienteId = Guid.NewGuid(), ServicoId = Guid.NewGuid(), VeiculoId = Guid.NewGuid(), Status = StatusOrdemServico.EmExecucao },
                new OrdemServico { Id = Guid.NewGuid(), ClienteId = Guid.NewGuid(), ServicoId = Guid.NewGuid(), VeiculoId = Guid.NewGuid(), Status = StatusOrdemServico.Finalizada }
            };

            _ordemServicoGateway.ObterTodosAsync().Returns(Task.FromResult<IEnumerable<OrdemServico>>(ordens));

            // Act
            var resultado = await handler.Handle();

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ObterTodosOrdensServico_QuandoGatewayLancaExcecao_DevePropagar()
        {
            // Arrange
            var logGatewayObterTodos = Substitute.For<ILogGateway<ObterTodosOrdensServicoHandler>>();
            var handler = new ObterTodosOrdensServicoHandler(_ordemServicoGateway, logGatewayObterTodos, _udtGateway, _usuarioLogadoGateway);
            _ordemServicoGateway.ObterTodosAsync().Returns(Task.FromException<IEnumerable<OrdemServico>>(new InvalidOperationException("Erro no banco")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await handler.Handle());
        }

        #endregion

        #region ObterOrdemServicoPorStatusHandler

        /// <summary>
        /// Verifica se ObterOrdemServicoPorStatus filtra corretamente por status
        /// Importância: CRÍTICA - Filtro por status é essencial para gestão
        /// Contribuição: Permite visualizar ordens em cada etapa do workflow
        /// </summary>
        [Theory]
        [InlineData(StatusOrdemServico.AguardandoAprovacao)]
        [InlineData(StatusOrdemServico.EmExecucao)]
        [InlineData(StatusOrdemServico.Finalizada)]
        [InlineData(StatusOrdemServico.Cancelada)]
        public async Task ObterOrdemServicoPorStatus_ComStatusValido_DeveRetornarOrdensFiltradas(StatusOrdemServico status)
        {
            // Arrange
            var logGatewayObterPorStatus = Substitute.For<ILogGateway<ObterOrdemServicoPorStatusHandler>>();
            var handler = new ObterOrdemServicoPorStatusHandler(_ordemServicoGateway, logGatewayObterPorStatus, _udtGateway, _usuarioLogadoGateway);
            var ordens = new List<OrdemServico>
            {
                new OrdemServico { Id = Guid.NewGuid(), ClienteId = Guid.NewGuid(), ServicoId = Guid.NewGuid(), VeiculoId = Guid.NewGuid(), Status = status },
                new OrdemServico { Id = Guid.NewGuid(), ClienteId = Guid.NewGuid(), ServicoId = Guid.NewGuid(), VeiculoId = Guid.NewGuid(), Status = status }
            };

            _ordemServicoGateway.ObterOrdemServicoPorStatusAsync(status).Returns(Task.FromResult<IEnumerable<OrdemServico>>(ordens));

            // Act
            var resultado = await handler.Handle(status);

            // Assert
            resultado.Should().HaveCount(2);
            resultado.Should().AllSatisfy(o => o.Status.Should().Be(status));
        }

        [Fact]
        public async Task ObterOrdemServicoPorStatus_QuandoGatewayLancaExcecao_DevePropagar()
        {
            // Arrange
            var logGatewayObterPorStatus = Substitute.For<ILogGateway<ObterOrdemServicoPorStatusHandler>>();
            var handler = new ObterOrdemServicoPorStatusHandler(_ordemServicoGateway, logGatewayObterPorStatus, _udtGateway, _usuarioLogadoGateway);
            _ordemServicoGateway.ObterOrdemServicoPorStatusAsync(StatusOrdemServico.Recebida)
                .Returns(Task.FromException<IEnumerable<OrdemServico>>(new InvalidOperationException("Erro no banco")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await handler.Handle(StatusOrdemServico.Recebida));
        }

        #endregion

        #region AceitarOrcamentoHandler

        /// <summary>
        /// Verifica se AceitarOrcamento muda status para EmExecucao
        /// Importância: CRÍTICA - Transição de status é regra de negócio fundamental
        /// Contribuição: Garante que workflow de aprovação funciona corretamente
        /// </summary>
        [Fact]
        public async Task AceitarOrcamento_ComOrdemValida_DeveMudarStatusParaEmExecucao()
        {
            // Arrange
            var logGatewayAceitar = Substitute.For<ILogGateway<AceitarOrcamentoHandler>>();
            var handler = new AceitarOrcamentoHandler(_ordemServicoGateway, logGatewayAceitar, _udtGateway, _usuarioLogadoGateway);
            var ordemId = Guid.NewGuid();
            var ordem = new OrdemServico
            {
                Id = ordemId,
                ClienteId = Guid.NewGuid(),
                ServicoId = Guid.NewGuid(),
                VeiculoId = Guid.NewGuid(),
                Status = StatusOrdemServico.AguardandoAprovacao
            };

            _ordemServicoGateway.ObterPorIdAsync(ordemId).Returns(Task.FromResult<OrdemServico?>(ordem));
            _ordemServicoGateway.EditarAsync(Arg.Any<OrdemServico>()).Returns(Task.CompletedTask);

            // Act
            await handler.Handle(ordemId);

            // Assert
            await _ordemServicoGateway.Received(1).EditarAsync(Arg.Is<OrdemServico>(o => o.Status == StatusOrdemServico.EmExecucao));
            await _udtGateway.Received(1).Commit();
        }

        [Fact]
        public async Task AceitarOrcamento_QuandoGatewayLancaExcecao_DevePropagar()
        {
            // Arrange
            var logGatewayAceitar = Substitute.For<ILogGateway<AceitarOrcamentoHandler>>();
            var handler = new AceitarOrcamentoHandler(_ordemServicoGateway, logGatewayAceitar, _udtGateway, _usuarioLogadoGateway);
            var ordemId = Guid.NewGuid();
            _ordemServicoGateway.ObterPorIdAsync(ordemId).Returns(Task.FromException<OrdemServico?>(new InvalidOperationException("Erro no banco")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await handler.Handle(ordemId));
        }

        #endregion

        #region RecusarOrcamentoHandler

        /// <summary>
        /// Verifica se RecusarOrcamento muda status para Cancelada
        /// Importância: CRÍTICA - Transição de status é regra de negócio fundamental
        /// Contribuição: Garante que workflow de recusa funciona corretamente
        /// </summary>
        [Fact]
        public async Task RecusarOrcamento_ComOrdemValida_DeveMudarStatusParaCancelada()
        {
            // Arrange
            var logGatewayRecusar = Substitute.For<ILogGateway<RecusarOrcamentoHandler>>();
            var eventosGateway = Substitute.For<IEventoGateway>();
            var handler = new RecusarOrcamentoHandler(_ordemServicoGateway, eventosGateway, logGatewayRecusar, _udtGateway, _usuarioLogadoGateway);
            var ordemId = Guid.NewGuid();
            var ordem = new OrdemServico
            {
                Id = ordemId,
                ClienteId = Guid.NewGuid(),
                ServicoId = Guid.NewGuid(),
                VeiculoId = Guid.NewGuid(),
                Status = StatusOrdemServico.AguardandoAprovacao
            };

            _ordemServicoGateway.ObterPorIdAsync(ordemId).Returns(Task.FromResult<OrdemServico?>(ordem));
            _ordemServicoGateway.EditarAsync(Arg.Any<OrdemServico>()).Returns(Task.CompletedTask);

            // Act
            await handler.Handle(ordemId);

            // Assert
            await _ordemServicoGateway.Received(1).EditarAsync(Arg.Is<OrdemServico>(o => o.Status == StatusOrdemServico.Cancelada));
            await _udtGateway.Received(1).Commit();
        }

        [Fact]
        public async Task RecusarOrcamento_QuandoGatewayLancaExcecao_DevePropagar()
        {
            // Arrange
            var logGatewayRecusar = Substitute.For<ILogGateway<RecusarOrcamentoHandler>>();
            var eventosGateway = Substitute.For<IEventoGateway>();
            var handler = new RecusarOrcamentoHandler(_ordemServicoGateway, eventosGateway, logGatewayRecusar, _udtGateway, _usuarioLogadoGateway);
            var ordemId = Guid.NewGuid();
            _ordemServicoGateway.ObterPorIdAsync(ordemId).Returns(Task.FromException<OrdemServico?>(new InvalidOperationException("Erro no banco")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await handler.Handle(ordemId));
        }

        #endregion
    }
}
