# MecanicaOS - Testes Unitários

Este projeto contém os testes unitários para o sistema MecanicaOS, implementados com xUnit, FluentAssertions e NSubstitute.

## Status Atual
✅ **289 testes aprovados** com 100% de sucesso  
✅ Testes funcionais completos para todas as entidades e RepositoryDTOs  
✅ Validação completa da arquitetura RepositoryDTO  
✅ Cobertura completa das entidades de domínio restantes

## Estrutura do Projeto

```
MecanicaOS.UnitTests/
├── Core/
│   ├── Entidades/          # Testes das entidades de domínio
│   └── DTOs/              # Testes dos DTOs de repositório
├── Fixtures/              # Classes para criação de dados de teste
└── Helpers/               # Utilitários e extensões para testes
```

## Dependências

- **xUnit**: Framework de testes
- **FluentAssertions**: Assertions expressivas
- **NSubstitute**: Mocking framework
- **Microsoft.NET.Test.Sdk**: SDK de testes do .NET

## Características dos Testes

- **Mensagens em português**: Todas as mensagens de erro estão em português brasileiro
- **Fixtures reutilizáveis**: Classes para criação consistente de dados de teste
- **Testes parametrizados**: Uso de `[Theory]` para testar múltiplos cenários
- **Isolamento**: Cada teste é independente e não afeta outros testes
- **Validação de auditoria**: Campos técnicos (Id, DataCadastro, DataAtualizacao, Ativo) são testados

## Como Executar

```bash
# Executar todos os testes
dotnet test

# Executar com detalhes
dotnet test -v normal

# Executar testes específicos
dotnet test --filter "ClassName~ClienteUnitTests"
```

## Funcionalidades Testadas

### Entidades de Domínio
- ✅ **Cliente** (8 testes) - Criação, propriedades, ativação/desativação, igualdade
- ✅ **Veiculo** (12 testes) - Placa, marca, modelo, ano, cor, anotações, relacionamentos
- ✅ **Endereco** (12 testes) - Rua, bairro, cidade, número, CEP, complemento, relacionamentos
- ✅ **Contato** (12 testes) - Telefone, email, formatos diversos, relacionamentos
- ✅ **OrdemServico** (13 testes) - Criação, propriedades, atualizações, status, relacionamentos
- ✅ **Servico** (13 testes) - Nome, descrição, valor, disponibilidade, atualizações
- ✅ **Usuario** (16 testes) - Email, senha, tipo, alertas, último acesso, atualizações
- ✅ **Estoque** (14 testes) - Insumo, descrição, preço, quantidades, ativação
- ✅ **AlertaEstoque** (7 testes) - Referência ao estoque, ativação, comparações
- ✅ **InsumoOS** (12 testes) - Quantidade, referências, relacionamentos

### DTOs de Repositório
- ✅ **ClienteRepositoryDTO** (13 testes) - Herança, auditoria, propriedades, relacionamentos
- ✅ **VeiculoRepositoryDto** (12 testes) - Herança, auditoria, propriedades, formatos de placa
- ✅ **EnderecoRepositoryDto** (12 testes) - Herança, auditoria, propriedades, formatos de CEP
- ✅ **ContatoRepositoryDTO** (12 testes) - Herança, auditoria, propriedades, valores nulos
- ✅ **OrdemServicoRepositoryDto** (13 testes) - Herança, auditoria, relacionamentos, status
- ✅ **ServicoRepositoryDto** (10 testes) - Herança, auditoria, propriedades obrigatórias
- ✅ **UsuarioRepositoryDto** (14 testes) - Herança, auditoria, tipos, valores nulos
- ✅ **EstoqueRepositoryDto** (12 testes) - Herança, auditoria, quantidades, valores nulos
- ✅ **AlertaEstoqueRepositoryDto** (5 testes) - Herança, auditoria, referências
- ✅ **InsumoOSRepositoryDto** (11 testes) - Herança, auditoria, relacionamentos

### Cenários Cobertos
- ✅ Criação de entidades com valores padrão
- ✅ Validação de propriedades obrigatórias
- ✅ Testes de igualdade e comparação
- ✅ Ativação e desativação de entidades
- ✅ Preservação de campos técnicos de auditoria
- ✅ Validação de relacionamentos entre entidades
- ✅ Diferenças comportamentais entre entidades e DTOs
- ✅ Testes parametrizados para múltiplos valores

## Arquitetura Validada

Os testes confirmam que:
- Entidades herdam corretamente de `Entidade` base
- DTOs herdam corretamente de `RepositoryDto` base  
- Campos de auditoria são preservados adequadamente
- Relacionamentos entre entidades funcionam corretamente
- Comportamentos específicos (ativação/desativação) funcionam como esperado

## Próximos Passos

1. Implementar testes para as demais entidades (Veiculo, Endereco, Contato)
2. Criar testes para os casos de uso (Use Cases)
3. Implementar testes de integração para gateways e repositórios
4. Adicionar testes para controllers da API

## 🎯 Cenários de Teste

### Cenários Felizes (Happy Path)
- Criação de entidades e DTOs com dados válidos
- Validação de campos obrigatórios
- Preservação de campos técnicos de auditoria
- Associações entre entidades

### Cenários de Falha
- Validação com dados inválidos
- Campos obrigatórios vazios ou nulos
- Valores fora dos limites esperados
- Formatos inválidos (email, telefone, CEP)

## 🔧 Campos Técnicos Testados

Todos os testes verificam a preservação dos campos técnicos de auditoria:
- `Id`: GUID único
- `Ativo`: Status de ativação
- `DataCadastro`: Data de criação
- `DataAtualizacao`: Data da última modificação

## 🚀 Como Executar

```bash
# Executar todos os testes
dotnet test MecanicaOS.UnitTests.csproj

# Executar com verbosidade detalhada
dotnet test MecanicaOS.UnitTests.csproj -v detailed

# Executar testes específicos
dotnet test --filter "ClassName~ClienteTests"
```

## 📊 Cobertura de Testes

Os testes cobrem:
- **Entidades**: Validações de negócio e integridade de dados
- **RepositoryDTOs**: Preservação de campos técnicos e mapeamentos
- **Fixtures**: Criação consistente de dados de teste
- **Cenários de erro**: Validação de comportamentos inválidos

## 🌟 Características dos Testes

- **Mensagens em pt-BR**: Todas as assertions têm mensagens customizadas em português
- **Fixtures reutilizáveis**: Criação padronizada de objetos de teste
- **Testes parametrizados**: Uso de `Theory` para múltiplos cenários
- **Isolamento**: Cada teste é independente e pode ser executado isoladamente
- **Nomenclatura clara**: Padrão `Metodo_QuandoCondicao_DeveComportamento`

## 🔍 Exemplos de Uso

```csharp
[Fact]
public void Cliente_QuandoCriadoComDadosValidos_DeveSerValido()
{
    // Arrange & Act
    var cliente = ClienteFixture.CriarClienteValido();

    // Assert
    cliente.Should().NotBeNull("a entidade não deve ser nula");
    cliente.Nome.Should().NotBeNullOrEmpty("o nome não deve ser vazio ou nulo");
}
```

Este projeto garante a qualidade e confiabilidade da nova arquitetura com RepositoryDTOs.
