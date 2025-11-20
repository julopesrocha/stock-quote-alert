using System;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;
using StockQuoteAlert.Services;
using Serilog;

class Program
{
    static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.Console()
        .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
        .CreateLogger();

        Log.Information("Iniciando aplicação...");


        if (args.Length != 3)
        {
            Log.Information("Uso correto: dotnet run <ATIVO> <PRECO_VENDA> <PRECO_COMPRA>");
            return;
        }

        string ativo = args[0];

        // Converte argumentos de preço usando cultura invariante

        if (!decimal.TryParse(args[1], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal precoVenda))
        {
            Log.Error("Erro: preço de venda inválido.");
            return;
        }

        if (!decimal.TryParse(args[2], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal precoCompra))
        {
            Log.Error("Erro: preço de compra inválido.");
            return;
        }

        // A API da AlphaVantage exige ".SA" para ativos da B3

        if (!ativo.EndsWith(".SA"))
        {
            ativo = ativo + ".SA";
        }

        // Carregamento de configuração (appsettings.json)

        var configuracao = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        string destinationEmail = ConfigHelper.GetRequiredString(configuracao, "Email:Destino");
        string smtpHost = ConfigHelper.GetRequiredString(configuracao, "Email:SMTP:Host");
        string smtpUser = ConfigHelper.GetRequiredString(configuracao, "Email:SMTP:Usuario");
        string smtpPassword = ConfigHelper.GetRequiredString(configuracao, "Email:SMTP:Senha");

        int smtpPort = int.Parse(ConfigHelper.GetRequiredString(configuracao, "Email:SMTP:Porta"));
        bool smtpUseSSL = bool.Parse(ConfigHelper.GetRequiredString(configuracao, "Email:SMTP:UseSSL"));
        int secondsInterval = int.Parse(ConfigHelper.GetRequiredString(configuracao, "Monitoramento:secondsInterval"));
        
        var apiKey = configuracao["AlphaVantage:ApiKey"]; // API Key para a AlphaVantage

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Log.Error("Erro: API Key da AlphaVantage não encontrada no appjson");
            return;
        }

        // Inicialização dos serviços
        var emailService = new EmailService(
            smtpHost,
            smtpPort,
            smtpUser,
            smtpPassword,
            destinationEmail
        );


        Log.Information("Configurações carregadas:");

        Log.Information($"Email destino: {destinationEmail}");
        Console.WriteLine($"Email destino: {destinationEmail}");

        Log.Information($"SMTP host: {smtpHost}");
        Console.WriteLine($"SMTP host: {smtpHost}");

        Log.Information($"Intervalo: {secondsInterval}s");
        Console.WriteLine($"Intervalo: {secondsInterval}s");

        var priceService = new StockPriceService(apiKey);

        decimal? lastNotified = null; // evita envio repetido de alertas idênticos

        // Loop de monitoramento contínuo
        while (true)
        {
            var price = await priceService.GetStockPriceAsync(ativo);

            if (price != null)
            {
                Console.WriteLine($"Preço atual de {ativo}: {price}");
                Log.Warning("Preço atual de {Ativo}: {Preco}", ativo, price);

                // Se preço >= limite de venda
                if (price >= precoVenda && lastNotified != 1)
                {
                    emailService.SendEmail(
                        $"ALTA! {ativo}",
                        $"O preço atingiu {price}, acima do limite de venda {precoVenda}"
                    );
                    lastNotified = 1;
                }
                // Se preço <= limite de compra
                else if (price <= precoCompra && lastNotified != -1)
                {
                    emailService.SendEmail(
                        $"BAIXA! {ativo}",
                        $"O preço caiu para {price}, abaixo do limite de compra {precoCompra}"
                    );
                    lastNotified = -1;
                }
            }
            else
            {
                Log.Warning("Não foi possível obter a cotação.");
                Console.WriteLine("Não foi possível obter a cotação.");
            }
            // Aguarda o intervalo configurado antes da próxima consulta
            await Task.Delay(TimeSpan.FromSeconds(secondsInterval));
        }
    }
}

// Helper para simplificar leitura obrigatória de strings
static class ConfigHelper
{
    public static string GetRequiredString(IConfiguration config, string key)
    {
        string? value = config[key];
        if (string.IsNullOrEmpty(value))
            throw new Exception($"Configuração obrigatória ausente: {key}");
        return value;
    }
}