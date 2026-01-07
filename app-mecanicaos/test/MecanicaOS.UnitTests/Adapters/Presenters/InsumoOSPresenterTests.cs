using Adapters.Presenters;
using Core.Entidades;
using FluentAssertions;

namespace MecanicaOS.UnitTests.Adapters.Presenters
{
    /// <summary>
    /// Testes para InsumoOSPresenter
    /// 
    /// IMPORTÂNCIA: Presenter responsável por converter entidades InsumoOS para responses da API.
    /// Inclui lógica de mapeamento de relacionamentos (Estoque) e tratamento de nulls.
    /// 
    /// COBERTURA: Valida conversão de InsumoOS para InsumoOSResponse.
    /// Testa cenários com e sem relacionamentos, listas vazias e nulls.
    /// </summary>
    public class InsumoOSPresenterTests
    {
        [Fact]
        public void ToResponse_ComInsumoValido_DeveConverterCorretamente()
        {
            // Arrange
            var presenter = new InsumoOSPresenter();
            var estoqueId = Guid.NewGuid();
            var ordemServicoId = Guid.NewGuid();
            
            var insumos = new List<InsumoOS>
            {
                new InsumoOS
                {
                    EstoqueId = estoqueId,
                    OrdemServicoId = ordemServicoId,
                    Quantidade = 5,
                    Estoque = new Estoque
                    {
                        Id = estoqueId,
                        Insumo = "Óleo de Motor",
                        QuantidadeDisponivel = 10,
                        QuantidadeMinima = 5,
                        Preco = 50.00m
                    }
                }
            };

            // Act
            var responses = presenter.ToResponse(insumos);

            // Assert
            responses.Should().HaveCount(1);
            var response = responses.First();
            response.EstoqueId.Should().Be(estoqueId);
            response.OrdemServicoId.Should().Be(ordemServicoId);
            response.Quantidade.Should().Be(5);
            response.Estoque.Should().NotBeNull();
            response.Estoque!.Insumo.Should().Be("Óleo de Motor");
            response.Estoque.QuantidadeDisponivel.Should().Be(10);
            response.Estoque.QuantidadeMinima.Should().Be(5);
            response.Estoque.Preco.Should().Be(50.00);
        }

        [Fact]
        public void ToResponse_ComListaVazia_DeveRetornarListaVazia()
        {
            // Arrange
            var presenter = new InsumoOSPresenter();
            var insumos = new List<InsumoOS>();

            // Act
            var responses = presenter.ToResponse(insumos);

            // Assert
            responses.Should().NotBeNull();
            responses.Should().BeEmpty();
        }

        [Fact]
        public void ToResponse_ComListaNull_DeveRetornarListaVazia()
        {
            // Arrange
            var presenter = new InsumoOSPresenter();

            // Act
            var responses = presenter.ToResponse(null!);

            // Assert
            responses.Should().NotBeNull();
            responses.Should().BeEmpty();
        }

        [Fact]
        public void ToResponse_ComEstoqueNull_DeveConverterComEstoqueNull()
        {
            // Arrange
            var presenter = new InsumoOSPresenter();
            var insumos = new List<InsumoOS>
            {
                new InsumoOS
                {
                    EstoqueId = Guid.NewGuid(),
                    OrdemServicoId = Guid.NewGuid(),
                    Quantidade = 3,
                    Estoque = null
                }
            };

            // Act
            var responses = presenter.ToResponse(insumos);

            // Assert
            responses.Should().HaveCount(1);
            responses.First().Estoque.Should().BeNull();
        }

        [Fact]
        public void ToResponse_ComMultiplosInsumos_DeveConverterTodos()
        {
            // Arrange
            var presenter = new InsumoOSPresenter();
            var insumos = new List<InsumoOS>
            {
                new InsumoOS
                {
                    EstoqueId = Guid.NewGuid(),
                    OrdemServicoId = Guid.NewGuid(),
                    Quantidade = 2,
                    Estoque = new Estoque
                    {
                        Id = Guid.NewGuid(),
                        Insumo = "Filtro de Óleo",
                        QuantidadeDisponivel = 20,
                        QuantidadeMinima = 10,
                        Preco = 25.00m
                    }
                },
                new InsumoOS
                {
                    EstoqueId = Guid.NewGuid(),
                    OrdemServicoId = Guid.NewGuid(),
                    Quantidade = 4,
                    Estoque = new Estoque
                    {
                        Id = Guid.NewGuid(),
                        Insumo = "Filtro de Ar",
                        QuantidadeDisponivel = 15,
                        QuantidadeMinima = 8,
                        Preco = 30.00m
                    }
                },
                new InsumoOS
                {
                    EstoqueId = Guid.NewGuid(),
                    OrdemServicoId = Guid.NewGuid(),
                    Quantidade = 1,
                    Estoque = null
                }
            };

            // Act
            var responses = presenter.ToResponse(insumos);

            // Assert
            responses.Should().HaveCount(3);
            responses.Count(r => r.Estoque != null).Should().Be(2);
            responses.Count(r => r.Estoque == null).Should().Be(1);
        }

