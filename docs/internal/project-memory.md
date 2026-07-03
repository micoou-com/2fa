# 项目记忆（2FA）

## 支持平台（产品范围）

- **正式支持**：**Windows**（WPF 客户端）、**Android**（.NET Android 客户端）。
- **暂不支持**：**iOS**、**Linux**、**macOS**、纯 **Web** 等。

## 技术栈

- **共享库** [`TwoFactorAuth.Core`](../../src/TwoFactorAuth.Core)（`net9.0`）：TOTP、Base32、`OtpAuthParser`、`AccountEntry`、`JsonAccountStore` / `IAccountStore`（JSON 文件持久化）。
- **Android** [`TwoFactorAuthApp`](../../src/TwoFactorAuthApp)（`net9.0-android`）：引用 Core；数据文件为应用私有目录 `accounts.json`；**扫码**为 CameraX 1.5 + ZXing。
- **Windows** [`TwoFactorAuth.Win`](../../src/TwoFactorAuth.Win)（`net9.0-windows` **WPF**）：同一 Core；数据目录 `%AppData%\TwoFactorAuth\accounts.json`；功能含列表倒计时、**手动添加**、**粘贴 otpauth URI**（自动尝试剪贴板）、删除所选；**无**摄像头扫码（可用粘贴 URI 代替）。

## 关键路径

- 解决方案：`TwoFactorAuth.sln`（含 Core、Android、Win、Tests）。
- **运行 Windows 客户端**：

```text
dotnet run --project src/TwoFactorAuth.Win/TwoFactorAuth.Win.csproj
```

- 可执行文件（Debug）：`src/TwoFactorAuth.Win/bin/Debug/net9.0-windows/TwoFactorAuth.Win.exe`
- **Release APK**：

```text
dotnet publish src/TwoFactorAuthApp/TwoFactorAuthApp.csproj -c Release -f net9.0-android -p:AndroidPackageFormats=apk
```

- **单元测试**：`tests/TwoFactorAuth.Logic.Tests` 引用 Core，`dotnet test TwoFactorAuth.sln`
- **全量构建**：`dotnet build TwoFactorAuth.sln`

## 规范

**共享密钥**（**仅** RFC 4648 Base32）见 [docs/spec/secret-input.md](../spec/secret-input.md)（英文）与 `docs/spec/RYIOF_DWS20260424002-2FA共享密钥输入规范.md` **v1.1.0**。实现为 `SecretBytes.TryDecodeSecret`。
