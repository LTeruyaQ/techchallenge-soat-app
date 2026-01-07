using Adapters.Gateways;
using Core.DTOs.Entidades.Cliente;
using Core.DTOs.Entidades.Estoque;
using Core.DTOs.Entidades.Servico;
using Core.DTOs.Entidades.Usuarios;
using Core.DTOs.Entidades.Veiculo;
using Core.Entidades;
using Core.Enumeradores;
using Core.Interfaces.Repositorios;
using Core.Interfaces.Servicos;
using Core.Interfaces.Jobs;
using System.Security.Claims;

namespace MecanicaOS.UnitTests.Adapters.Gateways
{
    /// <summary>
    /// Testes para Gateways críticos
    /// ROI CRÍTICO: Gateways fazem conversão entre Entidades e DTOs - erros aqui corrompem dados.
    /// Importância: Valida ToDto/FromDto e operações CRUD.
    /// </summary>
    public class GatewaysTests
    {
        #region EstoqueGateway

        [Fact]
        public void EstoqueGateway_ToDto_DeveConverterCorretamente()
        {
            // Arrange
            var estoque = new Estoque
            {
                Id = Guid.NewGuid(),
                Insumo = "Óleo 5W30",
                Descricao = "Óleo sintético",
                Preco = 45.90m,
                QuantidadeDisponivel = 100,
                QuantidadeMinima = 20,
                Ativo = true,
                DataCadastro = DateTime.UtcNow
            };

            // Act
            var dto = EstoqueGateway.ToDto(estoque);

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(estoque.Id);
            dto.Insumo.Should().Be("Óleo 5W30");
            dto.Preco.Should().Be(45.90m);
            dto.QuantidadeDisponivel.Should().Be(100);
            dto.Ativo.Should().BeTrue();
        }

        [Fact]
        public void EstoqueGateway_FromDto_DeveConverterCorretamente()
        {
            // Arrange
            var dto = new EstoqueEntityDto
            {
                Id = Guid.NewGuid(),
                Insumo = "Óleo 5W30",
                Descricao = "Óleo sintético",
                Preco = 45.90m,
                QuantidadeDisponivel = 100,
                QuantidadeMinima = 20,
                Ativo = true,
                DataCadastro = DateTime.UtcNow
            };

            // Act
            var estoque = EstoqueGateway.FromDto(dto);

            // Assert
            estoque.Should().NotBeNull();
            estoque.Id.Should().Be(dto.Id);
            estoque.Insumo.Should().Be("Óleo 5W30");
            estoque.Preco.Should().Be(45.90m);
            estoque.QuantidadeDisponivel.Should().Be(100);
        }

        [Fact]
        public async Task EstoqueGateway_ObterPorId_ComIdExistente_DeveRetornarEstoque()
        {
            // Arrange
            var repositorio = Substitute.For<IRepositorio<EstoqueEntityDto>>();
            var gateway = new EstoqueGateway(repositorio);
            var id = Guid.NewGuid();
            var dto = new EstoqueEntityDto
            {
                Id = id,
                Insumo = "Teste",
                Descricao = "Desc",
                Preco = 10m,
                QuantidadeDisponivel = 5,
                QuantidadeMinima = 1
            };

            repositorio.ObterPorIdSemRastreamentoAsync(id).Returns(Task.FromResult<EstoqueEntityDto?>(dto));

            // Act
            var resultado = await gateway.ObterPorIdAsync(id);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(id);
        }

        [Fact]
        public async Task EstoqueGateway_ObterPorId_ComIdInexistente_DeveRetornarNull()
        {
            // Arrange
            var repositorio = Substitute.For<IRepositorio<EstoqueEntityDto>>();
            var gateway = new EstoqueGateway(repositorio);
            var id = Guid.NewGuid();

            repositorio.ObterPorIdSemRastreamentoAsync(id).Returns(Task.FromResult<EstoqueEntityDto?>(null));

            // Act
            var resultado = await gateway.ObterPorIdAsync(id);

            // Assert
            resultado.Should().BeNull();
        }

        #endregion

        #region ServicoGateway

