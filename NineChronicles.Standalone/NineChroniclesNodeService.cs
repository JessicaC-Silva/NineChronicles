using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Libplanet.Action;
using Libplanet.Blockchain;
using Libplanet.Blockchain.Policies;
using Libplanet.Crypto;
using Libplanet.Net;
using Libplanet.Standalone.Hosting;
using MagicOnion.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nekoyume.Action;
using Nekoyume.BlockChain;
using Nekoyume.Model.State;
using Nito.AsyncEx;
using Serilog;

using NineChroniclesActionType = Libplanet.Action.PolymorphicAction<Nekoyume.Action.ActionBase>;

namespace NineChronicles.Standalone
{
    public class NineChroniclesNodeService
    {
        private LibplanetNodeService<NineChroniclesActionType> NodeService { get; set; }

        private LibplanetNodeServiceProperties Properties { get; }

        private RpcNodeServiceProperties RpcProperties { get; }

        public AsyncAutoResetEvent BootstrapEnded => NodeService.BootstrapEnded;

        public AsyncAutoResetEvent PreloadEnded => NodeService.PreloadEnded;

        public Swarm<NineChroniclesActionType> Swarm => NodeService?.Swarm;

        public NineChroniclesNodeService(
            LibplanetNodeServiceProperties properties,
            RpcNodeServiceProperties rpcNodeServiceProperties,
            Progress<PreloadState> preloadProgress = null
        )
        {
            Properties = properties;
            RpcProperties = rpcNodeServiceProperties;

            // BlockPolicy shared through Lib9c.
            IBlockPolicy<PolymorphicAction<ActionBase>> blockPolicy = BlockPolicy.GetPolicy(
                properties.MinimumDifficulty
            );
            async Task minerLoopAction(
                BlockChain<NineChroniclesActionType> chain,
                Swarm<NineChroniclesActionType> swarm,
                PrivateKey privateKey,
                CancellationToken cancellationToken)
            {
                var miner = new Miner(chain, swarm, privateKey);
                while (!cancellationToken.IsCancellationRequested)
                {
                    Log.Debug("Miner called.");
                    try
                    {
                        await miner.MineBlockAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Exception occurred.");
                    }
                }
            }

            NodeService = new LibplanetNodeService<NineChroniclesActionType>(
                Properties,
                blockPolicy,
                minerLoopAction,
                preloadProgress
            );

            if (BlockPolicy.ActivationSet is null)
            {
                var tableSheetState = NodeService?.BlockChain?.GetState(TableSheetsState.Address);
                BlockPolicy.UpdateActivationSet(tableSheetState);
            }
        }

        public async Task Run(CancellationToken cancellationToken = default)
        {
            IHostBuilder hostBuilder = Host.CreateDefaultBuilder();
            if (RpcProperties.RpcServer)
            {
                hostBuilder = hostBuilder
                    .UseMagicOnion(
                        new ServerPort(RpcProperties.RpcListenHost, RpcProperties.RpcListenPort, ServerCredentials.Insecure)
                    )
                    .ConfigureServices((ctx, services) =>
                    {
                        services.AddHostedService(provider => new ActionEvaluationPublisher(
                            NodeService.BlockChain,
                            IPAddress.Loopback.ToString(),
                            RpcProperties.RpcListenPort
                        ));
                    });
            }

            await hostBuilder.ConfigureServices((ctx, services) =>
            {
                services.AddHostedService(provider => NodeService);
                services.AddSingleton(provider => NodeService.Swarm);
                services.AddSingleton(provider => NodeService.BlockChain);
            }).RunConsoleAsync(cancellationToken);
        }
    }
}