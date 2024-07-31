using Dumpify;
using Polly;

var client = new HttpClient();

var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(
        retryCount: 2,
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(10),
        onRetryAsync: async (exception, timespan, retryAttempt, context) =>
        {
            string message =
                $"Retry attempt {retryAttempt} after {timespan.TotalSeconds} seconds due to: {exception.Message}";
            message.Dump();
        }
    );

try
{
    var result = await retryPolicy.ExecuteAsync(async () =>
    {
        var response = await client.GetAsync("https://fakestoreapi.com/products");
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
