# Guia Completo de Deploy do MecanicaOS na AWS

## Visao Geral

Este guia vai te ensinar a publicar o MecanicaOS na AWS, passo a passo, como se voce tivesse 5 anos de idade.

```
+------------------+     +------------------+     +------------------+
|   Seu Codigo     | --> |   Docker Image   | --> |    AWS ECR       |
|   (MecanicaOS)   |     |   (Container)    |     |   (Repositorio)  |
+------------------+     +------------------+     +------------------+
                                                          |
                                                          v
+------------------+     +------------------+     +------------------+
|   Sua API        | <-- |   Kubernetes     | <-- |    AWS EKS       |
|   Funcionando!   |     |   (Orquestrador) |     |   (Cluster)      |
+------------------+     +------------------+     +------------------+
```

---

## PARTE 1: Preparacao (Coisas que voce precisa ter)

### 1.1 Ferramentas Necessarias

| Ferramenta | Para que serve | Link |
|------------|----------------|------|
| AWS CLI | Falar com a AWS | https://aws.amazon.com/cli/ |
| Terraform | Criar infraestrutura | https://www.terraform.io/downloads |
| Docker Desktop | Criar containers | https://www.docker.com/products/docker-desktop |
| kubectl | Gerenciar Kubernetes | https://kubernetes.io/docs/tasks/tools/ |

### 1.2 Verificar se tudo esta instalado

Abra o PowerShell e execute:

```powershell
# Verificar AWS CLI
aws --version

# Verificar Terraform
terraform --version

# Verificar Docker
docker --version

# Verificar kubectl
kubectl version --client
```

Se algum comando der erro, instale a ferramenta correspondente.

---

## PARTE 2: Configurar Credenciais AWS Academy

### Passo 2.1: Acessar AWS Academy

1. Acesse seu curso no AWS Academy
2. Clique em **"Modules"** > **"Learner Lab"**
3. Clique em **"Start Lab"** (botao verde)
4. Aguarde o indicador ficar **VERDE**

### Passo 2.2: Pegar as Credenciais

1. Clique em **"AWS Details"** (canto superior direito)
2. Clique em **"Show"** ao lado de **"AWS CLI"**
3. Voce vera algo assim:

```
[default]
aws_access_key_id=ASIAXXXXXXXXXXX
aws_secret_access_key=xxxxxxxxxxxxxxxxxxxxxxxx
aws_session_token=xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx...
```

### Passo 2.3: Salvar as Credenciais

**Opcao A - Arquivo (RECOMENDADO):**

```powershell
# Abrir arquivo de credenciais
notepad $HOME\.aws\credentials
```

Cole o conteudo que voce copiou e salve.

**Opcao B - Variaveis de Ambiente:**

```powershell
$env:AWS_ACCESS_KEY_ID = "ASIAXXXXXXXXXXX"
$env:AWS_SECRET_ACCESS_KEY = "xxxxxxxxxxxxxxxxxxxxxxxx"
$env:AWS_SESSION_TOKEN = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx..."
$env:AWS_DEFAULT_REGION = "us-east-1"
```

### Passo 2.4: Testar

```powershell
aws sts get-caller-identity
```

Deve mostrar seu Account ID. Se der erro, as credenciais estao erradas ou expiraram.

> **IMPORTANTE**: Credenciais do AWS Academy expiram a cada 4 horas!

---

## PARTE 3: Descobrir os Nomes das Roles

### Por que isso e importante?

O AWS Academy nao deixa voce criar roles. Voce precisa usar as roles que ja existem.

### Passo 3.1: Acessar o Console AWS

1. No AWS Academy, clique em **"Open AWS Console"**
2. No console, va em **Services** > **IAM** > **Roles**

### Passo 3.2: Buscar as Roles

1. Na barra de busca, digite: **LabEks**
2. Voce vai encontrar duas roles:

| Role | Exemplo de Nome |
|------|-----------------|
| LabEksClusterRole | `c175509a4540172l11442646t1w891377-LabEksClusterRole-QQH0SV203Gtw` |
| LabEksNodeRole | `c175509a4540172l11442646t1w891377135-LabEksNodeRole-r3HYcSAYWMXX` |

