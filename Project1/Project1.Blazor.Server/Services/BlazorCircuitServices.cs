using DevExpress.ExpressApp.Blazor.Services;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace Project1.Blazor.Server.Services
{
    internal class CircuitHandlerProxy : CircuitHandler
    {
        readonly IScopedCircuitHandler scopedCircuitHandler;
        public CircuitHandlerProxy(IScopedCircuitHandler scopedCircuitHandler)
        {
            this.scopedCircuitHandler = scopedCircuitHandler;
        }
        public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            return scopedCircuitHandler.OnCircuitOpenedAsync(cancellationToken);
        }
        public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            return scopedCircuitHandler.OnConnectionUpAsync(cancellationToken);
        }
        public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            return scopedCircuitHandler.OnCircuitClosedAsync(cancellationToken);
        }
        public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            return scopedCircuitHandler.OnConnectionDownAsync(cancellationToken);
        }
    }

    internal class ProxyHubConnectionHandler<THub> : HubConnectionHandler<THub> where THub : Hub
    {
        readonly IValueManagerStorageContainerInitializer storageContainerInitializer;
        public ProxyHubConnectionHandler(
            HubLifetimeManager<THub> lifetimeManager,
            IHubProtocolResolver protocolResolver,
            IOptions<HubOptions> globalHubOptions,
            IOptions<HubOptions<THub>> hubOptions,
            ILoggerFactory loggerFactory,
            IUserIdProvider userIdProvider,
            IServiceScopeFactory serviceScopeFactory,
            IValueManagerStorageContainerInitializer storageContainerAccessor)
            : base(lifetimeManager, protocolResolver, globalHubOptions, hubOptions, loggerFactory, userIdProvider, serviceScopeFactory)
        {
            this.storageContainerInitializer = storageContainerAccessor;
        }

        public override Task OnConnectedAsync(ConnectionContext connection)
        {
            storageContainerInitializer.Initialize();
            return base.OnConnectedAsync(connection);
        }
    }
}
