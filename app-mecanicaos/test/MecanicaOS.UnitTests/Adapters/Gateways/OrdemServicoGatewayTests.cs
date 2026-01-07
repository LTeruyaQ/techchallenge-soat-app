using Adapters.Gateways;
using Core.DTOs.Entidades.Cliente;
using Core.DTOs.Entidades.Estoque;
using Core.DTOs.Entidades.OrdemServicos;
using Core.Entidades;
using Core.Enumeradores;
using Core.Interfaces.Repositorios;
using Core.Especificacoes.Base.Interfaces;

namespace MecanicaOS.UnitTests.Adapters.Gateways
{
    /// <summary>
    /// Testes para OrdemServicoGateway
    /// Importância CRÍTICA: Valida o trânsito de dados complexos (OrdemServico com relacionamentos).
    /// Garante que conversões ToDto/FromDto preservem todos os campos e relacionamentos (Cliente, Insumos).
    /// </summary>
    public class OrdemServicoGatewayTests
    {
        /// <summary>
        /// Verifica se o gateway cadastra ordem de serviço preservando campos técnicos
        /// Importância: Valida operação principal de cadastro
        /// </summary>
        [Fact]
        public async Task CadastrarAsync_DevePreservarCamposTecnicos()
        {
            // Arrange
            var repositorioMock = Substitute.For<IRepositorio<OrdemServicoEntityDto>>();
            
            var ordemServico = new OrdemServico
            {
                Id = Guid.NewGuid(),
                ClienteId = Guid.NewGuid(),
                VeiculoId = Guid.NewGuid(),
                ServicoId = Guid.NewGuid(),
                Descricao = "Troca de óleo",
                Status = StatusOrdemServico.Recebida,
                Orcamento = 150.00m,
                Ativo = true,
                DataCadastro = DateTime.UtcNow,
                InsumosOS = new List<InsumoOS>()
            };
            
            var dto = OrdemServicoGateway.ToDto(ordemServico);
            
            repositorioMock.CadastrarAsync(Arg.Any<OrdemServicoEntityDto>())
                .Returns(dto);
            
            var gateway = new OrdemServicoGateway(repositorioMock);
            
            // Act
            var resultado = await gateway.CadastrarAsync(ordemServico);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado.Id.Should().Be(ordemServico.Id);
            resultado.ClienteId.Should().Be(ordemServico.ClienteId);
            resultado.Status.Should().Be(StatusOrdemServico.Recebida);
            resultado.Ativo.Should().BeTrue();
            
            await repositorioMock.Received(1).CadastrarAsync(Arg.Any<OrdemServicoEntityDto>());
        }

        /// <summary>
        /// Verifica se o gateway edita ordem de serviço
        /// Importância: Valida operação de atualização
        /// </summary>
        [Fact]
        public async Task EditarAsync_DeveEditarOrdemServico()
        {
            // Arrange
            var repositorioMock = Substitute.For<IRepositorio<OrdemServicoEntityDto>>();
            
            var ordemServico = new OrdemServico
            {
                Id = Guid.NewGuid(),
                ClienteId = Guid.NewGuid(),
                VeiculoId = Guid.NewGuid(),
                ServicoId = Guid.NewGuid(),
                Descricao = "Troca de óleo atualizada",
                Status = StatusOrdemServico.EmExecucao,
                Ativo = true,
                DataCadastro = DateTime.UtcNow,
                InsumosOS = new List<InsumoOS>()
            };
            
            repositorioMock.EditarAsync(Arg.Any<OrdemServicoEntityDto>())
                .Returns(Task.CompletedTask);
            
            var gateway = new OrdemServicoGateway(repositorioMock);
            
            // Act
            await gateway.EditarAsync(ordemServico);
            
            // Assert
            await repositorioMock.Received(1).EditarAsync(Arg.Any<OrdemServicoEntityDto>());
        }

