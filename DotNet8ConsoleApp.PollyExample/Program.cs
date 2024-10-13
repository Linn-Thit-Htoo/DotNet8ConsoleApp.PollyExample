using Dumpify;
using Polly;

var client = new HttpClient();

var fallbackPolicy = Policy<string>
    .Handle<HttpRequestException>()
    .Or<Exception>()
    .FallbackAsync(
        fallbackValue: "Fallback: Could not retrieve products.",
        onFallbackAsync: async (exception, context) =>
        {
            string fallbackMessage = $"Executing fallback due to: {exception}";
            fallbackMessage.Dump();
            await Task.CompletedTask;
        }
    );

var retryPolicy = Policy<string>
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(
        retryCount: 2,
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(10),
        onRetryAsync: async (exception, timespan, retryAttempt, context) =>
        {
            string message =
                $"Retry attempt {retryAttempt} after {timespan.TotalSeconds} seconds due to: {exception}";
            message.Dump();
            await Task.CompletedTask;
        }
    );

var policyWrap = Policy.WrapAsync(fallbackPolicy, retryPolicy);

try
{
    var result = await policyWrap.ExecuteAsync(async () =>
    {
        var response = await client.GetAsync("https://fakestoreapis.com/products");
        response.EnsureSuccessStatusCode();

        string jsonStr = await response.Content.ReadAsStringAsync();
        return jsonStr;
    });

    result.Dump();
}
catch (Exception ex)
{
    throw new Exception(ex.Message);
}
