using CaptureKit;

using var client = new CaptureKitClient(); // reads CAPTUREKIT_API_KEY
var response = await client.ContentAsync(new ContentParams { Url = "https://example.com" });
Console.WriteLine(response.Json);
