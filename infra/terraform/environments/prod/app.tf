resource "azurerm_service_plan" "fieldops" {
  name                = "asp-${var.project_name}-${var.environment}"
  resource_group_name = azurerm_resource_group.fieldops.name
  location            = azurerm_resource_group.fieldops.location

  os_type  = "Windows"
  sku_name = var.app_service_sku

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

    "ConnectionStrings__FieldOps" = "Server=tcp:${azurerm_mssql_server.fieldops.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.fieldops.name};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=Active Directory Managed Identity;"
  }
}

resource "azurerm_app_service_connection" "sql" {
  name               = "conn-${var.project_name}-${var.environment}-sql"
  app_service_id     = azurerm_windows_web_app.fieldops.id
  target_resource_id = azurerm_mssql_database.fieldops.id

  client_type = "none"

  authentication {
    type = "systemAssignedIdentity"
  }
}