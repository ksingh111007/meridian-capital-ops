using System.Text.Json.Nodes;

namespace Meridian.Api.IntegrationTests.Support;

internal static class HttpAssert
{
    public static async Task<JsonNode> ReadJsonAsync(this HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync();
        return JsonNode.Parse(text) ?? throw new InvalidOperationException($"Empty JSON body (status {response.StatusCode}).");
    }

    public static async Task<JsonNode> GetJsonAsync(this HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"GET {url} failed with {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        }

        return await response.ReadJsonAsync();
    }
}
