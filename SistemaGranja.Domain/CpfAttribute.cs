using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SistemaGranja.Domain
{
    // Valida um CPF pelos dígitos verificadores. Aceita com ou sem pontuação.
    public class CpfAttribute : ValidationAttribute
    {
        public CpfAttribute()
        {
            ErrorMessage = "CPF inválido.";
        }

        public override bool IsValid(object? value)
        {
            if (value is not string texto || string.IsNullOrWhiteSpace(texto))
                return true; // campo vazio é tratado pelo [Required]

            return EhValido(texto);
        }

        public static string SomenteNumeros(string? texto) =>
            new string((texto ?? string.Empty).Where(char.IsDigit).ToArray());

        public static string Formatar(string? cpf)
        {
            var numeros = SomenteNumeros(cpf);
            return numeros.Length == 11
                ? $"{numeros[..3]}.{numeros[3..6]}.{numeros[6..9]}-{numeros[9..]}"
                : cpf ?? string.Empty;
        }

        public static bool EhValido(string texto)
        {
            var cpf = SomenteNumeros(texto);
            if (cpf.Length != 11) return false;

            // 000.000.000-00, 111.111.111-11... passam no cálculo, mas não existem
            if (cpf.Distinct().Count() == 1) return false;

            int CalcularDigito(int quantidade)
            {
                var soma = 0;
                for (var i = 0; i < quantidade; i++)
                    soma += (cpf[i] - '0') * (quantidade + 1 - i);
                var resto = soma % 11;
                return resto < 2 ? 0 : 11 - resto;
            }

            return CalcularDigito(9) == cpf[9] - '0'
                && CalcularDigito(10) == cpf[10] - '0';
        }
    }
}
