output "resource_group_name" {
  description = "Terraform state resource group."
  value       = azurerm_resource_group.terraform_state.name
}

output "storage_account_name" {
  description = "Terraform state storage account."
  value       = azurerm_storage_account.terraform_state.name
}

output "container_name" {
  description = "Terraform state blob container."
  value       = azurerm_storage_container.terraform_state.name
}