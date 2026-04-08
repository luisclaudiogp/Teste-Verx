# Ecossistema Financeiro: Lançamentos e Consolidado

Este projeto é uma demonstração prática de uma arquitetura de microsserviços resiliente, focada em resolver desafios de alta vazão e consistência de dados. Aqui você encontrará padrões como **Transactional Outbox**, **CQRS** e **Event-Driven Architecture** aplicados de forma pragmática.

---

## 🛠 O que tem "debaixo do capô"?

Para garantir que o sistema aguente o tranco e seja fácil de manter, usamos:

- **Microsserviços**: `Lancamentos.API` (Escrita) e `Consolidado.API` (Leitura).
- **Mensageria**: RabbitMQ com MassTransit para garantir que nenhum dado se perca.
- **Bancos de Dados**: PostgreSQL isolado por serviço.
- **Cache**: Redis para que a consulta de saldo seja instantânea (sub-milissegundos).
- **Segurança**: Identity Management com **Keycloak** (OIDC/JWT).
- **Observabilidade**: OpenTelemetry exportando para Jaeger (Tracing) e Prometheus (Métricas).

---

## 🚀 Como subir o projeto

O ecossistema todo sobe com um único comando (você só precisa do Docker instalado):

```bash
docker compose up -d --build
```

### Endpoints principais:
- **Swagger/Scalar (Lançamentos)**: `http://localhost:5000/scalar/v1`
- **Swagger/Scalar (Consolidado)**: `http://localhost:5001/scalar/v1`
- **Jaeger (Tracing)**: `http://localhost:16686`
- **Keycloak (Painel)**: `http://localhost:8081` (admin/admin)

---

## 🔐 Como testar o fluxo completo

Siga estes passos para ver a mágica da consistência eventual acontecendo:

### 1. Pegar o Token de Acesso

Acesse o Scalar da **Lancamentos.API** (`http://localhost:5000/scalar/v1`), procure o endpoint `POST /api/Auth/token` e use as credenciais:

- **username**: `admin`
- **password**: `admin`

*Copie o `access_token` gerado.*

### 2. Autorizar
Clique no botão **Authorize** no topo do Scalar e cole o token. Repita isso no Scalar do **Consolidado** se for testar por lá também.

### 3. Fazer um Lançamento
Use o `POST /api/Lancamentos` para enviar um Crédito ou Débito.
Exemplo de corpo:
```json
{
  "valor": 1500.00,
  "tipo": "credito"
}
```

### 4. Consultar o Saldo

Vá até a **Consolidado.API** (`http://localhost:5001/scalar/v1`) e execute o `GET /api/Consolidado/saldo`. O saldo estará atualizado.

---

## 🛡 Teste de Resiliência (O Desafio)

Quer ver o sistema se recuperando sozinho?
1. Pare o RabbitMQ: `docker compose stop rabbitmq`.
2. Faça alguns lançamentos na API. Eles serão salvos no banco, mas o saldo não vai mudar (porque o broker está fora).
3. Suba o RabbitMQ: `docker compose start rabbitmq`.
4. Em alguns segundos, o **Outbox Message** vai disparar os eventos pendentes e o saldo no Consolidado será corrigido automaticamente.

---

## 📊 Código e Qualidade (SonarQube)

Para rodar a análise de qualidade (SAST) localmente:

1. Garanta que o Sonar esteja de pé: `docker compose up -d sonarqube`.
2. Rode o script de scan:
```powershell
.\run-scan.ps1
```
3. Veja o resultado em: `http://localhost:9000`. 
*(O scan está configurado para ignorar arquivos de infraestrutura e focar apenas na sua lógica de negócio).*

---

## 🏗 Arquitetura

O sistema segue os princípios de **Clean Architecture** e **DDD**, garantindo que as regras de negócio fiquem isoladas de detalhes de implementação como banco de dados ou brokers de mensagem.

Para uma visão visual detalhada, confira o documento na pasta `Documentacao/Arquitetura_Verx.html`.
