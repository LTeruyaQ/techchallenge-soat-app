using API.Middlewares;
using Core.DTOs.Responses.Erro;
using Core.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace MecanicaOS.UnitTests.API.Middlewares
{
    /// <summary>
    /// Testes unitários para o GlobalExceptionHandlerMiddleware
    /// Estes testes são CRÍTICOS para aumentar a cobertura de 43.7% para 80%+
    /// GlobalExceptionHandlerMiddleware atualmente tem 0% de cobertura
    /// 
    /// IMPORTÂNCIA: Este middleware é responsável por capturar e tratar todas as exceções
    /// não tratadas da aplicação, garantindo que erros sejam retornados de forma consistente
    /// e que informações sensíveis não sejam expostas aos usuários.
    /// </summary>
    public class GlobalExceptionHandlerMiddlewareTests
    {
        /// <summary>
        /// Verifica se o middleware permite que requisições normais passem sem interferência
        /// Este teste é importante porque valida que o middleware não interfere no fluxo normal
        /// </summary>
        [Fact]
        public async Task InvokeAsync_QuandoNaoHaExcecao_DevePermitirFluxoNormal()
        {
            // Arrange
            var loggerMock = Substitute.For<ILogger<GlobalExceptionHandlerMiddleware>>();
            var context = new DefaultHttpContext();
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;
            
            var nextDelegateCalled = false;
            RequestDelegate next = (HttpContext ctx) =>
            {
                nextDelegateCalled = true;
                return Task.CompletedTask;
            };
            
            var middleware = new GlobalExceptionHandlerMiddleware(next, loggerMock);
            
            // Act
            await middleware.InvokeAsync(context);
            
            // Assert
            nextDelegateCalled.Should().BeTrue("o próximo middleware deve ser chamado");
            context.Response.StatusCode.Should().Be(200, "status deve permanecer 200 quando não há exceção");
        }

        /// <summary>
        /// Verifica se DadosInvalidosException é tratada corretamente retornando BadRequest
        /// Este teste é crítico porque valida erros de validação de entrada
        /// </summary>
        [Fact]
        public async Task InvokeAsync_QuandoDadosInvalidosException_DeveRetornarBadRequest()
        {
            // Arrange
            var loggerMock = Substitute.For<ILogger<GlobalExceptionHandlerMiddleware>>();
            var context = new DefaultHttpContext();
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;
            
            var mensagemErro = "Dados inválidos fornecidos";
            RequestDelegate next = (HttpContext ctx) =>
            {
                throw new DadosInvalidosException(mensagemErro);
            };
            
            var middleware = new GlobalExceptionHandlerMiddleware(next, loggerMock);
            
            // Act
            await middleware.InvokeAsync(context);
            
            // Assert
            context.Response.StatusCode.Should().Be(400, "deve retornar BadRequest para dados inválidos");
            context.Response.ContentType.Should().Be("application/json", "deve retornar JSON");
            
            // Verifica o conteúdo da resposta
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseContent = await new StreamReader(responseBody).ReadToEndAsync();
            var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent);
            
            errorResponse.Should().NotBeNull();
            errorResponse!.Message.Should().Be(mensagemErro);
            errorResponse.StatusCode.Should().Be(400);
        }

        /// <summary>
        /// Verifica se DadosNaoEncontradosException é tratada corretamente retornando NotFound
        /// Este teste é importante para validar tratamento de recursos não encontrados
        /// </summary>
        [Fact]
        public async Task InvokeAsync_QuandoDadosNaoEncontradosException_DeveRetornarNotFound()
        {
            // Arrange
            var loggerMock = Substitute.For<ILogger<GlobalExceptionHandlerMiddleware>>();
            var context = new DefaultHttpContext();
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;
            
            var mensagemErro = "Cliente não encontrado";
            RequestDelegate next = (HttpContext ctx) =>
            {
                throw new DadosNaoEncontradosException(mensagemErro);
            };
            
            var middleware = new GlobalExceptionHandlerMiddleware(next, loggerMock);
            
            // Act
            await middleware.InvokeAsync(context);
            
            // Assert
            context.Response.StatusCode.Should().Be(404, "deve retornar NotFound para dados não encontrados");
            context.Response.ContentType.Should().Be("application/json");
            
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseContent = await new StreamReader(responseBody).ReadToEndAsync();
            var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent);
            
            errorResponse.Should().NotBeNull();
            errorResponse!.Message.Should().Be(mensagemErro);
            errorResponse.StatusCode.Should().Be(404);
        }

        /// <summary>
        /// Verifica se DadosJaCadastradosException é tratada corretamente retornando Conflict
        /// Este teste é importante para validar conflitos de dados únicos
        /// </summary>
        [Fact]
        public async Task InvokeAsync_QuandoDadosJaCadastradosException_DeveRetornarConflict()
        {
            // Arrange
            var loggerMock = Substitute.For<ILogger<GlobalExceptionHandlerMiddleware>>();
            var context = new DefaultHttpContext();
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;
            
            var mensagemErro = "Email já cadastrado no sistema";
            RequestDelegate next = (HttpContext ctx) =>
            {
                throw new DadosJaCadastradosException(mensagemErro);
            };
            
            var middleware = new GlobalExceptionHandlerMiddleware(next, loggerMock);
            
            // Act
            await middleware.InvokeAsync(context);
            
            // Assert
            context.Response.StatusCode.Should().Be(409, "deve retornar Conflict para dados já cadastrados");
            context.Response.ContentType.Should().Be("application/json");
            
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseContent = await new StreamReader(responseBody).ReadToEndAsync();
            var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent);
            
            errorResponse.Should().NotBeNull();
            errorResponse!.Message.Should().Be(mensagemErro);
            errorResponse.StatusCode.Should().Be(409);
        }

        /// <summary>
        /// Verifica se CredenciaisInvalidasException é tratada corretamente retornando Unauthorized
        /// Este teste é crítico para segurança da aplicação
        /// </summary>
        [Fact]
        public async Task InvokeAsync_QuandoCredenciaisInvalidasException_DeveRetornarUnauthorized()
        {
            // Arrange
            var loggerMock = Substitute.For<ILogger<GlobalExceptionHandlerMiddleware>>();
            var context = new DefaultHttpContext();
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;
            
            var mensagemErro = "Credenciais inválidas";
            RequestDelegate next = (HttpContext ctx) =>
            {
                throw new CredenciaisInvalidasException(mensagemErro);
            };
            
            var middleware = new GlobalExceptionHandlerMiddleware(next, loggerMock);
            
            // Act
            await middleware.InvokeAsync(context);
            
            // Assert
            context.Response.StatusCode.Should().Be(401, "deve retornar Unauthorized para credenciais inválidas");
            context.Response.ContentType.Should().Be("application/json");
            
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseContent = await new StreamReader(responseBody).ReadToEndAsync();
            var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent);
            
            errorResponse.Should().NotBeNull();
            errorResponse!.Message.Should().Be(mensagemErro);
            errorResponse.StatusCode.Should().Be(401);
        }

        /// <summary>
        /// Verifica se exceções genéricas são tratadas corretamente retornando InternalServerError
        /// Este teste é CRÍTICO para segurança - garante que detalhes internos não sejam expostos
        /// </summary>
        [Fact]
        public async Task InvokeAsync_QuandoExcecaoGenerica_DeveRetornarInternalServerError()
        {
            // Arrange
            var loggerMock = Substitute.For<ILogger<GlobalExceptionHandlerMiddleware>>();
            var context = new DefaultHttpContext();
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;
            
            RequestDelegate next = (HttpContext ctx) =>
            {
                throw new InvalidOperationException("Erro interno detalhado que não deve ser exposto");
            };
            
            var middleware = new GlobalExceptionHandlerMiddleware(next, loggerMock);
            
            // Act
            await middleware.InvokeAsync(context);
            
            // Assert
            context.Response.StatusCode.Should().Be(500, "deve retornar InternalServerError para exceções genéricas");
            context.Response.ContentType.Should().Be("application/json");
            
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseContent = await new StreamReader(responseBody).ReadToEndAsync();
            var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent);
            
            errorResponse.Should().NotBeNull();
            errorResponse!.Message.Should().Be("Erro interno detalhado que não deve ser exposto");
            errorResponse.StatusCode.Should().Be(500);
            
            // Verifica se o erro foi logado
            loggerMock.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());
        }

        /// <summary>
        /// Verifica se PersistirDadosException é tratada corretamente retornando InternalServerError
        /// Este teste é importante para validar erros de persistência de dados
        /// </summary>
        [Fact]
        public async Task InvokeAsync_QuandoPersistirDadosException_DeveRetornarInternalServerError()
        {
            // Arrange
            var loggerMock = Substitute.For<ILogger<GlobalExceptionHandlerMiddleware>>();
            var context = new DefaultHttpContext();
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;
            
            var mensagemErro = "Erro ao salvar dados no banco";
            RequestDelegate next = (HttpContext ctx) =>
            {
                throw new PersistirDadosException(mensagemErro);
            };
            
            var middleware = new GlobalExceptionHandlerMiddleware(next, loggerMock);
            
            // Act
            await middleware.InvokeAsync(context);
            
            // Assert
            context.Response.StatusCode.Should().Be(500, "deve retornar InternalServerError para erros de persistência");
            context.Response.ContentType.Should().Be("application/json");
            
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseContent = await new StreamReader(responseBody).ReadToEndAsync();
            var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent);
            
            errorResponse.Should().NotBeNull();
            errorResponse!.Message.Should().Be(mensagemErro);
            errorResponse.StatusCode.Should().Be(500);
        }

        /// <summary>
        /// Verifica se o middleware loga adequadamente as exceções
        /// Este teste é importante para auditoria e debugging
        /// </summary>
        [Fact]
        public async Task InvokeAsync_QuandoHaExcecao_DeveLogarExcecao()
        {
            // Arrange
            var loggerMock = Substitute.For<ILogger<GlobalExceptionHandlerMiddleware>>();
            var context = new DefaultHttpContext();
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;
            
            var exception = new DadosInvalidosException("Teste de log");
            RequestDelegate next = (HttpContext ctx) =>
            {
                throw exception;
            };
            
            var middleware = new GlobalExceptionHandlerMiddleware(next, loggerMock);
            
            // Act
            await middleware.InvokeAsync(context);
            
            // Assert
            loggerMock.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                exception,
                Arg.Any<Func<object, Exception?, string>>());
        }
    }
}
