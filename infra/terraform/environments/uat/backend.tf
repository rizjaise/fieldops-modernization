terraform {
  backend "azurerm" {
    resource_group_name  = "rg-fieldops-tfstate"
    storage_account_name = "stfieldopstfstate"
    container_name       = "tfstate"
    key                  = "fieldops-uat.tfstate"
  }
}