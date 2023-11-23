using System.Collections.Generic;
using CactusPie.MapLocation.Common.Requests.Data;

namespace CactusPie.MapLocation.Common.Requests
{
    public sealed class QuestDataResponse
    {
        public IReadOnlyList<QuestData> Quests { get; set; }
    }
}