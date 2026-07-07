// One Linux App Service (Web App for Containers) pulling its image from ACR
// with its system-assigned managed identity. Shared by the frontend and the
// backend API — everything app-specific arrives via params.

param name string
param location string
param appServicePlanId string
param acrLoginServer string

@description('Container image (repository:tag) within the ACR.')
param image string

@description('Port the container listens on (WEBSITES_PORT).')
param targetPort int

param healthCheckPath string
param alwaysOn bool

@description('Extra app settings as { name, value } objects.')
param appSettings array = []

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: name
  location: location
  identity: { type: 'SystemAssigned' }
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOCKER|${acrLoginServer}/${image}'
      acrUseManagedIdentityCreds: true
      alwaysOn: alwaysOn
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      healthCheckPath: healthCheckPath
      appSettings: concat(
        [{ name: 'WEBSITES_PORT', value: string(targetPort) }],
        appSettings
      )
    }
  }
}

output name string = webApp.name
output defaultHostName string = webApp.properties.defaultHostName
output principalId string = webApp.identity.principalId
