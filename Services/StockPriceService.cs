using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

public class StockPriceService
{
    private static readonly HttpClient _httpClient = new HttpClient();
    private readonly string _apiKey;

    public StockPriceService(string apiKey)
    {
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
    }

    public async Task<decimal?> GetStockPriceAsync(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            return null;

        try
        {
            string url = $"https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol={symbol}&apikey={_apiKey}";
            var resp = await _httpClient.GetAsync(url);

            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync();


            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("Global Quote", out var quote))
            {
                if (quote.TryGetProperty("05. price", out var priceElement))
                {
                    string? priceStr = priceElement.GetString();

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
            Console.WriteLine($"Erro ao consultar API: {ex.Message}");
            return null;
        }
    }
}
