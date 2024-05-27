@description('Which Pricing tier our App Service Plan to')
param skuName string = 'B1'

@description('How many instances of our app service will be scaled out to')
param skuCapacity int = 1

@description('Location for all resources.')
param location string = resourceGroup().location

@description('Name that will be used to build associated artifacts')
param appName string = uniqueString(resourceGroup().id)

var appServicePlanName = toLower('asp-${appName}')
@description('Web application name to be deployed to')
param webSiteName string = toLower('wapp-${appName}')

@description('Web application name to be deployed to')
param sqlConn string = 'ConnectionString'

resource appServicePlan 'Microsoft.Web/serverfarms@2020-06-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: skuName
    capacity: skuCapacity
  }
  tags: {
    displayName: 'HostingPlan'
    ProjectName: appName
  }
}

resource appService 'Microsoft.Web/sites@2020-06-01' = {
  name: webSiteName
  location: location  
  tags: {
    displayName: 'Website'
    ProjectName: appName
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion:'v8.0'
      minTlsVersion: '1.2'      
    }
  }
}

resource environmentSettings 'Microsoft.Web/sites/config@2023-12-01' = {
    parent: appService
    name: 'appsettings'
    properties: {        
        SqlOptions__ConnectionString: sqlConn       
    }
}

resource applicationLogsSettings 'Microsoft.Web/sites/config@2020-06-01' = {
  parent: appService
  name: 'logs'
  properties: {
    applicationLogs: {
      fileSystem: {
        level: 'Warning'
      }
    }
    httpLogs: {
      fileSystem: {
        retentionInMb: 40
        enabled: true
      }
    }
    failedRequestsTracing: {
      enabled: true
    }
    detailedErrorMessages: {
      enabled: true
    }
  }
}

@description('Output url of the website')
output pageUrl string = 'https://${webSiteName}.azurewebsites.net'