# Documentation

TwoFactorAuth is a **free, open-source public-good project**. The apps are local-only TOTP authenticators with no accounts, ads, subscriptions, or cloud services.

## Specifications

| Document | Description |
|----------|-------------|
| [spec/secret-input.md](spec/secret-input.md) | TOTP shared secret input rules (normalization, Base32 decoding, rejection of recovery codes) |

Implementation: `TwoFactorAuth.Core.Totp.SecretBytes`.

## Supported platforms

| Platform | Client | Data location |
|----------|--------|---------------|
| Windows | WPF (`src/TwoFactorAuth.Win`) | `%AppData%\TwoFactorAuth\accounts.json` |
| Android | .NET Android (`src/TwoFactorAuthApp`) | App-private `accounts.json` |

iOS, Linux, macOS, and web clients are **not** in scope today.

## Build reference

```bash
# Windows client
dotnet run --project src/TwoFactorAuth.Win/TwoFactorAuth.Win.csproj

# Android APK
dotnet publish src/TwoFactorAuthApp/TwoFactorAuthApp.csproj \
  -c Release -f net9.0-android -p:AndroidPackageFormats=apk

# Tests
dotnet test TwoFactorAuth.sln
```

On Windows you can also run `.\build-apk.ps1`.

## Contributing

See [CONTRIBUTING.md](../CONTRIBUTING.md). For secret handling, treat [spec/secret-input.md](spec/secret-input.md) and `tests/TwoFactorAuth.Logic.Tests` as the source of truth.
