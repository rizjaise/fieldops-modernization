resource "azurerm_monitor_metric_alert" "app_http_5xx" {
  name                = "alert-${var.project_name}-${var.environment}-http-5xx"
  resource_group_name = azurerm_resource_group.fieldops.name
  scopes              = [azurerm_windows_web_app.fieldops.id]

  description = "Alerts when the FieldOps application returns HTTP 5xx responses."
  severity    = 2

  criteria {
    metric_namespace = "Microsoft.Web/sites"
    metric_name      = "Http5xx"
    aggregation      = "Total"
    operator         = "GreaterThan"
    threshold        = 5
  }

  frequency   = "PT5M"
  window_size = "PT5M"

  tags = {
    project     = var.project_name
    environment = var.environment
    managed_by  = "terraform"
  }
}

resource "azurerm_monitor_metric_alert" "app_response_time" {
  name                = "alert-${var.project_name}-${var.environment}-response-time"
  resource_group_name = azurerm_resource_group.fieldops.name
  scopes              = [azurerm_windows_web_app.fieldops.id]

  description = "Alerts when FieldOps application response time exceeds the expected threshold."
  severity    = 2

  criteria {
    metric_namespace = "Microsoft.Web/sites"
    metric_name      = "AverageResponseTime"
    aggregation      = "Average"
    operator         = "GreaterThan"
    threshold        = 2
  }

  frequency   = "PT5M"
  window_size = "PT5M"

  tags = {
    project     = var.project_name
    environment = var.environment
    managed_by  = "terraform"
  }
}