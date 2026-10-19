output "resource_group_name" {
  description = "Name of the FieldOps development resource group."
  value       = azurerm_resource_group.fieldops.name
}

output "resource_group_location" {
  description = "Azure region of the FieldOps development resource group."
  value       = azurerm_resource_group.fieldops.location
}

output "app_service_possible_outbound_ips" {
  description = "Possible outbound IP addresses for the FieldOps App Service."
  value       = azurerm_windows_web_app.fieldops.possible_outbound_ip_address_list
}