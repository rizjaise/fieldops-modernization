resource "azurerm_mssql_server" "fieldops" {
  name                         = "sql-${var.project_name}-${var.environment}"
  resource_group_name          = azurerm_resource_group.fieldops.name
  location                     = azurerm_resource_group.fieldops.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_username
  administrator_login_password = var.sql_admin_password

  minimum_tls_version = "1.2"

  tags = {
    project     = var.project_name
    environment = var.environment
    purpose     = "application-database"
    managed_by  = "terraform"
  }
}

resource "azurerm_mssql_database" "fieldops" {
  name      = "sqldb-${var.project_name}-${var.environment}"
  server_id = azurerm_mssql_server.fieldops.id

  sku_name = "Basic"

  tags = {
    project     = var.project_name
    environment = var.environment
    purpose     = "application-database"
    managed_by  = "terraform"
  }
}