        [Fact]
        public async Task ServicoGateway_CadastrarAsync_DeveCadastrarERetornarServico()
        {
            // Arrange
            var repositorio = Substitute.For<IRepositorio<ServicoEntityDto>>();
            var gateway = new ServicoGateway(repositorio);
            var servico = new Servico
            {
                Id = Guid.NewGuid(),
                Nome = "Troca de Óleo",
                Descricao = "Serviço completo",
                Valor = 150.00m,
                Disponivel = true
            };

            repositorio.CadastrarAsync(Arg.Any<ServicoEntityDto>())
                .Returns(Task.FromResult(new ServicoEntityDto
                {
                    Id = servico.Id,
                    Nome = servico.Nome,
                    Descricao = servico.Descricao,
                    Valor = servico.Valor,
                    Disponivel = servico.Disponivel
                }));

            // Act
            var resultado = await gateway.CadastrarAsync(servico);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Nome.Should().Be("Troca de Óleo");
            await repositorio.Received(1).CadastrarAsync(Arg.Any<ServicoEntityDto>());
        }

        [Fact]
        public async Task ServicoGateway_ObterPorIdAsync_ComIdExistente_DeveRetornarServico()
        {
            // Arrange
            var repositorio = Substitute.For<IRepositorio<ServicoEntityDto>>();
            var gateway = new ServicoGateway(repositorio);
            var id = Guid.NewGuid();

            repositorio.ObterPorIdSemRastreamentoAsync(id)
                .Returns(Task.FromResult<ServicoEntityDto?>(new ServicoEntityDto
                {
                    Id = id,
                    Nome = "Teste",
                    Descricao = "Desc",
                    Valor = 100m,
                    Disponivel = true
                }));

            // Act
            var resultado = await gateway.ObterPorIdAsync(id);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(id);
        }

        [Fact]
        public async Task ServicoGateway_ObterPorIdAsync_ComIdInexistente_DeveRetornarNull()
        {
            // Arrange
            var repositorio = Substitute.For<IRepositorio<ServicoEntityDto>>();
            var gateway = new ServicoGateway(repositorio);
            var id = Guid.NewGuid();

            repositorio.ObterPorIdSemRastreamentoAsync(id)
                .Returns(Task.FromResult<ServicoEntityDto?>(null));

            // Act
            var resultado = await gateway.ObterPorIdAsync(id);

            // Assert
            resultado.Should().BeNull();
        }

        [Fact]
        public async Task ServicoGateway_ObterTodosAsync_DeveRetornarListaDeServicos()
        {
            // Arrange
            var repositorio = Substitute.For<IRepositorio<ServicoEntityDto>>();
            var gateway = new ServicoGateway(repositorio);
            var dtos = new List<ServicoEntityDto>
            {
                new ServicoEntityDto { Id = Guid.NewGuid(), Nome = "Serviço 1", Descricao = "Desc1", Valor = 100m, Disponivel = true },
                new ServicoEntityDto { Id = Guid.NewGuid(), Nome = "Serviço 2", Descricao = "Desc2", Valor = 200m, Disponivel = true }
            };

            repositorio.ObterTodosAsync().Returns(Task.FromResult<IEnumerable<ServicoEntityDto>>(dtos));

            // Act
            var resultado = await gateway.ObterTodosAsync();

            // Assert
            resultado.Should().HaveCount(2);
        }

        [Fact]
        public async Task ServicoGateway_EditarAsync_DeveChamarRepositorio()
        {
            // Arrange
            var repositorio = Substitute.For<IRepositorio<ServicoEntityDto>>();
            var gateway = new ServicoGateway(repositorio);
            var servico = new Servico
            {
                Id = Guid.NewGuid(),
                Nome = "Serviço Atualizado",
                Descricao = "Nova descrição",
                Valor = 200m,
                Disponivel = false
            };

            // Act
            await gateway.EditarAsync(servico);

            // Assert
            await repositorio.Received(1).EditarAsync(Arg.Any<ServicoEntityDto>());
        }

        [Fact]
        public async Task ServicoGateway_DeletarAsync_DeveChamarRepositorio()
        {
            // Arrange
            var repositorio = Substitute.For<IRepositorio<ServicoEntityDto>>();
            var gateway = new ServicoGateway(repositorio);
            var servico = new Servico { Id = Guid.NewGuid(), Nome = "Teste", Descricao = "Desc", Valor = 100m };

            // Act
            await gateway.DeletarAsync(servico);

            // Assert
            await repositorio.Received(1).DeletarAsync(Arg.Any<ServicoEntityDto>());
        }

        #endregion

        #region UsuarioGateway

