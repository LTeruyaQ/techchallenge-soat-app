using Core.Validacoes;

namespace MecanicaOS.UnitTests.Core.Validacoes
{
    /// <summary>
    /// Testes para validadores de CPF e CNPJ
    /// ROI CRÍTICO: Validação de documentos é essencial para integridade de dados.
    /// Importância: Previne cadastro de documentos inválidos no sistema.
    /// </summary>
    public class ValidadoresTests
    {
        #region ValidadorCpf

        /// <summary>
        /// Verifica se ValidadorCpf aceita CPF válido sem formatação
        /// Importância: CRÍTICA - Validação correta de CPF é fundamental
        /// Contribuição: Garante que CPFs válidos são aceitos
        /// </summary>
        [Theory]
        [InlineData("12345678909")]
        [InlineData("11144477735")]
        [InlineData("52998224725")]
        public void ValidadorCpf_ComCpfValido_DeveRetornarTrue(string cpf)
        {
            // Act
            var resultado = ValidadorCpf.Valido(cpf);

            // Assert
            resultado.Should().BeTrue();
        }

        /// <summary>
        /// Verifica se ValidadorCpf aceita CPF válido com formatação
        /// Importância: ALTA - Sistema deve aceitar CPF formatado
        /// Contribuição: Melhora UX permitindo entrada formatada
        /// </summary>
        [Theory]
        [InlineData("123.456.789-09")]
        [InlineData("111.444.777-35")]
        [InlineData("529.982.247-25")]
        public void ValidadorCpf_ComCpfValidoFormatado_DeveRetornarTrue(string cpf)
        {
            // Act
            var resultado = ValidadorCpf.Valido(cpf);

            // Assert
            resultado.Should().BeTrue();
        }

        /// <summary>
        /// Verifica se ValidadorCpf rejeita CPF inválido
        /// Importância: CRÍTICA - Deve rejeitar CPFs com dígitos verificadores errados
        /// Contribuição: Previne cadastro de documentos inválidos
        /// </summary>
        [Theory]
        [InlineData("12345678900")]
        [InlineData("11111111111")]
        [InlineData("00000000000")]
        [InlineData("99999999999")]
        public void ValidadorCpf_ComCpfInvalido_DeveRetornarFalse(string cpf)
        {
            // Act
            var resultado = ValidadorCpf.Valido(cpf);

            // Assert
            resultado.Should().BeFalse();
        }

        /// <summary>
        /// Verifica se ValidadorCpf rejeita CPF com tamanho incorreto
        /// Importância: ALTA - Deve validar tamanho do CPF
        /// Contribuição: Previne entrada de dados malformados
        /// </summary>
        [Theory]
        [InlineData("123")]
        [InlineData("123456789")]
        [InlineData("123456789012")]
        public void ValidadorCpf_ComTamanhoIncorreto_DeveRetornarFalse(string cpf)
        {
            // Act
            var resultado = ValidadorCpf.Valido(cpf);

            // Assert
            resultado.Should().BeFalse();
        }

        /// <summary>
        /// Verifica se ValidadorCpf rejeita CPF nulo ou vazio
        /// Importância: ALTA - Deve tratar entrada inválida
        /// Contribuição: Previne NullReferenceException
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidadorCpf_ComCpfNuloOuVazio_DeveRetornarFalse(string cpf)
        {
            // Act
            var resultado = ValidadorCpf.Valido(cpf);

            // Assert
            resultado.Should().BeFalse();
        }

        #endregion

        #region ValidadorCnpj

        /// <summary>
        /// Verifica se ValidadorCnpj aceita CNPJ válido sem formatação
        /// Importância: CRÍTICA - Validação correta de CNPJ é fundamental
        /// Contribuição: Garante que CNPJs válidos são aceitos
        /// </summary>
        [Theory]
        [InlineData("11222333000181")]
        [InlineData("11444777000161")]
        public void ValidadorCnpj_ComCnpjValido_DeveRetornarTrue(string cnpj)
        {
            // Act
            var resultado = ValidadorCnpj.Valido(cnpj);

            // Assert
            resultado.Should().BeTrue();
        }

        /// <summary>
        /// Verifica se ValidadorCnpj aceita CNPJ válido com formatação
        /// Importância: ALTA - Sistema deve aceitar CNPJ formatado
        /// Contribuição: Melhora UX permitindo entrada formatada
        /// </summary>
        [Theory]
        [InlineData("11.222.333/0001-81")]
        [InlineData("11.444.777/0001-61")]
        public void ValidadorCnpj_ComCnpjValidoFormatado_DeveRetornarTrue(string cnpj)
        {
            // Act
            var resultado = ValidadorCnpj.Valido(cnpj);

            // Assert
            resultado.Should().BeTrue();
        }

        /// <summary>
        /// Verifica se ValidadorCnpj rejeita CNPJ inválido
        /// Importância: CRÍTICA - Deve rejeitar CNPJs com dígitos verificadores errados
        /// Contribuição: Previne cadastro de documentos inválidos
        /// </summary>
        [Theory]
        [InlineData("11222333000180")]
        [InlineData("11111111111111")]
        [InlineData("00000000000000")]
        [InlineData("99999999999999")]
        public void ValidadorCnpj_ComCnpjInvalido_DeveRetornarFalse(string cnpj)
        {
            // Act
            var resultado = ValidadorCnpj.Valido(cnpj);

            // Assert
            resultado.Should().BeFalse();
        }

        /// <summary>
        /// Verifica se ValidadorCnpj rejeita CNPJ com tamanho incorreto
        /// Importância: ALTA - Deve validar tamanho do CNPJ
        /// Contribuição: Previne entrada de dados malformados
        /// </summary>
        [Theory]
        [InlineData("123")]
        [InlineData("12345678901")]
        [InlineData("123456789012345")]
        public void ValidadorCnpj_ComTamanhoIncorreto_DeveRetornarFalse(string cnpj)
        {
            // Act
            var resultado = ValidadorCnpj.Valido(cnpj);

            // Assert
            resultado.Should().BeFalse();
        }

        /// <summary>
        /// Verifica se ValidadorCnpj rejeita CNPJ nulo ou vazio
        /// Importância: ALTA - Deve tratar entrada inválida
        /// Contribuição: Previne NullReferenceException
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidadorCnpj_ComCnpjNuloOuVazio_DeveRetornarFalse(string cnpj)
        {
            // Act
            var resultado = ValidadorCnpj.Valido(cnpj);

            // Assert
            resultado.Should().BeFalse();
        }

        #endregion
    }
}
