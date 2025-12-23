# Explicação Detalhada das Melhorias no Projeto EKS

## Visão Geral

O objetivo principal de todas as alterações foi transformar o processo de deploy de um estado "funcional" para um estado "pronto para produção". Isso significa torná-lo mais robusto, previsível, seguro e, acima de tudo, fornecer uma experiência de alta qualidade para o desenvolvedor ou aluno que o utiliza, especialmente considerando as particularidades do ambiente AWS Academy.

---

### 1. Health Checks de Produção (`liveness` e `readiness`)

-   **O que faz:**
    Separa a verificação de saúde da aplicação em duas categorias:
    -   **Liveness (`/health/live`):** Responde à pergunta "O aplicativo está rodando e não travou?".
    -   **Readiness (`/health/ready`):** Responde à pergunta "O aplicativo está pronto para receber tráfego e todas as suas dependências (como o banco de dados) estão funcionando?".

-   **Porquê:**
    Esta é uma prática essencial em ambientes orquestrados como o Kubernetes. Sem essa separação, um pod que está apenas demorando para se conectar ao banco de dados poderia ser considerado "não saudável" e ser reiniciado desnecessariamente. Com a separação:
    -   O Kubernetes usa o **liveness probe** para reiniciar um contêiner apenas se ele travar de verdade.
    -   O Kubernetes e o Load Balancer usam o **readiness probe** para saber quando o contêiner está 100% pronto para ser adicionado ao balanceamento de carga e começar a receber tráfego. Isso torna a aplicação muito mais resiliente.

-   **Como:**
    1.  **Backend (.NET):** O antigo `HealthController` foi substituído pela biblioteca nativa `Microsoft.Extensions.Diagnostics.HealthChecks`. No arquivo `MecanicaOS/API/Program.cs`, foram configurados dois endpoints distintos.
    2.  **Terraform:** Os arquivos de infraestrutura foram atualizados:
        -   Em `terraform/k8s-probes.tf`, `livenessProbe` foi apontado para `/health/live` e `readinessProbe` para `/health/ready`.
        -   Em `terraform/k8s-service.tf`, a `annotation` do AWS Load Balancer também foi apontada para `/health/ready`.

---

### 2. Detecção Inteligente de Ambiente e Validação Robusta de IAM Roles

-   **O que faz:**
    O Terraform agora detecta automaticamente se está rodando em um ambiente AWS Academy ou em uma conta AWS "normal". Com base nisso, ele adapta seu comportamento para buscar ou validar as IAM roles de forma segura, sem falhar abruptamente.

-   **Porquê:**
    Este era um dos maiores pontos de falha. Nomes de roles complexos no Academy ou a ausência de roles em uma conta nova causavam erros confusos. A nova abordagem fornece um "fail-fast" com mensagens de erro claras e humanas.

-   **Como:**
    1.  **Detecção:** Em `terraform/locals.tf` e `data.tf`, foi implementada uma lógica que busca por roles indicadoras do Academy (como `LabRole`) usando `data "aws_iam_roles"`. A presença dessas roles ativa a flag `local.is_academy`.
    2.  **Comportamento Condicional:**
        -   **Se Academy:** O Terraform busca dinamicamente as roles do EKS e desativa as validações de anexo de policies (`count = 0`). Uma validação extra foi adicionada para garantir que exatamente uma `LabRole` seja encontrada.
        -   **Se Conta Normal:** O Terraform usa os nomes de roles fornecidos nas variáveis e verifica se as policies essenciais estão anexadas.
    3.  **Mensagens de Erro:** A `lifecycle { precondition { ... } }` nos recursos do EKS interrompe o processo com uma mensagem de erro customizada e clara em caso de falha.

---

### 3. Script de Deploy "Premium" (`deploy-completo.ps1`)

-   **O que faz:**
    Transforma o script de deploy de uma sequência de comandos para uma ferramenta de orquestração inteligente que guia o usuário e fornece diagnóstico em tempo real.

-   **Porquê:**
    Um deploy na nuvem pode ser um processo longo e opaco. O objetivo foi dar ao usuário total visibilidade, confiança e um diagnóstico claro em caso de problemas, melhorando a Developer Experience (DX).

-   **Como:**
    1.  **Diagnóstico Ativo do Load Balancer:** O antigo `Start-Sleep` foi substituído por um loop inteligente que, a cada 10 segundos, verifica ativamente o status do namespace, do service e dos pods, exibindo eventos do Kubernetes em caso de falha.
    2.  **Relatório Final e Outputs Amigáveis:** Ao final, o script imprime um relatório de status conciso (`✔️ Namespace criado`, `⚠️ OTEL não configurado (opcional)`, etc.) e uma lista de URLs formatada (`Sua API está no ar! 🎉`).
    3.  **Mensagens Pedagógicas (OTEL):** A ausência de chaves de API para a observabilidade é tratada como uma informação (`[INFO]`), não como um erro.

---

### 4. Refatoração para Recursos Nativos do Terraform

-   **O que faz:**
    Migrou a criação de recursos-chave do Kubernetes (como `Service` e `Namespace`) de arquivos YAML estáticos (usando `kubectl_manifest`) para recursos nativos do provedor Terraform (`kubernetes_service`, `kubernetes_namespace`).

-   **Porquê:**
    -   **Correção de Bug Crítico:** Corrigiu um bug que impedia os `outputs` do Terraform (como `api_url`) de funcionarem, pois eles não conseguiam referenciar recursos que não existiam no estado do Terraform.
    -   **Melhores Práticas de IaC:** Permite que o Terraform entenda o grafo de dependências completo e gerencie o ciclo de vida dos recursos de forma mais inteligente.

-   **Como:**
    1.  O conteúdo dos arquivos `.yaml` foi "traduzido" para a sintaxe HCL do Terraform em novos arquivos (`terraform/k8s-service.tf`, `terraform/k8s-namespaces.tf`).
    2.  Os arquivos YAML originais foram deletados.
    3.  As referências aos antigos `kubectl_manifest` foram removidas de `terraform/k8s-from-files.tf`.
