# Azure Authentication for GitHub Actions

## Purpose

FieldOps Modernization uses GitHub Actions for CI/CD and Terraform for Infrastructure as Code.

GitHub Actions must authenticate to Azure without storing long-lived Azure credentials in the GitHub repository.

The selected authentication model is OpenID Connect (OIDC) with Microsoft Entra ID workload identity federation.

## Authentication Flow

The planned authentication flow is:

```text
GitHub Actions
      |
      | OIDC token
      v
Microsoft Entra ID
      |
      | Federated identity validation
      v
Azure identity
      |
      | Azure RBAC
      v
Terraform
      |
      v
Azure resources