        [Fact]
        public async Task UsuarioGateway_CadastrarAsync_DeveCadastrarERetornarUsuario()
        {
            // Arrange
            var repositorio = Substitute.For<IRepositorio<UsuarioEntityDto>>();
            var gateway = new UsuarioGateway(repositorio);
            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Email = "teste@email.com",
                Senha = "senha123",
                TipoUsuario = TipoUsuario.Admin
            };

            repositorio.CadastrarAsync(Arg.Any<UsuarioEntityDto>())
                .Returns(Task.FromResult(new UsuarioEntityDto
                {
                    Id = usuario.Id,
                    Email = usuario.Email,
                    Senha = usuario.Senha,
                    TipoUsuario = usuario.TipoUsuario
                }));

            // Act
            var resultado = await gateway.CadastrarAsync(usuario);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Email.Should().Be("teste@email.com");
            await repositorio.Received(1).CadastrarAsync(Arg.Any<UsuarioEntityDto>());
        }

        [Fact]
        public async Task UsuarioGateway_ObterPorIdAsync_ComIdExistente_DeveRetornarUsuario()
        {
            // Arrange
            var repositorio = Substitute.For<IRepositorio<UsuarioEntityDto>>();
            var gateway = new UsuarioGateway(repositorio);
            var id = Guid.NewGuid();

            repositorio.ObterPorIdSemRastreamentoAsync(id)
                .Returns(Task.FromResult<UsuarioEntityDto?>(new UsuarioEntityDto
                {
                    Id = id,
                    Email = "teste@email.com",
                    Senha = "senha",
                    TipoUsuario = TipoUsuario.Admin
                }));

            // Act
            var resultado = await gateway.ObterPorIdAsync(id);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(id);
        }

        [Fact]
        public async Task UsuarioGateway_ObterTodosAsync_DeveRetornarListaDeUsuarios()
        {
            // Arrange
            var repositorio = Substitute.For<IRepositorio<UsuarioEntityDto>>();
            var gateway = new UsuarioGateway(repositorio);
            var dtos = new List<UsuarioEntityDto>
            {
                new UsuarioEntityDto { Id = Guid.NewGuid(), Email = "user1@email.com", Senha = "senha", TipoUsuario = TipoUsuario.Admin },
                new UsuarioEntityDto { Id = Guid.NewGuid(), Email = "user2@email.com", Senha = "senha", TipoUsuario = TipoUsuario.Cliente }
            };

            repositorio.ObterTodosAsync().Returns(Task.FromResult<IEnumerable<UsuarioEntityDto>>(dtos));

            // Act
            var resultado = await gateway.ObterTodosAsync();

            // Assert
            resultado.Should().HaveCount(2);
        }

        [Fact]
        public async Task UsuarioGateway_EditarAsync_DeveChamarRepositorio()
        {
            // Arrange
            var repositorio = Substitute.For<IRepositorio<UsuarioEntityDto>>();
            var gateway = new UsuarioGateway(repositorio);
            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Email = "atualizado@email.com",
                Senha = "novaSenha",
                TipoUsuario = TipoUsuario.Cliente
            };

            // Act
            await gateway.EditarAsync(usuario);

