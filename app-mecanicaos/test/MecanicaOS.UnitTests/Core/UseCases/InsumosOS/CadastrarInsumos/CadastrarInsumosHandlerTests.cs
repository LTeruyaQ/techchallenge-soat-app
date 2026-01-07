using Core.DTOs.UseCases.OrdemServico.InsumoOS;
using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.UseCases.InsumosOS.CadastrarInsumos;

namespace MecanicaOS.UnitTests.Core.UseCases.InsumosOS.CadastrarInsumos
{
    /// <summary>
    /// Testes para CadastrarInsumosHandler
    /// ROI ALTO: Gestão de insumos é crítica para controle de estoque e custos.
    /// Importância: Valida cadastro de insumos em ordens de serviço e alertas de estoque crítico.
    /// </summary>
    public class CadastrarInsumosHandlerTests
    {
        private readonly ILogGateway<CadastrarInsumosHandler> _logGateway;
        private readonly IUnidadeDeTrabalhoGateway _udtGateway;
        private readonly IUsuarioLogadoServicoGateway _usuarioLogadoGateway;
        private readonly IVerificarEstoqueJobGateway _verificarEstoqueJobGateway;
        private readonly IInsumosGateway _insumosGateway;

        public CadastrarInsumosHandlerTests()
        {
            _logGateway = Substitute.For<ILogGateway<CadastrarInsumosHandler>>();
            _udtGateway = Substitute.For<IUnidadeDeTrabalhoGateway>();
            _usuarioLogadoGateway = Substitute.For<IUsuarioLogadoServicoGateway>();
            _verificarEstoqueJobGateway = Substitute.For<IVerificarEstoqueJobGateway>();
            _insumosGateway = Substitute.For<IInsumosGateway>();
            _udtGateway.Commit().Returns(Task.FromResult(true));
        }

        /// <summary>
        /// Verifica se cadastra insumos com sucesso
        /// Importância: CRÍTICA - Operação principal do handler
        /// Contribuição: Garante que insumos são cadastrados na ordem de serviço
        /// </summary>
        [Fact]
        public async Task Handle_ComInsumosValidos_DeveCadastrarComSucesso()
        {
            // Arrange
            var handler = new CadastrarInsumosHandler(_logGateway, _udtGateway, _usuarioLogadoGateway, _verificarEstoqueJobGateway, _insumosGateway);
            var ordemServicoId = Guid.NewGuid();
            var estoqueId1 = Guid.NewGuid();
            var estoqueId2 = Guid.NewGuid();
            var insumos = new List<CadastrarInsumoOSUseCaseDto>
            {
                new CadastrarInsumoOSUseCaseDto
                {
                    EstoqueId = estoqueId1,
                    Quantidade = 5,
                    Estoque = new Estoque { Id = estoqueId1, QuantidadeDisponivel = 100, QuantidadeMinima = 10 }
                },
                new CadastrarInsumoOSUseCaseDto
                {
                    EstoqueId = estoqueId2,
                    Quantidade = 3,
                    Estoque = new Estoque { Id = estoqueId2, QuantidadeDisponivel = 50, QuantidadeMinima = 5 }
                }
            };

            _insumosGateway.CadastrarVariosAsync(Arg.Any<List<InsumoOS>>()).Returns(Task.FromResult<IEnumerable<InsumoOS>>(new List<InsumoOS>()));

            // Act
            var resultado = await handler.Handle(ordemServicoId, insumos);

            // Assert
            resultado.Should().HaveCount(2);
            resultado.Should().AllSatisfy(i => i.OrdemServicoId.Should().Be(ordemServicoId));
            await _insumosGateway.Received(1).CadastrarVariosAsync(Arg.Any<List<InsumoOS>>());
            await _udtGateway.Received(1).Commit();
        }

        /// <summary>
        /// Verifica se dispara alerta quando estoque fica crítico
        /// Importância: CRÍTICA - Alertas de estoque previnem falta de insumos
        /// Contribuição: Garante que sistema notifica quando estoque está baixo
        /// </summary>
        [Fact]
        public async Task Handle_QuandoEstoqueFicaCritico_DeveDispararAlerta()
        {
            // Arrange
            var handler = new CadastrarInsumosHandler(_logGateway, _udtGateway, _usuarioLogadoGateway, _verificarEstoqueJobGateway, _insumosGateway);
            var ordemServicoId = Guid.NewGuid();
            var estoqueId = Guid.NewGuid();
            var insumos = new List<CadastrarInsumoOSUseCaseDto>
            {
                new CadastrarInsumoOSUseCaseDto
                {
                    EstoqueId = estoqueId,
                    Quantidade = 15,
                    Estoque = new Estoque 
                    { 
                        Id = estoqueId, 
                        QuantidadeDisponivel = 20, 
                        QuantidadeMinima = 10 
                    }
                }
            };

            _insumosGateway.CadastrarVariosAsync(Arg.Any<List<InsumoOS>>()).Returns(Task.FromResult<IEnumerable<InsumoOS>>(new List<InsumoOS>()));
            _verificarEstoqueJobGateway.VerificarEstoqueAsync().Returns(Task.CompletedTask);

            // Act
            var resultado = await handler.Handle(ordemServicoId, insumos);

            // Assert
            await _verificarEstoqueJobGateway.Received(1).VerificarEstoqueAsync();
        }

