using System;
using System.Collections.Generic;
using CactusPie.MapLocation.Common.Requests.Data;
using EFT;

namespace CactusPie.MapLocation.Services.Bots
{
    public interface IBotDataService : IDisposable
    {
        IReadOnlyDictionary<int, BotOwner> SpawnedBots { get; }

        BotType GetBotType(BotOwner bot);

        void InitializeBotDataForCurrentGame();

        void UnloadBotDataForCurrentGame();
    }
}