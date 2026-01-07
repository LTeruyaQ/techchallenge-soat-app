// src/API/Controllers/ClienteAutenticacaoController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Core.DTOs.Requests.Autenticacao;
using Core.DTOs.Responses.Autenticacao;
using Core.Interfaces.Gateways;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClienteAutenticacaoController : ControllerBase
    {
        private readonly IClienteGateway _clienteGateway;
        private readonly IConfiguration _configuration;

        public ClienteAutenticacaoController(IClienteGateway clienteGateway, IConfiguration configuration)
        {
            _clienteGateway = clienteGateway;
            _configuration = configuration;
        }

        [HttpPost("login-cliente")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginClienteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginClienteRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Cpf))
            {
                return BadRequest("CPF é obrigatório.");
            }

            var cliente = await _clienteGateway.ObterClientePorDocumentoAsync(request.Cpf);

            if (cliente == null || !cliente.Ativo)
            {
                return Unauthorized("Cliente não encontrado ou inativo.");
            }

            var token = GerarTokenJwt(cliente.Documento);

            return Ok(new LoginClienteResponse
            {
                Token = token.Token,
                Validade = token.Validade
            });
        }

        private LoginClienteResponse GerarTokenJwt(string cpf)
        {
            var jwtSecret = _configuration["Jwt:SecretKey"];
            if(string.IsNullOrEmpty(jwtSecret))
            {
                throw new InvalidOperationException("O segredo JWT não está configurado.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.UtcNow.AddHours(1);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, cpf),
                new Claim("cpf", cpf),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "MecanicaOS",
                audience: "MecanicaOS-API",
                claims: claims,
                expires: expiry,
                signingCredentials: creds
            );

            return new LoginClienteResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Validade = expiry
            };
        }
    }
}
