using System;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

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


        // Carrega appsettings.json
        var configuracao = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        string emailDestino = ConfigHelper.GetRequiredString(configuracao, "Email:Destino");
        string smtpHost = ConfigHelper.GetRequiredString(configuracao, "Email:SMTP:Host");
        string smtpUsuario = ConfigHelper.GetRequiredString(configuracao, "Email:SMTP:Usuario");
        string smtpSenha = ConfigHelper.GetRequiredString(configuracao, "Email:SMTP:Senha");

        int smtpPorta = int.Parse(ConfigHelper.GetRequiredString(configuracao, "Email:SMTP:Porta"));
        bool smtpUseSSL = bool.Parse(ConfigHelper.GetRequiredString(configuracao, "Email:SMTP:UseSSL"));
        int intervaloSegundos = int.Parse(ConfigHelper.GetRequiredString(configuracao, "Monitoramento:IntervaloSegundos"));

        var apiKey = configuracao["AlphaVantage:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine("ERRO: API Key da AlphaVantage não encontrada no appsettings.json");
            return;
        }


        Console.WriteLine("Configurações carregadas:");
        Console.WriteLine($"Email destino: {emailDestino}");
        Console.WriteLine($"SMTP host: {smtpHost}");
        Console.WriteLine($"Intervalo: {intervaloSegundos}s");

        var priceService = new StockPriceService(apiKey);

        while (true)
        {
            var precoAtual = await priceService.GetStockPriceAsync(ativo);

            if (precoAtual != null)
            {
                Console.WriteLine($"Preço atual de {ativo}: {precoAtual}");
            }
            else
            {
                Console.WriteLine("Não foi possível obter a cotação.");
            }

            await Task.Delay(intervaloSegundos * 1000);
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
