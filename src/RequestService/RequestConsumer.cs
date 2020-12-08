namespace RequestService
{
    using System.Threading;
    using System.Threading.Tasks;
    using MassTransit;
    using MassTransit.Logging;
    using Sample.MessageTypes;


    public class RequestConsumer :
        IConsumer<ISimpleRequest>
    {
        readonly ILog _log = Logger.Get<RequestConsumer>();

        public async Task Consume(ConsumeContext<ISimpleRequest> context)
        {
            _log.InfoFormat($"Recebendo request para o Posto {context.Message.PostoID}");

            var postoResposta = new SimpleResponse();

            switch (context.Message.PostoID)
            {
                case "1":
                    postoResposta.PostoNome = "Posto Ipiranga";
                    break;
                case "2":
                    postoResposta.PostoNome = "Posto Shell";
                    break;
                case "3":
                    _log.InfoFormat("Esperando o processamento");
                    Thread.Sleep(5000);
                    postoResposta.PostoNome = "Posto Demorado";
                    break;
                default:
                    postoResposta.PostoNome = "Posto não cadastrado";
                    break;
            }

            context.Respond(postoResposta);
        }


        class SimpleResponse :
            ISimpleResponse
        {
            public string PostoNome { get; set; }
        }
    }
}