// src/Core/DTOs/Requests/Autenticacao/LoginClienteRequest.cs
using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Requests.Autenticacao
{
    public class LoginClienteRequest
    {
        [Required]
        public string Cpf { get; set; }
    }
}
