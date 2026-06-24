---
IOF: RYIOF_SRS20260424001
title: Android TOTP 身份验证器（扫码与手动添加）需求规格说明
class: SRS
version: v1.1.0
status: draft
company: ReeGenius
author: AI Assistant
date: 2026-04-24
memo: 用户确认需同时具备 TOTP 生成与二维码扫描能力；实现为 .NET Android 示例应用。
---

# Android TOTP 身份验证器（扫码与手动添加）

## 摘要

本文档规定在 Android 设备上运行的 TOTP（基于时间的一次性密码）验证器示例应用的功能与约束。算法侧与主流验证器一致：计数器取 $$C=\lfloor T / T_{\text{period}}\rfloor$$（$$T$$ 为 UTC  Unix 时间，$$T_{\text{period}}$$ 默认 30 秒），以 **RFC 4648 Base32 共享密钥** 经 HMAC-SHA1 派生动态码\cite{RFC6238}。应用须支持 **扫描二维码** 导入 `otpauth://totp/...`，并支持 **手动输入与 `secret` 相同的 Base32 密钥**；条目保存在本机 JSON 文件中，用于演示，不作为安全产品规格。

## 关键词

TOTP；二维码；otpauth；.NET Android；CameraX；ZXing

## 1. 功能需求

### 1.1 动态码生成

- 对已保存条目按 RFC 6238 计算 6 位（或可配置 6/8 位）动态码，周期默认 30 秒。
- 列表界面展示剩余秒数或等效倒计时提示，便于用户输入。

### 1.2 二维码扫描导入

- 调用相机预览，实时解析画面中的 **QR Code**。
- 仅当解析结果为 **`otpauth://totp/`** 且可解析出有效 `secret`（Base32）时导入；**不支持** `otpauth://hotp/`。

### 1.3 手动添加

- 用户可输入显示名称（可选）与 **Base32 密钥**（与 `otpauth` 中 `secret` 及网站 Secret 一致）；解码仅支持 RFC 4648 Base32，见 `docs/spec/RYIOF_DWS20260424002-2FA共享密钥输入规范.md` v1.1.0；仅当 `TryDecodeSecret` 成功时写入列表。

### 1.4 条目管理

- 支持删除单条条目（长按触发确认后删除本地记录）。

## 2. 非功能需求与约束

- 目标平台：**.NET 9 Android**（API 24+），与 Visual Studio 2022 配合开发、部署。
- 密钥与 JSON 存储于应用沙箱；**未实现**硬件安全模块、加密存储与备份加密，需在后续版本中单独立项。
- 依赖 CameraX、Material 与 ZXing.Net；构建时可能出现 Android 16 页面大小相关警告（上游 AAR），不影响当前目标 API 的调试构建。

## 3. 参考文献

\cite{RFC6238}

- RFC 6238, *TOTP: Time-Based One-Time Password Algorithm*.
- RFC 4648, *The Base16, Base32, and Base64 Data Encodings*（Base32 密钥编码）。

## 4. 修订历史

| 版本 | 日期 | 修订人 | 修订内容 |
| ----- | ---- | ----- | --------- |
| v1.0.0 | 2026-04-24 | AI Assistant | 初稿：扫码 + 手动 + 列表与删除 |
| v1.0.1 | 2026-04-24 | AI Assistant | 手动添加密钥与 DWS20260424002 对齐（十六进制 / Base32）。 |
| v1.0.2 | 2026-04-24 | AI Assistant | 手动添加：区分恢复码与共享密钥（与 DWS v1.0.2 一致）。 |
| v1.0.3 | 2026-04-24 | AI Assistant | 以 Base32 2FA 密钥为主修订摘要与 §1.3；与 DWS v1.0.3 一致。 |
| v1.0.4 | 2026-04-24 | AI Assistant | 十六进制形态与恢复码说明；界面提示与 DWS v1.0.4 一致。 |
| v1.1.0 | 2026-04-24 | AI Assistant | 与 DWS v1.1.0 一致：仅 Base32 密钥用于 TOTP。 |
