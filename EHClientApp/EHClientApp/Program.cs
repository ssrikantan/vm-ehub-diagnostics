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
