﻿namespace Client
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using log4net.Config;
    using MassTransit;
    using MassTransit.Log4NetIntegration.Logging;
    using MassTransit.Util;
    using Sample.MessageTypes;


    class Program
    {
        static void Main()
        {
            ConfigureLogger();

            // MassTransit to use Log4Net
            Log4NetLogger.Use();

            IBusControl busControl = CreateBus();

            TaskUtil.Await(() => busControl.StartAsync());

            try
            {
                IRequestClient<ISimpleRequest, ISimpleResponse> client = CreateRequestClient(busControl);

                for (; ; )
                {
                    Console.Write("Entre com o codigo do Posto (quit exits): ");
                    string postoId = Console.ReadLine();
                    if (postoId == "quit")
                        break;

                    // this is run as a Task to avoid weird console application issues
                    Task.Run(async () =>
                    {
                        ISimpleResponse response = await client.Request(new SimpleRequest(postoId));

                        Console.WriteLine($"Nome do Posto é : {response.PostoNome}");
                    }).Wait();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Deu ruim!!! OMG!!! {0}", ex);
            }
            finally
            {
                busControl.Stop();
            }
        }


        static IRequestClient<ISimpleRequest, ISimpleResponse> CreateRequestClient(IBusControl busControl)
        {
            var serviceAddress = new Uri(ConfigurationManager.AppSettings["ServiceAddress"]);
            IRequestClient<ISimpleRequest, ISimpleResponse> client =
                busControl.CreateRequestClient<ISimpleRequest, ISimpleResponse>(serviceAddress, TimeSpan.FromSeconds(10));

            return client;
        }

        static IBusControl CreateBus()
        {
            return Bus.Factory.CreateUsingRabbitMq(x => x.Host(new Uri(ConfigurationManager.AppSettings["RabbitMQHost"]), h =>
            {
                h.Username("guest");
                h.Password("guest");
            }));
        }

        static void ConfigureLogger()
        {
            const string logConfig = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<log4net>
  <root>
    <level value=""INFO"" />
    <appender-ref ref=""console"" />
  </root>
  <appender name=""console"" type=""log4net.Appender.ColoredConsoleAppender"">
    <layout type=""log4net.Layout.PatternLayout"">
      <conversionPattern value=""%m%n"" />
    </layout>
  </appender>
</log4net>";

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(logConfig)))
            {
                XmlConfigurator.Configure(stream);
            }
        }


        class SimpleRequest :
            ISimpleRequest
        {
            private readonly string _postoId;
            
            readonly DateTime _timestamp;

            public SimpleRequest(string postoID)
            {
                _postoId = postoID;
                
                _timestamp = DateTime.UtcNow;
            }

            public DateTime Timestamp
            {
                get { return _timestamp; }
            }

            public string PostoID
            {
                get { return _postoId; }
            }

 
        }
    }
}