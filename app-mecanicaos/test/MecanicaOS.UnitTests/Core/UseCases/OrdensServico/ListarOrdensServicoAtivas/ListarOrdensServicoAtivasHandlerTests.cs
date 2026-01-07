using Core.Entidades;
using Core.Enumeradores;
using Core.Interfaces.Gateways;
using Core.UseCases.OrdensServico.ListarOrdensServicoAtivas;

namespace MecanicaOS.UnitTests.Core.UseCases.OrdensServico.ListarOrdensServicoAtivas
{
    /// <summary>
    /// Testes para ListarOrdensServicoAtivasHandler
    /// ROI CRÍTICO: Listagem de OS ativas é operação frequente para gestão operacional
    /// Importância: Garante que apenas OS relevantes são exibidas com ordenação correta
    /// </summary>
    public class ListarOrdensServicoAtivasHandlerTests
    {
        private readonly IOrdemServicoGateway _ordemServicoGateway;
        private readonly ILogGateway<ListarOrdensServicoAtivasHandler> _logGateway;
        private readonly IUnidadeDeTrabalhoGateway _udtGateway;
        private readonly IUsuarioLogadoServicoGateway _usuarioLogadoGateway;
        private readonly ListarOrdensServicoAtivasHandler _handler;

        public ListarOrdensServicoAtivasHandlerTests()
        {
            _ordemServicoGateway = Substitute.For<IOrdemServicoGateway>();
            _logGateway = Substitute.For<ILogGateway<ListarOrdensServicoAtivasHandler>>();
            _udtGateway = Substitute.For<IUnidadeDeTrabalhoGateway>();
            _usuarioLogadoGateway = Substitute.For<IUsuarioLogadoServicoGateway>();

            _handler = new ListarOrdensServicoAtivasHandler(
                _ordemServicoGateway,
                _logGateway,
                _udtGateway,
                _usuarioLogadoGateway);
        }

        /// <summary>
        /// Verifica se retorna lista vazia quando não há ordens ativas
        /// Importância: MÉDIA - Cenário comum em sistema novo ou sem demanda
        /// Contribuição: Garante que handler não falha com lista vazia
        /// </summary>
        [Fact]
        public async Task Handle_SemOrdensAtivas_DeveRetornarListaVazia()
        {
            // Arrange
            _ordemServicoGateway.ObterOrdensServicoAtivasAsync()
                .Returns(Task.FromResult<IEnumerable<OrdemServico>>(new List<OrdemServico>()));

            // Act
            var resultado = await _handler.Handle();

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().BeEmpty();
            await _ordemServicoGateway.Received(1).ObterOrdensServicoAtivasAsync();
        }

        /// <summary>
        /// Verifica se ordena corretamente por prioridade de status
        /// Importância: CRÍTICA - Ordenação por prioridade é requisito principal
        /// Contribuição: Garante que OS mais urgentes aparecem primeiro
        /// </summary>
        [Fact]
        public async Task Handle_ComVariosStatus_DeveOrdenarPorPrioridade()
        {
            // Arrange
            var ordensServico = new List<OrdemServico>
            {
                new OrdemServico { Id = Guid.NewGuid(), Status = StatusOrdemServico.Recebida, DataCadastro = DateTime.Now.AddDays(-1) },
                new OrdemServico { Id = Guid.NewGuid(), Status = StatusOrdemServico.EmExecucao, DataCadastro = DateTime.Now.AddDays(-2) },
                new OrdemServico { Id = Guid.NewGuid(), Status = StatusOrdemServico.AguardandoAprovacao, DataCadastro = DateTime.Now.AddDays(-3) },
                new OrdemServico { Id = Guid.NewGuid(), Status = StatusOrdemServico.EmDiagnostico, DataCadastro = DateTime.Now.AddDays(-4) }
            };

            _ordemServicoGateway.ObterOrdensServicoAtivasAsync()
                .Returns(Task.FromResult<IEnumerable<OrdemServico>>(ordensServico));

            // Act
            var resultado = await _handler.Handle();

            // Assert
            var lista = resultado.ToList();
            lista.Should().HaveCount(4);
            lista[0].Status.Should().Be(StatusOrdemServico.EmExecucao); // Prioridade 1
            lista[1].Status.Should().Be(StatusOrdemServico.AguardandoAprovacao); // Prioridade 2
            lista[2].Status.Should().Be(StatusOrdemServico.EmDiagnostico); // Prioridade 3
            lista[3].Status.Should().Be(StatusOrdemServico.Recebida); // Prioridade 4
        }

