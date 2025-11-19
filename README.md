# 📈 Stock Quote Alert

Um monitor de cotação para ativos da B3 que envia **alertas por e-mail** quando o preço ultrapassa limites definidos para **compra** e **venda**.

Aplicação desenvolvida em **C# (.NET)** como console app, utilizando:

- AlphaVantage API para obter cotações
- SMTP (Gmail ou outro servidor) para envio de alertas
- Configuração via arquivo `appsettings.json`

---

## 🚀 Objetivo

Monitorar continuamente o preço de um ativo da B3 (ex: PETR4, VALE3, ITUB4) e enviar e-mails quando:

- 📤 **Preço ultrapassar o limite de VENDA**
- 📥 **Preço cair abaixo do limite de COMPRA**

Exemplo:
dotnet run PETR4 40.00 30.00

---

## 📦 Instalação

Clone o repositório:

git clone https://github.com/julopesrocha/stock-quote-alert.git
cd stock-quote-alert

- Restaure dependências:

dotnet restore

- Compile:

dotnet build

----

## ⚙️ Configuração

Crie (ou edite) o arquivo appsettings.json na raiz do projeto:

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

## 🔑 Como obter a API Key da AlphaVantage:

https://www.alphavantage.co/

Clique em Get Your Free API Key

Copie a chave para appsettings.json

---

## ▶️ Como executar

Estrutura:

dotnet run <ATIVO> <PRECO_VENDA> <PRECO_COMPRA>


*Exemplo prático*:

dotnet run PETR4 40 30


Se digitar somente o ativo sem “.SA”, o programa adicionará automaticamente.

---

## 📬 Funcionamento dos alertas

O programa roda em loop infinito enquanto monitora:

Se preço >= preço_venda → envia e-mail aconselhando VENDA

Se preço <= preço_compra → envia e-mail aconselhando COMPRA

Logs aparecem no console:

Preço atual de PETR4.SA: 32,99
>>> Email enviado: ALTA! PETR4


O sistema evita repetição de alertas seguidos usando estado interno (lastNotified).

## 🧱 Arquitetura
/src
 ├─ Program.cs              → Ponto principal, loop de monitoramento
 ├─ StockPriceService.cs    → Consulta cotações na AlphaVantage
 ├─ EmailService.cs         → Envio de alertas SMTP
 └─ appsettings.json        → Configurações externas

## 🔒 Segurança

Use *App Password* caso utilize Gmail

**Não faça commit de sua API key ou senha de e-mail**

Recomenda-se usar variáveis de ambiente em produção