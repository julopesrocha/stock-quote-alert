# 📈 Stock Quote Alert

Monitor de cotações para ativos da B3 que envia alertas por e-mail quando o preço ultrapassa limites definidos para compra e venda.

Aplicação desenvolvida em C# (.NET 8), com arquitetura organizada em serviços e configuração externa.

## 🚀 Objetivo

Monitorar continuamente o preço de um ativo da B3 (ex: PETR4, VALE3, ITUB4) e notificar por e-mail quando:

📤 Preço >= limite de VENDA → alerta de alta

📥 Preço <= limite de COMPRA → alerta de baixa

Exemplo:

dotnet run PETR4 40.00 30.00

## 🧱 Arquitetura do Projeto
stock-quote-alert/
│
├─ Program.cs                 # Ponto de entrada: loop de monitoramento
├─ Services/
│   ├─ StockPriceService.cs   # Consulta de cotações (AlphaVantage)
│   └─ EmailService.cs        # Envio de alertas via SMTP
│
├─ logs/                      # Arquivos de log gerados pela aplicação
│
├─ appsettings.json           # Configurações externas
├─ stock-quote-alert.csproj
│
├─ Dockerfile                 # Execução do projeto em container
└─ README.md

## ⚙️ Configuração (appsettings.json)

Crie ou edite o arquivo appsettings.json na raiz do projeto:

{
  "Email": {
    "Destino": "email@exemplo.com",
    "SMTP": {
      "Host": "smtp.gmail.com",
      "Porta": 587,
      "Usuario": "seu-email@gmail.com",
      "Senha": "sua-senha-ou-app-password",
      "UseSSL": true
    }
  },

  "AlphaVantage": {
    "ApiKey": "SUA_API_KEY_AQUI"
  },

  "Monitoramento": {
    "secondsInterval": 10
  }
}

## 🔑 Como obter sua API Key (AlphaVantage)

Acesse: https://www.alphavantage.co/

Clique em Get Your Free API Key

Copie a chave e cole em appsettings.json

📦 Instalação
git clone https://github.com/julopesrocha/stock-quote-alert.git
cd stock-quote-alert

dotnet restore
dotnet build

## ▶️ Como executar

Formato:

dotnet run < ATIVO> <PRECO_VENDA> <PRECO_COMPRA>


Exemplo real:

dotnet run PETR4 40 30


Se você digitar apenas "PETR4", o programa adiciona automaticamente o sufixo “.SA”.

## 📬 Funcionamento dos Alertas

A aplicação roda continuamente:

Se preço >= preço_venda → envia alerta de VENDA

Se preço <= preço_compra → envia alerta de COMPRA

Exemplo de log:

Preço atual de PETR4.SA: 32,99
>>> Email enviado: ALERTA DE COMPRA PARA PETR4


Para evitar spam, o sistema não envia alertas repetidos usando estado interno (lastNotified).

## 🐳 Executando via Docker

Monte a imagem:

docker build -t stock-alert .


Execute:

docker run stock-alert PETR4 40 30


Lembre-se de montar um volume se quiser persistir os logs.

## 🔒 Segurança

Nunca faça commit de senha de e-mail ou API key.

Para Gmail, utilize App Password.

Em produção, prefira variáveis de ambiente.
