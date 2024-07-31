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
            Console.WriteLine($"Retry attempt {retryAttempt} after {timespan.TotalSeconds} seconds due to: {exception.Message}");
        });

var result = await retryPolicy.ExecuteAsync(async () =>
{
    var response = await client.GetAsync("https://fakestoreapia.com/products");
    response.EnsureSuccessStatusCode();

    string jsonStr = await response.Content.ReadAsStringAsync();
    return jsonStr;
});

result.Dump();
