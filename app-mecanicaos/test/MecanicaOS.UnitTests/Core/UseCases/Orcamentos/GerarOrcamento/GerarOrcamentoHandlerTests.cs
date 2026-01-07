using Core.DTOs.UseCases.Orcamento;
using Core.Entidades;
using Core.Interfaces.Gateways;
using Core.UseCases.Orcamentos.GerarOrcamento;

namespace MecanicaOS.UnitTests.Core.UseCases.Orcamentos.GerarOrcamento
{
    /// <summary>
    /// Testes para GerarOrcamentoHandler
    /// ROI ALTO: Cálculo correto de orçamentos é essencial para precificação e lucratividade.
    /// Importância: Valida cálculo de valores (serviço + insumos) para orçamentos precisos.
    /// </summary>
    public class GerarOrcamentoHandlerTests
    {
        private readonly ILogGateway<GerarOrcamentoHandler> _logGateway;
        private readonly IUnidadeDeTrabalhoGateway _udtGateway;
        private readonly IUsuarioLogadoServicoGateway _usuarioLogadoGateway;

        public GerarOrcamentoHandlerTests()
        {
            _logGateway = Substitute.For<ILogGateway<GerarOrcamentoHandler>>();
            _udtGateway = Substitute.For<IUnidadeDeTrabalhoGateway>();
            _usuarioLogadoGateway = Substitute.For<IUsuarioLogadoServicoGateway>();
        }

        /// <summary>
        /// Verifica se calcula orçamento com serviço e insumos
        /// Importância: CRÍTICA - Cálculo correto é fundamental
        /// Contribuição: Garante que valor total é soma de serviço + insumos
        /// </summary>
        [Fact]
        public void Handle_ComServicoEInsumos_DeveCalcularValorTotal()
        {
            // Arrange
            var handler = new GerarOrcamentoHandler(_logGateway, _udtGateway, _usuarioLogadoGateway);
            var ordemServico = new OrdemServico
            {
                Servico = new Servico 
                { 
                    Nome = "Troca de Óleo",
                    Descricao = "Óleo",
                    Valor = 150.00m,
                    Disponivel = true
                },
                InsumosOS = new List<InsumoOS>
                {
                    new InsumoOS 
                    { 
                        Quantidade = 2, 
                        Estoque = new Estoque { Preco = 50.00m } 
                    },
                    new InsumoOS 
                    { 
                        Quantidade = 1, 
                        Estoque = new Estoque { Preco = 30.00m } 
                    }
                }
            };
            var useCase = new GerarOrcamentoUseCase(ordemServico);

            // Act
            var resultado = handler.Handle(useCase);

            // Assert
            // 150 (serviço) + (2 * 50) + (1 * 30) = 150 + 100 + 30 = 280
            resultado.Should().Be(280.00m);
        }

        /// <summary>
        /// Verifica se calcula orçamento apenas com serviço (sem insumos)
        /// Importância: ALTA - Nem todas ordens têm insumos
        /// Contribuição: Garante que cálculo funciona sem insumos
        /// </summary>
        [Fact]
        public void Handle_ApenasComServico_DeveRetornarValorDoServico()
        {
            // Arrange
            var handler = new GerarOrcamentoHandler(_logGateway, _udtGateway, _usuarioLogadoGateway);
            var ordemServico = new OrdemServico
            {
                Servico = new Servico 
                { 
                    Nome = "Diagnóstico",
                    Descricao = "Diagnóstico",
                    Valor = 100.00m,
                    Disponivel = true
                },
                InsumosOS = new List<InsumoOS>()
            };
            var useCase = new GerarOrcamentoUseCase(ordemServico);

            // Act
            var resultado = handler.Handle(useCase);

            // Assert
            resultado.Should().Be(100.00m);
        }