### Passo 3.3: Copiar os Nomes

Clique em cada role e copie o nome COMPLETO (incluindo o prefixo longo).

---

## PARTE 4: Configurar o Terraform

### Passo 4.1: Ir para a pasta do Terraform

```powershell
cd c:\Users\user\source\repos\TechChallenge-SOAT1\terraform
```

### Passo 4.2: Criar arquivo de configuracao

```powershell
# Copiar o exemplo
Copy-Item terraform.tfvars.example terraform.tfvars

# Abrir para editar
notepad terraform.tfvars
```

### Passo 4.3: Preencher os valores

Edite o arquivo e preencha:

```hcl
aws_region   = "us-east-1"
project_name = "mecanicaos"
environment  = "production"

# COLE AQUI os nomes das roles que voce copiou
eks_cluster_role_name = "COLE_AQUI_O_NOME_DA_LabEksClusterRole"
eks_node_role_name    = "COLE_AQUI_O_NOME_DA_LabEksNodeRole"

# ECR
docker_image_repo = "mecanicaos-ecr"
docker_image_tag  = "latest"

# Kubernetes
replicas = 2

# Banco de dados (Supabase)
db_host     = "seu-host.supabase.com"
db_port     = "5432"
db_name     = "postgres"
db_username = "postgres"
db_password = "SUA_SENHA_DO_BANCO"

# JWT
jwt_secret_key     = "uma_chave_secreta_bem_longa_com_pelo_menos_64_caracteres_aqui"
jwt_issuer         = "MecanicaOS"
jwt_audience       = "MecanicaOS-API"
jwt_expiry_minutes = 120
```

Salve o arquivo.

---

## PARTE 5: Executar o Deploy

### Opcao A: Script Automatico (MAIS FACIL)

```powershell
# Ir para a pasta do terraform
cd c:\Users\user\source\repos\TechChallenge-SOAT1\terraform

# Executar o deploy completo
.\deploy-completo.ps1
```

O script vai:
1. Verificar se voce tem tudo instalado
2. Verificar suas credenciais AWS
3. Criar o repositorio ECR
4. Fazer build da imagem Docker
5. Enviar a imagem para o ECR
6. Criar toda a infraestrutura com Terraform
7. Configurar o kubectl
8. Mostrar o status do deploy

### Opcao B: Passo a Passo Manual

Se preferir fazer manualmente:

#### Passo 5.1: Inicializar Terraform

```powershell
cd c:\Users\user\source\repos\TechChallenge-SOAT1\terraform
terraform init
```

#### Passo 5.2: Ver o que sera criado

```powershell
terraform plan
```

#### Passo 5.3: Criar a infraestrutura

```powershell
terraform apply
```

Digite `yes` quando perguntado.

> **ATENCAO**: Isso leva 15-20 minutos!

#### Passo 5.4: Fazer build da imagem Docker

```powershell
cd c:\Users\user\source\repos\TechChallenge-SOAT1

# Fazer login no ECR
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin SEU_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com

# Build da imagem
docker build -t mecanicaos-api .

# Tag para o ECR
docker tag mecanicaos-api:latest SEU_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/mecanicaos-ecr:latest

# Push para o ECR
docker push SEU_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/mecanicaos-ecr:latest
```

#### Passo 5.5: Configurar kubectl

```powershell
aws eks update-kubeconfig --region us-east-1 --name eks-mecanicaos
```

---

## PARTE 6: Verificar se Funcionou

### Passo 6.1: Ver os pods

```powershell
kubectl get pods -n mecanicaos
```

Deve mostrar algo como:
```
NAME                             READY   STATUS    RESTARTS   AGE
mecanicaos-api-xxxxxxxxx-xxxxx   1/1     Running   0          5m
mecanicaos-api-xxxxxxxxx-xxxxx   1/1     Running   0          5m
```

### Passo 6.2: Ver os services

```powershell
kubectl get svc -n mecanicaos
```

Deve mostrar algo como:
```
NAME                 TYPE           CLUSTER-IP      EXTERNAL-IP                              PORT(S)
mecanicaos-service   LoadBalancer   10.100.xxx.xxx  xxxxx.us-east-1.elb.amazonaws.com        80:xxxxx/TCP
```

