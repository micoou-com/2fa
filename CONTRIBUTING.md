# Contributing

Thank you for your interest in contributing to TwoFactorAuth — a free, open-source public-good project.

## Getting started

1. Fork the repository and create a branch from `main`.
2. Install the [.NET 9 SDK](https://dotnet.microsoft.com/download).
3. For Android work: `dotnet workload install android` (or `dotnet workload restore`).
4. Build and test:

```bash
dotnet restore TwoFactorAuth.sln
dotnet test TwoFactorAuth.sln
```

## Pull requests

- Keep changes focused; one logical change per PR when possible.
- Run `dotnet test TwoFactorAuth.sln` before opening a PR.
- Update user-facing strings in **both** English and Simplified Chinese when you change UI or error messages:
  - Core: `src/TwoFactorAuth.Core/Localization/strings.{en,zh-CN}.json`
  - Windows: `src/TwoFactorAuth.Win/Localization/ui.{en,zh-CN}.json`
  - Android: `src/TwoFactorAuthApp/Resources/values/strings.xml` and `values-zh-rCN/strings.xml`
- If you change secret parsing or validation, update [docs/spec/secret-input.md](docs/spec/secret-input.md) and add or adjust tests in `tests/TwoFactorAuth.Logic.Tests`.

## Code style

- Match existing naming and structure in the file you edit.
- Prefer small, readable changes over large refactors unless discussed in an issue first.
- Do not commit build outputs (`bin/`, `obj/`, `*.apk`).

## Security

If you find a security issue, please read [SECURITY.md](SECURITY.md) and report it privately. Do not open a public issue for undisclosed vulnerabilities.

## License

By contributing, you agree that your contributions will be licensed under the [Apache License 2.0](LICENSE).

## Commit attribution

- Commits must list **human contributors only**. Do **not** add `Co-authored-by` trailers for AI tools (including Cursor) unless the maintainer explicitly approves.
- In Cursor: disable automatic co-author injection before committing (Settings → search **co-author** or **attribution**, turn off commit co-author / attribution features).
- If an unauthorized co-author appears in history, rewrite the commit message before pushing (maintainers may force-push to fix the contributor graph).
