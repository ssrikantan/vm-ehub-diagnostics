# vm-ehub-diagnostics
Sample that demonstrates the use of Azure Event Hubs as a sink for streaming Diagnostic Logs from Azure Virtual Machines. Azure Functions is configured as the Event Hub Consumers that logs the incoming log stream to console. Azure Storage is used as an additional endpoint where the logs are streamed to.

## Create an Azure Event Hub Namespace and add an Event Hub endpoint
These steps are described here - https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-quickstart-portal 

## Create an Azure Function and configure an input trigger binding on the Event Hub configured above
Create a new Azure Function App (version 1.x is used in this example) and configure an input trigger on the Event Hub created in the earlier steps. This link https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-first-azure-function pertains to configuring a HTTP Trigger, but the steps for Event Hub input triggers are similar.

## Create a Storage Account in Azure where the streaming Diagnostics from Azure VMs would be sent to, in addition to Azure Event Hubs
These steps are described [here](https://docs.microsoft.com/en-us/azure/storage/common/storage-quickstart-create-account?toc=%2Fazure%2Fstorage%2Fblobs%2Ftoc.json&tabs=portal)
