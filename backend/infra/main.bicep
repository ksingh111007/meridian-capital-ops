// Azure App Service (Web App for Containers) deployment for the Meridian API.
//
//   az group create -n rg-meridian-dev -l eastus2
//   az deployment group create -g rg-meridian-dev -f main.bicep -p main.parameters.json
//
// Then build & push the image (see infra/github/deploy-backend.yml for the CI version):
//   az acr build -r <acrName> -t meridian-api:latest ../
//
// Hardened defaults for a financial-services workload: HTTPS-only, TLS 1.2+, FTPS
// disabled, ACR pulls via system-assigned managed identity (no admin credentials),
// health-check probe on /healthz, logs + App Insights wired to a workspace.

@description('Base name applied to all resources, e.g. "meridian".')
param baseName string

@allowed(['dev', 'staging', 'prod'])
param environmentName string = 'dev'

param location string = resourceGroup().location

@description('Container image (repository:tag) within the ACR created here.')
param containerImage string = 'meridian-api:latest'

@description('App Service plan SKU. B1 for dev; P1v3+ for production.')
param appServicePlanSku string = 'B1'

var suffix = '${baseName}-${environmentName}'

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: 'log-${suffix}'
  location: location
  properties: {
    retentionInDays: 90
    sku: { name: 'PerGB2018' }
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'appi-${suffix}'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: replace('acr${suffix}', '-', '')
  location: location
  sku: { name: 'Basic' }
  properties: {
    adminUserEnabled: false
  }
}

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: 'plan-${suffix}'
  location: location
  kind: 'linux'
  sku: { name: appServicePlanSku }
  properties: {
    reserved: true
  }
}

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: 'app-${suffix}'
  location: location
  identity: { type: 'SystemAssigned' }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOCKER|${containerRegistry.properties.loginServer}/${containerImage}'
      acrUseManagedIdentityCreds: true
      alwaysOn: appServicePlanSku != 'B1' ? true : false
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      healthCheckPath: '/healthz'
      appSettings: [
        { name: 'WEBSITES_PORT', value: '8080' }
        { name: 'ASPNETCORE_ENVIRONMENT', value: environmentName == 'prod' ? 'Production' : 'Development' }
        { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsights.properties.ConnectionString }
      ]
    }
  }
}

// Grant the web app's managed identity pull rights on the registry.
resource acrPullRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(containerRegistry.id, webApp.id, 'acrpull')
  scope: containerRegistry
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d') // AcrPull
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

output webAppName string = webApp.name
output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
output acrLoginServer string = containerRegistry.properties.loginServer
