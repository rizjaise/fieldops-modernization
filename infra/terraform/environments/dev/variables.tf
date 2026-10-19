variable "subscription_id" {
  description = "Azure subscription ID."
  type        = string
  sensitive   = true
}

variable "location" {
  description = "Azure region for the FieldOps environment."
  type        = string
  default     = "Central India"
}

variable "environment" {
  description = "Deployment environment."
  type        = string
  default     = "dev"
}

variable "project_name" {
  description = "Project identifier."
  type        = string
  default     = "fieldops"
}

variable "sql_entra_admin_login" {
  description = "Display name of the Microsoft Entra security group that administers Azure SQL."
  type        = string
}

variable "sql_entra_admin_object_id" {
  description = "Object ID of the Microsoft Entra security group that administers Azure SQL."
  type        = string
}

variable "app_service_sku" {
  description = "App Service Plan SKU for this environment."
  type        = string
  default     = "B1"

  validation {
    condition     = contains(["B1", "S1"], var.app_service_sku)
    error_message = "app_service_sku must be either B1 or S1."
  }
}

variable "sql_allowed_ip_addresses" {
  description = "Explicit IP addresses permitted to connect to Azure SQL."
  type        = set(string)
  default     = []
}