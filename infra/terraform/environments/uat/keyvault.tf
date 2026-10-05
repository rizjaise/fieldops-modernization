resource "azurerm_key_vault" "fieldops" {
  name                       = "kv-${var.project_name}-${var.environment}"
  location                   = azurerm_resource_group.fieldops.location
  resource_group_name        = azurerm_resource_group.fieldops.name
  tenant_id                  = data.azurerm_client_config.current.tenant_id
  sku_name                   = "standard"
  rbac_authorization_enabled = true

  purge_protection_enabled   = true
  soft_delete_retention_days = 7

  tags = {
    project     = var.project_name
    environment = var.environment
    purpose     = "application-secrets"
    managed_by  = "terraform"
  }
}

data "azurerm_client_config" "current" {}

resource "azurerm_role_assignment" "app_key_vault_secrets_user" {
  scope                = azurerm_key_vault.fieldops.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_windows_web_app.fieldops.identity[0].principal_id
}