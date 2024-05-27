<#

# .SYNOPSIS

Install all required resources for Azure 2 Sharepoint demo from scratch.

.DESCRIPTION
 
This script will install all required resources for Azure 2 Sharepoint demo from scratch
with the use of Bicep templates. It will also install Bicep and Azure CLI if selected and get environment 
variables from local.env file. 
  
.EXAMPLE

PS > Init-Environment -Location "WestEurope" -InstallModules -InstallBicep -UseEnvFile
install modules, bicep and deploy resources to West Europe

.EXAMPLE

PS > Init-Environment  -Location "WestEurope" -OpenLogFile
install all required resources to West Europe and open log file with notepad with detailed data
 
.EXAMPLE

PS > Init-Environment  -Location "WestEurope" -OpenLogFile -OpenBrowserWithLatestUrl
install all required resources to West Europe and open log file with notepad with detailed data and the latest url in browser
 
. LINK

http://github.com/bovrhovn
 
#>
param(
    [Parameter(Mandatory = $false)]
    $Location = "SwedenCentral",
    [Parameter(Mandatory = $false)]
    $EnvFileToReadFrom = "local.env",
    [Parameter(Mandatory = $false)]
    [switch]$InstallModules,
    [Parameter(Mandatory = $false)]
    [switch]$InstallBicep,
    [Parameter(Mandatory = $false)]
    [switch]$InstallAzCli,
    [Parameter(Mandatory = $false)]
    [switch]$UseEnvFile,   
    [Parameter(Mandatory = $false)]
    [switch]$LoginToAzure, 
    [Parameter(Mandatory = $false)]
    [switch]$OpenLogFile,
    [Parameter(Mandatory = $false)]
    [switch]$OpenBrowserWithLatestUrl   
)

$ProgressPreference = 'SilentlyContinue';
Start-Transcript -Path "$HOME/Downloads/bootstrapper.log" -Force
# Write-Output "Sign in to Azure account." 
# login to Azure account if not logged in

if ($LoginToAzure)
{
    Write-Output "Sign in to Azure account." 
    Connect-AzAccount
}

if ($InstallModules)
{
    Write-Output "Install Az module and register providers."
    #install Az module
    Install-Module -Name Az -Scope CurrentUser -Repository PSGallery -Force
    Install-Module -Name Az.App

    #register providers
    Register-AzResourceProvider -ProviderNamespace Microsoft.App
    # add support for log analytics
    Register-AzResourceProvider -ProviderNamespace Microsoft.OperationalInsights
    Write-Output "Modules installed and registered, continuing to Azure deployment nad if selected, Bicep install."
}

if ($InstallBicep)
{
    # install bicep
    Write-Output "Installing Bicep."
    # & Install-Bicep.ps1
    Start-Process powershell.exe -FilePath Install-Bicep.ps1 -NoNewWindow -Wait
    Write-Output "Bicep installed, continuing to Azure deployment."
}

if ($InstallAzCli)
{
    Write-Output "Installing Azure CLI."

    Invoke-WebRequest -Uri https://aka.ms/installazurecliwindows -OutFile .\AzureCLI.msi;
    Start-Process msiexec.exe -Wait -ArgumentList '/I AzureCLI.msi /quiet';
    ## cleanup of Azure CLI
    Remove-Item .\AzureCLI.msi
    Write-Output "Az CLI installed, continuing to Azure deployment."
}
if ($UseEnvFile)
{
    Get-Content $EnvFileToReadFrom | ForEach-Object {
        $name, $value = $_.split('=')
        Set-Content env:\$name $value
        Write-Information "Writing $name to environment variable with $value."
    }
}

# create resource group if it doesn't exist with bicep file stored in bicep folder
$groupNameExport = New-AzSubscriptionDeployment -Location $Location -TemplateFile "..\Bicep\rg.bicep" -TemplateParameterFile "..\Bicep\rg.parameters.json" -Verbose

Write-Verbose "Resource group created $groupNameExport"
$groupName = $groupNameExport.Outputs.rgName.value
Write-Output "The resource group name is $groupName"

# deploy web app with application insights
$sqlDetails = New-AzResourceGroupDeployment -ResourceGroupName $groupName -TemplateFile "..\Bicep\sql.bicep" -TemplateParameterFile "..\Bicep\sql.parameters.json" -Verbose
Write-Output "SQL deployed to $Location with details $sqlDetails"
$appSettings = Get-Content -Path "..\Bicep\sql.parameters.json" | ConvertFrom-Json
$serverName=$appSettings.parameters.serverName.value
$sqlDBName=$appSettings.parameters.sqlDBName.value
$administratorLogin=$appSettings.parameters.administratorLogin.value
$administratorLoginPassword=$appSettings.parameters.administratorLoginPassword.value
Write-Information "Reading data from sql.parameters.json $sqlDBName and $serverName and $administratorLogin and $administratorLoginPassword."
$sqlConnString = "Server=tcp:$serverName.database.windows.net,1433;Initial Catalog=$sqlDBName;Persist Security Info=False;User ID=$administratorLogin@mcm-data-server.database.windows.net;Password=$administratorLoginPassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

$appSettings = Get-Content -Path "..\Bicep\webapp.parameters.json" | ConvertFrom-Json
Write-Verbose "Getting app settings from webapp.parameters.json $appSettings"
Write-Output "SQL connection string to set is $sqlConnString"
Write-Verbose "Updating connection string setting for web app."
$appSettings.parameters.sqlConn.value=$sqlConnString
Write-Verbose "Updating app settings with connection string $sqlConnString"
Set-Content -Path "..\Bicep\webapp.parameters.json" -Value ($appSettings | ConvertTo-Json)
Write-Output "Connection string updated for web app. Deploying web app."

$urlDetails = New-AzResourceGroupDeployment -ResourceGroupName $groupName -TemplateFile "..\Bicep\webapp.bicep" -TemplateParameterFile "..\Bicep\webapp.parameters.json" -Verbose
$urlToOpen = $sqlDetails.Outputs.pageUrl.value
if (-not [string]::IsNullOrEmpty($urlToOpen))
{
    Write-Output "Web App deployed to $Location with $urlToOpen"
}
Stop-Transcript

if ($OpenBrowserWithLatestUrl)
{
    # open browser with latest url
    Start-Process $urlToOpen
}

if ($OpenLogFile)
{
    # open file for viewing
    Start-Process notepad.exe -ArgumentList "$HOME/Downloads/bootstrapper.log"
}