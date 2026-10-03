variable "subscription_id" {
  description = "Azure Subscription Id."
  type        = string
  sensitive   = true
}

variable "location" {
  description = "Azure region for the Terraform state resources."
  type        = string
  default     = "Central India"
}

variable "project_name" {
  description = "Project identifier."
  type        = string
  default     = "fieldops"
}