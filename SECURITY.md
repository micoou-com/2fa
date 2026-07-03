# Security policy

## Supported versions

| Version | Supported |
|---------|-----------|
| `main` branch | Yes |

## Reporting a vulnerability

**Please do not report security vulnerabilities through public GitHub issues.**

This project is a **demo / educational** TOTP client. Secrets are stored in **plain JSON** on disk without hardware-backed encryption. It is **not** intended as a production-grade security product.

If you still believe you have found a security-relevant defect (for example, incorrect TOTP computation, secret leakage through logs, or unsafe parsing), please report it privately:

1. Email the maintainers at **security@reegenius.com** (replace with your project's contact if different), or
2. Use GitHub **Private vulnerability reporting** if enabled on the repository.

Include:

- A description of the issue and impact
- Steps to reproduce
- Affected platform (Windows / Android / Core library)
- Any suggested fix, if you have one

We will acknowledge receipt within a reasonable time and work with you on a fix before any public disclosure, when applicable.

## Scope

Out of scope for this repository's threat model:

- Physical access to an unlocked device with a readable `accounts.json`
- Users pasting recovery codes or non-Base32 material into the secret field (rejected by design; see [docs/spec/secret-input.md](docs/spec/secret-input.md))

In scope:

- Incorrect RFC 6238 / Base32 implementation
- Memory or log exposure of secrets
- Injection or corruption via `otpauth` URI parsing
