resource "azurerm_storage_account" "attachments" {
  name                     = "stfieldops${var.environment}"
  resource_group_name      = azurerm_resource_group.fieldops.name
  location                 = azurerm_resource_group.fieldops.location
  account_tier             = "Standard"
  account_replication_type = "LRS"

  min_tls_version                 = "TLS1_2"
  public_network_access           = "Enabled"
  allow_nested_items_to_be_public = false

  tags = {
    project     = var.project_name
    environment = var.environment
    purpose     = "attachments"
    managed_by  = "terraform"
  }
}

resource "azurerm_storage_container" "attachments" {
  name                  = "attachments"
  storage_account_id    = azurerm_storage_account.attachments.id
  container_access_type = "private"
}