        [Fact]
        public void ToResponse_ComQuantidadeZero_DeveConverterCorretamente()
        {
            // Arrange
            var presenter = new InsumoOSPresenter();
            var insumos = new List<InsumoOS>
            {
                new InsumoOS
                {
                    EstoqueId = Guid.NewGuid(),
                    OrdemServicoId = Guid.NewGuid(),
                    Quantidade = 0,
                    Estoque = new Estoque
                    {
                        Id = Guid.NewGuid(),
                        Insumo = "Teste",
                        QuantidadeDisponivel = 0,
                        QuantidadeMinima = 0,
                        Preco = 0m
                    }
                }
            };

            // Act
            var responses = presenter.ToResponse(insumos);

            // Assert
            responses.Should().HaveCount(1);
            responses.First().Quantidade.Should().Be(0);
            responses.First().Estoque!.QuantidadeDisponivel.Should().Be(0);
            responses.First().Estoque.Preco.Should().Be(0);
        }

        [Fact]
        public void ToResponse_ComPrecoDecimal_DeveConverterParaDouble()
        {
            // Arrange
            var presenter = new InsumoOSPresenter();
            var insumos = new List<InsumoOS>
            {
                new InsumoOS
                {
                    EstoqueId = Guid.NewGuid(),
                    OrdemServicoId = Guid.NewGuid(),
                    Quantidade = 1,
                    Estoque = new Estoque
                    {
                        Id = Guid.NewGuid(),
                        Insumo = "Teste",
                        QuantidadeDisponivel = 10,
                        QuantidadeMinima = 5,
                        Preco = 99.99m
                    }
                }
            };

            // Act
            var responses = presenter.ToResponse(insumos);

            // Assert
            responses.First().Estoque!.Preco.Should().Be(99.99);
            responses.First().Estoque.Preco.Should().BeApproximately(99.99, 0.01);
        }

        [Fact]
        public void ToResponse_DevePreservarIdsOriginais()
        {
            // Arrange
            var presenter = new InsumoOSPresenter();
            var estoqueId = Guid.NewGuid();
            var ordemServicoId = Guid.NewGuid();
            
            var insumos = new List<InsumoOS>
            {
                new InsumoOS
                {
                    EstoqueId = estoqueId,
                    OrdemServicoId = ordemServicoId,
                    Quantidade = 1,
                    Estoque = new Estoque
                    {
                        Id = estoqueId,
                        Insumo = "Teste",
                        QuantidadeDisponivel = 10,
                        QuantidadeMinima = 5,
                        Preco = 10m
                    }
                }
            };

            // Act
            var responses = presenter.ToResponse(insumos);

            // Assert
            responses.First().EstoqueId.Should().Be(estoqueId);
            responses.First().OrdemServicoId.Should().Be(ordemServicoId);
            responses.First().Estoque!.Id.Should().Be(estoqueId);
        }

        [Fact]
        public void Presenter_DeveImplementarInterface()
        {
            // Arrange & Act
            var presenter = new InsumoOSPresenter();

            // Assert
            presenter.Should().BeAssignableTo<global::Core.Interfaces.Presenters.IInsumoPresenter>();
        }

        [Fact]
        public void ToResponse_ComInsumoComNomeComCaracteresEspeciais_DeveConverterCorretamente()
        {
            // Arrange
            var presenter = new InsumoOSPresenter();
            var nomeEspecial = "Óleo 10W-40 (Semi-Sintético) - 1L";
            var insumos = new List<InsumoOS>
            {
                new InsumoOS
                {
                    EstoqueId = Guid.NewGuid(),
                    OrdemServicoId = Guid.NewGuid(),
                    Quantidade = 1,
                    Estoque = new Estoque
                    {
                        Id = Guid.NewGuid(),
                        Insumo = nomeEspecial,
                        QuantidadeDisponivel = 10,
                        QuantidadeMinima = 5,
                        Preco = 50m
                    }
                }
            };

            // Act
            var responses = presenter.ToResponse(insumos);

            // Assert
            responses.First().Estoque!.Insumo.Should().Be(nomeEspecial);
        }
    }
}
