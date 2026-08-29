using System.Collections.Generic;
using Project.Core;
using Project.Data;

namespace Project.Systems
{

    public interface IInventoryService : IGameService
    {
        void AddPart(WeaponPartData part, int amount);
        int GetPartCount(WeaponPartData part);
        IReadOnlyDictionary<WeaponPartData, int> GetAllParts();

        void ClearAll();
    }
}