using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace St.HolyChain.TestTools;
public static class TestExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };
    public static async Task<T> DeserializeHttpContentAsync<T>(this HttpContent? content, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(content);

        var responseContent = await content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(responseContent))
        {
            return default!;
        }

        var value = JsonSerializer.Deserialize<T>(responseContent, JsonSerializerOptions)
                          ?? throw new Exception("An error has occurred");

        return value;
    }

    public static HttpContent SerializeHttpContent<T>(this T request) where T : class
    {
        var content = new StringContent(JsonSerializer.Serialize(request, JsonSerializerOptions), Encoding.UTF8, "application/json");

        return content;
    }

    public static string Serialize<T>(this T? value) where T : class
    {
        var content = JsonSerializer.Serialize(value, JsonSerializerOptions);

        return content;
    }

    public static T Deserialize<T>(this string value) where T : class
    {
        var content = JsonSerializer.Deserialize<T>(value, JsonSerializerOptions);

        if (content is null)
        {
            throw new Exception("Deserialization failed");
        }

        return content;
    }
}
