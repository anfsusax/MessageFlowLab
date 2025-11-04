# Message Flow Lab

Demonstração de um sistema de mensageria distribuído usando RabbitMQ, Kafka e MySQL.

## Arquitetura

```
[MessageGenerator] → RabbitMQ → [RabbitWorker] → Kafka → [KafkaWorker] → MySQL
                                                               ↓
                                                        [MessageLogger]
                                                               ↓
                                                     [DashboardBlazor (UI)]
```

## Componentes

1. **MessageGenerator**: Gera mensagens simulando perguntas de usuários e publica no RabbitMQ
2. **RabbitWorker**: Consome do RabbitMQ, processa e republica no Kafka (com retry/DLQ)
3. **KafkaWorker**: Consome do Kafka e persiste no MySQL
4. **MessageLogger**: Monitora status das mensagens
5. **DashboardBlazor**: Interface web para visualização em tempo real

## Pré-requisitos

- .NET 8.0 SDK
- Docker e Docker Compose
- Visual Studio 2022 ou VS Code (opcional)

## Como Executar

1. Iniciar os serviços de infraestrutura:
```bash
docker-compose up -d
```

2. Verificar status dos serviços:
```bash
docker-compose ps
```

3. Build da solução:
```bash
dotnet build
```

4. Executar os componentes (em terminais separados):
```bash
# Terminal 1 - MessageGenerator
cd src/MessageGenerator
dotnet run

# Terminal 2 - RabbitWorker
cd src/RabbitWorker
dotnet run

# Terminal 3 - KafkaWorker
cd src/KafkaWorker
dotnet run

# Terminal 4 - MessageLogger
cd src/MessageLogger
dotnet run

# Terminal 5 - DashboardBlazor
cd src/DashboardBlazor
dotnet run
```

## Portas Utilizadas

- RabbitMQ: 5672 (AMQP), 15672 (Management UI - user/password)
- Kafka: 9092
- MySQL: 3306 (user/password)
- DashboardBlazor: 5000

## Monitoramento

- RabbitMQ Management: http://localhost:15672
- DashboardBlazor: http://localhost:5000

## Volumes de Dados

Os dados são persistidos nos seguintes volumes Docker:
- rabbitmq_data: Mensagens e configurações do RabbitMQ
- zookeeper_data e zookeeper_log: Dados do Zookeeper
- kafka_data: Tópicos e mensagens do Kafka
- mysql_data: Banco de dados MySQL

## Limpeza

Para parar e remover todos os containers e volumes:
```bash
docker-compose down -v
```

## Estrutura de Logs

Todos os componentes utilizam Serilog para logging estruturado, com os seguintes campos:
- CorrelationId: ID único para rastreamento da mensagem
- Component: Nome do componente que gerou o log
- Status: Estado atual do processamento
- Timestamp: Data/hora do evento