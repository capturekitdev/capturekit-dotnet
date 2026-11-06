# Security Policy

## Supported versions

Security fixes are released for the latest published version of the CaptureKit .NET SDK.

## Reporting a vulnerability

Please do not report security vulnerabilities through public GitHub issues.

Email **contact@capturekit.dev** with a description of the issue, the affected version, and steps to reproduce. We acknowledge reports within three business days and keep you informed until the issue is resolved.

## Handling API keys

- Keep your CaptureKit API key out of source control. Load it from the `CAPTUREKIT_API_KEY` environment variable or a secret manager.
- Rotate the key from the [CaptureKit dashboard](https://app.capturekit.dev) if you believe it was exposed.
