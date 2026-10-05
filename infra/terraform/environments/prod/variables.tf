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
  default     = "prod"
}

variable "project_name" {
  description = "Project identifier."
  type        = string
  default     = "fieldops"
}

variable "sql_admin_username" {
  description = "SQL administrator username."
  type        = string
  sensitive   = true
}

variable "sql_admin_password" {
  description = "SQL administrator password."
  type        = string
  sensitive   = true
}