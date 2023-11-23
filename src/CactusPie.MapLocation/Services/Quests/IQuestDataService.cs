using System.Collections.Generic;
using CactusPie.MapLocation.Common.Requests.Data;
using EFT.Interactive;

namespace CactusPie.MapLocation.Services.Quests
{
    public interface IQuestDataService
    {
        IReadOnlyList<QuestData> QuestMarkers { get; }

        void ReloadQuestData(TriggerWithId[] allTriggers);
    }
}