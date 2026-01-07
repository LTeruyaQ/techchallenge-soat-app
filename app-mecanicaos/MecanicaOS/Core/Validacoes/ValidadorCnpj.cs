namespace Core.Validacoes;

public static class ValidadorCnpj
{
    public static bool Valido(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return false;

        cnpj = new string([.. cnpj.Where(char.IsDigit)]);

        if (cnpj.Length != 14 || cnpj.All(c => c == cnpj[0]))
            return false;

        var numeros = cnpj.Select(c => int.Parse(c.ToString())).ToArray();

        int[] primeirosPesos = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] segundosPesos = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        int soma = 0;
        for (int i = 0; i < 12; i++)
            soma += numeros[i] * primeirosPesos[i];

        int restante = soma % 11;
        int digito1 = restante < 2 ? 0 : 11 - restante;

        if (numeros[12] != digito1)
            return false;

        soma = 0;
        for (int i = 0; i < 13; i++)
            soma += numeros[i] * segundosPesos[i];

        restante = soma % 11;
        int digito2 = restante < 2 ? 0 : 11 - restante;

        return numeros[13] == digito2;
    }
}