### Passo 6.3: Acessar a API

Copie o EXTERNAL-IP e acesse no navegador:

```
http://xxxxx.us-east-1.elb.amazonaws.com/api/v1/docs
```

Voce deve ver a documentacao Swagger da API!

---

## PARTE 7: Comandos Uteis

### Ver logs da aplicacao

```powershell
kubectl logs -n mecanicaos -l app=mecanicaos-api --tail=100
```

### Ver detalhes de um pod

```powershell
kubectl describe pod -n mecanicaos -l app=mecanicaos-api
```

### Reiniciar os pods

```powershell
kubectl rollout restart deployment mecanicaos-api -n mecanicaos
```

### Destruir tudo

```powershell
cd c:\Users\user\source\repos\TechChallenge-SOAT1\terraform
terraform destroy
```

---

## PARTE 8: Usando GitHub Actions (Automatico)

Se voce quiser que o deploy seja automatico quando fizer push no GitHub:

### Passo 8.1: Configurar Secrets no GitHub

1. Va no seu repositorio no GitHub
2. Clique em **Settings** > **Secrets and variables** > **Actions**
3. Clique em **New repository secret**
4. Adicione cada secret:

| Nome do Secret | Valor |
|----------------|-------|
| AWS_ACCESS_KEY_ID | Sua access key |
| AWS_SECRET_ACCESS_KEY | Sua secret key |
| AWS_SESSION_TOKEN | Seu session token |
| EKS_CLUSTER_ROLE_NAME | Nome da LabEksClusterRole |
| EKS_NODE_ROLE_NAME | Nome da LabEksNodeRole |
| DB_HOST | Host do banco de dados |
| DB_PASSWORD | Senha do banco |
| JWT_SECRET_KEY | Chave secreta JWT |

### Passo 8.2: Executar o Workflow

1. Va em **Actions** no seu repositorio
2. Clique em **"Deploy ECR + EKS"**
3. Clique em **"Run workflow"**
4. Escolha a acao (deploy, build-only, destroy)
5. Clique em **"Run workflow"**

---

## Problemas Comuns

### Erro: "Credenciais invalidas"

**Causa**: Credenciais expiraram (duram 4 horas no AWS Academy)

**Solucao**: Pegue novas credenciais no AWS Academy

### Erro: "Role not found"

**Causa**: Nome da role esta errado

**Solucao**: 
1. Va no Console AWS > IAM > Roles
2. Busque "LabEks"
3. Copie o nome COMPLETO da role

### Erro: "ImagePullBackOff"

**Causa**: Kubernetes nao consegue baixar a imagem

**Solucao**:
1. Verifique se a imagem foi enviada para o ECR
2. Verifique se a tag esta correta no terraform.tfvars

### Erro: "Pending" nos pods

**Causa**: Nodes ainda nao estao prontos

**Solucao**: Aguarde alguns minutos. Se persistir:
```powershell
kubectl get nodes
kubectl describe nodes
```

---

## Resumo Visual

```
+------------------------------------------------------------------+
|                    FLUXO DE DEPLOY                               |
+------------------------------------------------------------------+
|                                                                  |
|  1. PREPARACAO                                                   |
|     [Instalar ferramentas] --> [Configurar credenciais AWS]      |
|                                                                  |
|  2. CONFIGURACAO                                                 |
|     [Descobrir roles] --> [Preencher terraform.tfvars]           |
|                                                                  |
|  3. DEPLOY                                                       |
|     [terraform init] --> [terraform apply] --> [15-20 min]       |
|                                                                  |
|  4. BUILD & PUSH                                                 |
|     [docker build] --> [docker push ECR]                         |
|                                                                  |
|  5. VERIFICACAO                                                  |
|     [kubectl get pods] --> [Acessar URL do LoadBalancer]         |
|                                                                  |
+------------------------------------------------------------------+
```

---

## Contato

Se tiver duvidas, verifique:
1. Os logs: `kubectl logs -n mecanicaos -l app=mecanicaos-api`
2. Os eventos: `kubectl get events -n mecanicaos`
3. O status dos pods: `kubectl describe pods -n mecanicaos`
