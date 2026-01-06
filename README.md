# MecanicaOS - Tech Challenge SOAT

Este projeto é uma solução completa para o Tech Challenge, implantado inteiramente na AWS. Ele utiliza uma arquitetura moderna e automatizada, combinando EKS, RDS, API Gateway e Lambda para fornecer uma API robusta e escalável para o sistema de gerenciamento de oficinas mecânicas MecanicaOS.

## Arquitetura da Solução

A arquitetura foi projetada para ser resiliente, segura e de baixo custo, ideal para o ambiente AWS Academy.

1.  **AWS API Gateway (HTTP API)**:
    *   Atua como o ponto de entrada único para todas as requisições.
    *   É responsável pelo roteamento e pela primeira camada de segurança.

2.  **AWS Lambda (Autorizador)**:
    *   Uma função Python que intercepta requisições para rotas protegidas.
    *   Valida o CPF do cliente (fornecido no header `x-cpf`).
    *   Conecta-se ao banco de dados RDS para verificar se o cliente existe e está ativo.
    *   Se a validação for bem-sucedida, gera um token JWT (JSON Web Token) e permite o acesso.
    *   Caso contrário, nega o acesso.

3.  **Amazon EKS (Elastic Kubernetes Service)**:
    *   Hospeda a aplicação principal da API MecanicaOS.
    *   O serviço da API é exposto dentro da VPC através de um Network Load Balancer.
    *   O API Gateway encaminha as requisições autorizadas para este Load Balancer.

4.  **Amazon RDS (Relational Database Service)**:
    *   Fornece um banco de dados PostgreSQL gerenciado.
    *   A instância do RDS está localizada em subnets privadas para máxima segurança, sem acesso direto da internet.
    *   A comunicação ocorre apenas com os recursos dentro da VPC (Lambda e EKS).

5.  **Amazon ECR (Elastic Container Registry)**:
    *   Armazena a imagem Docker da aplicação MecanicaOS API.

## Fluxo de Autenticação

O fluxo de autenticação é simples e seguro, utilizando um modelo "CPF-first" para autorização.

1.  O cliente (frontend, Postman, etc.) faz uma requisição para uma rota protegida no API Gateway.
2.  A requisição **deve** incluir um header `x-cpf` com o CPF do cliente.
3.  O API Gateway aciona o autorizador Lambda, passando as informações da requisição.
4.  O Lambda:
    a. Extrai o CPF do header.
    b. Conecta-se ao banco de dados RDS.
    c. Procura por um cliente com o CPF fornecido e que tenha o status 'ATIVO'.
    d. Se o cliente for válido, gera um token JWT de curta duração.
    e. Retorna uma política `Allow` (Permitir) para o API Gateway, incluindo o token JWT no contexto da resposta.
5.  O API Gateway recebe a política de permissão e encaminha a requisição original para o serviço da API rodando no EKS.
6.  O token JWT retornado pelo autorizador pode ser utilizado em requisições subsequentes, se a API interna o exigir.

## Como Subir a Solução (Deploy Completo)

O processo de deploy é 100% automatizado através de um único script PowerShell.

**Pré-requisitos:**

*   AWS CLI configurado com credenciais de administrador.
*   Terraform instalado.
*   kubectl instalado.
*   Docker Desktop rodando.
*   Git instalado.

**Comando de Deploy:**

Para implantar toda a infraestrutura e a aplicação, navegue até o diretório `terraform` e execute o seguinte comando:

```powershell
./deploy-completo.ps1
```

O script cuidará de tudo:
1.  Fará o build da imagem Docker da aplicação.
2.  Enviará a imagem para o repositório ECR.
3.  Executará o `terraform apply` para criar ou atualizar toda a infraestrutura na AWS (VPC, EKS, RDS, Lambda, API Gateway).
4.  Configurará seu `kubectl` para conectar-se ao novo cluster EKS.
5.  Implantará a aplicação no Kubernetes, injetando a connection string do RDS de forma segura.

Ao final, o script exibirá a URL pública do API Gateway, que você poderá usar para acessar o Swagger UI e testar a API.

**Para Destruir a Infraestrutura:**

Para remover todos os recursos criados na AWS e evitar custos, execute:

```powershell
./deploy-completo.ps1 -Destroy
```
