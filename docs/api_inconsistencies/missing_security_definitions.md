# API Inconsistency: Missing OpenAPI Security Definitions

## Overview
- **API Version**: v1
- **Affected Endpoints**: All endpoints requiring authentication (Non-market endpoints).
- **Affected Parameters/Properties**:
    - Global `security` requirement.
    - Components `securitySchemes` definition.

## Description
The Luno OpenAPI specification (`luno_api_spec.json`) fails to utilize standard OpenAPI 3.0 security objects. Specifically:
1.  **Missing `securitySchemes`**: There is no top-level definition of how to authenticate (e.g., API Key, HTTP Basic, or Bearer).
2.  **Missing Operation `security`**: Individual endpoints do not specify that they require authentication.
3.  **Human-Readable Metadata**: Permission requirements are documented solely as human-readable text within the `description` field of each operation (e.g., `Permissions required: Perm_R_Balance`).

Without formal security definitions, standard OpenAPI tooling like **Microsoft Kiota** cannot:
- Automatically determine if an endpoint requires authentication.
- Map endpoints to specific permission scopes (e.g., Read vs. Write).
- Generate typed metadata to assist the `AuthenticationProvider` in making decision about when to attach credentials.

## Current Workaround
As of the current SDK version, the client treats all endpoints as having the same security context. Developers must manually cross-reference the `description` fields in the spec to understand which permissions are required for their API keys.

## Planned Resolution
We intend to enhance the `scripts/patch-spec.js` pipeline to:
1.  Inject a global `securitySchemes` definition for `ApiKeyAuth` (using HTTP Basic or Header based on Luno's requirements).
2.  Regex-parse operation `description` fields to extract the `Perm_...` identifiers.
3.  Inject corresponding `security` requirement objects into each operation's metadata.

This will enable Kiota to generate a more descriptive and secure API client that understands its own permission surface.
