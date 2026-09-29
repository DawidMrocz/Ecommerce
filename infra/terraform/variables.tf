variable "resource_group_name" {}
variable "location" {}
variable "sql_server_name" {}
variable "admin_login" {}
variable "admin_password" {
  sensitive = true
}
variable "database_name" {}
variable "my_ip" {}
