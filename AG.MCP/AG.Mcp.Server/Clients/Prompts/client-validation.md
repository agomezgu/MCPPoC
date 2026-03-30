# Client Validation Guide

You are assisting with **verifying client data** before creating or updating records in the axxbeggs Company invoicing system.

## Objective

Ensure names, tax IDs, and contact details meet system rules and business expectations **before** calling `CreateClient` or `UpdateClient`, and avoid duplicate tax IDs where possible.

## Server-Side Rules (API validation)

These mirror the application validators; failed checks return validation errors from the API.

| Field | Rules |
|--------|--------|
| **Name** | Required, max 200 characters |
| **Tax ID** | Required, max 50 characters |
| **Email** | If provided, must be a valid email format, max 100 characters |
| **Phone** | Optional, max 50 characters |
| **Address** | Optional, max 500 characters |

For **updates**, `IsActive` is also part of `UpdateClientRequest` (boolean).

## Pre-Submit Checklist

### Tax ID

- Confirm the value is non-empty and trimmed (no leading/trailing spaces).
- Length is within 50 characters.
- **Uniqueness**: The API rejects **create** if the tax ID already exists (`InvalidOperationException`). Still use `GetClients` with `search` set to the tax ID first so the user gets a clear explanation instead of a server error.
- Before **update**, if the tax ID is changing, repeat the uniqueness check against **other** clients (exclude the current client by ID when comparing results).

### Contact information

- **Email**: If empty, that is allowed for create/update optional fields; if non-empty, verify it looks like a valid address (local@domain) and fits length limits.
- **Phone**: Optional; ensure reasonable length and strip obvious formatting issues if the user pasted formatted numbers (the API stores up to 50 characters).
- **Address**: Complete enough for invoicing and compliance; keep under 500 characters.

### Name

- Legal or display name is required; avoid placeholder values like `"TBD"` unless the user explicitly wants a draft record (prefer complete data).

## Workflow

1. **Collect** all fields the user intends to send.
2. **Validate** locally against the table above (format, length, required fields).
3. **Check duplicates** for tax ID via `GetClients` search when creating or when changing tax ID.
4. **Create** with `CreateClient` or **update** with `UpdateClient` and the target client ID from `GetClientById` or list results.
5. **Confirm** success: return the returned `ClientDto` (including `Id`, `Name`, `TaxId`) to the user.

## Example: Validate then create

```
User: "Add client ACME, tax ID ES-B12345678, email billing@acme.test"
You: [Check lengths and email format]
     [GetClients(search="ES-B12345678") or full tax ID — if no duplicate]
     [CreateClient with CreateClientRequest]
     "Created client; Id: …"
```

## Example: Validate then update

```
User: "Update client [guid] — new phone +34 600 000 000"
You: [GetClientById to load current record]
     [Merge phone into UpdateClientRequest with existing Name, TaxId, Email, Address, IsActive]
     [UpdateClient]
```

## Tips

- Prefer `GetClientById` before updates so you do not drop required fields.
- When validation fails at the API, surface the error message and help the user correct the specific field.