        /// <summary>
        /// Verifica se o gateway edita várias ordens de serviço em lote
        /// Importância: Valida operação em lote
        /// </summary>
        [Fact]
        public async Task EditarVariosAsync_DeveEditarVariasOrdensServico()
        {
            // Arrange
            var repositorioMock = Substitute.For<IRepositorio<OrdemServicoEntityDto>>();
            
            var ordensServico = new List<OrdemServico>
            {
                new OrdemServico
                {
                    Id = Guid.NewGuid(),
                    ClienteId = Guid.NewGuid(),
                    VeiculoId = Guid.NewGuid(),
                    ServicoId = Guid.NewGuid(),
                    Status = StatusOrdemServico.Recebida,
                    InsumosOS = new List<InsumoOS>()
                },
                new OrdemServico
                {
                    Id = Guid.NewGuid(),
                    ClienteId = Guid.NewGuid(),
                    VeiculoId = Guid.NewGuid(),
                    ServicoId = Guid.NewGuid(),
                    Status = StatusOrdemServico.EmExecucao,
                    InsumosOS = new List<InsumoOS>()
                }
            };
            
            repositorioMock.EditarVariosAsync(Arg.Any<IEnumerable<OrdemServicoEntityDto>>())
                .Returns(Task.CompletedTask);
            
            var gateway = new OrdemServicoGateway(repositorioMock);
            
            // Act
            await gateway.EditarVariosAsync(ordensServico);
            
            // Assert
            await repositorioMock.Received(1).EditarVariosAsync(Arg.Any<IEnumerable<OrdemServicoEntityDto>>());
        }

        /// <summary>
        /// Verifica se o gateway lista ordens com orçamento expirado
        /// Importância: Valida filtro de orçamentos expirados
        /// </summary>
        [Fact]
        public async Task ListarOSOrcamentoExpiradoAsync_DeveUsarEspecificacao()
        {
            // Arrange
            var repositorioMock = Substitute.For<IRepositorio<OrdemServicoEntityDto>>();
            
            var ordensServico = new List<OrdemServico>
            {
                new OrdemServico
                {
                    Id = Guid.NewGuid(),
                    Status = StatusOrdemServico.AguardandoAprovacao,
                    DataEnvioOrcamento = DateTime.UtcNow.AddDays(-8)
                }
            };
            
            repositorioMock.ListarProjetadoAsync<OrdemServico>(Arg.Any<IEspecificacao<OrdemServicoEntityDto>>())
                .Returns(ordensServico);
            
            var gateway = new OrdemServicoGateway(repositorioMock);
            
            // Act
            var resultado = await gateway.ListarOSOrcamentoExpiradoAsync();
            
            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(1);
            
            await repositorioMock.Received(1).ListarProjetadoAsync<OrdemServico>(Arg.Any<IEspecificacao<OrdemServicoEntityDto>>());
        }

        /// <summary>
        /// Verifica se o gateway obtém ordem por ID com insumos
        /// Importância: Valida busca com relacionamentos
        /// </summary>
        [Fact]
        public async Task ObterOrdemServicoPorIdComInsumos_DeveUsarEspecificacao()
        {
            // Arrange
            var repositorioMock = Substitute.For<IRepositorio<OrdemServicoEntityDto>>();
            
            var id = Guid.NewGuid();
            var ordemServico = new OrdemServico
            {
                Id = id,
                ClienteId = Guid.NewGuid(),
                InsumosOS = new List<InsumoOS>
                {
                    new InsumoOS { Id = Guid.NewGuid(), Quantidade = 5 }
                }
            };
            
            repositorioMock.ObterUmProjetadoSemRastreamentoAsync<OrdemServico>(Arg.Any<IEspecificacao<OrdemServicoEntityDto>>())
                .Returns(ordemServico);
            
            var gateway = new OrdemServicoGateway(repositorioMock);
            
            // Act
            var resultado = await gateway.ObterOrdemServicoPorIdComInsumos(id);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(id);
            resultado.InsumosOS.Should().HaveCount(1);
            
            await repositorioMock.Received(1).ObterUmProjetadoSemRastreamentoAsync<OrdemServico>(Arg.Any<IEspecificacao<OrdemServicoEntityDto>>());
        }

