resource "azurerm_service_plan" "fieldops" {
  name                = "asp-${var.project_name}-${var.environment}"
  resource_group_name = azurerm_resource_group.fieldops.name
  location            = azurerm_resource_group.fieldops.location

  os_type  = "Windows"
  sku_name = "B1"

  tags = {
    project     = var.project_name
    environment = var.environment
    managed_by  = "terraform"
  }
}

resource "azurerm_windows_web_app" "fieldops" {
  name                = "app-${var.project_name}-${var.environment}"
  resource_group_name = azurerm_resource_group.fieldops.name
  location            = azurerm_resource_group.fieldops.location
  service_plan_id     = azurerm_service_plan.fieldops.id

  https_only = true

  site_config {
    always_on = true

    application_stack {
      current_stack  = "dotnet"
      dotnet_version = "v8.0"
    }
  }

  identity {
    type = "SystemAssigned"
  }

  tags = {
    project     = var.project_name
    environment = var.environment
    managed_by  = "terraform"
  }

  app_settings = {
    APPLICATIONINSIGHTS_CONNECTION_STRING = azurerm_application_insights.fieldops.connection_string
  }
}