        /// <summary>
        /// Verifica se ordena por data quando status é igual
        /// Importância: ALTA - Ordenação secundária por data é requisito
        /// Contribuição: Garante que OS mais antigas são priorizadas
        /// </summary>
        [Fact]
        public async Task Handle_ComMesmoStatus_DeveOrdenarPorDataMaisAntigaPrimeiro()
        {
            // Arrange
            var dataBase = DateTime.Now;
            var ordensServico = new List<OrdemServico>
            {
                new OrdemServico { Id = Guid.NewGuid(), Status = StatusOrdemServico.EmExecucao, DataCadastro = dataBase.AddDays(-1) },
                new OrdemServico { Id = Guid.NewGuid(), Status = StatusOrdemServico.EmExecucao, DataCadastro = dataBase.AddDays(-5) },
                new OrdemServico { Id = Guid.NewGuid(), Status = StatusOrdemServico.EmExecucao, DataCadastro = dataBase.AddDays(-3) }
            };

            _ordemServicoGateway.ObterOrdensServicoAtivasAsync()
                .Returns(Task.FromResult<IEnumerable<OrdemServico>>(ordensServico));

            // Act
            var resultado = await _handler.Handle();

            // Assert
            var lista = resultado.ToList();
            lista.Should().HaveCount(3);
            lista[0].DataCadastro.Should().Be(dataBase.AddDays(-5)); // Mais antiga
            lista[1].DataCadastro.Should().Be(dataBase.AddDays(-3));
            lista[2].DataCadastro.Should().Be(dataBase.AddDays(-1)); // Mais recente
        }

        /// <summary>
        /// Verifica ordenação completa: prioridade + data
        /// Importância: CRÍTICA - Testa requisito completo de ordenação
        /// Contribuição: Garante comportamento correto em cenário real
        /// </summary>
        [Fact]
        public async Task Handle_ComStatusEDatasDiferentes_DeveOrdenarCorretamente()
        {
            // Arrange
            var dataBase = DateTime.Now;
            var ordensServico = new List<OrdemServico>
            {
                new OrdemServico { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Status = StatusOrdemServico.Recebida, DataCadastro = dataBase.AddDays(-10) },
                new OrdemServico { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), Status = StatusOrdemServico.EmExecucao, DataCadastro = dataBase.AddDays(-2) },
                new OrdemServico { Id = Guid.Parse("00000000-0000-0000-0000-000000000003"), Status = StatusOrdemServico.EmExecucao, DataCadastro = dataBase.AddDays(-5) },
                new OrdemServico { Id = Guid.Parse("00000000-0000-0000-0000-000000000004"), Status = StatusOrdemServico.AguardandoAprovacao, DataCadastro = dataBase.AddDays(-8) },
                new OrdemServico { Id = Guid.Parse("00000000-0000-0000-0000-000000000005"), Status = StatusOrdemServico.AguardandoAprovacao, DataCadastro = dataBase.AddDays(-3) },
                new OrdemServico { Id = Guid.Parse("00000000-0000-0000-0000-000000000006"), Status = StatusOrdemServico.EmDiagnostico, DataCadastro = dataBase.AddDays(-7) },
                new OrdemServico { Id = Guid.Parse("00000000-0000-0000-0000-000000000007"), Status = StatusOrdemServico.Recebida, DataCadastro = dataBase.AddDays(-1) }
            };

            _ordemServicoGateway.ObterOrdensServicoAtivasAsync()
                .Returns(Task.FromResult<IEnumerable<OrdemServico>>(ordensServico));

            // Act
            var resultado = await _handler.Handle();

            // Assert
            var lista = resultado.ToList();
            lista.Should().HaveCount(7);
            
            // EmExecucao (prioridade 1) - mais antigas primeiro
            lista[0].Id.Should().Be(Guid.Parse("00000000-0000-0000-0000-000000000003")); // EmExecucao -5 dias
            lista[1].Id.Should().Be(Guid.Parse("00000000-0000-0000-0000-000000000002")); // EmExecucao -2 dias
            
            // AguardandoAprovacao (prioridade 2) - mais antigas primeiro
            lista[2].Id.Should().Be(Guid.Parse("00000000-0000-0000-0000-000000000004")); // AguardandoAprovacao -8 dias
            lista[3].Id.Should().Be(Guid.Parse("00000000-0000-0000-0000-000000000005")); // AguardandoAprovacao -3 dias
            
            // EmDiagnostico (prioridade 3)
            lista[4].Id.Should().Be(Guid.Parse("00000000-0000-0000-0000-000000000006")); // EmDiagnostico -7 dias
            
            // Recebida (prioridade 4) - mais antigas primeiro
            lista[5].Id.Should().Be(Guid.Parse("00000000-0000-0000-0000-000000000001")); // Recebida -10 dias
            lista[6].Id.Should().Be(Guid.Parse("00000000-0000-0000-0000-000000000007")); // Recebida -1 dia
        }

        /// <summary>
        /// Verifica que gateway é chamado corretamente
        /// Importância: MÉDIA - Validação de integração com gateway
        /// Contribuição: Garante que especificação é aplicada corretamente
        /// </summary>
        [Fact]
        public async Task Handle_DeveChamarGatewayComMetodoCorreto()
        {
            // Arrange
            _ordemServicoGateway.ObterOrdensServicoAtivasAsync()
                .Returns(Task.FromResult<IEnumerable<OrdemServico>>(new List<OrdemServico>()));

            // Act
            await _handler.Handle();

            // Assert
            await _ordemServicoGateway.Received(1).ObterOrdensServicoAtivasAsync();
        }

