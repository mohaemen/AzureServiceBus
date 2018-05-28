using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Configuration;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            string connStr = ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];

            Console.WriteLine("Client");
            try
            {

                var payload = new Messages.Message();
                payload.MyMessage = "Hello world";

                var messageId = Guid.NewGuid().ToString();
                Console.WriteLine($"My Mesasge ID is {messageId}");
                BrokeredMessage bm = new BrokeredMessage(payload);
                bm.ReplyToSessionId = messageId;

                NamespaceManager ns = NamespaceManager.CreateFromConnectionString(connStr);


                if (!ns.QueueExists("MyQueueRequest"))
                {
                    QueueDescription queue = new QueueDescription("MyQueueRequest");
                    queue.EnablePartitioning = false;
                    queue.RequiresSession = false;

                    ns.CreateQueue(queue);
                }
                if (!ns.QueueExists("MyQueueResponse"))
                {
                    QueueDescription queue = new QueueDescription("MyQueueResponse");
                    queue.EnablePartitioning = false;
                    queue.RequiresSession = true;

                    ns.CreateQueue(queue);
                }

                // Sending to a queue
                QueueClient qClient = QueueClient.CreateFromConnectionString(connStr, "MyQueueRequest");
                qClient.Send(bm);
                Console.WriteLine("Sent Message!");


                // Listen for a session based message.
                QueueClient qClientResponse = QueueClient.CreateFromConnectionString(connStr, "MyQueueResponse");
                var messageSession = qClientResponse.AcceptMessageSession(messageId);
                messageSession.OnMessage(message =>
                {
                    Console.WriteLine("I've Received a Message.");

                    Console.WriteLine($"Message ID: {message.MessageId} - {message.SessionId}");

                    var receivedPayload = message.GetBody<Messages.Message>();

                    Console.WriteLine($"Message from service - {receivedPayload.MyMessage}");

                },
                new OnMessageOptions());

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.ReadLine();
        }
    }

}

namespace Messages
{
    [Serializable]
    public class Message
    {
        public string MyMessage { get; set; }
    }
}