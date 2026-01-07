# Repositório: Infraestrutura Kubernetes e Orquestração

Este repositório é o "cérebro" da infraestrutura da solução MecanicaOS. Ele contém o código Terraform para provisionar os recursos de nuvem (exceto o banco de dados) e os manifestos Kubernetes para o deploy da aplicação.

## 1. Visão Geral

Este repositório gerencia:
- **Descoberta da Rede:** Encontra a VPC e sub-redes existentes.
- **AWS API Gateway:** Cria o ponto de entrada da API, integrando com a Lambda de autenticação e o serviço da aplicação.
- **AWS Lambda:** Provisiona a função Lambda de autenticação, utilizando o artefato `.zip` gerado pelo repositório `lambda-auth`.
- **Manifestos Kubernetes:** Contém todos os arquivos (`.yaml`) para o deploy da aplicação no EKS, incluindo Deployments, Services, Secrets, ConfigMaps e HPA.

## 2. Ordem de Provisionamento

O Terraform neste repositório depende de recursos que já devem existir:
1.  Um Cluster EKS.
2.  Uma VPC com sub-redes públicas e privadas devidamente tagueadas.
3.  Roles de IAM para o EKS (em ambientes AWS Academy).

## 3. Execução (Provisionamento e Deploy)

Para aplicar a infraestrutura e fazer o deploy da aplicação:

1.  **Pré-requisitos:**
    -   Terraform CLI e `kubectl` instalados.
    -   Credenciais da AWS configuradas e `kubeconfig` apontando para o cluster EKS.

2.  **Inicializar o Terraform:**
    ```bash
    terraform init
    ```

3.  **Planejar e Aplicar:**
    ```bash
    terraform plan -out=tfplan
    terraform apply "tfplan"
    ```
    O `apply` do Terraform irá provisionar os recursos AWS (API Gateway, Lambda) e, em seguida, aplicar os manifestos do diretório `k8s/` no cluster EKS.

## 4. Pipeline de CI/CD

O pipeline deste repositório é o orquestrador final do deploy:

1.  **Trigger:** Um push para a `main` ou um webhook de outros repositórios (como `app-mecanicaos` notificando uma nova imagem Docker).
2.  **Terraform Plan:** Um `terraform plan` é executado para validar as mudanças na infraestrutura.
3.  **Apply (com Aprovação):** Após aprovação, o `terraform apply` é executado, atualizando a infraestrutura (ex: nova versão da Lambda) e o estado dos recursos no Kubernetes para a versão mais recente (ex: nova imagem Docker da API).

## 5. Diagrama de Orquestração

```mermaid
graph TD
    subgraph "Repositório: infra-kubernetes"
        A[Terraform (EKS, VPC, GW, Lambda)] --> B{CI/CD Pipeline};
        C[Manifestos Kubernetes] --> B;
    end

    subgraph "AWS Cloud"
        B --> D[API Gateway];
        B --> E[AWS Lambda];
    end

    subgraph "Cluster EKS"
        B --> F[Deployments, Services, HPA, etc.];
    end

    D --> E;
    D --> F;

    style B fill:#f5c6cb,stroke:#721c24
```
