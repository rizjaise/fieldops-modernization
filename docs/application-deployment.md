# Application Deployment Strategy

## Purpose

The FieldOps application uses an immutable build artifact model for application deployment.

The application is built and tested once, packaged into a deployment artifact, and the same artifact is promoted through DEV, UAT, and PROD.

## Build Pipeline

The application CI pipeline performs:

1. Restore .NET dependencies.
2. Build the FieldOps solution.
3. Execute automated tests.
4. Publish the FieldOps API.
5. Package the published application as a ZIP file.
6. Upload the ZIP as a GitHub Actions artifact.

The resulting artifact is:

```text
fieldops-api.zip