# TwoFactorAuth

[English](README.md) | [简体中文](README.zh-CN.md)

A lightweight **TOTP** authenticator for **Windows** and **Android**, built with **.NET 9**.

- Scan `otpauth://totp/` QR codes (Android) or paste URIs (Windows)
- Manually add secrets with **Base32** validation and recovery-code detection
- Local JSON storage in the app sandbox / `%AppData%`
- **Bilingual UI**: English and Simplified Chinese (follows system language)

> **Demo / educational use.** Secrets are stored in plain JSON without hardware-backed encryption. Not a production security product.

## Screenshots

| Windows | Android |
|---------|---------|
| List with live countdown | QR scan + manual entry |

## Quick start

### Requirements

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- **Windows**: desktop runtime for `net9.0-windows`
- **Android**: Android workload (`dotnet workload install android`) and API 24+ device or emulator

### Run (Windows)

```bash
git clone https://github.com/ReeGenius/two-factor-auth.git
cd two-factor-auth
dotnet run --project src/TwoFactorAuth.Win/TwoFactorAuth.Win.csproj
```

Data file: `%AppData%\TwoFactorAuth\accounts.json`

### Build Android APK

```bash
dotnet publish src/TwoFactorAuthApp/TwoFactorAuthApp.csproj \
  -c Release -f net9.0-android -p:AndroidPackageFormats=apk
```

Or on Windows:

```powershell
.\build-apk.ps1
.\build-apk.ps1 -Install   # build and adb install
```

### Tests

```bash
dotnet test TwoFactorAuth.sln
```

## Project layout

| Path | Description |
|------|-------------|
| `src/TwoFactorAuth.Core` | TOTP, Base32, `otpauth` parser, JSON store, i18n strings |
| `src/TwoFactorAuth.Win` | WPF desktop client |
| `src/TwoFactorAuthApp` | .NET Android client (CameraX + ZXing) |
| `tests/TwoFactorAuth.Logic.Tests` | Unit tests |
| `docs/` | Specifications and contributor documentation |

## Secret input rules

Only **RFC 4648 Base32** setup keys are accepted (same as `secret` in `otpauth` URIs). The app **rejects**:

- Account **recovery codes** (often contain `0`, `1`, `8`, `9` or fixed segment layouts)
- **Hexadecimal** keys
- Invalid or too-short Base32

See [docs/spec/secret-input.md](docs/spec/secret-input.md).

## Localization

| Platform | Mechanism |
|----------|-----------|
| Core (errors) | `Localization/strings.{en,zh-CN}.json` via `Loc.T()` |
| Windows UI | `Localization/ui.{en,zh-CN}.json` via `UiLoc.T()` |
| Android UI | `values/strings.xml` (default EN) + `values-zh-rCN/strings.xml` |

Language follows the OS locale (`en` or `zh-*`).

## Contributing

Issues and pull requests are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md). Please run `dotnet test` before submitting.

## License

Licensed under the [Apache License, Version 2.0](LICENSE). See [NOTICE](NOTICE) for attribution and third-party notices.
