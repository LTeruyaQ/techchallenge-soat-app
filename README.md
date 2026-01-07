# MecanicaOS: Sistema de Gerenciamento para Oficinas - Tech Challenge

## 1. Visão Geral

O **MecanicaOS** é uma solução completa para gerenciamento de oficinas mecânicas, projetada para otimizar a rotina de pequenas e médias empresas. O sistema foi desenvolvido como parte de um Tech Challenge, com foco em **automação total da infraestrutura e do deploy** em um ambiente Cloud Native na AWS.

O projeto implementa uma API RESTful robusta e escalável, totalmente provisionada e implantada com um **único comando**, sem nenhuma intervenção manual.

---

## 2. Arquitetura da Solução

A arquitetura foi desenhada para ser resiliente, segura e totalmente automatizada, utilizando os melhores serviços da AWS para cada função.

```mermaid
graph TD
    subgraph "Usuário Final"
        A[Cliente via Browser/Mobile] --> B{AWS API Gateway};
    end

    subgraph "AWS Cloud"
        B -- "CPF na primeira chamada" --> C[Lambda Authorizer];
        C -- "Valida CPF" --> D[AWS RDS PostgreSQL];
        C -- "Gera JWT" --> A;

        B -- "Proxy com JWT" --> E[AWS EKS];

        subgraph E["Amazon EKS Cluster"]
            direction LR
            F[Load Balancer] --> G[API Service];
            G --> H[Pods da API .NET];
        end

        H -- "Conecta" --> D;
    end

    subgraph "CI/CD & Automação"
        I[Script deploy-completo.ps1] --> J[Terraform];
        J -- "Provisiona" --> B;
        J -- "Provisiona" --> C;
        J -- "Provisiona" -- "Cria credenciais" --> D;
        J -- "Provisiona" --> E;

        I -- "Build & Push" --> K[AWS ECR];
        K -- "Imagem Docker" --> H;
    end

    style D fill:#add,stroke:#333,stroke-width:2px
```

### Componentes Principais:
- **AWS API Gateway (HTTP API):** Atua como o ponto de entrada único para todas as requisições. É responsável por receber o tráfego, encaminhar para o autorizador e fazer proxy para o serviço rodando no EKS.
- **AWS Lambda Authorizer:** Função serverless que centraliza a lógica de autenticação.
    1. Recebe um CPF.
    2. Valida o cliente no banco de dados RDS.
    3. Gera um JWT (JSON Web Token) válido se o cliente for ativo.
    - Para requisições subsequentes, a Lambda valida o JWT antes de permitir o acesso à API.
- **Amazon RDS for PostgreSQL:** Banco de dados relacional gerenciado, executando em subnets privadas para máxima segurança. É acessado tanto pela Lambda quanto pela API no EKS.
- **Amazon EKS (Elastic Kubernetes Service):** Orquestra os contêineres da aplicação .NET. O cluster é provisionado automaticamente, e a aplicação é implantada com escalabilidade automática (HPA).
- **Amazon ECR (Elastic Container Registry):** Repositório privado para as imagens Docker da aplicação.
- **Terraform:** Ferramenta de Infraestrutura como Código (IaC) utilizada para provisionar e gerenciar todos os recursos na AWS de forma declarativa.
- **Script de Automação (`deploy-completo.ps1`):** Orquestrador do processo de ponta a ponta. Automatiza o build da imagem, o push para o ECR, a execução do Terraform e a configuração do `kubectl`.

---

## 3. Endpoints da API

A documentação completa da API está disponível via Swagger após o deploy. O endpoint principal para autenticação é:

| Verbo | Endpoint | Descrição | Corpo da Requisição (JSON) |
|---|---|---|---|
| **POST** | `/auth` | **Autenticação:** Gera um token JWT para um cliente ativo. | `{"cpf": "12345678901"}` |

Todas as demais rotas são protegidas e exigem o `Authorization: Bearer <token>` no cabeçalho.

---

## 4. Deploy Automatizado (UM Comando)

Todo o ambiente, desde a rede VPC até a aplicação rodando, é provisionado com um único script. **Nenhuma configuração manual é necessária.**

### 4.1. Pré-requisitos
- [Terraform CLI](https://learn.hashicorp.com/tutorials/terraform/install-cli) (>= 1.5.0)
- [AWS CLI](https://aws.amazon.com/cli/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [kubectl](https://kubernetes.io/docs/tasks/tools/)
- PowerShell (para o script de deploy)

### 4.2. Execução

1. **Configure suas credenciais da AWS.**
   - Para o ambiente AWS Academy, configure as credenciais de sessão temporárias.
   - Para uma conta AWS normal, use `aws configure`.

2. **Execute o script de deploy.**
   - Navegue até o diretório `terraform/` e execute:
     ```powershell
     .\deploy-completo.ps1
     ```

O script executará todas as etapas necessárias:
- **Verificação** do ambiente e ferramentas.
- **Descoberta automática** das IAM Roles do AWS Academy.
- **Build e Push** da imagem Docker da API para o ECR.
- **Provisionamento** de toda a infraestrutura com Terraform.
- **Configuração** do `kubectl` para o novo cluster.

Ao final, o script exibirá a **URL do API Gateway** e o **link para o Swagger**.

### 4.3. Destruindo o Ambiente

Para remover todos os recursos criados e evitar custos, execute o script com o parâmetro `-Destroy`:
```powershell
.\deploy-completo.ps1 -Destroy
```

---

## 5. Tecnologias Utilizadas

| Categoria | Tecnologia |
|---|---|
| **Cloud** | AWS (EKS, RDS, API Gateway, Lambda, ECR, VPC) |
| **Backend** | .NET 9.0, ASP.NET Core |
| **Arquitetura** | Clean Architecture, DDD |
| **Banco de Dados** | PostgreSQL |
| **ORM** | Entity Framework Core 9.0 |
| **Infra como Código**| Terraform |
| **Automação** | PowerShell |
| **Containerização** | Docker |
| **Orquestração** | Kubernetes |
| **Autenticação** | JWT (via Lambda Authorizer) |
| **Documentação da API**| Swagger (OpenAPI) |
