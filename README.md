# vm-ehub-diagnostics
Sample that demonstrates the use of Azure Event Hubs as a sink for streaming Diagnostic Logs from Azure Virtual Machines. Azure Functions is configured as the Event Hub Consumers that logs the incoming log stream to console. Azure Storage is used as an additional endpoint where the logs are streamed to.

## Create an Azure Event Hub end point and generate a SAS Token
1. Create the Event Hub Namespace and an Event Hub endpoint in it

These steps are described in detail [here](https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-quickstart-portal)
2. Generate a SAS Token with permissions to send messages to this Event Hub Endpoint

Choose from any of these methods [here](https://docs.microsoft.com/en-us/rest/api/eventhub/generate-sas-token) to generate a SAS Token.
I have used a .NET Framework 4.6.1 Console Program to generate this SAS Token
````
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;

namespace EHClientApp
{
    /// <summary>
    /// A .NET framework 4.6.1 console App to perform management operations on Azure Event Hubs
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            new EHubClient().getSasKey();
        }
    }
/// <summary>
/// Creates an event Hub End point in an existing Event Hub Namespace and generates a SAS Token that has
/// permissions to send messages to this endpoint.
/// </summary>
    public class EHubClient
    {

        string serviceNamespace = "insightsehub";
        string namespaceManageKeyName = "RootManageSharedAccessKey";
        string namespaceManageKey = "<The Access key to this Event Hub>";
        string ehubname = "vmdiaglogsink";
        public void getSasKey()
        {
            try
            {
                // Create namespace manager.
                Uri uri = ServiceBusEnvironment.CreateServiceUri("sb", serviceNamespace, string.Empty);
                TokenProvider td = TokenProvider.CreateSharedAccessSignatureTokenProvider(namespaceManageKeyName, namespaceManageKey);
                NamespaceManager nm = new NamespaceManager(uri, td);

                // Create event hub with a SAS rule that enables sending to that event hub
                EventHubDescription ed = new EventHubDescription(ehubname) { PartitionCount = 32 };
                string eventHubSendKeyName = "EventHubSendKey";
                string eventHubSendKey = SharedAccessAuthorizationRule.GenerateRandomKey();
                SharedAccessAuthorizationRule eventHubSendRule = new SharedAccessAuthorizationRule
                    (eventHubSendKeyName, eventHubSendKey, new[] { AccessRights.Send });
                ed.Authorization.Add(eventHubSendRule);
                nm.CreateEventHub(ed);
                string resource = "insightsehub.servicebus.windows.net/" + ehubname;

                //Copy this SAS Key for use in configuring the VM Diagnostics
                string saskey = SharedAccessSignatureTokenProvider.GetSharedAccessSignature(eventHubSendKeyName,
                    eventHubSendKey, resource, TimeSpan.FromDays(365));
                Console.WriteLine("Event HUb Endpoint created");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception " + ex.StackTrace);
            }
        }
    }
    }

````
The SAS Token generated would have this form
"SharedAccessSignature sr=%2f%2finsightsehub.servicebus.windows.net%2fvmdiaglogsink%2fpublishers%2fvmlogger&sig=<gibberish>se=<somenumber>&skn=EventHubSendKey"

This has to be changed to a Https SAS Token URL before it can be used by the VM Diganostic agent. Remove 'SharedAccessSignature' from it and prefix it with 'https://<your event hub namespace>/<your event hub endpoint>?'. 
The SAS Token URL should look like:

https://insightsehub.servicebus.windows.net/vmdiaglogsink?sr=insightsehub.servicebus.windows.net%2fvmdiaglogsink&sig=<some gibberish>&se=<some number>&skn=EventHubSendKey

## Create an Azure Function and configure an input trigger binding on the Event Hub configured above
Create a new Azure Function App (version 1.x is used in this example) and configure an input trigger on the Event Hub created in the earlier steps. This link https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-first-azure-function pertains to configuring a HTTP Trigger, but the steps for Event Hub input triggers are similar.

## Create a Storage Account in Azure as a sink for diagnostic logs
1. Configure an Azure Storage Account

This is where the streaming Diagnostics from Azure VMs would be sent to, in addition to Azure Event Hubs. These steps are described [here](https://docs.microsoft.com/en-us/azure/storage/common/storage-quickstart-create-account?toc=%2Fazure%2Fstorage%2Fblobs%2Ftoc.json&tabs=portal)

2. Generate a SAS Token for the Storage Account

This token would be used by the Diagnostic agent to connect to the Azure Storage account and write the logs. Refer to this link [here](https://docs.microsoft.com/en-us/azure/virtual-machines/extensions/diagnostics-linux#installing-and-configuring-lad-30-via-cli) that demonstrates how the Azure portal could be used to generate the SAS Token.
