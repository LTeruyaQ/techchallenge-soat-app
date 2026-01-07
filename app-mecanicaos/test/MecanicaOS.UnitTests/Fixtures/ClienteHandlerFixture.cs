using Core.DTOs.Entidades;
using Core.DTOs.Entidades.Cliente;
using Core.DTOs.Entidades.Autenticacao;
using Core.DTOs.UseCases.Cliente;
using Core.Entidades;
using Core.Enumeradores;
using Core.Interfaces.Gateways;
using Core.Interfaces.Repositorios;
using NSubstitute;

namespace MecanicaOS.UnitTests.Fixtures
{
    /// <summary>
    /// Fixture para testes de handlers relacionados a Cliente
    /// </summary>
    public static class ClienteHandlerFixture
    {
        /// <summary>
        /// Cria um mock de IClienteGateway para testes
        /// </summary>
        /// <returns>Mock de IClienteGateway</returns>
        public static IClienteGateway CriarClienteGatewayMock()
        {
            return Substitute.For<IClienteGateway>();
        }

        /// <summary>
        /// Cria um mock de IRepositorio<ClienteEntityDto> para testes
        /// </summary>
        /// <returns>Mock de IRepositorio<ClienteEntityDto></returns>
        public static IRepositorio<ClienteEntityDto> CriarClienteRepositorioMock()
        {
            return Substitute.For<IRepositorio<ClienteEntityDto>>();
        }

        /// <summary>
        /// Cria um mock de IUnidadeDeTrabalhoGateway para testes
        /// </summary>
        /// <returns>Mock de IUnidadeDeTrabalhoGateway</returns>
        public static IUnidadeDeTrabalhoGateway CriarUnidadeDeTrabalhMock()
        {
            return Substitute.For<IUnidadeDeTrabalhoGateway>();
        }

        /// <summary>
        /// Cria um DTO para cadastro de cliente válido
        /// </summary>
        /// <returns>DTO para cadastro de cliente</returns>
        public static CadastrarClienteUseCaseDto CriarCadastrarClienteUseCaseDto()
        {
            return new CadastrarClienteUseCaseDto
            {
                Nome = "Cliente Teste",
                Sexo = "M",
                TipoCliente = TipoCliente.PessoaFisica,
                Documento = "12345678900",
                DataNascimento = "1990-01-01",
                Rua = "Rua Teste",
                Numero = "123",
                Bairro = "Bairro Teste",
                Cidade = "São Paulo",
                CEP = "01234567",
                Email = "cliente@teste.com",
                Telefone = "11999999999"
            };
        }

        /// <summary>
        /// Cria um DTO para atualização de cliente válido
        /// </summary>
        /// <returns>DTO para atualização de cliente</returns>
        public static AtualizarClienteUseCaseDto CriarAtualizarClienteUseCaseDto()
        {
            return new AtualizarClienteUseCaseDto
            {
                Nome = "Cliente Atualizado",
                Sexo = "M",
                EnderecoId = Guid.NewGuid(),
                Rua = "Rua Atualizada",
                Numero = "456",
                Bairro = "Bairro Atualizado",
                Cidade = "Rio de Janeiro",
                CEP = "98765432",
                ContatoId = Guid.NewGuid(),
                Email = "atualizado@teste.com",
                Telefone = "21888888888"
            };
        }

        /// <summary>
        /// Cria um cliente válido para testes
        /// </summary>
        /// <returns>Cliente válido</returns>
        public static Cliente CriarCliente()
        {
            return new Cliente
            {
                Id = Guid.NewGuid(),
                Nome = "Cliente Teste",
                TipoCliente = TipoCliente.PessoaFisica,
                Documento = "12345678900",
                Endereco = new Endereco
                {
                    Id = Guid.NewGuid(),
                    Rua = "Rua Teste",
                    Numero = "123",
                    Bairro = "Bairro Teste",
                    Cidade = "São Paulo",
                        CEP = "01234567"
                },
                Contato = new Contato
                {
                    Id = Guid.NewGuid(),
                    Email = "cliente@teste.com",
                    Telefone = "11999999999"
                },
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now,
                Ativo = true
            };
        }

        /// <summary>
        /// Cria uma lista de clientes para testes
        /// </summary>
        /// <param name="quantidade">Quantidade de clientes a serem criados</param>
        /// <returns>Lista de clientes</returns>
        public static List<Cliente> CriarListaClientes(int quantidade = 3)
        {
            var clientes = new List<Cliente>();
            
            for (int i = 0; i < quantidade; i++)
            {
                var cliente = new Cliente
                {
                    Id = Guid.NewGuid(),
                    Nome = $"Cliente Teste {i}",
                    TipoCliente = i % 2 == 0 ? TipoCliente.PessoaFisica : TipoCliente.PessoaJuridico,
                    Documento = i % 2 == 0 ? $"1234567890{i}" : $"1234567800019{i}",
                    Endereco = new Endereco
                    {
                        Id = Guid.NewGuid(),
                        Rua = $"Rua Teste {i}",
                        Numero = $"{i}00",
                        Bairro = $"Bairro {i}",
                        Cidade = "São Paulo",
                                CEP = $"0123456{i}"
                    },
                    Contato = new Contato
                    {
                        Id = Guid.NewGuid(),
                        Email = $"cliente{i}@teste.com",
                        Telefone = $"1199999999{i}"
                    },
                    DataCadastro = DateTime.Now,
                    DataAtualizacao = DateTime.Now,
                    Ativo = true
                };
                
                clientes.Add(cliente);
            }
            
            return clientes;
        }
    }
}
