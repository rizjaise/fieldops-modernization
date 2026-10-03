resource "azurerm_application_insights_web_test" "fieldops_health" {
  name                    = "test-${var.project_name}-${var.environment}-health"
  location                = azurerm_resource_group.fieldops.location
  resource_group_name     = azurerm_resource_group.fieldops.name
  application_insights_id = azurerm_application_insights.fieldops.id

  kind      = "ping"
  frequency = 300
  timeout   = 30
  enabled   = true

  geo_locations = [
    "us-va-ash-azr",
    "emea-nl-ams-azr"
  ]

  configuration = <<XML
<WebTest Name="FieldOps Health Check" Id="${var.project_name}-${var.environment}-health" Enabled="True" CssProjectStructure="https://schemas.microsoft.com/WebTest/2010" CssIteration="https://schemas.microsoft.com/WebTest/2010">
  <Items>
    <Request Method="GET" Guid="00000000-0000-0000-0000-000000000001" Version="1.1" Url="https://app-${var.project_name}-${var.environment}.azurewebsites.net/health" />
  </Items>
</WebTest>
XML

  tags = {
    project     = var.project_name
    environment = var.environment
    purpose     = "availability-monitoring"
    managed_by  = "terraform"
  }
}