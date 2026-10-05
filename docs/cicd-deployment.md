# CI/CD Deployment Strategy

## Purpose

FieldOps uses GitHub Actions to automate application and infrastructure validation and to provide a controlled deployment path into Azure environments.

The deployment model separates continuous validation from infrastructure deployment and uses environment-specific controls for DEV, UAT, and PROD.

## Pipeline Structure

```text
Pull Request
    │
    ├── Application CI
    │     ├── Restore
    │     ├── Build
    │     └── Test
    │
    └── Terraform CI
          ├── Format
          ├── Init
          └── Validate

             │
             ▼

           main
             │
             ▼
      Terraform Deploy
             │
             ▼
          DEV
             │
             ▼
          UAT
             │
             ▼
      Production Approval
             │
             ▼
          PROD