        /// <summary>
        /// Verifica se não dispara alerta quando estoque está OK
        /// Importância: MÉDIA - Evita alertas desnecessários
        /// Contribuição: Garante que alertas são disparados apenas quando necessário
        /// </summary>
        [Fact]
        public async Task Handle_QuandoEstoqueNaoFicaCritico_NaoDeveDispararAlerta()
        {
            // Arrange
            var handler = new CadastrarInsumosHandler(_logGateway, _udtGateway, _usuarioLogadoGateway, _verificarEstoqueJobGateway, _insumosGateway);
            var ordemServicoId = Guid.NewGuid();
            var estoqueId = Guid.NewGuid();
            var insumos = new List<CadastrarInsumoOSUseCaseDto>
            {
                new CadastrarInsumoOSUseCaseDto
                {
                    EstoqueId = estoqueId,
                    Quantidade = 5,
                    Estoque = new Estoque 
                    { 
                        Id = estoqueId, 
                        QuantidadeDisponivel = 100, 
                        QuantidadeMinima = 10 
                    }
                }
            };

            _insumosGateway.CadastrarVariosAsync(Arg.Any<List<InsumoOS>>()).Returns(Task.FromResult<IEnumerable<InsumoOS>>(new List<InsumoOS>()));

            // Act
            var resultado = await handler.Handle(ordemServicoId, insumos);

            // Assert
            await _verificarEstoqueJobGateway.DidNotReceive().VerificarEstoqueAsync();
        }

        /// <summary>
        /// Verifica se lança exceção quando commit falha
        /// Importância: ALTA - Garantia de persistência
        /// Contribuição: Previne perda de dados
        /// </summary>
        [Fact]
        public async Task Handle_QuandoCommitFalha_DeveLancarPersistirDadosException()
        {
            // Arrange
            var handler = new CadastrarInsumosHandler(_logGateway, _udtGateway, _usuarioLogadoGateway, _verificarEstoqueJobGateway, _insumosGateway);
            var ordemServicoId = Guid.NewGuid();
            var insumos = new List<CadastrarInsumoOSUseCaseDto>
            {
                new CadastrarInsumoOSUseCaseDto
                {
                    EstoqueId = Guid.NewGuid(),
                    Quantidade = 5,
                    Estoque = new Estoque { QuantidadeDisponivel = 100, QuantidadeMinima = 10 }
                }
            };

            _insumosGateway.CadastrarVariosAsync(Arg.Any<List<InsumoOS>>()).Returns(Task.FromResult<IEnumerable<InsumoOS>>(new List<InsumoOS>()));
            _udtGateway.Commit().Returns(Task.FromResult(false));

            // Act & Assert
            await handler.Invoking(h => h.Handle(ordemServicoId, insumos))
                .Should().ThrowAsync<PersistirDadosException>()
                .WithMessage("Erro ao cadastrar insumos na ordem de serviço");
        }

        /// <summary>
        /// Verifica se cadastra múltiplos insumos corretamente
        /// Importância: ALTA - Ordens podem ter vários insumos
        /// Contribuição: Garante que todos os insumos são processados
        /// </summary>
        [Fact]
        public async Task Handle_ComMultiplosInsumos_DeveCadastrarTodos()
        {
            // Arrange
            var handler = new CadastrarInsumosHandler(_logGateway, _udtGateway, _usuarioLogadoGateway, _verificarEstoqueJobGateway, _insumosGateway);
            var ordemServicoId = Guid.NewGuid();
            var insumos = new List<CadastrarInsumoOSUseCaseDto>
            {
                new CadastrarInsumoOSUseCaseDto { EstoqueId = Guid.NewGuid(), Quantidade = 5 },
                new CadastrarInsumoOSUseCaseDto { EstoqueId = Guid.NewGuid(), Quantidade = 3 },
                new CadastrarInsumoOSUseCaseDto { EstoqueId = Guid.NewGuid(), Quantidade = 10 }
            };

            _insumosGateway.CadastrarVariosAsync(Arg.Any<List<InsumoOS>>()).Returns(Task.FromResult<IEnumerable<InsumoOS>>(new List<InsumoOS>()));

            // Act
            var resultado = await handler.Handle(ordemServicoId, insumos);

            // Assert
            resultado.Should().HaveCount(3);
            resultado[0].Quantidade.Should().Be(5);
            resultado[1].Quantidade.Should().Be(3);
            resultado[2].Quantidade.Should().Be(10);
        }

        /// <summary>
        /// Verifica se dispara alerta quando quantidade fica exatamente no mínimo
        /// Importância: ALTA - Validação de regra de negócio
        /// Contribuição: Garante que alerta é disparado no limite exato
        /// </summary>
        [Fact]
        public async Task Handle_QuandoQuantidadeFicaIgualAoMinimo_DeveDispararAlerta()
        {
            // Arrange
            var handler = new CadastrarInsumosHandler(_logGateway, _udtGateway, _usuarioLogadoGateway, _verificarEstoqueJobGateway, _insumosGateway);
            var ordemServicoId = Guid.NewGuid();
            var estoqueId = Guid.NewGuid();
            var insumos = new List<CadastrarInsumoOSUseCaseDto>
            {
                new CadastrarInsumoOSUseCaseDto
                {
                    EstoqueId = estoqueId,
                    Quantidade = 10,
                    Estoque = new Estoque 
                    { 
                        Id = estoqueId, 
                        QuantidadeDisponivel = 20, 
                        QuantidadeMinima = 10 
                    }
                }
            };

            _insumosGateway.CadastrarVariosAsync(Arg.Any<List<InsumoOS>>()).Returns(Task.FromResult<IEnumerable<InsumoOS>>(new List<InsumoOS>()));
            _verificarEstoqueJobGateway.VerificarEstoqueAsync().Returns(Task.CompletedTask);

            // Act
            var resultado = await handler.Handle(ordemServicoId, insumos);

            // Assert
            await _verificarEstoqueJobGateway.Received(1).VerificarEstoqueAsync();
        }
    }
}
