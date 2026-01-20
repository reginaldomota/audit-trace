#!/bin/sh

# Aguardar LocalStack estar pronto
echo "Aguardando LocalStack..."
until curl -s http://localstack:4566/_localstack/health | grep -q '"sqs": "available"'; do
    sleep 2
done

echo "LocalStack pronto! Criando fila SQS..."

# Criar fila SQS usando awslocal (vem com LocalStack)
curl -X POST "http://localstack:4566/" \
    -H "Content-Type: application/x-www-form-urlencoded" \
    -d "Action=CreateQueue&QueueName=product-registration-queue&Version=2012-11-05"

echo ""
echo "Fila 'product-registration-queue' criada com sucesso!"
