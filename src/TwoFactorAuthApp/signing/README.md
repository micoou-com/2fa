# Android release signing & APK build rules

This folder contains the **public release keystore** for reproducible open-source APK builds.

## Signing files

| File | Purpose |
|------|---------|
| `twofactorauth-release.keystore` | Release keystore (committed) |
| `Signing.props` | MSBuild signing settings (auto-imported on Release) |
| `signing.properties` | Same credentials (reference / Gradle-style) |

## Credentials

| Property | Value |
|----------|-------|
| Key alias | `twofactorauth` |
| Store password | `twofactorauth` |
| Key password | `twofactorauth` |
| Certificate DN | `CN=TwoFactorAuth, OU=Open Source, O=ReeGenius, C=CN` |

## Build rules (must follow)

1. **Always use Release** — Debug builds are not for distribution.
2. **Use the project keystore only** — do not replace with a local debug key; `Signing.props` is imported automatically when present.
3. **Install the Signed APK** — output file name ends with `-Signed.apk`.
4. **Do not commit APK files** — only the keystore and props live in git; APKs go to `bin/` (gitignored).
5. **Bump version before release** — edit `ApplicationDisplayVersion` / `ApplicationVersion` in `TwoFactorAuthApp.csproj`.

## Build a signed APK

From repository root:

```bash
dotnet publish src/TwoFactorAuthApp/TwoFactorAuthApp.csproj -c Release -f net9.0-android
```

Windows:

```powershell
.\build-apk.ps1
```

**Output (install this file):**

```
src/TwoFactorAuthApp/bin/Release/net9.0-android/publish/com.reegenius.twofactorauth-Signed.apk
```

## Verify keystore

```bash
keytool -list -v -keystore twofactorauth-release.keystore -storepass twofactorauth
```

## Verify APK signature

```bash
jarsigner -verify -verbose -certs com.reegenius.twofactorauth-Signed.apk
```

Certificate subject must be `CN=TwoFactorAuth, OU=Open Source, O=ReeGenius, C=CN`.

## Security note

Credentials are **public by design** for community reproducible builds. Do not reuse this keystore for unrelated commercial apps.

---

# Android 发布签名与 APK 构建规则

## 构建规则（必须遵守）

1. **仅 Release 构建** — 不要分发 Debug 包。
2. **仅使用本目录 keystore** — Release 会自动加载 `Signing.props`，勿换成本地 debug 密钥。
3. **安装 `-Signed.apk`** — 发布/真机测试用带 `-Signed` 后缀的文件。
4. **不要把 APK 提交到 git** — 仓库只保留 keystore 与配置文件。
5. **发版前递增版本号** — 修改 `TwoFactorAuthApp.csproj` 中的 `ApplicationDisplayVersion`。

## 构建命令

```powershell
.\build-apk.ps1
```

签名输出路径：

`src/TwoFactorAuthApp/bin/Release/net9.0-android/publish/com.reegenius.twofactorauth-Signed.apk`
