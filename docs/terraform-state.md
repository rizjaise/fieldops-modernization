# Terraform State Architecture

## Purpose

FieldOps Modernization uses Terraform as the Infrastructure as Code (IaC) tool for managing Azure infrastructure.

Terraform state is stored remotely in Azure Storage so that infrastructure state is persistent, centrally accessible, and available to CI/CD workflows.

Terraform state files are not committed to Git.

## Why Remote State Is Required

Local Terraform state creates several problems in a team or CI/CD environment:

* State exists only on one developer's machine.
* GitHub Actions cannot reliably access a developer's local state.
* Concurrent Terraform operations can cause state conflicts.
* State can be lost if the local development environment is damaged.
* Terraform state can contain sensitive infrastructure metadata.

Remote state provides a shared source of truth for Terraform-managed infrastructure.

## Bootstrap Problem

The main Terraform configuration requires a remote backend.

However, the remote backend itself consists of Azure resources that must first be created.

This creates a dependency:

```text
Main Terraform configuration
        |
        v
Requires remote state
        |
        v
Azure Storage Account
        |
        v
Must be created before the backend exists
        |
        v
Requires Terraform
```

This creates a circular dependency if the main Terraform configuration attempts to create its own backend.

The solution is a separate Terraform bootstrap configuration.

```text
infra/bootstrap/
        |
        +-- Resource Group
        |
        +-- Storage Account
        |
        +-- Blob Container
                  |
                  v
        Terraform remote state
                  |
                  v
infra/terraform/
        |
        +-- environments/dev
        |
        +-- environments/uat
        |
        +-- environments/prod
```

The bootstrap configuration is responsible only for creating the infrastructure required to store Terraform state.

## State Storage Structure

The planned Azure state storage structure is:

```text
Azure
└── rg-fieldops-tfstate
    └── stfieldopstfstate
        └── tfstate
            ├── fieldops-dev.tfstate
            ├── fieldops-uat.tfstate
            └── fieldops-prod.tfstate
```

Each environment has a separate state file.

This prevents changes in one environment from being represented in another environment's state.

## Environment Separation

FieldOps has three planned environments:

| Environment | State Key               |
| ----------- | ----------------------- |
| DEV         | `fieldops-dev.tfstate`  |
| UAT         | `fieldops-uat.tfstate`  |
| PROD        | `fieldops-prod.tfstate` |

Each environment-specific Terraform configuration manages only the resources belonging to that environment.

This separation also allows infrastructure changes to progress independently through the environment lifecycle.

## State Locking and Concurrency

Terraform operations must not modify the same state concurrently.

The Azure Storage backend provides state-locking capabilities to coordinate concurrent operations against the same state.

This is particularly important when Terraform is executed through GitHub Actions because multiple workflow runs could otherwise attempt to modify the same infrastructure simultaneously.

The CI/CD process should therefore prevent overlapping production infrastructure changes and require controlled execution for sensitive environments.

## Security Considerations

Terraform state is treated as sensitive infrastructure data.

The project follows these rules:

* `.tfstate` files are never committed to Git.
* `.terraform/` directories are never committed.
* The Terraform state blob container is private.
* Access to the state storage account is controlled through Azure identity and authorization.
* Long-lived Azure credentials should not be stored in GitHub repository files.
* GitHub Actions will use workload identity federation (OIDC) for Azure authentication when deployment is enabled.
* Application secrets are managed through Azure Key Vault rather than stored in source code.

## Bootstrap State

The bootstrap Terraform configuration is intentionally separate from the main FieldOps infrastructure state.

Initially, bootstrap state may be maintained locally while the remote state storage resources are created.

Once the state storage infrastructure exists, the main FieldOps Terraform configurations can use the Azure Storage backend.

The bootstrap process is therefore treated as a controlled initialization step rather than infrastructure that is recreated during every application deployment.

The bootstrap resources should themselves be protected from accidental deletion because they provide the state storage required by the other Terraform configurations.

## Planned Backend Configuration

Once the Azure subscription is active and the Terraform state storage resources have been created, the DEV environment will use a backend similar to:

```hcl
terraform {
  backend "azurerm" {
    resource_group_name  = "rg-fieldops-tfstate"
    storage_account_name = "stfieldopstfstate"
    container_name       = "tfstate"
    key                  = "fieldops-dev.tfstate"
  }
}
```

UAT and PROD will use separate state keys:

```text
fieldops-uat.tfstate
fieldops-prod.tfstate
```

The backend configuration is deliberately not enabled until the corresponding Azure state resources exist.

## CI/CD Integration

Terraform CI currently performs:

1. Terraform formatting validation.
2. Provider initialization.
3. Bootstrap configuration validation.
4. DEV configuration validation.

The future Terraform deployment pipeline will extend this process:

```text
Pull Request
     |
     v
Terraform fmt
     |
     v
Terraform validate
     |
     v
Terraform plan
     |
     v
Review / approval
     |
     v
Terraform apply
```

The production environment will require explicit approval before infrastructure changes are applied.

Terraform plan output should be reviewed before an apply operation so that unintended resource creation, modification, or destruction can be identified before changes reach the target environment.

## Design Decision

FieldOps Modernization uses separate Terraform configurations for bootstrap infrastructure and application infrastructure.

This separation exists because the Terraform backend must exist before the main Terraform configurations can use it.

The approach:

* avoids a circular dependency during initial provisioning;
* separates state-management infrastructure from application infrastructure;
* provides independent state for DEV, UAT, and PROD;
* supports centralized CI/CD execution;
* reduces the risk of local state drift;
* establishes a foundation for controlled infrastructure changes through GitHub Actions.

The resulting architecture is:

```text
                    GitHub Repository
                           |
                           v
                  Terraform Configuration
                           |
                 +---------+---------+
                 |                   |
                 v                   v
          Bootstrap State       Environment State
                 |                   |
                 v                   v
        Azure Storage Account   DEV / UAT / PROD
                 |                   |
                 +---------+----
```
