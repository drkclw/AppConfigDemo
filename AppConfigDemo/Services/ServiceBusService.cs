using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace AppConfigDemo.Services
{
    public class ServiceBusService
    {
        private readonly IConfiguration _configuration;
        private readonly ServiceBusClient _client;
        private string QUEUE_NAME = Environment.GetEnvironmentVariable("AppConfigDemoServiceBusSub");
        private readonly ILogger _logger;
        private ServiceBusProcessor? _processor;
        private IConfigurationRefresher _configurationRefresher;

        public ServiceBusService(IConfiguration configuration,
            ILogger<ServiceBusService> logger,
            IConfigurationRefresherProvider refresherProvider)
        {
            _configuration = configuration;
            _logger = logger;

            var connectionString = Environment.GetEnvironmentVariable("AppConfigDemoServiceBusCs");
            _client = new ServiceBusClient(connectionString);
            _configurationRefresher = refresherProvider.Refreshers.FirstOrDefault();
        }

        public async Task RegisterOnMessageHandlerAndReceiveMessages()
        {
            ServiceBusProcessorOptions _serviceBusProcessorOptions = new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 1,
                AutoCompleteMessages = false,
            };

            _processor = _client.CreateProcessor(QUEUE_NAME, _serviceBusProcessorOptions);
            _processor.ProcessMessageAsync += ProcessMessagesAsync;
            _processor.ProcessErrorAsync += ProcessErrorAsync;
            await _processor.StartProcessingAsync().ConfigureAwait(false);
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            _logger.LogError(arg.Exception, "Message handler encountered an exception");
            _logger.LogDebug($"- ErrorSource: {arg.ErrorSource}");
            _logger.LogDebug($"- Entity Path: {arg.EntityPath}");
            _logger.LogDebug($"- FullyQualifiedNamespace: {arg.FullyQualifiedNamespace}");

            return Task.CompletedTask;
        }

        private async Task ProcessMessagesAsync(ProcessMessageEventArgs args)
        {
            //var myPayload = args.Message.Body.ToObjectFromJson<MyPayload>();
            //await _processData.Process(myPayload).ConfigureAwait(false);            
            await _configurationRefresher.RefreshAsync();
            await args.CompleteMessageAsync(args.Message).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            if (_processor != null)
            {
                await _processor.DisposeAsync().ConfigureAwait(false);
            }

            if (_client != null)
            {
                await _client.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task CloseQueueAsync()
        {
            await _processor.CloseAsync().ConfigureAwait(false);
        }
    }
}
