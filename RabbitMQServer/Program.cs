using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Couchbase;
using System.Collections.Generic;
using Newtonsoft.Json;
using Couchbase.Configuration.Client;
using Couchbase.Management;
using Couchbase.Authentication;
using Couchbase.Core;
using Couchbase.Views;

namespace RabbitMQServer
{
    /* Classe com as informações de log*/
    public class LogInformation
    {
        public string userToken { get; set; }
        public string functionName { get; set; }
        public IDictionary<string, object> arguments { get; set; }
        public DateTime dateNavigation { get; set; }
        public string type { get { return GetType().ToString(); } }
        public DateTime dateLog { get { return DateTime.Now; } }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Test ViajaNet");
            while (true)
            {
                Console.WriteLine("");
                Console.WriteLine("Choise (press <ENTER> after choise): ");
                Console.WriteLine("");
                Console.WriteLine("1- Rabbit MQ Server");
                Console.WriteLine("2- Map/Reduce");
                Console.WriteLine("F- Finish");
                switch (Console.ReadLine().ToUpper())
                {
                    case "1":
                        RabbitMQServer();
                        break;

                    case "2":
                        MapReduce();
                        break;

                    case "F":
                        return;

                    default:
                        Console.WriteLine("Invalid choise  !");
                        break;
                }

            }
        }

        /* Função para receber as mensagens do RabbitMQServer*/
        static void RabbitMQServer()
        {
            Console.WriteLine("RabbitMQ Server ...");

            try
            {
                var factory = new ConnectionFactory() { HostName = "localhost" };
                using (IConnection connection = factory.CreateConnection())
                using (IModel model = connection.CreateModel())
                {
                    model.QueueDeclare("logNavigation", false, false, false, null);
                    EventingBasicConsumer consumer = new EventingBasicConsumer(model);
                    consumer.Received += (modelPar, ea) =>
                    {
                        try
                        {
                            string message = Encoding.UTF8.GetString(ea.Body);
                            Console.WriteLine("Received: {0}", message);
                            ClientConfiguration config = new ClientConfiguration() { Servers = new List<Uri>() { new Uri("http://localhost:8091") } };
                            config.SetAuthenticator(new PasswordAuthenticator("test", "123456"));
                            using (Cluster cluster = new Cluster(config))
                            using (IBucket bucket = cluster.OpenBucket("viajanet"))
                            {
                                LogInformation logInformation = JsonConvert.DeserializeObject<LogInformation>(message);
                                Document<LogInformation> document = new Document<LogInformation>()
                                {
                                    Id = String.Format("{0}_{1}", logInformation.userToken, DateTime.Now.ToString("yyyyMMddHHmmss")),
                                    Content = logInformation
                                };
                                if (!bucket.Upsert(document).Success)
                                    Console.WriteLine("Error to save log !");
                            }
                            model.BasicAck(ea.DeliveryTag, false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error (save or get message):  {0}", ex.Message);
                        }
                    };
                    model.BasicConsume("logNavigation", false, consumer);
                    Console.WriteLine("Press <ENTER> to finish");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
            }
        }

        /* Função para demonstrar o uso do */
        static void MapReduce()
        {
            Console.WriteLine("Map/Reduce");
            Console.WriteLine("");

            ClientConfiguration config = new ClientConfiguration() { Servers = new List<Uri>() { new Uri("http://localhost:8091") } };
            config.SetAuthenticator(new PasswordAuthenticator("test", "123456"));
            using (Cluster cluster = new Cluster(config))
            using (IBucket bucket = cluster.OpenBucket("viajanet"))
            {
                IViewQuery query = new ViewQuery().From("clients", "clientCountNavigation").GroupLevel(2);
                IViewResult<dynamic> clientNavigation = bucket.Query<dynamic>(query);
                string userToken = "";
                foreach (var client in clientNavigation.Rows)
                {
                    if (client.Key[0] != userToken)
                    { 
                        Console.WriteLine("Client {0}", client.Key[0]);
                        userToken = client.Key[0];
                    }
                    Console.WriteLine("    Function: {0} -> Count: {1}", client.Key[1], client.Value);                    
                }
            }
        }
    }


}

