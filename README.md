# CaptureKit .NET SDK

The official .NET SDK for the [CaptureKit](https://www.capturekit.dev) screenshot API.

Capture websites as PNG, JPEG, WebP or PDF, extract their content, or analyse them with AI.

The SDK exposes **7 typed async methods**, one per documented endpoint (with `required` properties checked at compile time), plus a generic `RequestAsync`.

## Requirements

- .NET 8 or newer

## Installation

```sh
dotnet add package CaptureKit
```

## Get your API key

[Sign up for CaptureKit](https://app.capturekit.dev/register) or sign in, then copy your API key from the [dashboard](https://app.capturekit.dev). `new CaptureKitClient()` reads the `CAPTUREKIT_API_KEY` environment variable.

## Quick start

```csharp
using CaptureKit;

using var client = new CaptureKitClient();
var response = await client.ContentAsync(new ContentParams { Url = "https://example.com" });
Console.WriteLine(response.Json);
```

Parameter classes mirror the [API reference](https://app.capturekit.dev/openapi). Use `response.Deserialize<MyType>()` to map the JSON onto your own types.

## Binary responses

Endpoints that return a file (image, PDF…) give you the bytes; `IsJson` tells both cases apart:

```csharp
var response = await client.CaptureAsync(new CaptureParams { Url = "https://example.com", Format = "png" });
if (!response.IsJson)
{
    await File.WriteAllBytesAsync("screenshot.png", response.Bytes);
}
```

## Async jobs

Any call accepts `is_async: true` (useful past the 60-second server limit): the response is then a `job_id`. `WaitForJobAsync` polls `GET /v1/jobs/{id}` until a terminal status (`CompletedJob`, `Error`, `NotFound`, `BadRequest`):

```csharp
var submitted = await client.ContentAsync(new ContentParams { Url = "https://example.com", IsAsync = true });
var job = await client.WaitForJobAsync(
    submitted.Json.GetProperty("job_id").GetString()!,
    onProgress: j => Console.WriteLine($"{j.GetProperty("status")} {j.GetProperty("progress")}%"));
```

## GET or POST

Methods use GET when the endpoint supports it (query string). POST sends the same parameters as JSON:

```csharp
await client.AnalyzeAsync(new AnalyzeParams { Url = "test" }, new RequestOptions { Method = "POST" });
```

## Any endpoint

```csharp
var response = await client.RequestAsync("GET", "/v1/usage");
```

## Errors

```csharp
try
{
    await client.ContentAsync(new ContentParams { Url = "https://example.com" });
}
catch (CaptureKitApiException exception)
{
    Console.Error.WriteLine($"{exception.StatusCode}: {exception.Message}");
}
```

| Exception | When |
| --- | --- |
| `CaptureKitConfigurationException` | Missing API key, invalid options or unsupported method. |
| `CaptureKitSerializationException` | A required parameter is missing or a value can't be sent. |
| `CaptureKitTransportException` | Network failure or timeout. |
| `CaptureKitApiException` | Non-2xx status (`StatusCode`, `ErrorMessage`, `ResponseBody`, `RawResponse`). |
| `CaptureKitResponseDecodingException` | A success response could not be decoded. |

All of them derive from `CaptureKitException`. Caller cancellation surfaces as `OperationCanceledException`.

## Retries

Requests are retried up to 3 times after a network error, a `408`, `500`, `502`, `503` or `504`, or a rate limit — which the API returns as `401` (or `429`) with a `Rate limit exceeded` message. Per-key quota errors (`…: quota daily`) are not retried, nor are `400`, `402`, `404` or client-side timeouts. The delay starts at 1 second and grows 1.5× per retry (capped at 30 seconds); a `Retry-After` header wins.

```csharp
using var client = new CaptureKitClient(new CaptureKitClientOptions { MaxRetries = 5, RetryDelay = TimeSpan.FromMilliseconds(500) });
await client.ContentAsync(new ContentParams { Url = "https://example.com" }, new RequestOptions { MaxRetries = 0 }); // one request without retries
```

## Batch requests

`BatchAsync` runs a function per item with at most 10 in flight (`concurrency`) and returns one `BatchResult` per item, in input order. A failing item never stops the others.

```csharp
string[] inputs = { "https://example.com", "https://www.capturekit.dev", "https://github.com" };
var results = await client.BatchAsync(inputs, (value, ct) =>
    client.ContentAsync(new ContentParams { Url = value }, cancellationToken: ct), concurrency: 5);
foreach (var result in results)
{
    if (result.Ok)
    {
        Console.WriteLine(result.Value!.Json);
    }
    else
    {
        Console.Error.WriteLine($"{inputs[result.Index]}: {result.Error!.Message}");
    }
}
```

| Constant | Default | Description |
| --- | --- | --- |
| `CaptureKitClient.DefaultConcurrency` | `10` | Tasks run at the same time by batch |
| `CaptureKitClient.DefaultTimeout` | `TimeSpan.FromSeconds(120)` | Request timeout |
| `CaptureKitClient.MaxRetries` | `3` | Retry attempts after a network error, a 408/5xx or a rate limit |
| `CaptureKitClient.RetryBackoffFactor` | `1.5` | Exponential backoff multiplier |
| `CaptureKitClient.DefaultRetryDelay` | `TimeSpan.FromSeconds(1)` | Delay before the first retry |

## Configuration

```csharp
using var client = new CaptureKitClient(new CaptureKitClientOptions
{
    ApiKey = Environment.GetEnvironmentVariable("CAPTUREKIT_API_KEY"),
    Timeout = TimeSpan.FromSeconds(60), // default 120 s
    HttpClient = myHttpClient,           // optional, you keep ownership
});
```

## Endpoints

7 methods, generated from the [OpenAPI document](https://app.capturekit.dev/openapi).

| Method | HTTP | Description |
| --- | --- | --- |
| `AnalyzeAsync` | GET / POST `/v1/analyze` | AI Content API analyzes any webpage and returns AI insights summary, value proposition, features, categories, use cases, and optional conta… |
| `CaptureAsync` | GET / POST `/v1/capture` | Browser-like Screenshot API that captures webpages as PNG/JPEG/WebP or PDF for previews, QA, reporting, monitoring, and archiving no headle… |
| `ContentAsync` | GET / POST `/v1/content` | CaptureKit Content API extracts structured webpage data, metadata, links, HTML/Markdown: in one request for SEO, monitoring, indexing and a… |
| `JobsAsync` | GET `/v1/jobs/{id}` | Poll the status, progress (0–100) and result of a job started with is_async=true. Polling is free. Poll until status is terminal: Completed… |
| `StatsAsync` | GET `/v1/stats` | Infrastructure health for an engine or endpoint over the last hour. Not scoped to your account or tenant. Enabled per account: HTTP 403 mea… |
| `UsageAsync` | GET `/v1/usage` | Retrieve real-time usage statistics and credit balance for your CaptureKit account. |
| `WebsiteCrawlerAsync` | GET / POST `/v1/website/crawler` | Website Crawler API fetches raw HTML from any webpage in request mode (no rendering) for fast scraping, audits, monitoring, and link/metada… |

## License

MIT — see [LICENSE](LICENSE).
