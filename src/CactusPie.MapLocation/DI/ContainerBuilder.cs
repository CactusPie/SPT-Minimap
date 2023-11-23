using System.Net;
using System.Net.Sockets;
using CactusPie.MapLocation.Common.Requests.Data;
using CactusPie.MapLocation.Services;
using CactusPie.MapLocation.Services.Airdrop;
using CactusPie.MapLocation.Services.Bots;
using CactusPie.MapLocation.Services.Quests;
using Comfort.Common;
using EFT;

namespace CactusPie.MapLocation.DI
{
    public static class ContainerBuilder
    {
        public static ServiceContainer BuildContainer()
        {
            var container = new ServiceContainer();

            container.Register<ILocalizationHelper, LocalizationHelper>();
            container.Register<IQuestDataService, QuestDataService>();
            container.Register<IBotDataService, BotDataService>();
            container.Register<IMapDataServer, MapDataServer>(new PerContainerLifetime());
            container.Register<IAirdropService, AirdropService>(new PerContainerLifetime());
            container.Register(_ => Singleton<GameWorld>.Instance);

            container.Compile();

            return container;
        }
    }
}