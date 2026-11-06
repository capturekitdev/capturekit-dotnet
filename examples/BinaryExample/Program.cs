using CaptureKit;

using var client = new CaptureKitClient(); // reads CAPTUREKIT_API_KEY
var response = await client.CaptureAsync(new CaptureParams { Url = "https://example.com", Format = "png" });
if (response.IsJson)
{
    Console.WriteLine(response.Json);
}
else
{
    await File.WriteAllBytesAsync("screenshot.png", response.Bytes);
    Console.WriteLine($"Saved screenshot.png ({response.ContentType}, {response.Bytes.Length} bytes)");
}
