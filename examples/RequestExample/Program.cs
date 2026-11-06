using CaptureKit;

using var client = new CaptureKitClient(); // reads CAPTUREKIT_API_KEY
try
{
    var response = await client.RequestAsync("GET", "/v1/usage");
    Console.WriteLine(response.Text);
}
catch (CaptureKitApiException exception)
{
    Console.Error.WriteLine($"HTTP {exception.StatusCode}: {exception.Message}");
}
