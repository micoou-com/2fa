# 2FA 密钥输入说明（已迁移）

规范正文见：

- **英文（推荐）**：[docs/spec/secret-input.md](docs/spec/secret-input.md)
- **简体中文**：[docs/spec/RYIOF_DWS20260424002-2FA共享密钥输入规范.md](docs/spec/RYIOF_DWS20260424002-2FA共享密钥输入规范.md)（v1.1.0：仅 RFC 4648 Base32，与 `otpauth` 的 `secret` / 网站 Secret 一致，用于 TOTP 计算。）

实现代码：`TwoFactorAuth.Core.Totp.SecretBytes`。
