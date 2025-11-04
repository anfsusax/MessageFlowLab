# Message Flow Lab

[![.NET](https://github.com/seu-usuario/MessageFlowLab/actions/workflows/dotnet.yml/badge.svg)](https://github.com/seu-usuario/MessageFlowLab/actions/workflows/dotnet.yml)
[![Docker](https://img.shields.io/docker/pulls/yourusername/messageflowlab)](https://hub.docker.com/r/yourusername/messageflowlab)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Sistema de mensageria distribuído com suporte a múltiplos idiomas e integração com IA, utilizando RabbitMQ, Kafka e MySQL.

[Users / IA Simulator]
        │
        ▼
[MessageGenerator] --(AMQP publish)--> [RabbitMQ Exchange]
                                           │
                                           └--> [questions.queue]  ---> [RabbitWorker Consumer]
                                                                  │ (enriquecer msg, adicionar CorrelationId, timestamps)
                                                                  ├--(success)--> publish to Kafka topic "processed.questions"
                                                                  └--(fail x retries)--> [RabbitMQ DLQ]
                                                                                          │
                                                                                          ▼
                                                                                  (DLQ inspection / reprocess)
                                                                  │
                                                                  ▼
[Kafka] (topic: processed.questions) ---> [KafkaWorker Consumer] ---> persist → MySQL
                                                                  │
                                                                  ▼
                                                        [MessageLogger] (metrics + alerts)
                                                                  │
                                                                  ▼
                                                        [DashboardBlazor (SignalR)]
                                                                  │
                                                                  ▼
                                              Visualizações, links (RabbitMQ UI, Kafdrop), tutoriais


## Funcionalidades

- Geração de mensagens em múltiplos idiomas (inglês, português, espanhol)
- Processamento assíncrono com RabbitMQ e Kafka
- Armazenamento persistente em MySQL
- Dashboard em tempo real com Blazor
- Integração com IA para melhoria de mensagens
- Sistema de cache e resiliência
- Monitoramento e métricas em tempo real

## Arquitetura

```
[MessageGenerator] → RabbitMQ → [RabbitWorker] → Kafka → [KafkaWorker] → MySQL
                                                       ↓
                                                [MessageLogger] → [DashboardBlazor (UI)]
                                                       ↓
                                              [Sistema de Métricas & Logs]
```

## Componentes Principais

### 1. MessageGenerator
- Gera perguntas simuladas em múltiplos idiomas
- Suporte a templates de mensagens
- Internacionalização integrada
- Publica mensagens no RabbitMQ

### 2. RabbitWorker
- Consome mensagens do RabbitMQ
- Processa e enriquece as mensagens
- Implementa DLQ (Dead Letter Queue)
- Publica mensagens processadas no Kafka

### 3. KafkaWorker
- Consome mensagens do Kafka
- Armazena mensagens no MySQL
- Implementa o padrão CQRS

### 4. MessageLogger
- Monitora o fluxo de mensagens
- Coleta métricas e estatísticas
- Gera alertas

### 5. DashboardBlazor
- Interface web em tempo real
- Visualização de métricas e status
- Suporte a múltiplos idiomas

## Começando

### Pré-requisitos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/products/docker-desktop) e [Docker Compose](https://docs.docker.com/compose/install/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) ou [VS Code](https://code.visualstudio.com/)
- Conta na [OpenAI](https://platform.openai.com/) (opcional, para uso da IA)

### Variáveis de Ambiente

Crie um arquivo `.env` na raiz do projeto:
```env
RABBITMQ_USER=user
RABBITMQ_PASSWORD=password
OPENAI_API_KEY=sua-chave-aqui
```

## Como Executar

### 1. Iniciar a Infraestrutura
```bash
# Iniciar todos os serviços em segundo plano
docker-compose up -d

# Verificar status dos serviços
docker-compose ps
```

### 2. Compilar a Solução
```bash
dotnet build
```

### 3. Executar os Componentes

#### Opção 1: Execução Manual (Recomendado para Desenvolvimento)
```bash
# Terminal 1 - MessageGenerator
cd src/MessageGenerator
dotnet run --launch-profile Development

# Terminal 2 - RabbitWorker
cd src/RabbitWorker
dotnet run --launch-profile Development

# Terminal 3 - KafkaWorker
cd src/KafkaWorker
dotnet run --launch-profile Development

# Terminal 4 - MessageLogger
cd src/MessageLogger
dotnet run --launch-profile Development

# Terminal 5 - DashboardBlazor
cd src/DashboardBlazor
dotnet run --launch-profile Development
```

#### Opção 2: Usando Docker (Produção)
```bash
# Construir as imagens
docker-compose -f docker-compose.yml -f docker-compose.override.yml build

# Iniciar todos os serviços
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d
```

## Portas e Acesso

| Serviço               | Porta      | URL de Acesso                 | Credenciais           |
|-----------------------|------------|--------------------------------|-----------------------|
| RabbitMQ (AMQP)      | 5672       | -                             | user/password        |
| RabbitMQ Management   | 15672      | http://localhost:15672        | user/password        |
| Kafka                | 9092       | PLAINTEXT://localhost:9092    | -                    |
| Kafdrop (UI Kafka)   | 9000       | http://localhost:9000         | -                    |
| MySQL                | 3306       | localhost:3306                | root/root           |
| DashboardBlazor      | 5000, 7001 | http://localhost:5000         | -                    |

## Monitoramento

- **Dashboard Principal**: http://localhost:5000
  - Visualização em tempo real das mensagens
  - Métricas de desempenho
  - Status dos serviços

- **RabbitMQ Management**: http://localhost:15672
  - Filas e exchanges
  - Taxa de mensagens
  - Conexões ativas

- **Kafdrop**: http://localhost:9000
  - Tópicos Kafka
  - Consumidores
  - Partições

## Armazenamento de Dados

### Volumes Docker

| Volume                | Descrição                               | Localização no Host                     |
|-----------------------|----------------------------------------|-----------------------------------------|
| `rabbitmq_data`      | Dados do RabbitMQ                     | `/var/lib/docker/volumes/rabbitmq_data` |
| `zookeeper_data`     | Dados do Zookeeper                    | `/var/lib/docker/volumes/zookeeper_data`|
| `zookeeper_log`      | Logs do Zookeeper                     | `/var/lib/docker/volumes/zookeeper_log` |
| `kafka_data`         | Tópicos e mensagens do Kafka          | `/var/lib/docker/volumes/kafka_data`    |
| `mysql_data`         | Banco de dados MySQL                  | `/var/lib/docker/volumes/mysql_data`    |

### Estrutura do Banco de Dados

O banco de dados MySQL contém as seguintes tabelas principais:
- `Messages`: Armazena todas as mensagens processadas
- `MessageLogs`: Registra eventos importantes no ciclo de vida das mensagens
- `Templates`: Armazena os templates de mensagens
- `Metrics`: Armazena métricas de desempenho

## Manutenção

### Parar os Serviços
```bash
# Parar todos os containers
docker-compose down

# Parar e remover todos os containers, redes e volumes
docker-compose down -v --remove-orphans

# Remover imagens não utilizadas
docker system prune -a
```

### Limpar Dados
```bash
# Remover todos os volumes
for volume in $(docker volume ls -q | grep messageflowlab); do
    docker volume rm $volume
done

# Limpar containers parados e redes não utilizadas
docker system prune -f
```

### Atualizar o Sistema
```bash
# Atualizar imagens Docker
docker-compose pull

# Reconstruir e reiniciar os serviços
docker-compose up -d --build
```

## Logs e Monitoramento

### Estrutura de Logs

Todos os componentes utilizam Serilog para logging estruturado, com os seguintes campos:

| Campo           | Tipo     | Descrição                                      |
|-----------------|----------|------------------------------------------------|
| `Timestamp`     | DateTime | Data/hora do evento no formato ISO 8601        |
| `Level`         | string   | Nível do log (Information, Warning, Error, etc)|
| `CorrelationId` | string   | ID único para rastreamento da mensagem        |
| `Component`     | string   | Nome do componente que gerou o log            |
| `Status`        | string   | Estado atual do processamento                 |
| `Message`       | string   | Mensagem descritiva do evento                 |
| `Exception`     | string   | Detalhes da exceção (se aplicável)            |
| `Properties`    | object   | Metadados adicionais                          |

### Níveis de Log

- **Information**: Eventos normais de operação
- **Warning**: Eventos que podem indicar problemas
- **Error**: Erros que não interrompem a execução
- **Critical**: Falhas graves que exigem atenção imediata

### Visualização de Logs

```bash
# Ver logs em tempo real
docker-compose logs -f

# Ver logs de um serviço específico
docker-compose logs -f messagegenerator

# Exportar logs para um arquivo
docker-compose logs --no-color > logs.txt
```

## Métricas

O sistema coleta as seguintes métricas em tempo real:

### Métricas de Desempenho
- Taxa de mensagens por segundo
- Tempo médio de processamento
- Taxa de sucesso/falha
- Uso de recursos (CPU, memória)

### Métricas de Negócio
- Total de mensagens processadas
- Mensagens por idioma
- Categorias mais comuns
- Taxa de utilização da IA

## Configuração Avançada

### Configuração do AppSettings

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Queue": "questions",
    "Username": "user",
    "Password": "password"
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "Topic": "processed-messages",
    "GroupId": "kafka-worker-group"
  },
  "Database": {
    "ConnectionString": "Server=localhost;Database=MessageFlow;User=root;Password=root;"
  },
  "AI": {
    "Enabled": false,
    "Endpoint": "https://api.openai.com/v1/chat/completions",
    "ApiKey": "sua-chave-aqui",
    "Model": "gpt-3.5-turbo",
    "MaxTokens": 500,
    "Temperature": 0.7
  },
  "Caching": {
    "Enabled": true,
    "SlidingExpiration": "01:00:00",
    "AbsoluteExpiration": "1.00:00:00"
  },
  "RetryPolicy": {
    "MaxRetryAttempts": 3,
    "InitialDelayMs": 1000,
    "UseExponentialBackoff": true
  }
}
```

## Contribuição

1. Faça um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## Licença

Distribuído sob a licença MIT. Veja `LICENSE` para mais informações.

## Contato

Seu Nome - [@seu_twitter](https://twitter.com/seu_twitter) - email@exemplo.com

Link do Projeto: [https://github.com/seu-usuario/MessageFlowLab](https://github.com/seu-usuario/MessageFlowLab)

---

<div align="center">
  <sub>Desenvolvido com ❤️ por <a href="https://github.com/seu-usuario">seu-nome</a></sub>
</div>

## Notas de Atualização

### v0.1.0 (2023-11-03)
- Versão inicial do projeto
- Implementação básica dos workers
- Suporte a RabbitMQ e Kafka

### v0.2.0 (2023-11-04)
- Adicionado suporte a múltiplos idiomas
- Integração com IA para melhoria de mensagens
- Sistema de cache e resiliência

### v0.3.0 (Em desenvolvimento)
- [ ] Autenticação e autorização
- [ ] Dashboard avançado
- [ ] Suporte a mais canais de comunicação
