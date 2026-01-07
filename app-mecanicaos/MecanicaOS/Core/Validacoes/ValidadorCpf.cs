namespace Core.Validacoes;

public static class ValidadorCpf
{
    public static bool Valido(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        cpf = new string([.. cpf.Where(char.IsDigit)]);

        if (cpf.Length != 11 || cpf.All(c => c == cpf[0]))
            return false;

        var numeros = cpf.Select(c => int.Parse(c.ToString())).ToArray();

        int soma = 0;
        for (int i = 0; i < 9; i++)
            soma += numeros[i] * (10 - i);

        int restante = soma % 11;
        int digitoUm = restante < 2 ? 0 : 11 - restante;

        if (numeros[9] != digitoUm)
            return false;

        soma = 0;
        for (int i = 0; i < 10; i++)
            soma += numeros[i] * (11 - i);

        restante = soma % 11;
        int digit2 = restante < 2 ? 0 : 11 - restante;

        return numeros[10] == digit2;
    }
}