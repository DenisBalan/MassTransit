namespace MassTransit.GrpcTransport.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Configuration;
    using Configurators;
    using GreenPipes;
    using MassTransit.Testing;
    using Registration;


    public class GrpcTestHarness :
        BusTestHarness
    {
        readonly GrpcBusConfiguration _busConfiguration;
        readonly string _inputQueueName;
        readonly IEnumerable<IBusInstanceSpecification> _specifications;

        public GrpcTestHarness()
            : this(Enumerable.Empty<IBusInstanceSpecification>())
        {
        }

        public GrpcTestHarness(IEnumerable<IBusInstanceSpecification> specifications)
        {
            BaseAddress = new Uri("http://127.0.0.1:19796/");

            _inputQueueName = "input-queue";
            _busConfiguration = new GrpcBusConfiguration(new GrpcTopologyConfiguration(GrpcBus.MessageTopology), BaseAddress);
            _specifications = specifications;

            InputQueueAddress = new Uri(BaseAddress, _inputQueueName);
        }

        public Uri BaseAddress { get; }

        public override Uri InputQueueAddress { get; }
        public override string InputQueueName => _inputQueueName;

        internal IGrpcHostConfiguration HostConfiguration => _busConfiguration?.HostConfiguration;

        public event Action<IGrpcBusFactoryConfigurator> OnConfigureGrpcBus;
        public event Action<IGrpcReceiveEndpointConfigurator> OnConfigureGrpcReceiveEndpoint;

        protected virtual void ConfigureGrpcBus(IGrpcBusFactoryConfigurator configurator)
        {
            OnConfigureGrpcBus?.Invoke(configurator);
        }

        protected virtual void ConfigureGrpcReceiveEndpoint(IGrpcReceiveEndpointConfigurator configurator)
        {
            OnConfigureGrpcReceiveEndpoint?.Invoke(configurator);
        }

        public virtual Task<IRequestClient<TRequest>> ConnectRequestClient<TRequest>()
            where TRequest : class
        {
            return ConnectRequestClient<TRequest>(InputQueueAddress);
        }

        public virtual Task<IRequestClient<TRequest>> ConnectRequestClient<TRequest>(Uri destinationAddress)
            where TRequest : class
        {
            return Task.FromResult(Bus.CreateRequestClient<TRequest>(destinationAddress, TestTimeout));
        }

        protected override IBusControl CreateBus()
        {
            var configurator = new GrpcBusFactoryConfigurator(_busConfiguration);

            configurator.Host(BaseAddress);

            ConfigureBus(configurator);

            ConfigureGrpcBus(configurator);

            configurator.ReceiveEndpoint(InputQueueName, e =>
            {
                ConfigureReceiveEndpoint(e);

                ConfigureGrpcReceiveEndpoint(e);
            });
            return configurator.Build(_busConfiguration, _specifications ?? Enumerable.Empty<ISpecification>());
        }
    }
}
