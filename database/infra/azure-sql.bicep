// Azure SQL Database for Meridian Capital Ops.
//
//   az group create -n rg-meridian-dev -l eastus2
//   az deployment group create -g rg-meridian-dev -f azure-sql.bicep \
//     -p baseName=meridian sqlAdminLogin=<aad-group-or-user> sqlAdminObjectId=<objectId>
//
// Then deploy the schema + seed with sqlpackage (see infra/github/deploy-database.yml
// for the CI version):
//   dotnet build ../Meridian.Database.sqlproj -c Release
//   sqlpackage /Action:Publish /SourceFile:../bin/Release/Meridian.Database.dacpac \
//     /Profile:../profiles/Azure.publish.xml \
//     /TargetConnectionString:"Server=tcp:<sqlServerFqdn>,1433;Database=meridian;Authentication=Active Directory Default;Encrypt=True;"
//
// Hardened defaults: Entra-only authentication (no SQL logins), TLS 1.2+,
// public network access restricted to Azure services (tighten with private
// endpoints or firewall rules for production).

@description('Base name applied to all resources, e.g. "meridian".')
param baseName string

@allowed(['dev', 'staging', 'prod'])
param environmentName string = 'dev'

param location string = resourceGroup().location

@description('Display name of the Entra ID admin (user or group) for the SQL server.')
param sqlAdminLogin string

@description('Object id of the Entra ID admin.')
param sqlAdminObjectId string

@description('Principal type of the Entra ID admin — must match the object behind sqlAdminObjectId.')
@allowed(['User', 'Group', 'Application'])
param sqlAdminPrincipalType string = 'Group'

@description('Database SKU. GP_S_Gen5_1 (serverless) suits dev; scale up for production.')
param databaseSkuName string = 'GP_S_Gen5_1'

var suffix = '${baseName}-${environmentName}'

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: 'sql-${suffix}'
  location: location
  properties: {
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    administrators: {
      administratorType: 'ActiveDirectory'
      azureADOnlyAuthentication: true
      login: sqlAdminLogin
      sid: sqlAdminObjectId
      tenantId: subscription().tenantId
      principalType: sqlAdminPrincipalType
    }
  }
}

// Allow Azure services (the App Service backend, GitHub-hosted runners via
// Azure relay) through the server firewall; replace with private endpoints
// + VNet integration for production isolation.
resource allowAzureServices 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
  parent: sqlServer
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource database 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServer
  name: 'meridian'
  location: location
  sku: {
    name: databaseSkuName
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    // Serverless auto-pause after an hour of inactivity (dev cost control).
    autoPauseDelay: startsWith(databaseSkuName, 'GP_S') ? 60 : -1
    requestedBackupStorageRedundancy: 'Local'
  }
}

output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output databaseName string = database.name
@description('Set this as ConnectionStrings__Default on the API, with Database__Provider=SqlServer.')
output apiConnectionString string = 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Database=${database.name};Authentication=Active Directory Default;Encrypt=True;'
