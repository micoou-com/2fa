# Documentation

## Public specifications

| Document | Description |
|----------|-------------|
| [spec/secret-input.md](spec/secret-input.md) | **TOTP shared secret input rules** (English, canonical for contributors) |
| [spec/RYIOF_DWS20260424002-2FA共享密钥输入规范.md](spec/RYIOF_DWS20260424002-2FA共享密钥输入规范.md) | Same specification in Simplified Chinese |

## What this repository implements today

The shipped clients are **local-only** TOTP authenticators:

- **Windows** — WPF desktop app (`src/TwoFactorAuth.Win`)
- **Android** — .NET Android app with QR scan (`src/TwoFactorAuthApp`)

Accounts are stored as plain JSON in the app sandbox / `%AppData%`. There is **no** cloud sync or account login in the current codebase.

## Maintainer notes

See [internal/project-memory.md](internal/project-memory.md) for a short platform and build reference.

If you contribute code, treat [spec/secret-input.md](spec/secret-input.md) and the unit tests as the source of truth for secret handling.
