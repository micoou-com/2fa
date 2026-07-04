# TwoFactorAuth（2FA 验证器）

[English](README.md) | [简体中文](README.zh-CN.md)

基于 **.NET 9** 的**免费开源** TOTP 身份验证器，支持 **Windows** 与 **Android**。

本项目以**公益**为目的：无广告、无订阅、无账号、无云同步。源码与文档采用 [Apache 2.0](LICENSE) 发布，供任何人免费使用、学习与改进。

## 功能

- **Android** 扫描二维码（`otpauth://totp/`），**Windows** 粘贴 URI
- 手动添加密钥，自动识别 **Base32** / 恢复码 / 十六进制误输入
- 数据保存在应用沙箱 / `%AppData%` 的 JSON 文件中
- **中英双语界面**，跟随系统语言

> **安全提示。** 密钥以明文 JSON 存储，未使用硬件级加密。适合学习与个人使用；用于高价值账户前请自行评估风险。

## 快速开始

### 环境

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- **Windows**：`net9.0-windows` 桌面运行时
- **Android**：安装 Android 工作负载（`dotnet workload install android`），API 24+ 设备或模拟器

### 运行 Windows 客户端

```bash
git clone https://github.com/ReeGenius/two-factor-auth.git
cd two-factor-auth
dotnet run --project src/TwoFactorAuth.Win/TwoFactorAuth.Win.csproj
```

数据文件：`%AppData%\TwoFactorAuth\accounts.json`

### 构建 Android APK

Release 包使用仓库内 **`src/TwoFactorAuthApp/signing/`** 中的公开签名文件（见 [signing/README.md](src/TwoFactorAuthApp/signing/README.md)）。

```bash
dotnet publish src/TwoFactorAuthApp/TwoFactorAuthApp.csproj \
  -c Release -f net9.0-android
```

签名输出：`src/TwoFactorAuthApp/bin/Release/net9.0-android/publish/com.reegenius.twofactorauth-Signed.apk`

或在 Windows 上：

```powershell
.\build-apk.ps1
.\build-apk.ps1 -Install   # 构建并通过 adb 安装
```

### 单元测试

```bash
dotnet test TwoFactorAuth.sln
```

## 项目结构

| 路径 | 说明 |
|------|------|
| `src/TwoFactorAuth.Core` | TOTP、Base32、otpauth 解析、JSON 存储、共享文案 |
| `src/TwoFactorAuth.Win` | WPF 桌面客户端 |
| `src/TwoFactorAuthApp` | .NET Android 客户端（CameraX + ZXing） |
| `tests/TwoFactorAuth.Logic.Tests` | 单元测试 |
| `docs/` | 规范与构建说明（英文） |

## 密钥输入规范

仅接受 **RFC 4648 Base32** 共享密钥（与 `otpauth` 中 `secret` 一致）。以下输入会被**拒绝**：

- 账户**恢复码**（常含 `0`、`1`、`8`、`9` 或固定分段格式）
- **十六进制**密钥
- 无效或过短的 Base32

详见 [docs/spec/secret-input.md](docs/spec/secret-input.md)。

## 多语言

| 平台 | 方式 |
|------|------|
| Core（错误提示） | `Localization/strings.{en,zh-CN}.json`，`Loc.T()` |
| Windows 界面 | `Localization/ui.{en,zh-CN}.json`，`UiLoc.T()` |
| Android 界面 | `values/strings.xml`（默认英文）+ `values-zh-rCN/strings.xml` |

语言随系统区域设置（`en` 或 `zh-*`）。

## 参与贡献

欢迎提交 Issue 与 Pull Request。请参阅 [CONTRIBUTING.md](CONTRIBUTING.md)。提交前请运行 `dotnet test`。

## 许可证

本项目采用 [Apache License 2.0](LICENSE)。第三方组件说明见 [NOTICE](NOTICE)。
