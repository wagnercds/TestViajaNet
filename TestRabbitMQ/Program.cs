using System;
using RabbitMQ.Client;
using System.Text;

namespace TestRabbitMQ
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("RabbitMQ");
            Console.WriteLine("Creating Factory ...");
            ConnectionFactory factory = new ConnectionFactory() { HostName = "localhost" };
            Console.WriteLine("Creating connnection ...");
            using (IConnection connection = factory.CreateConnection())
            {
                Console.WriteLine("Creating channel ...");
                using (IModel channel = connection.CreateModel())
                {
                    channel.QueueDeclare("hello", false, false, false, null);
                    string message = "Hello world!";
                    byte[] body = Encoding.UTF8.GetBytes(message);
                    Console.WriteLine("Sending messagem ...");
                    channel.BasicPublish("", "hello", null, body);
                    Console.WriteLine("[x] Sent {0}", message);

                }
            }
            Console.WriteLine("Press [enter] to exit");
            Console.ReadLine();
        }
    }
}
