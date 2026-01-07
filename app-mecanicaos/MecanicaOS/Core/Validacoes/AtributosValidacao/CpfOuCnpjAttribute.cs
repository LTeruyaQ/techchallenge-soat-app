using System.ComponentModel.DataAnnotations;

namespace Core.Validacoes.AtributosValidacao;

public class CpfOuCnpjAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is not string doc || string.IsNullOrWhiteSpace(doc))
            return true;

        doc = new string([.. doc.Where(char.IsDigit)]);

        return ValidadorCpf.Valido(doc) || ValidadorCnpj.Valido(doc);
    }
}