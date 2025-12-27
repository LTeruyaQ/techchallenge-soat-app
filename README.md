# Tech Challenge: Infraestrutura - API Gateway

Este repositório é responsável pelo provisionamento e gerenciamento da infraestrutura do API Gateway da aplicação, utilizando Terraform para garantir uma abordagem de Infraestrutura como Código (IaC).

O API Gateway atua como a porta de entrada para todas as requisições, centralizando o controle de acesso, roteamento e segurança.

## Arquitetura e Fluxo de Autenticação

O diagrama abaixo ilustra como o API Gateway se integra com os outros componentes da arquitetura, incluindo a função Serverless de autenticação e a aplicação principal no Kubernetes.

```mermaid
graph TD
    subgraph "Usuário"
        A[Cliente (CPF)] --> B{API Gateway};
    end

    subgraph "AWS"
        B -- Rota de Autenticação --> C[Lambda (Serverless)];
        C -- Valida CPF --> D{Banco de Dados};
        D -- Retorna Status do Cliente --> C;
        C -- Gera Token JWT --> B;
        B -- Token JWT --> A;
        A -- Requisição com Token --> B;
        B -- Rota Protegida --> E[Aplicação (Kubernetes)];
    end

    style C fill:#FF9900,stroke:#333,stroke-width:2px
    style E fill:#3498DB,stroke:#333,stroke-width:2px
```

**Fluxo:**

1.  O cliente envia seu CPF para o endpoint de autenticação no API Gateway.
2.  O API Gateway aciona a função Lambda, que valida o CPF e consulta o status do cliente no banco de dados.
3.  Se o cliente for válido, a Lambda gera um token JWT e o retorna.
4.  O cliente utiliza o token JWT para acessar as rotas protegidas da aplicação.
5.  O API Gateway valida o token e encaminha a requisição para a aplicação principal no cluster Kubernetes.

---

## Tecnologias Utilizadas

| Categoria                | Tecnologia                     |
| ------------------------ | ------------------------------ |
| **Infra como Código**    | Terraform                      |
| **Nuvem**                | AWS (Amazon Web Services)      |
| **API Gateway**          | AWS API Gateway                |
| **Autenticação**         | AWS Lambda (Function Serverless) |
| **CI/CD**                | GitHub Actions                 |

---

## Instruções de Execução e Deploy

### Pré-requisitos

-   [Terraform CLI](https://learn.hashicorp.com/tutorials/terraform/install-cli) instalado.
-   Credenciais da AWS configuradas no seu ambiente.

### Provisionamento Manual

1.  **Clone o repositório:**
    ```bash
    git clone https://github.com/LTeruyaQ/techchallenge-soat-infra-gateway.git
    cd techchallenge-soat-infra-gateway
    ```

2.  **Inicialize o Terraform:**
    ```bash
    terraform init
    ```

3.  **Planeje a infraestrutura:**
    ```bash
    terraform plan
    ```

4.  **Aplique as alterações:**
    ```bash
    terraform apply
    ```

### Pipeline de CI/CD

Este repositório está configurado com um pipeline de CI/CD utilizando GitHub Actions. O deploy para os ambientes de homologação e produção é automatizado:

-   **Branch `develop`:** Commits e merges nesta branch acionam o deploy para o ambiente de **homologação**.
-   **Branch `main`:** Merges nesta branch (via Pull Request) acionam o deploy para o ambiente de **produção**.

---

## Documentação da API

O API Gateway não possui uma interface Swagger/Postman própria, pois sua função é apenas rotear requisições. A documentação completa dos endpoints da aplicação principal pode ser encontrada no repositório correspondente.
