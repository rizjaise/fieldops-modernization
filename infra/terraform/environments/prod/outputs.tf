output "resource_group_name" {
  description = "Name of the FieldOps development resource group."
  value       = azurerm_resource_group.fieldops.name
}

output "resource_group_location" {
  description = "Azure region of the FieldOps development resource group."
  value       = azurerm_resource_group.fieldops.location
}