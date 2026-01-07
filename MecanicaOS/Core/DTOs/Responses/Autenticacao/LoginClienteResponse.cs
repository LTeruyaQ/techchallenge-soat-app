// src/Core/DTOs/Responses/Autenticacao/LoginClienteResponse.cs
namespace Core.DTOs.Responses.Autenticacao
{
    public class LoginClienteResponse
    {
        public string Token { get; set; }
        public DateTime Validade { get; set; }
    }
}
