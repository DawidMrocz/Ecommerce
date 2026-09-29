terraform {
  required_version = ">= 1.5"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }
}

provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "rg" {
  name     = var.resource_group_name
  location = var.location
  managed_by = "Terraform"
  tags = {
    Environment = "Dev"
    Owner       = "TeamX"
    Project     = "Demo"
  }
}

resource "azurerm_storage_account" "storage" {
  name                     = "mroczwaresa"
  resource_group_name      = azurerm_resource_group.rg.name
  location                 = azurerm_resource_group.rg.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_storage_container" "container" {
  name                  = "demo-container"
  storage_account_name  = azurerm_storage_account.storage.name
  container_access_type = "private"
}


# resource "azurerm_mssql_server" "sql_server" {
#   name                         = var.sql_server_name
#   resource_group_name          = azurerm_resource_group.rg.name
#   location                     = azurerm_resource_group.rg.location
#   version                      = "12.0"
#   administrator_login          = var.admin_login
#   administrator_login_password = var.admin_password

#   minimum_tls_version = "1.2"
# }

# resource "azurerm_mssql_firewall_rule" "allow_my_ip" {
#   name             = "AllowMyIP"
#   server_id        = azurerm_mssql_server.sql_server.id
#   start_ip_address = var.my_ip
#   end_ip_address   = var.my_ip
# }

# resource "azurerm_mssql_database" "sql_db" {
#   name           = var.database_name
#   server_id      = azurerm_mssql_server.sql_server.id
#   sku_name       = "Basic"
#   collation      = "SQL_Latin1_General_CP1_CI_AS"
#   max_size_gb    = 2
# }

data "azurerm_client_config" "current" {}

resource "azurerm_key_vault" "kv" {
  name                        = "mroczwarekv22"
  location                    = azurerm_resource_group.rg.location
  resource_group_name         = azurerm_resource_group.rg.name
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  sku_name                    = "standard"
  purge_protection_enabled    = true
}

resource "azurerm_key_vault_access_policy" "terraform_user" {
  key_vault_id = azurerm_key_vault.kv.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = data.azurerm_client_config.current.object_id

  secret_permissions = [
    "Get",
    "Set",
    "List"
  ]
}

# locals {
#   sql_connection_string = "Server=tcp:${azurerm_mssql_server.sql_server.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.sql_db.name};User ID=${var.admin_login};Password=${var.admin_password};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
# }

# resource "azurerm_key_vault_secret" "default_connection" {
#   name         = "ConnectionStrings--DefaultConnection"
#   value        = local.sql_connection_string
#   key_vault_id = azurerm_key_vault.kv.id

#   depends_on = [
#     azurerm_key_vault_access_policy.terraform_user
#   ]
# }

# resource "azurerm_container_registry" "acr" {
#   name                = "mroczwareRegistry"
#   resource_group_name = azurerm_resource_group.rg.name
#   location            = azurerm_resource_group.rg.location
#   sku                 = "Basic"
#   admin_enabled       = true
# }


