namespace RequestService
{
    using System;
    using System.Configuration;
    using MassTransit;
    using MassTransit.RabbitMqTransport;
    using MassTransit.Util;
    using Topshelf;
    using Topshelf.Logging;


    class RequestService :
        ServiceControl
    {
        readonly LogWriter _log = HostLogger.Get<RequestService>();

        IBusControl _busControl;

        public bool Start(HostControl hostControl)
        {
            _log.Info("Criando o bus...");

            _busControl = Bus.Factory.CreateUsingRabbitMq(x =>
            {
                IRabbitMqHost host = x.Host(new Uri(ConfigurationManager.AppSettings["RabbitMQHost"]), h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                x.ReceiveEndpoint(host, ConfigurationManager.AppSettings["ServiceQueueName"],
                    e => { e.Consumer<RequestConsumer>(); });
            });

            _log.Info("Inicializando o bus...");

            TaskUtil.Await(() => _busControl.StartAsync());

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            _log.Info("Parando o bus...");

            _busControl?.Stop();

            return true;
        }
    }
}