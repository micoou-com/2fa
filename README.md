# TwoFactorAuth

[English](README.md) | [简体中文](README.zh-CN.md)

A **free, open-source** TOTP authenticator for **Windows** and **Android**, built with **.NET 9**.

This is a **public-good project**: no ads, no subscriptions, no accounts, and no cloud sync. Source code and documentation are published under [Apache 2.0](LICENSE) for anyone to use, study, and improve.

## Why this project?

Two-factor codes are basic security hygiene, but many existing authenticator apps get in the way:

1. **Some charge money** — subscriptions, paid tiers, or “pro” features for something as simple as showing a TOTP code.
2. **Many are too heavy** — sign-up, cloud accounts, sync setup, ads, and extra screens before you can add a single token.

TwoFactorAuth does the opposite: **scan or paste, then see your codes**. Data stays on your device. No registration, no billing, no cloud dependency — just a small tool that does one job well.

## Features

- Scan `otpauth://totp/` QR codes (Android) or paste URIs (Windows)
- Manually add secrets with **Base32** validation and recovery-code detection
- Local JSON storage in the app sandbox / `%AppData%`
- **Bilingual UI**: English and Simplified Chinese (follows system language)

> **Security note.** Secrets are stored in plain JSON without hardware-backed encryption. Suitable for learning and personal use; evaluate carefully before relying on it for high-value accounts.

## Quick start

### Requirements

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- **Windows**: desktop runtime for `net9.0-windows`
- **Android**: Android workload (`dotnet workload install android`) and API 24+ device or emulator

### Run (Windows)

```bash
git clone https://github.com/micoou-com/2fa.git
cd 2fa
dotnet run --project src/TwoFactorAuth.Win/TwoFactorAuth.Win.csproj
```

Data file: `%AppData%\TwoFactorAuth\accounts.json`

### Build Android APK

Release APKs are signed with the **public keystore** in `src/TwoFactorAuthApp/signing/` (see [signing/README.md](src/TwoFactorAuthApp/signing/README.md)).

```bash
dotnet publish src/TwoFactorAuthApp/TwoFactorAuthApp.csproj \
  -c Release -f net9.0-android
```

Signed output: `src/TwoFactorAuthApp/bin/Release/net9.0-android/publish/com.reegenius.twofactorauth-Signed.apk`

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
| `docs/` | Specifications and build reference |

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
