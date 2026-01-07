using Core.Entidades;

namespace MecanicaOS.UnitTests.Fixtures
{
    /// <summary>
    /// Fixture para criação de objetos Estoque para testes
    /// </summary>
    public static class EstoqueFixture
    {
        /// <summary>
        /// Cria um estoque válido para testes
        /// </summary>
        /// <returns>Uma instância de Estoque com dados válidos</returns>
        public static Estoque CriarEstoqueValido()
        {
            return new Estoque
            {
                Id = Guid.NewGuid(),
                Insumo = "Óleo de Motor 5W30",
                Descricao = "Óleo sintético para motores a gasolina",
                Preco = 45.90m,
                QuantidadeDisponivel = 20,
                QuantidadeMinima = 5,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now,
                Ativo = true
            };
        }

        /// <summary>
        /// Cria um estoque crítico (abaixo da quantidade mínima) para testes
        /// </summary>
        /// <returns>Uma instância de Estoque com quantidade abaixo do mínimo</returns>
        public static Estoque CriarEstoqueCritico()
        {
            return new Estoque
            {
                Id = Guid.NewGuid(),
                Insumo = "Filtro de Óleo",
                Descricao = "Filtro de óleo para motores a gasolina",
                Preco = 25.50m,
                QuantidadeDisponivel = 2,
                QuantidadeMinima = 5,
                DataCadastro = DateTime.Now,
                DataAtualizacao = DateTime.Now,
                Ativo = true
            };
        }

        /// <summary>
        /// Cria uma lista de estoques para testes
        /// </summary>
        /// <param name="quantidade">Quantidade de estoques a serem criados</param>
        /// <returns>Lista de estoques</returns>
        public static List<Estoque> CriarListaEstoques(int quantidade = 3)
        {
            var estoques = new List<Estoque>();
            
            for (int i = 0; i < quantidade; i++)
            {
                var estoque = new Estoque
                {
                    Id = Guid.NewGuid(),
                    Insumo = $"Insumo {i}",
                    Descricao = $"Descrição do insumo {i}",
                    Preco = 10.00m * (i + 1),
                    QuantidadeDisponivel = 10 * (i + 1),
                    QuantidadeMinima = 5,
                    DataCadastro = DateTime.Now,
                    DataAtualizacao = DateTime.Now,
                    Ativo = true
                };
                
                estoques.Add(estoque);
            }
            
            return estoques;
        }

        /// <summary>
        /// Cria uma lista de estoques críticos para testes
        /// </summary>
        /// <param name="quantidade">Quantidade de estoques a serem criados</param>
        /// <returns>Lista de estoques críticos</returns>
        public static List<Estoque> CriarListaEstoquesCriticos(int quantidade = 3)
        {
            var estoques = new List<Estoque>();
            
            for (int i = 0; i < quantidade; i++)
            {
                var estoque = new Estoque
                {
                    Id = Guid.NewGuid(),
                    Insumo = $"Insumo Crítico {i}",
                    Descricao = $"Descrição do insumo crítico {i}",
                    Preco = 10.00m * (i + 1),
                    QuantidadeDisponivel = i, // Quantidade abaixo do mínimo
                    QuantidadeMinima = 5,
                    DataCadastro = DateTime.Now,
                    DataAtualizacao = DateTime.Now,
                    Ativo = true
                };
                
                estoques.Add(estoque);
            }
            
            return estoques;
        }
    }
}