            // Assert
            await repositorio.Received(1).EditarAsync(Arg.Any<UsuarioEntityDto>());
        }

        #endregion

        #region VeiculoGateway

        [Fact]
        public async Task VeiculoGateway_CadastrarAsync_DeveChamarRepositorio()
        {
            // Arrange
            var repositorio = Substitute.For<IRepositorio<VeiculoEntityDto>>();
            var gateway = new VeiculoGateway(repositorio);
            var veiculo = new Veiculo
            {
                Id = Guid.NewGuid(),
                Placa = "ABC1234",
                Marca = "Toyota",
                Modelo = "Corolla",
                Ano = "2023",
                Cor = "Prata",
                ClienteId = Guid.NewGuid()
            };

            repositorio.CadastrarAsync(Arg.Any<VeiculoEntityDto>())
                .Returns(Task.FromResult(new VeiculoEntityDto
                {
                    Id = veiculo.Id,
                    Placa = veiculo.Placa,
                    Marca = veiculo.Marca,
                    Modelo = veiculo.Modelo,
                    Ano = veiculo.Ano,
                    Cor = veiculo.Cor,
                    ClienteId = veiculo.ClienteId
                }));

            // Act
            await gateway.CadastrarAsync(veiculo);

            // Assert
            await repositorio.Received(1).CadastrarAsync(Arg.Any<VeiculoEntityDto>());
        }

        [Fact]
        public async Task VeiculoGateway_ObterPorIdAsync_ComIdExistente_DeveRetornarVeiculo()
        {
            // Arrange
            var repositorio = Substitute.For<IRepositorio<VeiculoEntityDto>>();
            var gateway = new VeiculoGateway(repositorio);
            var id = Guid.NewGuid();

            repositorio.ObterPorIdSemRastreamentoAsync(id)
                .Returns(Task.FromResult<VeiculoEntityDto?>(new VeiculoEntityDto
                {
                    Id = id,
                    Placa = "XYZ5678",
                    Marca = "Honda",
                    Modelo = "Civic",
                    Ano = "2022",
                    Cor = "Preto",
                    ClienteId = Guid.NewGuid()
                }));

            // Act
            var resultado = await gateway.ObterPorIdAsync(id);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(id);
        }

        [Fact]
        public async Task VeiculoGateway_ObterTodosAsync_DeveRetornarListaDeVeiculos()
        {
            // Arrange
            var repositorio = Substitute.For<IRepositorio<VeiculoEntityDto>>();
            var gateway = new VeiculoGateway(repositorio);
            var dtos = new List<VeiculoEntityDto>
            {
                new VeiculoEntityDto { Id = Guid.NewGuid(), Placa = "ABC1234", Marca = "Toyota", Modelo = "Corolla", Ano = "2023", Cor = "Prata", ClienteId = Guid.NewGuid() },
                new VeiculoEntityDto { Id = Guid.NewGuid(), Placa = "XYZ5678", Marca = "Honda", Modelo = "Civic", Ano = "2022", Cor = "Preto", ClienteId = Guid.NewGuid() }
            };

            repositorio.ObterTodosAsync().Returns(Task.FromResult<IEnumerable<VeiculoEntityDto>>(dtos));

            // Act
            var resultado = await gateway.ObterTodosAsync();

            // Assert
            resultado.Should().HaveCount(2);
        }

        [Fact]
        public async Task VeiculoGateway_DeletarAsync_DeveChamarRepositorio()
        {
            // Arrange
            var repositorio = Substitute.For<IRepositorio<VeiculoEntityDto>>();
            var gateway = new VeiculoGateway(repositorio);
            var veiculo = new Veiculo
            {
                Id = Guid.NewGuid(),
                Placa = "DEF9012",
                Marca = "Ford",
                Modelo = "Focus",
                Ano = "2021",
                Cor = "Branco",
                ClienteId = Guid.NewGuid()
            };

            // Act
            await gateway.DeletarAsync(veiculo);

            // Assert
            await repositorio.Received(1).DeletarAsync(Arg.Any<VeiculoEntityDto>());
        }

        #endregion

        #region ContatoGateway

        [Fact]
        public async Task ContatoGateway_CadastrarAsync_DeveChamarRepositorio()
        {
            // Arrange
            var repositorio = Substitute.For<IRepositorio<ContatoEntityDto>>();
            var gateway = new ContatoGateway(repositorio);
            var contato = new Contato
            {
                Id = Guid.NewGuid(),
                Email = "contato@email.com",
                Telefone = "11999999999",
                IdCliente = Guid.NewGuid()
            };

            // Act
            await gateway.CadastrarAsync(contato);

            // Assert
            await repositorio.Received(1).CadastrarAsync(Arg.Any<ContatoEntityDto>());
        }

        [Fact]
        public async Task ContatoGateway_EditarAsync_DeveChamarRepositorio()
        {
            // Arrange
            var repositorio = Substitute.For<IRepositorio<ContatoEntityDto>>();
            var gateway = new ContatoGateway(repositorio);
            var contato = new Contato
            {
                Id = Guid.NewGuid(),
                Email = "atualizado@email.com",
                Telefone = "11888888888",
                IdCliente = Guid.NewGuid()
            };

            // Act
            await gateway.EditarAsync(contato);

            // Assert
            await repositorio.Received(1).EditarAsync(Arg.Any<ContatoEntityDto>());
        }

        [Fact]
        public async Task ContatoGateway_ObterPorIdAsync_ComIdExistente_DeveRetornarContato()
        {
            // Arrange
            var repositorio = Substitute.For<IRepositorio<ContatoEntityDto>>();
            var gateway = new ContatoGateway(repositorio);
            var id = Guid.NewGuid();

            repositorio.ObterPorIdAsync(id)
                .Returns(Task.FromResult<ContatoEntityDto?>(new ContatoEntityDto
                {
                    Id = id,
                    Email = "teste@email.com",
                    Telefone = "11999999999",
                    IdCliente = Guid.NewGuid()
                }));

            // Act
            var resultado = await gateway.ObterPorIdAsync(id);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(id);
            resultado.Email.Should().Be("teste@email.com");
        }

        [Fact]
        public async Task ContatoGateway_ObterPorIdAsync_ComIdInexistente_DeveRetornarNull()
        {
            // Arrange
            var repositorio = Substitute.For<IRepositorio<ContatoEntityDto>>();
            var gateway = new ContatoGateway(repositorio);
            var id = Guid.NewGuid();

            repositorio.ObterPorIdAsync(id).Returns(Task.FromResult<ContatoEntityDto?>(null));

            // Act
            var resultado = await gateway.ObterPorIdAsync(id);

            // Assert
            resultado.Should().BeNull();
        }

        #endregion

        #region EnderecoGateway

        [Fact]
        public async Task EnderecoGateway_CadastrarAsync_DeveChamarRepositorio()
        {
            // Arrange
            var repositorio = Substitute.For<IRepositorio<EnderecoEntityDto>>();
            var gateway = new EnderecoGateway(repositorio);
            var endereco = new Endereco
            {
                Id = Guid.NewGuid(),
                Rua = "Rua Teste",
                Numero = "123",
                Bairro = "Centro",
                Cidade = "São Paulo",
                CEP = "01234-567",
                IdCliente = Guid.NewGuid()
            };

            // Act
            await gateway.CadastrarAsync(endereco);

            // Assert
            await repositorio.Received(1).CadastrarAsync(Arg.Any<EnderecoEntityDto>());
        }

        [Fact]
        public async Task EnderecoGateway_EditarAsync_DeveChamarRepositorio()
        {
            // Arrange
            var repositorio = Substitute.For<IRepositorio<EnderecoEntityDto>>();
            var gateway = new EnderecoGateway(repositorio);
            var endereco = new Endereco
            {
                Id = Guid.NewGuid(),
                Rua = "Rua Atualizada",
                Numero = "456",
                Bairro = "Jardim",
                Cidade = "São Paulo",
                CEP = "98765-432",
                IdCliente = Guid.NewGuid()
            };

            // Act
            await gateway.EditarAsync(endereco);

            // Assert
            await repositorio.Received(1).EditarAsync(Arg.Any<EnderecoEntityDto>());
        }

        [Fact]
        public async Task EnderecoGateway_ObterPorIdAsync_ComIdExistente_DeveRetornarEndereco()
        {
            // Arrange
            var repositorio = Substitute.For<IRepositorio<EnderecoEntityDto>>();
            var gateway = new EnderecoGateway(repositorio);
            var id = Guid.NewGuid();

            repositorio.ObterPorIdAsync(id)
                .Returns(Task.FromResult<EnderecoEntityDto?>(new EnderecoEntityDto
                {
                    Id = id,
                    Rua = "Rua Teste",
                    Numero = "123",
                    Bairro = "Centro",
                    Cidade = "São Paulo",
                    CEP = "01234-567",
                    IdCliente = Guid.NewGuid()
                }));

            // Act
            var resultado = await gateway.ObterPorIdAsync(id);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(id);
            resultado.Rua.Should().Be("Rua Teste");
            resultado.Cidade.Should().Be("São Paulo");
        }

        [Fact]
        public async Task EnderecoGateway_ObterPorIdAsync_ComIdInexistente_DeveRetornarNull()
        {
            // Arrange
            var repositorio = Substitute.For<IRepositorio<EnderecoEntityDto>>();
            var gateway = new EnderecoGateway(repositorio);
            var id = Guid.NewGuid();

            repositorio.ObterPorIdAsync(id).Returns(Task.FromResult<EnderecoEntityDto?>(null));

            // Act
            var resultado = await gateway.ObterPorIdAsync(id);

            // Assert
            resultado.Should().BeNull();
        }

        #endregion

        #region EstoqueGateway - Testes Adicionais

        [Fact]
        public async Task EstoqueGateway_CadastrarAsync_DeveChamarRepositorio()
        {
            // Arrange
            var repositorio = Substitute.For<IRepositorio<EstoqueEntityDto>>();
            var gateway = new EstoqueGateway(repositorio);
            var estoque = new Estoque
            {
                Id = Guid.NewGuid(),
                Insumo = "Filtro de Óleo",
                Descricao = "Filtro original",
                Preco = 35.90m,
                QuantidadeDisponivel = 50,
                QuantidadeMinima = 10
            };

            // Act
            await gateway.CadastrarAsync(estoque);

            // Assert
            await repositorio.Received(1).CadastrarAsync(Arg.Any<EstoqueEntityDto>());
        }

        [Fact]
        public async Task EstoqueGateway_EditarAsync_DeveChamarRepositorio()
        {
            // Arrange
            var repositorio = Substitute.For<IRepositorio<EstoqueEntityDto>>();
            var gateway = new EstoqueGateway(repositorio);
            var estoque = new Estoque
            {
                Id = Guid.NewGuid(),
                Insumo = "Filtro Atualizado",
                Descricao = "Nova descrição",
                Preco = 40.00m,
                QuantidadeDisponivel = 60,
                QuantidadeMinima = 15
            };

            // Act
            await gateway.EditarAsync(estoque);

            // Assert
            await repositorio.Received(1).EditarAsync(Arg.Any<EstoqueEntityDto>());
        }

        [Fact]
        public async Task EstoqueGateway_DeletarAsync_DeveChamarRepositorio()
        {
            // Arrange
            var repositorio = Substitute.For<IRepositorio<EstoqueEntityDto>>();
            var gateway = new EstoqueGateway(repositorio);
            var estoque = new Estoque
            {
                Id = Guid.NewGuid(),
                Insumo = "Item a deletar",
                Descricao = "Desc",
                Preco = 10m,
                QuantidadeDisponivel = 5,
                QuantidadeMinima = 1
            };

            // Act
            await gateway.DeletarAsync(estoque);

            // Assert
            await repositorio.Received(1).DeletarAsync(Arg.Any<EstoqueEntityDto>());
        }

        [Fact]
        public async Task EstoqueGateway_ObterTodosAsync_DeveRetornarListaDeEstoques()
        {
            // Arrange
            var repositorio = Substitute.For<IRepositorio<EstoqueEntityDto>>();
            var gateway = new EstoqueGateway(repositorio);
            var dtos = new List<EstoqueEntityDto>
            {
                new EstoqueEntityDto { Id = Guid.NewGuid(), Insumo = "Item 1", Descricao = "Desc1", Preco = 10m, QuantidadeDisponivel = 5, QuantidadeMinima = 1 },
                new EstoqueEntityDto { Id = Guid.NewGuid(), Insumo = "Item 2", Descricao = "Desc2", Preco = 20m, QuantidadeDisponivel = 10, QuantidadeMinima = 2 }
            };

            repositorio.ObterTodosAsync().Returns(Task.FromResult<IEnumerable<EstoqueEntityDto>>(dtos));

            // Act
            var resultado = await gateway.ObterTodosAsync();

            // Assert
            resultado.Should().HaveCount(2);
        }

        #endregion

        #region SegurancaGateway

        [Fact]
        public void SegurancaGateway_VerificarSenha_ComSenhaCorreta_DeveRetornarTrue()
        {
            // Arrange
            var servicoSenha = Substitute.For<IServicoSenha>();
            var servicoJwt = Substitute.For<IServicoJwt>();
            var gateway = new SegurancaGateway(servicoSenha, servicoJwt);

            servicoSenha.VerificarSenha("senha123", "hash123").Returns(true);

            // Act
            var resultado = gateway.VerificarSenha("senha123", "hash123");

            // Assert
            resultado.Should().BeTrue();
            servicoSenha.Received(1).VerificarSenha("senha123", "hash123");
        }

        [Fact]
        public void SegurancaGateway_VerificarSenha_ComSenhaIncorreta_DeveRetornarFalse()
        {
            // Arrange
            var servicoSenha = Substitute.For<IServicoSenha>();
            var servicoJwt = Substitute.For<IServicoJwt>();
            var gateway = new SegurancaGateway(servicoSenha, servicoJwt);

            servicoSenha.VerificarSenha("senhaErrada", "hash123").Returns(false);

            // Act
            var resultado = gateway.VerificarSenha("senhaErrada", "hash123");

            // Assert
            resultado.Should().BeFalse();
        }

        [Fact]
        public void SegurancaGateway_GerarToken_DeveRetornarToken()
        {
            // Arrange
            var servicoSenha = Substitute.For<IServicoSenha>();
            var servicoJwt = Substitute.For<IServicoJwt>();
            var gateway = new SegurancaGateway(servicoSenha, servicoJwt);
            var usuarioId = Guid.NewGuid();
            var permissoes = new List<string> { "Leitura", "Escrita" };

            servicoJwt.GerarToken(usuarioId, "teste@email.com", "Admin", null, permissoes)
                .Returns("token_jwt_gerado");

            // Act
            var resultado = gateway.GerarToken(usuarioId, "teste@email.com", "Admin", null, permissoes);

            // Assert
            resultado.Should().Be("token_jwt_gerado");
            servicoJwt.Received(1).GerarToken(usuarioId, "teste@email.com", "Admin", null, permissoes);
        }

        [Fact]
        public void SegurancaGateway_Construtor_ComServicoSenhaNull_DeveLancarException()
        {
            // Arrange
            var servicoJwt = Substitute.For<IServicoJwt>();

            // Act & Assert
            Action act = () => new SegurancaGateway(null!, servicoJwt);
            act.Should().Throw<ArgumentNullException>().WithParameterName("servicoSenha");
        }

        #endregion

        #region UnidadeDeTrabalhoGateway

        [Fact]
        public async Task UnidadeDeTrabalhoGateway_Commit_ComSucesso_DeveRetornarTrue()
        {
            // Arrange
            var unidadeDeTrabalho = Substitute.For<IUnidadeDeTrabalho>();
            var gateway = new UnidadeDeTrabalhoGateway(unidadeDeTrabalho);

            unidadeDeTrabalho.Commit().Returns(Task.FromResult(true));

            // Act
            var resultado = await gateway.Commit();

            // Assert
            resultado.Should().BeTrue();
            await unidadeDeTrabalho.Received(1).Commit();
        }

        [Fact]
        public async Task UnidadeDeTrabalhoGateway_Commit_ComFalha_DeveRetornarFalse()
        {
            // Arrange
            var unidadeDeTrabalho = Substitute.For<IUnidadeDeTrabalho>();
            var gateway = new UnidadeDeTrabalhoGateway(unidadeDeTrabalho);

            unidadeDeTrabalho.Commit().Returns(Task.FromResult(false));

            // Act
            var resultado = await gateway.Commit();

            // Assert
            resultado.Should().BeFalse();
        }

        [Fact]
        public void UnidadeDeTrabalhoGateway_Construtor_ComUnidadeNull_DeveLancarException()
        {
            // Act & Assert
            Action act = () => new UnidadeDeTrabalhoGateway(null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("unidadeDeTrabalho");
        }

        #endregion

        #region UsuarioLogadoServicoGateway

        [Fact]
        public void UsuarioLogadoServicoGateway_UsuarioId_DeveRetornarValorDoServico()
        {
            // Arrange
            var servico = Substitute.For<IUsuarioLogadoServico>();
            var gateway = new UsuarioLogadoServicoGateway(servico);
            var usuarioId = Guid.NewGuid();

            servico.UsuarioId.Returns(usuarioId);

            // Act
            var resultado = gateway.UsuarioId;

            // Assert
            resultado.Should().Be(usuarioId);
        }

        [Fact]
        public void UsuarioLogadoServicoGateway_Email_DeveRetornarValorDoServico()
        {
            // Arrange
            var servico = Substitute.For<IUsuarioLogadoServico>();
            var gateway = new UsuarioLogadoServicoGateway(servico);

            servico.Email.Returns("teste@email.com");

            // Act
            var resultado = gateway.Email;

            // Assert
            resultado.Should().Be("teste@email.com");
        }

        [Fact]
        public void UsuarioLogadoServicoGateway_EstaAutenticado_DeveRetornarValorDoServico()
        {
            // Arrange
            var servico = Substitute.For<IUsuarioLogadoServico>();
            var gateway = new UsuarioLogadoServicoGateway(servico);

            servico.EstaAutenticado.Returns(true);

            // Act
            var resultado = gateway.EstaAutenticado;

            // Assert
            resultado.Should().BeTrue();
        }

        [Fact]
        public void UsuarioLogadoServicoGateway_EstaNaRole_DeveChamarServico()
        {
            // Arrange
            var servico = Substitute.For<IUsuarioLogadoServico>();
            var gateway = new UsuarioLogadoServicoGateway(servico);

            servico.EstaNaRole("Admin").Returns(true);

            // Act
            var resultado = gateway.EstaNaRole("Admin");

            // Assert
            resultado.Should().BeTrue();
            servico.Received(1).EstaNaRole("Admin");
        }

        [Fact]
        public void UsuarioLogadoServicoGateway_PossuiPermissao_DeveChamarServico()
        {
            // Arrange
            var servico = Substitute.For<IUsuarioLogadoServico>();
            var gateway = new UsuarioLogadoServicoGateway(servico);

            servico.PossuiPermissao("Leitura").Returns(true);

            // Act
            var resultado = gateway.PossuiPermissao("Leitura");

            // Assert
            resultado.Should().BeTrue();
            servico.Received(1).PossuiPermissao("Leitura");
        }

        [Fact]
        public void UsuarioLogadoServicoGateway_ObterTodasClaims_DeveRetornarClaims()
        {
            // Arrange
            var servico = Substitute.For<IUsuarioLogadoServico>();
            var gateway = new UsuarioLogadoServicoGateway(servico);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Usuario"),
                new Claim(ClaimTypes.Email, "teste@email.com")
            };

            servico.ObterTodasClaims().Returns(claims);

            // Act
            var resultado = gateway.ObterTodasClaims();

            // Assert
            resultado.Should().HaveCount(2);
            servico.Received(1).ObterTodasClaims();
        }

        [Fact]
        public void UsuarioLogadoServicoGateway_ObterUsuarioLogado_DeveRetornarUsuario()
        {
            // Arrange
            var servico = Substitute.For<IUsuarioLogadoServico>();
            var gateway = new UsuarioLogadoServicoGateway(servico);
            var usuario = new Usuario { Id = Guid.NewGuid(), Email = "teste@email.com", Senha = "hash", TipoUsuario = TipoUsuario.Admin };

            servico.ObterUsuarioLogado().Returns(usuario);

            // Act
            var resultado = gateway.ObterUsuarioLogado();

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Email.Should().Be("teste@email.com");
        }

        #endregion

        #region LogGateway

        [Fact]
        public void LogGateway_LogInicio_DeveChamarServico()
        {
            // Arrange
            var logServico = Substitute.For<ILogServico<GatewaysTests>>();
            var gateway = new LogGateway<GatewaysTests>(logServico);

            // Act
            gateway.LogInicio("MetodoTeste", new { Id = 1 });

            // Assert
            logServico.Received(1).LogInicio("MetodoTeste", Arg.Any<object>());
        }

        [Fact]
        public void LogGateway_LogFim_DeveChamarServico()
        {
            // Arrange
            var logServico = Substitute.For<ILogServico<GatewaysTests>>();
            var gateway = new LogGateway<GatewaysTests>(logServico);

            // Act
            gateway.LogFim("MetodoTeste", new { Resultado = "Sucesso" });

            // Assert
            logServico.Received(1).LogFim("MetodoTeste", Arg.Any<object>());
        }

        [Fact]
        public void LogGateway_LogErro_DeveChamarServico()
        {
            // Arrange
            var logServico = Substitute.For<ILogServico<GatewaysTests>>();
            var gateway = new LogGateway<GatewaysTests>(logServico);
            var exception = new Exception("Erro de teste");

            // Act
            gateway.LogErro("MetodoTeste", exception);

            // Assert
            logServico.Received(1).LogErro("MetodoTeste", exception);
        }

        [Fact]
        public void LogGateway_Construtor_ComServicoNull_DeveLancarException()
        {
            // Act & Assert
            Action act = () => new LogGateway<GatewaysTests>(null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logServico");
        }

        #endregion

        #region VerificarEstoqueJobGateway

        [Fact]
        public async Task VerificarEstoqueJobGateway_VerificarEstoqueAsync_DeveChamarJob()
        {
            // Arrange
            var job = Substitute.For<IVerificarEstoqueJob>();
            var gateway = new VerificarEstoqueJobGateway(job);

            // Act
            await gateway.VerificarEstoqueAsync();

            // Assert
            await job.Received(1).ExecutarAsync();
        }

        [Fact]
        public void VerificarEstoqueJobGateway_Construtor_ComJobValido_DeveCriarInstancia()
        {
            // Arrange
            var job = Substitute.For<IVerificarEstoqueJob>();

            // Act
            var gateway = new VerificarEstoqueJobGateway(job);

            // Assert
            gateway.Should().NotBeNull();
        }

        #endregion
    }
}
