# MecanicaOS: Sistema de Gerenciamento para Oficinas - Fase 2 (Arquitetura Automatizada na AWS)

## 1. Visão Geral

O **MecanicaOS** é uma solução de gerenciamento para oficinas mecânicas, reimaginada na Fase 2 para operar em uma arquitetura de nuvem moderna, totalmente automatizada e resiliente. O projeto foi aprimorado para usar **Infraestrutura como Código (IaC)** com Terraform e um script de orquestração que provisiona e configura todo o ambiente na AWS com um único comando, sem intervenção humana.

A solução agora é composta por uma API .NET rodando em **Amazon EKS**, um banco de dados **Amazon RDS PostgreSQL**, uma função **AWS Lambda** para autenticação desacoplada e um **Amazon API Gateway (HTTP API)** como ponto de entrada seguro e de baixo custo.

---

## 2. Arquitetura da Solução

A arquitetura foi desenhada para ser escalável, segura e de baixo custo, ideal para o ambiente AWS Academy.

```mermaid
graph TD
    subgraph "Público"
        direction LR
        A[Usuário Final]
    end

    subgraph "AWS Cloud"
        direction LR
        B[API Gateway (HTTP API)]
        C[AWS Lambda (Autenticação)]
        D[Amazon EKS (Cluster Kubernetes)]
        E[API .NET (Pods)]
        F[Amazon RDS (PostgreSQL)]
        H[Amazon ECR]
    end

    A -- CPF via HTTPS --> B
    B -- /auth --> C
    C -- Valida CPF --> F
    C -- Gera JWT --> A
    A -- Requisição com JWT --> B
    B -- Proxy --> D
    D --> E
    E -- Acessa dados --> F

    style F fill:#add,stroke:#333,stroke-width:2px
    style D fill:#e1f5ff,stroke:#01579b,stroke-width:2px
```

### Componentes Principais:
- **API Gateway (HTTP API):** Ponto de entrada para todas as requisições. Roteia chamadas de autenticação para a Lambda e outras requisições para a API no EKS.
- **AWS Lambda:** Função Python que valida o CPF do cliente diretamente no RDS, verifica seu status e gera um JWT, desacoplando a autenticação da aplicação principal.
- **Amazon EKS:** Cluster Kubernetes que orquestra os contêineres da API .NET, configurado com **Horizontal Pod Autoscaler (HPA)** para escalabilidade automática baseada em CPU.
- **Amazon RDS for PostgreSQL:** Banco de dados gerenciado, operando em subnets privadas para segurança, com esquema inicializado automaticamente pelo script de deploy.
- **Amazon ECR:** Repositório de contêineres privado onde a imagem Docker da aplicação é armazenada após o build com Kaniko.
- **Gerenciamento de Segredos:** Devido a restrições do ambiente AWS Academy, segredos como a senha do banco de dados e a chave JWT **não são armazenados no Secrets Manager**. Eles são gerados dinamicamente pelo Terraform a cada deploy e injetados diretamente nos serviços (Lambda e EKS) como variáveis de ambiente.

### Fluxo de Autenticação:
1. O cliente envia seu CPF para o endpoint `/auth` no API Gateway.
2. O API Gateway aciona a função Lambda de autenticação.
3. A Lambda conecta-se ao banco de dados (cujas credenciais são injetadas pelo script de deploy) e verifica se o cliente com o CPF fornecido existe e está ativo.
4. Se for válido, a Lambda gera um JWT assinado e o retorna ao cliente.
5. O cliente utiliza o JWT no cabeçalho `Authorization` para acessar as rotas protegidas da API, que são roteadas pelo API Gateway para o serviço no EKS.

---

## 3. Como Subir o Ambiente (Um Comando)

O processo de deploy é 100% automatizado. Um único script provisiona a infraestrutura, configura o banco de dados, constrói a imagem da aplicação e a implanta.

### Pré-requisitos:
- [AWS CLI](https://aws.amazon.com/cli/) configurado com credenciais válidas.
- [Terraform CLI](https://learn.hashicorp.com/tutorials/terraform/install-cli).
- [kubectl](https://kubernetes.io/docs/tasks/tools/install-kubectl/).
- [PostgreSQL Client (psql)](https://www.postgresql.org/download/) para a inicialização do schema.
- Git.

### Execução:
Navegue até o diretório `terraform` e execute o script:

```powershell
# No Windows (PowerShell)
.\deploy-completo.ps1

# Para destruir toda a infraestrutura criada:
.\deploy-completo.ps1 -Destroy
```

O script detectará automaticamente os recursos da VPC, criará todos os serviços, executará o `rds-init.sql` e, ao final, imprimirá as URLs de acesso. **Nenhum input manual é necessário.**

---

## 4. Checklist de Validação

- [ ] `.\deploy-completo.ps1` roda sem erros e sem solicitar inputs humanos.
- [ ] RDS criado e acessível pela Lambda (Security Groups configurados).
- [ ] Endpoint de autenticação (`/auth`) com CPF funciona e retorna um JWT válido.
- [ ] Rotas protegidas da API exigem o JWT; rotas públicas (como Swagger) continuam acessíveis.
- [ ] Swagger da API está publicamente acessível através da URL do API Gateway.
- [ ] Observabilidade (se existente) permaneceu inalterada.
- [ ] Nada que já funcionava foi quebrado.

---

## 5. O que é IA e o que é SIM?

### IA (Inteligência Artificial)
Conjunto de técnicas e modelos que automatizam decisões, análises e geração de conteúdo. No contexto deste desafio, a IA é uma ferramenta de apoio que gera instruções, código e automações, mas não substitui a validação humana final em produção.

### SIM (Sistema de Informação e Monitoramento)
Refere-se ao conjunto de componentes que fornecem visibilidade e controle do ambiente (logs, métricas, dashboards, alertas). Exemplos: Datadog, NewRelic, dashboards de latência, consumo de CPU/memória, healthchecks e correlação de logs. As integrações SIM existentes não foram alteradas.
