using System.Collections.Generic;
using Project.Core;

namespace Project.Systems
{
    public interface IKeyItemService : IGameService
    {
        void Unlock(string keyId);
        bool HasKey(string keyId);
        IReadOnlyCollection<string> GetAllUnlockedKeys();
    }
}