        /// <summary>
        /// Verifica se calcula corretamente com múltiplos insumos
        /// Importância: ALTA - Ordens complexas têm vários insumos
        /// Contribuição: Garante precisão com múltiplos itens
        /// </summary>
        [Fact]
        public void Handle_ComMultiplosInsumos_DeveCalcularSomaCorreta()
        {
            // Arrange
            var handler = new GerarOrcamentoHandler(_logGateway, _udtGateway, _usuarioLogadoGateway);
            var ordemServico = new OrdemServico
            {
                Servico = new Servico 
                { 
                    Nome = "Revisão Completa",
                    Descricao = "Revisão",
                    Valor = 500.00m,
                    Disponivel = true
                },
                InsumosOS = new List<InsumoOS>
                {
                    new InsumoOS { Quantidade = 4, Estoque = new Estoque { Preco = 25.00m } },
                    new InsumoOS { Quantidade = 2, Estoque = new Estoque { Preco = 75.00m } },
                    new InsumoOS { Quantidade = 1, Estoque = new Estoque { Preco = 120.00m } },
                    new InsumoOS { Quantidade = 3, Estoque = new Estoque { Preco = 15.00m } }
                }
            };
            var useCase = new GerarOrcamentoUseCase(ordemServico);

            // Act
            var resultado = handler.Handle(useCase);

            // Assert
            // 500 + (4*25) + (2*75) + (1*120) + (3*15) = 500 + 100 + 150 + 120 + 45 = 915
            resultado.Should().Be(915.00m);
        }

        /// <summary>
        /// Verifica se calcula com valores decimais precisos
        /// Importância: ALTA - Precisão financeira é crítica
        /// Contribuição: Garante que não há perda de precisão
        /// </summary>
        [Fact]
        public void Handle_ComValoresDecimais_DeveManterPrecisao()
        {
            // Arrange
            var handler = new GerarOrcamentoHandler(_logGateway, _udtGateway, _usuarioLogadoGateway);
            var ordemServico = new OrdemServico
            {
                Servico = new Servico 
                { 
                    Nome = "Serviço",
                    Descricao = "Teste",
                    Valor = 99.99m,
                    Disponivel = true
                },
                InsumosOS = new List<InsumoOS>
                {
                    new InsumoOS { Quantidade = 3, Estoque = new Estoque { Preco = 33.33m } }
                }
            };
            var useCase = new GerarOrcamentoUseCase(ordemServico);

            // Act
            var resultado = handler.Handle(useCase);

            // Assert
            // 99.99 + (3 * 33.33) = 99.99 + 99.99 = 199.98
            resultado.Should().Be(199.98m);
        }

        /// <summary>
        /// Verifica se calcula com quantidade zero de insumo
        /// Importância: MÉDIA - Validação de edge case
        /// Contribuição: Garante que quantidade zero não causa erro
        /// </summary>
        [Fact]
        public void Handle_ComQuantidadeZero_DeveCalcularCorretamente()
        {
            // Arrange
            var handler = new GerarOrcamentoHandler(_logGateway, _udtGateway, _usuarioLogadoGateway);
            var ordemServico = new OrdemServico
            {
                Servico = new Servico 
                { 
                    Nome = "Serviço",
                    Descricao = "Teste",
                    Valor = 200.00m,
                    Disponivel = true
                },
                InsumosOS = new List<InsumoOS>
                {
                    new InsumoOS { Quantidade = 0, Estoque = new Estoque { Preco = 50.00m } },
                    new InsumoOS { Quantidade = 2, Estoque = new Estoque { Preco = 25.00m } }
                }
            };
            var useCase = new GerarOrcamentoUseCase(ordemServico);

            // Act
            var resultado = handler.Handle(useCase);

            // Assert
            // 200 + (0 * 50) + (2 * 25) = 200 + 0 + 50 = 250
            resultado.Should().Be(250.00m);
        }

        /// <summary>
        /// Verifica se calcula com serviço gratuito
        /// Importância: MÉDIA - Alguns serviços podem ser cortesia
        /// Contribuição: Garante flexibilidade no cálculo
        /// </summary>
        [Fact]
        public void Handle_ComServicoGratuito_DeveCalcularApenasInsumos()
        {
            // Arrange
            var handler = new GerarOrcamentoHandler(_logGateway, _udtGateway, _usuarioLogadoGateway);
            var ordemServico = new OrdemServico
            {
                Servico = new Servico 
                { 
                    Nome = "Cortesia",
                    Descricao = "Gratuito",
                    Valor = 0.00m,
                    Disponivel = true
                },
                InsumosOS = new List<InsumoOS>
                {
                    new InsumoOS { Quantidade = 2, Estoque = new Estoque { Preco = 40.00m } }
                }
            };
            var useCase = new GerarOrcamentoUseCase(ordemServico);

            // Act
            var resultado = handler.Handle(useCase);

            // Assert
            // 0 + (2 * 40) = 80
            resultado.Should().Be(80.00m);
        }
    }
}
