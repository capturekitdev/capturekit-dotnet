using CaptureKit;

using var client = new CaptureKitClient(); // reads CAPTUREKIT_API_KEY
var submitted = await client.ContentAsync(new ContentParams { Url = "https://example.com", IsAsync = true });
var job = await client.WaitForJobAsync(
    submitted.Json.GetProperty("job_id").GetString()!,
    onProgress: j => Console.WriteLine($"{j.GetProperty("status")} {j.GetProperty("progress")}%"));
Console.WriteLine(job.Json);
