using Microsoft.ServiceBus.Messaging;
using System;
using System.Configuration;

namespace Service
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Service");
                string connStr = ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];

                // Listen for a session based message.
                QueueClient qClientResponse = QueueClient.CreateFromConnectionString(connStr, "MyQueueRequest");
                qClientResponse.OnMessage(message =>
                {
                    Console.WriteLine("I've Received a Message.");

                    Console.WriteLine($"Message ID: {message.MessageId} - {message.ReplyToSessionId}");

                    var receivedPayload = message.GetBody<Messages.Message>();
                    var sessionToRelyTo = message.ReplyToSessionId;

                    Console.WriteLine($"Message from client - {receivedPayload.MyMessage}");

                    var responsePayload = new Messages.Message();
                    receivedPayload.MyMessage = "Bye bye world!!!!!!!";
                    BrokeredMessage bm = new BrokeredMessage(receivedPayload);
                    bm.SessionId = sessionToRelyTo;


                    QueueClient qClient = QueueClient.CreateFromConnectionString(connStr, "MyQueueResponse");
                    qClient.Send(bm);
                    Console.WriteLine("Sent Message!");
                });
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
    [Serializable]
    public class Response
    {
        public string MyMessage { get; set; }
    }
}