        /// <summary>
        /// Verifica se o gateway obtém ordens por status
        /// Importância: Valida filtro por status
        /// </summary>
        [Fact]
        public async Task ObterOrdemServicoPorStatusAsync_DeveUsarEspecificacao()
        {
            // Arrange
            var repositorioMock = Substitute.For<IRepositorio<OrdemServicoEntityDto>>();
            
            var status = StatusOrdemServico.EmExecucao;
            var ordensServico = new List<OrdemServico>
            {
                new OrdemServico { Id = Guid.NewGuid(), Status = status }
            };
            
            repositorioMock.ListarProjetadoAsync<OrdemServico>(Arg.Any<IEspecificacao<OrdemServicoEntityDto>>())
                .Returns(ordensServico);
            
            var gateway = new OrdemServicoGateway(repositorioMock);
            
            // Act
            var resultado = await gateway.ObterOrdemServicoPorStatusAsync(status);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(1);
            resultado.First().Status.Should().Be(status);
            
            await repositorioMock.Received(1).ListarProjetadoAsync<OrdemServico>(Arg.Any<IEspecificacao<OrdemServicoEntityDto>>());
        }

        /// <summary>
        /// Verifica se o gateway obtém ordem por ID
        /// Importância: Valida busca simples por ID
        /// </summary>
        [Fact]
        public async Task ObterPorIdAsync_DeveUsarEspecificacao()
        {
            // Arrange
            var repositorioMock = Substitute.For<IRepositorio<OrdemServicoEntityDto>>();

            var id = Guid.NewGuid();
            var ordemServico = new OrdemServico { Id = id };

            repositorioMock
                .ObterUmProjetadoSemRastreamentoAsync<OrdemServico>(Arg.Any<IEspecificacao<OrdemServicoEntityDto>>())
                .Returns(Task.FromResult((OrdemServico?)ordemServico));

            var gateway = new OrdemServicoGateway(repositorioMock);

            // Act
            var resultado = await gateway.ObterPorIdAsync(id);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(id);

            await repositorioMock.Received(1)
                .ObterUmProjetadoSemRastreamentoAsync<OrdemServico>(Arg.Any<IEspecificacao<OrdemServicoEntityDto>>());
        }


        /// <summary>
        /// Verifica se o gateway obtém todas as ordens
        /// Importância: Valida listagem completa
        /// </summary>
        [Fact]
        public async Task ObterTodosAsync_DeveRetornarTodasOrdens()
        {
            // Arrange
            var repositorioMock = Substitute.For<IRepositorio<OrdemServicoEntityDto>>();
            
            var dtos = new List<OrdemServicoEntityDto>
            {
                new OrdemServicoEntityDto
                {
                    Id = Guid.NewGuid(),
                    ClienteId = Guid.NewGuid(),
                    VeiculoId = Guid.NewGuid(),
                    ServicoId = Guid.NewGuid(),
                    Status = StatusOrdemServico.Recebida,
                    InsumosOS = new List<InsumoOSEntityDto>()
                }
            };
            
            repositorioMock.ObterTodosAsync().Returns(dtos);
            
            var gateway = new OrdemServicoGateway(repositorioMock);
            
            // Act
            var resultado = await gateway.ObterTodosAsync();
            
            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(1);
            
            await repositorioMock.Received(1).ObterTodosAsync();
        }

        /// <summary>
        /// Verifica se conversão ToDto preserva todos os campos incluindo insumos
        /// Importância: Valida integridade da conversão com relacionamentos
        /// </summary>
        [Fact]
        public void ToDto_ComInsumos_DeveConverterTodosOsCampos()
        {
            // Arrange
            var ordemServico = new OrdemServico
            {
                Id = Guid.NewGuid(),
                ClienteId = Guid.NewGuid(),
                VeiculoId = Guid.NewGuid(),
                ServicoId = Guid.NewGuid(),
                Descricao = "Teste",
                Status = StatusOrdemServico.Recebida,
                Orcamento = 200.00m,
                DataEnvioOrcamento = DateTime.UtcNow,
                Ativo = true,
                DataCadastro = DateTime.UtcNow,
                InsumosOS = new List<InsumoOS>
                {
                    new InsumoOS
                    {
                        Id = Guid.NewGuid(),
                        EstoqueId = Guid.NewGuid(),
                        Quantidade = 3
                    }
                }
            };
            
            // Act
            var dto = OrdemServicoGateway.ToDto(ordemServico);
            
            // Assert
            dto.Id.Should().Be(ordemServico.Id);
            dto.ClienteId.Should().Be(ordemServico.ClienteId);
            dto.Status.Should().Be(ordemServico.Status);
            dto.Orcamento.Should().Be(ordemServico.Orcamento);
            dto.InsumosOS.Should().HaveCount(1);
            dto.InsumosOS.First().Quantidade.Should().Be(3);
        }

