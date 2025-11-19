using System;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;
using StockQuoteAlert.Services;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length != 3)
        {
            Console.WriteLine("Uso correto:");
            Console.WriteLine("dotnet run <ATIVO> <PRECO_VENDA> <PRECO_COMPRA>");
            return;
        }

        string ativo = args[0];

        if (!decimal.TryParse(args[1], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal precoVenda))
        {
            Console.WriteLine("Erro: preço de venda inválido.");
            return;
        }

        if (!decimal.TryParse(args[2], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal precoCompra))
        {
            Console.WriteLine("Erro: preço de compra inválido.");
            return;
        }
        if (!ativo.EndsWith(".SA"))
        {
            ativo = ativo + ".SA";
        }


        // Carrega appjson
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

        var apiKey = configuracao["AlphaVantage:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine("ERRO: API Key da AlphaVantage não encontrada no appjson");
            return;
        }

        var emailService = new EmailService(
            smtpHost,
            smtpPort,
            smtpUser,
            smtpPassword,
            destinationEmail
        );


        Console.WriteLine("Configurações carregadas:");
        Console.WriteLine($"Email destino: {destinationEmail}");
        Console.WriteLine($"SMTP host: {smtpHost}");
        Console.WriteLine($"Intervalo: {secondsInterval}s");

        var priceService = new StockPriceService(apiKey);

        decimal? lastNotified = null;

        while (true)
        {
            var price = await priceService.GetStockPriceAsync(ativo);

            if (price != null)
            {
                Console.WriteLine($"Preço atual de {ativo}: {price}");

                if (price >= precoVenda && lastNotified != 1)
                {
                    emailService.SendEmail(
                        $"ALTA! {ativo}",
                        $"O preço atingiu {price}, acima do limite de venda {precoVenda}"
                    );
                    lastNotified = 1;
                }
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
                Console.WriteLine("Não foi possível obter a cotação.");
            }

            await Task.Delay(TimeSpan.FromSeconds(secondsInterval));
        }
    }
}

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
