// Whole-stack Azure deployment for Meridian Capital Ops: frontend (Next.js),
// backend (.NET API), and Azure SQL, plus the shared plumbing (ACR, App Service
// plan, Log Analytics + App Insights).
//
// One deployment per environment (dev/prod), each in its own resource group:
//
//   ./deploy.sh dev      # az group create + az deployment group create
//   ./deploy.sh prod     # (see README.md for the full first-deploy walkthrough)
//
// Hardened defaults for a financial-services workload: HTTPS-only, TLS 1.2+,
// FTPS disabled, ACR pulls via system-assigned managed identity (no admin
// credentials), Entra-only SQL auth (no SQL logins), health-check probes,
// logs + App Insights wired to a workspace.

@description('Base name applied to all resources, e.g. "meridian". Keep it short and globally distinctive: ACR and web-app names derived from it must be globally unique.')
param baseName string

@allowed(['dev', 'prod'])
param environmentName string

param location string = resourceGroup().location

@description('Display name of the Entra ID admin (user or group) for the SQL server.')
param sqlAdminLogin string

@description('Object id of the Entra ID admin.')
param sqlAdminObjectId string

@description('Principal type of the Entra ID admin — must match the object behind sqlAdminObjectId.')
@allowed(['User', 'Group', 'Application'])
param sqlAdminPrincipalType string = 'Group'

@description('App Service plan SKU shared by the two web apps. B1 for dev; P1v3+ for production.')
param appServicePlanSku string = 'B1'

@description('Database SKU. GP_S_Gen5_1 (serverless, auto-pause) suits dev; provisioned (e.g. GP_Gen5_2) for production.')
param databaseSkuName string = 'GP_S_Gen5_1'

@description('Backend container image (repository:tag) within the ACR created here.')
param apiImage string = 'meridian-api:latest'

@description('Frontend container image (repository:tag) within the ACR created here.')
param webImage string = 'meridian-web:latest'

@description('Pins the API business "today" to match the seeded story (frontend TODAY = 2026-07-05). Set to an empty string to use the real clock.')
param businessDate string = '2026-07-05'

var suffix = '${baseName}-${environmentName}'
var isProd = environmentName == 'prod'
var apiAppName = 'app-${baseName}-api-${environmentName}'
var webAppName = 'app-${baseName}-web-${environmentName}'

// ---------------------------------------------------------------- monitoring

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: 'log-${suffix}'
  location: location
  properties: {
    retentionInDays: isProd ? 90 : 30
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

// ------------------------------------------------------- registry + compute

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

// ------------------------------------------------------------------ database

module sql 'modules/sql.bicep' = {
  name: 'sql'
  params: {
    baseName: baseName
    environmentName: environmentName
    location: location
    sqlAdminLogin: sqlAdminLogin
    sqlAdminObjectId: sqlAdminObjectId
    sqlAdminPrincipalType: sqlAdminPrincipalType
    databaseSkuName: databaseSkuName
  }
}

// ---------------------------------------------------------------------- apps

// Backend API. NOTE: authentication is still the X-User-Id dev stand-in
// (backend/README.md § AuthN) — do not treat this endpoint as
// production-ready until BACKEND_TODO step 1 (real SSO) lands. See
// README.md § Security for interim options.
module api 'modules/web-app.bicep' = {
  name: 'api'
  params: {
    name: apiAppName
    location: location
    appServicePlanId: appServicePlan.id
    acrLoginServer: containerRegistry.properties.loginServer
    image: apiImage
    targetPort: 8080
    healthCheckPath: '/healthz'
    alwaysOn: appServicePlanSku != 'B1'
    appSettings: concat(
      [
        { name: 'ASPNETCORE_ENVIRONMENT', value: isProd ? 'Production' : 'Development' }
        { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsights.properties.ConnectionString }
        { name: 'Database__Provider', value: 'SqlServer' }
        { name: 'ConnectionStrings__Default', value: sql.outputs.apiConnectionString }
      ],
      empty(businessDate) ? [] : [{ name: 'BusinessDate', value: businessDate }]
    )
  }
}

// Frontend. Talks to the API server-side only (src/lib/api.ts + the /api
// proxy route), so no CORS configuration is needed on the API.
module web 'modules/web-app.bicep' = {
  name: 'web'
  params: {
    name: webAppName
    location: location
    appServicePlanId: appServicePlan.id
    acrLoginServer: containerRegistry.properties.loginServer
    image: webImage
    targetPort: 3000
    healthCheckPath: '/healthz'
    alwaysOn: appServicePlanSku != 'B1'
    appSettings: [
      { name: 'MERIDIAN_API_URL', value: 'https://${api.outputs.defaultHostName}' }
    ]
  }
}

// Grant both web apps' managed identities pull rights on the registry.
resource acrPullApi 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(containerRegistry.id, apiAppName, 'acrpull')
  scope: containerRegistry
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d') // AcrPull
    principalId: api.outputs.principalId
    principalType: 'ServicePrincipal'
  }
}

resource acrPullWeb 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(containerRegistry.id, webAppName, 'acrpull')
  scope: containerRegistry
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d') // AcrPull
    principalId: web.outputs.principalId
    principalType: 'ServicePrincipal'
  }
}

// ------------------------------------------------------------------- outputs

output webUrl string = 'https://${web.outputs.defaultHostName}'
output apiUrl string = 'https://${api.outputs.defaultHostName}'
output webAppName string = web.outputs.name
output apiAppName string = api.outputs.name
output acrName string = containerRegistry.name
output acrLoginServer string = containerRegistry.properties.loginServer
output sqlServerFqdn string = sql.outputs.sqlServerFqdn
output databaseName string = sql.outputs.databaseName
@description('The API reaches the database with its managed identity — after deploying, create the contained user (scripts/grant-api-db-access.sh).')
output apiConnectionString string = sql.outputs.apiConnectionString
