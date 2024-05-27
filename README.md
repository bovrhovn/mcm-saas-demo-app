# Simple Demo Web Application for Microsoft Commercial Marketplace

Simple web application to demonstrate how to deploy a web application to Microsoft Azure and publish it to the Microsoft
Commercial Marketplace.

## Prerequisites

The solution use the following technology:
- [Bootstrap CSS](https://getbootstrap.com/) for design
- [ASP.NET](https://asp.net) as a technology to provide UI and code to call the API's
- [HTMX](https://htmx.org/) for communication with backend
- [Azure Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview) to get information about metrics / logs and to build interactive, rich dashboards
- [PowerShell](https://docs.microsoft.com/en-us/powershell/) support for Automating operations, scripting and deploying the environment

### Minimal requirements

1. an active [Azure](https://www.azure.com) subscription - [MSDN](https://my.visualstudio.com) or trial
   or [Azure Pass](https://microsoftazurepass.com) is fine - you can also do all of the work
   in [Azure Shell](https://shell.azure.com) (all tools installed) and by
   using [Github Codespaces](https://docs.github.com/en/codespaces/developing-in-codespaces/creating-a-codespace)
2. [PowerShell](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows?view=powershell-7.2)
   installed - we do recommend an editor like [Visual Studio Code](https://code.visualstudio.com) to be able to write
   scripts, YAML pipelines and connect to repos to submit changes.
3. [OPTIONAL] [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/) installed to work with Azure or Azure PowerShell
   module installed
4. [OPTIONAL] [Windows Terminal](https://learn.microsoft.com/en-us/windows/terminal/install) to be able to work with
   multiple terminal Windows with ease

### Local machine

If you will be working on your local machines, you will need to have:

1. [Powershell](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows)
   installed
2. git installed - instructions step by step [here](https://docs.github.com/en/get-started/quickstart/set-up-git)
3. [.NET](https://dot.net) installed to run the application if you want to run it locally
4. an editor (besides notepad) to see and work with code, scripts and more (for
   example [Visual Studio Code](https://code.visualstudio.com) or [NeoVim](https://neovim.io/))

## Demo structure

It contains:

1. **Web Application**: A simple web application that focus on 2 main parts - one from customer perspective and the same
   application for the publisher perspective with minimum required app.

    - **Customer Perspective**: A simple web application that gives an overview of what a customer gets when they
      purchase a product from Microsoft Commercial Marketplace.
    - **Publisher Perspective**: A simple web application that shows what you get as a publisher from Microsoft
      Commercial Marketplace.

   It adheres to the following structure:

   ![SaaS lifecycle](https://learn.microsoft.com/en-us/partner-center/marketplace/partner-center-portal/media/saas-subscription-lifecycle-api-v2.png)

2. The web application is built using the following technologies:
- **Frontend**: HTML, CSS, and JavaScript, VueJs, ASP.NET Core RazorPages.
- **Backend**: ASP.NET Core, C#.

3. The web application is hosted on [Azure App Service](https://learn.microsoft.com/en-us/azure/app-service/overview) for demonstration purposes.
4. **Webhook** for the publisher perspective with minimum required api's to respond to communication from Microsoft
   Marketplace. More [here](https://learn.microsoft.com/en-us/partner-center/marketplace/partner-center-portal/pc-saas-fulfillment-webhook).
5. **Bicep Templates**: Bicep templates to deploy the web application to Azure. Available [here](./Bicep).

# Additional information

You can read about different techniques and options here:

1. [ASP.NET Core](https://asp.net)
2. [VueJs](https://vuejs.org/)
3. [GitHub and DevOps](https://resources.github.com/devops/)
4. [Azure Samples](https://github.com/Azure-Samples)
   or [use code browser](https://docs.microsoft.com/en-us/samples/browse/?products=azure)
5. [Azure Architecture Center](https://docs.microsoft.com/en-us/azure/architecture/)
6. [Application Architecture Guide](https://docs.microsoft.com/en-us/azure/architecture/guide/)
7. [Cloud Adoption Framework](https://docs.microsoft.com/en-us/azure/cloud-adoption-framework/)
8. [Well-Architected Framework](https://docs.microsoft.com/en-us/azure/architecture/framework/)
9. [Microsoft Learn](https://docs.microsoft.com/en-us/learn/roles/solutions-architect)

# Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.