        /// <summary>
        /// Verifica que apenas status ativos são retornados
        /// Importância: CRÍTICA - Validação do filtro principal
        /// Contribuição: Garante que especificação filtra corretamente
        /// </summary>
        [Fact]
        public async Task Handle_DeveRetornarApenasStatusAtivos()
        {
            // Arrange
            var ordensServico = new List<OrdemServico>
            {
                new OrdemServico { Id = Guid.NewGuid(), Status = StatusOrdemServico.EmExecucao, DataCadastro = DateTime.Now },
                new OrdemServico { Id = Guid.NewGuid(), Status = StatusOrdemServico.AguardandoAprovacao, DataCadastro = DateTime.Now },
                new OrdemServico { Id = Guid.NewGuid(), Status = StatusOrdemServico.EmDiagnostico, DataCadastro = DateTime.Now },
                new OrdemServico { Id = Guid.NewGuid(), Status = StatusOrdemServico.Recebida, DataCadastro = DateTime.Now }
            };

            _ordemServicoGateway.ObterOrdensServicoAtivasAsync()
                .Returns(Task.FromResult<IEnumerable<OrdemServico>>(ordensServico));

            // Act
            var resultado = await _handler.Handle();

            // Assert
            resultado.Should().HaveCount(4);
            resultado.Should().OnlyContain(os => 
                os.Status == StatusOrdemServico.EmExecucao ||
                os.Status == StatusOrdemServico.AguardandoAprovacao ||
                os.Status == StatusOrdemServico.EmDiagnostico ||
                os.Status == StatusOrdemServico.Recebida);
            
            // Verifica que NÃO contém status inativos
            resultado.Should().NotContain(os => os.Status == StatusOrdemServico.Finalizada);
            resultado.Should().NotContain(os => os.Status == StatusOrdemServico.Entregue);
            resultado.Should().NotContain(os => os.Status == StatusOrdemServico.Cancelada);
            resultado.Should().NotContain(os => os.Status == StatusOrdemServico.OrcamentoExpirado);
        }

        /// <summary>
        /// Verifica comportamento com apenas uma ordem
        /// Importância: BAIXA - Caso edge simples
        /// Contribuição: Garante robustez em cenários mínimos
        /// </summary>
        [Fact]
        public async Task Handle_ComApenasUmaOrdem_DeveRetornarCorretamente()
        {
            // Arrange
            var ordem = new OrdemServico 
            { 
                Id = Guid.NewGuid(), 
                Status = StatusOrdemServico.EmExecucao, 
                DataCadastro = DateTime.Now 
            };

            _ordemServicoGateway.ObterOrdensServicoAtivasAsync()
                .Returns(Task.FromResult<IEnumerable<OrdemServico>>(new List<OrdemServico> { ordem }));

            // Act
            var resultado = await _handler.Handle();

            // Assert
            resultado.Should().HaveCount(1);
            resultado.First().Id.Should().Be(ordem.Id);
        }

        /// <summary>
        /// Verifica que prioridades estão corretas conforme especificação
        /// Importância: CRÍTICA - Validação das regras de negócio
        /// Contribuição: Documenta e valida ordem de prioridade
        /// </summary>
        [Theory]
        [InlineData(StatusOrdemServico.EmExecucao, 1)]
        [InlineData(StatusOrdemServico.AguardandoAprovacao, 2)]
        [InlineData(StatusOrdemServico.EmDiagnostico, 3)]
        [InlineData(StatusOrdemServico.Recebida, 4)]
        public async Task Handle_DeveRespeitarPrioridadeDeStatus(StatusOrdemServico status, int posicaoEsperada)
        {
            // Arrange
            var ordensServico = new List<OrdemServico>
            {
                new OrdemServico { Id = Guid.NewGuid(), Status = StatusOrdemServico.Recebida, DataCadastro = DateTime.Now },
                new OrdemServico { Id = Guid.NewGuid(), Status = StatusOrdemServico.EmDiagnostico, DataCadastro = DateTime.Now },
                new OrdemServico { Id = Guid.NewGuid(), Status = StatusOrdemServico.AguardandoAprovacao, DataCadastro = DateTime.Now },
                new OrdemServico { Id = Guid.NewGuid(), Status = StatusOrdemServico.EmExecucao, DataCadastro = DateTime.Now }
            };

            _ordemServicoGateway.ObterOrdensServicoAtivasAsync()
                .Returns(Task.FromResult<IEnumerable<OrdemServico>>(ordensServico));

            // Act
            var resultado = await _handler.Handle();

            // Assert
            var lista = resultado.ToList();
            var ordemComStatus = lista.First(o => o.Status == status);
            var posicaoReal = lista.IndexOf(ordemComStatus) + 1;
            posicaoReal.Should().Be(posicaoEsperada, 
                $"Status {status} deve ter prioridade {posicaoEsperada}");
        }
    }
}
