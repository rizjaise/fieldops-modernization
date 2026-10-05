resource "azurerm_log_analytics_workspace" "fieldops" {
  name                = "law-${var.project_name}-${var.environment}"
  location            = azurerm_resource_group.fieldops.location
  resource_group_name = azurerm_resource_group.fieldops.name

  sku               = "PerGB2018"
  retention_in_days = 30

  tags = {
    project     = var.project_name
    environment = var.environment
    purpose     = "centralized-logging"
    managed_by  = "terraform"
  }
}

resource "azurerm_application_insights" "fieldops" {
  name                = "appi-${var.project_name}-${var.environment}"
  location            = azurerm_resource_group.fieldops.location
  resource_group_name = azurerm_resource_group.fieldops.name

  application_type = "web"
  workspace_id     = azurerm_log_analytics_workspace.fieldops.id

  tags = {
    project     = var.project_name
    environment = var.environment
    purpose     = "application-monitoring"
    managed_by  = "terraform"
  }
}