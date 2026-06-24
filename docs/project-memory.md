# 项目记忆（2FA）

## 支持平台（产品范围）

- **正式支持**：**Windows**（WPF 客户端）、**Android**（.NET Android 客户端）。
- **暂不支持**：**iOS**、**Linux**、**macOS**、纯 **Web** 等；以 `docs/requirements/RYIOF_SRS20260424003-客户端支持平台范围.md` 为准。

## 技术栈

- **共享库** [`TwoFactorAuth.Core`](d:\Projects\2FA\src\TwoFactorAuth.Core)（`net9.0`）：TOTP、Base32、`OtpAuthParser`、`AccountEntry`、`JsonAccountStore` / `IAccountStore`（JSON 文件持久化）。
- **Android** [`TwoFactorAuthApp`](d:\Projects\2FA\src\TwoFactorAuthApp)（`net9.0-android`）：引用 Core；数据文件为应用私有目录 `accounts.json`；**扫码**为 CameraX 1.5 + ZXing；**不支持**通过应用宝侧载安装（需系统安装器或 adb）。
- **Windows** [`TwoFactorAuth.Win`](d:\Projects\2FA\src\TwoFactorAuth.Win)（`net9.0-windows` **WPF**）：同一 Core；数据目录 `%AppData%\TwoFactorAuth\accounts.json`；功能含列表倒计时、**手动添加**、**粘贴 otpauth URI**（自动尝试剪贴板）、删除所选；**无**摄像头扫码（可用粘贴 URI 代替）。

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

## 需求追溯

- **TOTP 客户端（Android/Win 示例）**：`docs/requirements/RYIOF_SRS20260424001-Android-TOTP认证器.md`。
- **客户端支持平台范围（仅 Win + Android）**：`docs/requirements/RYIOF_SRS20260424003-客户端支持平台范围.md`。
- **账号绑定与商业化（微信/QQ/支付宝、按账号收费、换机付费）**：`docs/requirements/RYIOF_SRS20260424002-账号绑定与商业化换机.md`（**draft**，当前代码库 **未实现**）。

**共享密钥**（**仅** RFC 4648 Base32，与 `otpauth` 的 `secret` 一致，用于 TOTP 计算）见 DWS `docs/spec/RYIOF_DWS20260424002-2FA共享密钥输入规范.md` **v1.1.0**。实现为 `SecretBytes.TryDecodeSecret`；根目录 `2FA密钥格式.md` 为入口。

## 修订历史（项目记忆）

| 日期 | 说明 |
|------|------|
| 2026-04-24 | 补充 DWS 密钥规范与 Core 解码 API 说明。 |
| 2026-04-24 | DWS/SRS/UI 以 2FA Base32 密钥为主修订。 |
| 2026-04-24 | DWS v1.0.4：十六进制形态与恢复码关系；SRS/UI 提示对齐。 |
| 2026-04-24 | DWS/SRS v1.1.0：仅 Base32 密钥；移除十六进制解码与冗长恢复码 UI。 |
| 2026-04-24 | 新增 SRS20260424002：第三方绑定、按账号计费、换机付费迁移（需求记录）。 |
| 2026-04-24 | SRS20260424002 v1.0.1：按账号收费 = 一个微信（第三方）登录用户一笔，非按 TOTP 条数。 |
| 2026-04-24 | 新增 SRS20260424003：终端仅 Windows + Android；iOS/Linux 等暂不纳入。 |