        /// <summary>
        /// Verifica se conversão FromDto preserva todos os campos
        /// Importância: Valida integridade da conversão de DTO para entidade
        /// </summary>
        [Fact]
        public void FromDto_SemRelacionamentos_DeveConverterCamposBasicos()
        {
            // Arrange
            var dto = new OrdemServicoEntityDto
            {
                Id = Guid.NewGuid(),
                ClienteId = Guid.NewGuid(),
                VeiculoId = Guid.NewGuid(),
                ServicoId = Guid.NewGuid(),
                Descricao = "Teste",
                Status = StatusOrdemServico.Recebida,
                Orcamento = 200.00m,
                Ativo = true,
                DataCadastro = DateTime.UtcNow,
                InsumosOS = new List<InsumoOSEntityDto>()
            };
            
            // Act
            var ordemServico = OrdemServicoGateway.FromDto(dto);
            
            // Assert
            ordemServico.Id.Should().Be(dto.Id);
            ordemServico.ClienteId.Should().Be(dto.ClienteId);
            ordemServico.Status.Should().Be(dto.Status);
            ordemServico.Orcamento.Should().Be(dto.Orcamento);
        }

        /// <summary>
        /// Verifica se conversão FromDto preserva relacionamento com Cliente
        /// Importância: Valida conversão de relacionamentos complexos
        /// </summary>
        [Fact]
        public void FromDto_ComCliente_DeveConverterRelacionamento()
        {
            // Arrange
            var dto = new OrdemServicoEntityDto
            {
                Id = Guid.NewGuid(),
                ClienteId = Guid.NewGuid(),
                VeiculoId = Guid.NewGuid(),
                ServicoId = Guid.NewGuid(),
                Status = StatusOrdemServico.Recebida,
                InsumosOS = new List<InsumoOSEntityDto>(),
                Cliente = new ClienteEntityDto
                {
                    Id = Guid.NewGuid(),
                    Nome = "João Silva",
                    Documento = "12345678900",
                    Contato = new ContatoEntityDto
                    {
                        Id = Guid.NewGuid(),
                        Email = "joao@teste.com",
                        Telefone = "11999999999",
                        IdCliente = Guid.NewGuid()
                    }
                }
            };
            
            // Act
            var ordemServico = OrdemServicoGateway.FromDto(dto);
            
            // Assert
            ordemServico.Cliente.Should().NotBeNull();
            ordemServico.Cliente!.Nome.Should().Be("João Silva");
            ordemServico.Cliente.Contato.Should().NotBeNull();
            ordemServico.Cliente.Contato!.Email.Should().Be("joao@teste.com");
        }

        /// <summary>
        /// Verifica se conversão FromDto preserva insumos com estoque
        /// Importância: Valida conversão de relacionamentos aninhados
        /// </summary>
        [Fact]
        public void FromDto_ComInsumosEEstoque_DeveConverterRelacionamentos()
        {
            // Arrange
            var dto = new OrdemServicoEntityDto
            {
                Id = Guid.NewGuid(),
                ClienteId = Guid.NewGuid(),
                VeiculoId = Guid.NewGuid(),
                ServicoId = Guid.NewGuid(),
                Status = StatusOrdemServico.Recebida,
                InsumosOS = new List<InsumoOSEntityDto>
                {
                    new InsumoOSEntityDto
                    {
                        Id = Guid.NewGuid(),
                        EstoqueId = Guid.NewGuid(),
                        Quantidade = 5,
                        Estoque = new EstoqueEntityDto
                        {
                            Id = Guid.NewGuid(),
                            Insumo = "Óleo 5W30",
                            Preco = 50.00m,
                            QuantidadeDisponivel = 100
                        }
                    }
                }
            };
            
            // Act
            var ordemServico = OrdemServicoGateway.FromDto(dto);
            
            // Assert
            ordemServico.InsumosOS.Should().HaveCount(1);
            var insumo = ordemServico.InsumosOS.First();
            insumo.Quantidade.Should().Be(5);
            insumo.Estoque.Should().NotBeNull();
            insumo.Estoque!.Insumo.Should().Be("Óleo 5W30");
        }
    }
}
