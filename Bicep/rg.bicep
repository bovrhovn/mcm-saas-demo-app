targetScope = 'subscription'
@description('Resource Group Name')
param resourceGroupName string = 'mcm-rg'

@description('Resource Group Location')
param resourceGroupLocation string = 'SwedenCentral'

param resourceTags object = {
  Description: 'Resource group for MCM demos'
  Environment: 'Demo'
  ResourceType: 'ResourceGroup'
}

// Creating resource group
resource rg 'Microsoft.Resources/resourceGroups@2021-01-01' = {
  name: resourceGroupName 
  tags: resourceTags 
  location: resourceGroupLocation
}

@description('Output resource group name')
output rgName string = resourceGroupName
