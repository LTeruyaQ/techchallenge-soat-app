using Core.Entidades;
using Core.Enumeradores;

namespace MecanicaOS.UnitTests.Fixtures
{
    /// <summary>
    /// Fixture para criação de objetos Cliente para testes
    /// </summary>
    public static class ClienteFixture
    {
        /// <summary>
        /// Cria um cliente válido do tipo pessoa física para testes
        /// </summary>
        /// <returns>Uma instância de Cliente com dados válidos</returns>
        public static Cliente CriarClienteValido()
        {
            return new Cliente
            {
                Nome = "Cliente Teste",
                TipoCliente = TipoCliente.PessoaFisica,
                Documento = "12345678900",
                Contato = new Contato
                {
                    Email = "cliente@teste.com",
                    Telefone = "11999999999"
                },
                Endereco = new Endereco
                {
                    Rua = "Rua Teste",
                    Numero = "123",
                    Bairro = "Bairro Teste",
                    Cidade = "São Paulo",
                    CEP = "01234567"
                }
            };
        }

        /// <summary>
        /// Cria um cliente válido do tipo pessoa jurídica para testes
        /// </summary>
        /// <returns>Uma instância de Cliente com dados válidos de pessoa jurídica</returns>
        public static Cliente CriarClientePessoaJuridica()
        {
            return new Cliente
            {
                Nome = "Empresa Teste LTDA",
                TipoCliente = TipoCliente.PessoaJuridico,
                Documento = "12345678000199",
                Contato = new Contato
                {
                    Email = "contato@empresateste.com",
                    Telefone = "1133333333"
                },
                Endereco = new Endereco
                {
                    Rua = "Avenida Empresarial",
                    Numero = "1000",
                    Bairro = "Centro Empresarial",
                    Cidade = "São Paulo",
                    CEP = "04567890"
                }
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
                    Nome = $"Cliente Teste {i}",
                    TipoCliente = i % 2 == 0 ? TipoCliente.PessoaFisica : TipoCliente.PessoaJuridico,
                    Documento = i % 2 == 0 ? $"1234567890{i}" : $"1234567800019{i}",
                    Contato = new Contato
                    {
                        Email = $"cliente{i}@teste.com",
                        Telefone = $"1199999999{i}"
                    },
                    Endereco = new Endereco
                    {
                        Rua = $"Rua Teste {i}",
                        Numero = $"{i}00",
                        Bairro = $"Bairro {i}",
                        Cidade = "São Paulo",
                            CEP = $"0123456{i}"
                    }
                };
                
                clientes.Add(cliente);
            }
            
            return clientes;
        }
    }
}
