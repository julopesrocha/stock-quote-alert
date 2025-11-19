using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

/// Serviço responsável por consultar a cotação de um ativo usando a API da AlphaVantage.
public class StockPriceService
{
    private static readonly HttpClient _httpClient = new HttpClient(); // HttpClient estático para evitar exaustão de sockets.
    private readonly string _apiKey;

    /// Construtor exige uma API key válida.
    public StockPriceService(string apiKey)
    {
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
    }

    /// Consulta o preço atual de um ativo no formato "SYMBOL.SA".
    /// Retorna null caso ocorra erro, limite da API ou resposta inválida.
    public async Task<decimal?> GetStockPriceAsync(string symbol)
    {
        // Símbolo inválido ou vazio
        if (string.IsNullOrWhiteSpace(symbol))
            return null;

        try
        {
            string url = $"https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol={symbol}&apikey={_apiKey}"; // Monta URL do endpoint "GLOBAL_QUOTE"
            var resp = await _httpClient.GetAsync(url);

            resp.EnsureSuccessStatusCode(); // Lança exceção se 4xx ou 5xx

            var json = await resp.Content.ReadAsStringAsync(); // Lê JSON como string

            // Faz parsing usando JsonDocument 
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("Global Quote", out var quote))
            {
                if (quote.TryGetProperty("05. price", out var priceElement)) // Dentro de "Global Quote", tenta pegar "05. price"
                {
                    string? priceStr = priceElement.GetString();

                    // Converte string → decimal usando cultura invariante
                    if (!string.IsNullOrWhiteSpace(priceStr) &&
                        decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                    {
                        return price;
                    }
                }
            }

            // API rate limit ou erro silencioso
            if (root.TryGetProperty("Note", out _))
                return null;

            return null;
        }
        catch (Exception ex)
        {
            // Falhas de rede, parsing, indisponibilidade da API, etc.
            Console.WriteLine($"Erro ao consultar API: {ex.Message}");
            return null;
        }
    }
}
