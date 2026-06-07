---
name: blazor-mailkit-smtp-env-configuration
description: Configure and implement a reusable SMTP email service in Blazor using MailKit, reading credentials and formatted sender details from `.env`.
source: auto-skill
extracted_at: '2026-06-07T17:15:00.000Z'
---

# MailKit SMTP Email Service with `.env` Configuration

When sending emails (e.g., password resets, confirmations) in a Blazor application, use `MailKit` configured via environment variables in `.env`. This keeps secrets out of code and allows easy environment switching (local vs. production).

## Approach

1. **Dependencies**: Add the `MailKit` NuGet package (`dotnet add package MailKit`).
2. **`.env` Variables**: Define the following in `.env`:
   ```env
   SMTP_HOST=mail.infomaniak.com
   SMTP_PORT=465
   SMTP_USER=your_user@domain.com
   SMTP_PASSWORD=your_app_password
   MAIL_FROM=Portfolio Arbiter <noreply@your-real-domain.com>
   BASE_URL=https://yourdomain.com
   ```
   ⚠️ **Critical**: The domain in `MAIL_FROM` and `SMTP_USER` **must be a real, verified domain** owned by you (e.g., `your-real-domain.com`). Providers like Infomaniak will reject the email with "Sender address rejected: Domain not found" if a placeholder like `votre-domaine.ch` is used.
3. **Service Implementation**: Create an `EmailService` that reads these variables using `DotNetEnv`.
   - Parse `MAIL_FROM` to extract both the display name and the email address if it contains `<` and `>`.
   - Use `MailKit.Security.SecureSocketOptions.Auto` when connecting, which automatically negotiates SSL/TLS based on the port (e.g., implicit SSL for 465, StartTLS for 587).
4. **Absolute Links**: Always use `Env.GetString("BASE_URL")` to construct absolute URLs for email links (like password reset tokens), rather than relying on `NavigationManager.BaseUri`, which can be unreliable in background tasks or certain hosting environments.

## Example Snippet (`EmailService.cs`)
```csharp
var mailFrom = Env.GetString("MAIL_FROM", "App <noreply@domain.com>");
if (mailFrom.Contains('<') && mailFrom.Contains('>'))
{
    _fromName = mailFrom.Split('<')[0].Trim();
    _fromEmail = mailFrom.Split('<')[1].Replace(">", "").Trim();
}
// ...
await client.ConnectAsync(_smtpHost, _smtpPort, MailKit.Security.SecureSocketOptions.Auto);
```

## Why
- **Security**: Credentials are never hardcoded.
- **Robustness**: `SecureSocketOptions.Auto` prevents connection errors when switching between ports 465 (SSL) and 587 (StartTLS).
- **Reliability**: Using `BASE_URL` from `.env` guarantees that email links point to the correct public domain, avoiding localhost links in production emails.