resource "azurerm_monitor_action_group" "fieldops" {
  name                = "ag-${var.project_name}-${var.environment}"
  resource_group_name = azurerm_resource_group.fieldops.name
  short_name          = "Fops${var.environment}"
  enabled             = true

  dynamic "email_receiver" {
    for_each = {
      for index, address in var.alert_email_addresses :
      tostring(index) => address
    }

    content {
      name                    = "fieldops-${var.environment}-email-${email_receiver.key}"
      email_address           = email_receiver.value
      use_common_alert_schema = true
    }
  }
}

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

  action {
    action_group_id = azurerm_monitor_action_group.fieldops.id
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

  action {
    action_group_id = azurerm_monitor_action_group.fieldops.id
  }
}