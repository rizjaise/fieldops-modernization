# Azure OIDC and RBAC Design

## Purpose

FieldOps Modernization uses GitHub Actions for CI/CD and Terraform for Azure Infrastructure as Code.

GitHub Actions requires authenticated access to Azure for Terraform plan and apply operations.

The project uses OpenID Connect (OIDC) with Microsoft Entra ID workload identity federation instead of long-lived Azure credentials.

## Authentication Architecture

The planned authentication flow is:

```text
GitHub Actions
      |
      | OIDC token
      v
Microsoft Entra ID
      |
      | Federated Identity Credential
      v
Azure Deployment Identity
      |
      | Azure RBAC
      v
Terraform
      |
      v
Azure Resources