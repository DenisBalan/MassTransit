namespace MassTransit.GrpcTransport.Builders
{
    using Configuration;
    using Contexts;
    using GreenPipes;
    using MassTransit.Builders;
    using Pipeline;


    public class GrpcReceiveEndpointBuilder :
        ReceiveEndpointBuilder
    {
        readonly IGrpcReceiveEndpointConfiguration _configuration;
        readonly IGrpcHostConfiguration _hostConfiguration;

        public GrpcReceiveEndpointBuilder(IGrpcHostConfiguration hostConfiguration, IGrpcReceiveEndpointConfiguration configuration)
            : base(configuration)
        {
            _hostConfiguration = hostConfiguration;
            _configuration = configuration;
        }

        public override ConnectHandle ConnectConsumePipe<T>(IPipe<ConsumeContext<T>> pipe, ConnectPipeOptions options)
        {
            if (_configuration.ConfigureConsumeTopology && options.HasFlag(ConnectPipeOptions.ConfigureConsumeTopology))
            {
                _configuration.Topology.Consume
                    .GetMessageTopology<T>()
                    .Bind();
            }

            return base.ConnectConsumePipe(pipe, options);
        }

        public GrpcReceiveEndpointContext CreateReceiveEndpointContext()
        {
            var context = new TransportGrpcReceiveEndpointContext(_hostConfiguration, _configuration);

            context.GetOrAddPayload(() => _hostConfiguration.HostTopology);

            return context;
        }
    }
}
