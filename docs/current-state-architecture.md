# Current-State Architecture

## 1. Overview

FieldOps Services Ltd. currently operates a legacy field-service application from its on-premises office/server environment.

The application is used by customers, dispatchers, technicians, and administrators to manage service requests, technician assignments, job status, and job-related documents.

The current platform is functional but has significant operational limitations, including manual deployments, limited observability, infrastructure-level single points of failure, and dependence on a small IT team.

The purpose of this document is to describe the existing environment before modernization and migration to Azure.

## 2. Current Architecture

The current environment consists of:

* A Windows Server hosting the FieldOps web application.
* An on-premises SQL Server containing transactional application data.
* A Windows file server containing job attachments and documents.
* Application-managed user authentication.
* Manual application deployments.
* Manual backup processes.
* Basic application and infrastructure logging.

High-level architecture:

```text
                         INTERNET
                            │
              ┌─────────────┴─────────────┐
              │                           │
         Customers                  Technicians
              │                           │
              └─────────────┬─────────────┘
                            │
                            ▼
                     Office Firewall
                            │
                            ▼
                    Internal Network
                            │
                            ▼
                  ┌──────────────────┐
                  │ Windows Server   │
                  │                  │
                  │ IIS              │
                  │ FieldOps Web App │
                  │ FieldOps API     │
                  └────────┬─────────┘
                           │
                 ┌─────────┴─────────┐
                 │                   │
                 ▼                   ▼
          ┌─────────────┐     ┌──────────────┐
          │ SQL Server  │     │ File Server  │
          │             │     │              │
          │ Business    │     │ Photos       │
          │ Data        │     │ Documents    │
          └─────────────┘     └──────────────┘
```

## 3. Users

The system has four primary user groups:

| User          | Responsibilities                         |
| ------------- | ---------------------------------------- |
| Customer      | Create and track service requests        |
| Dispatcher    | Assign technicians and manage jobs       |
| Technician    | View assigned jobs and update job status |
| Administrator | Manage application and infrastructure    |

## 4. Application

The FieldOps application is implemented as a modular monolith hosted on Windows Server using IIS.

The application provides:

* Customer management
* Service request management
* Technician management
* Technician assignment
* Job tracking
* Status updates
* File attachments
* Notifications
* Reporting

The web application and API are hosted on the same application server.

## 5. Data

The application uses an on-premises SQL Server for transactional data.

The database contains information such as:

* Customers
* Technicians
* Service requests
* Jobs
* Assignments
* Job status history
* Application users
* Attachment metadata

Job photographs and documents are stored separately on a Windows file server.

The database stores metadata about attachments while the actual files are stored on the file server.

## 6. Authentication

The current application manages its own user accounts and authentication.

Users authenticate directly against the FieldOps application.

This creates an additional application-level responsibility for:

* User accounts
* Password management
* Authentication
* Role assignment
* Access management

## 7. Deployment

Application deployments are currently performed manually.

The process is approximately:

```text
Developer
    │
    ▼
Application Build
    │
    ▼
Deployment Package
    │
    ▼
IT Administrator
    │
    ▼
Windows Server
    │
    ▼
IIS Deployment
```

There is currently no automated CI/CD pipeline.

This creates dependency on the IT administrator and increases the risk of deployment errors and inconsistent releases.

## 8. Monitoring

The current environment has limited centralized observability.

The IT team primarily relies on:

* Windows Event Logs
* Application logs
* Server-level monitoring
* User reports

There is no centralized platform providing end-to-end visibility into application requests, dependencies, database performance, or deployment health.

When an application problem occurs, investigation is largely manual.

## 9. Backup and Recovery

Database and file backups are performed using manually managed processes.

The current environment does not provide a sufficiently automated recovery workflow.

Recovery procedures are not regularly tested against defined RPO and RTO targets.

## 10. Current Dependencies

The application depends on:

```text
FieldOps Application
        │
        ├── IIS
        │
        ├── Windows Server
        │
        ├── SQL Server
        │
        ├── File Server
        │
        └── Application-managed authentication
```

The application therefore has dependencies on both the application platform and the underlying on-premises infrastructure.

## 11. Current-State Problems

The assessment identifies the following major problems:

1. The application depends on a single application server.
2. Application deployments are manual.
3. Infrastructure management is largely manual.
4. Application observability is limited.
5. Troubleshooting requires manual investigation across multiple systems.
6. File storage is tied to on-premises infrastructure.
7. Authentication is managed inside the application.
8. Backup and recovery processes are operationally dependent on administrators.
9. There is no proper staging environment.
10. The platform has limited ability to scale without additional infrastructure investment.

## 12. Migration Implications

The current architecture indicates several areas that should be considered during modernization:

* Application hosting should reduce server-management overhead.
* Database hosting should reduce database infrastructure management.
* File storage should be decoupled from the application server.
* Authentication should move toward centralized identity.
* Application deployments should be automated.
* Infrastructure should be managed through Infrastructure as Code.
* Application and infrastructure telemetry should be centralized.
* Backup and recovery should be automated and testable.
* The target architecture should provide a credible scaling path.
