# TOTP shared secret input specification

**Version:** 1.1.0  
**Status:** stable  
**Implementation:** `TwoFactorAuth.Core.Totp.SecretBytes`

## Summary

This document defines the only secret format accepted by this application for **RFC 6238 TOTP** computation: **RFC 4648 Base32** strings, as used in `otpauth://totp/...` URIs and on provider setup pages. After **normalization** (strip whitespace and hyphen separators), the string is decoded to key bytes *K*. The app does **not** accept hexadecimal keys, recovery codes, or other encodings.

## Keywords

TOTP; 2FA; shared secret; Base32; RFC 4648; normalization

## 1. Terminology

| Term | Meaning |
|------|---------|
| **Base32 alphabet** | RFC 4648: `A`–`Z` and `2`–`7`; optional trailing padding `=`. |
| **Normalized string** | The continuous string *T* produced from user input by the rules in §3. |

**Note:** Providers may also issue **account recovery codes** (separate one-time codes for account login). Those are **not** TOTP shared secrets. This app does **not** parse recovery codes, and users must **not** paste them into the secret field.

## 2. Formatting characters (non-secret)

Providers often display Base32 secrets in **segments** for readability, for example:

`JBSWY-3DPE-HPK3-PXP`

Hyphens (`U+002D`) and similar Unicode dash characters are **not** part of the secret and are **removed** during normalization (§3).

## 3. Normalization algorithm

**Input:** string *S* (may span multiple lines).  
**Output:** *T*.

For each character *c* in *S*: if `char.IsWhiteSpace(c)` or *c* ∈ {`\u002D`, `\u2013`, `\u2014`, `\u2212`}, discard it; otherwise append *c* unchanged to *T*.

Letter case is preserved; no characters are inserted.

## 4. Base32 decoding

1. If `|T| = 0`: error code `empty`.
2. If *T* does not match **strict Base32 surface form** (after uppercasing, body contains only `A`–`Z`, `2`–`7`, and `=` only as contiguous trailing padding): error code `base32_invalid`.
3. Otherwise call `Base32.Decode` to obtain *K*. If `|K| = 0`: error code `base32_empty`; otherwise success.

The implementation may additionally reject inputs that look like **recovery codes** (e.g. fixed segment layouts or digits outside the Base32 alphabet) before decoding.

## 5. Relationship to TOTP

*K* is used as the HMAC-SHA1 key in RFC 6238. Period, digit count, and algorithm follow each account's `otpauth` parameters or manual-entry defaults.

## 6. References

- [RFC 4648](https://www.rfc-editor.org/rfc/rfc4648) — *The Base16, Base32, and Base64 Data Encodings*
- [RFC 6238](https://www.rfc-editor.org/rfc/rfc6238) — *TOTP: Time-Based One-Time Password Algorithm*

## Alternate language

Simplified Chinese version: [RYIOF_DWS20260424002-2FA共享密钥输入规范.md](RYIOF_DWS20260424002-2FA共享密钥输入规范.md)
