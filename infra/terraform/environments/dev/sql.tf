resource "azurerm_mssql_server" "fieldops" {
  name                = "sql-${var.project_name}-${var.environment}"
  resource_group_name = azurerm_resource_group.fieldops.name
  location            = azurerm_resource_group.fieldops.location
  version             = "12.0"
  azuread_administrator {
    login_username              = var.sql_entra_admin_login
    object_id                   = var.sql_entra_admin_object_id
    tenant_id                   = data.azurerm_client_config.current.tenant_id
    azuread_authentication_only = true
  }
  public_network_access_enabled = true

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

resource "azurerm_mssql_firewall_rule" "app_service" {
  for_each         = var.sql_allowed_ip_addresses
  name             = "appservice-${replace(each.value, ".", "-")}"
  server_id        = azurerm_mssql_server.fieldops.id
  start_ip_address = each.value
  end_ip_address   = each.value
}