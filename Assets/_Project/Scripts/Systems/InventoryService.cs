using System.Collections.Generic;
using Project.Core;
using Project.Data;

namespace Project.Systems
{
    public class InventoryService : IInventoryService
    {
        private readonly Dictionary<WeaponPartData, int> _parts = new Dictionary<WeaponPartData, int>();

        public void Initialize()
        {
            _parts.Clear();
        }

        public void Shutdown()
        {
            _parts.Clear();
        }

        public void AddPart(WeaponPartData part, int amount)
        {
            if (part == null || amount <= 0) return;

            if (_parts.ContainsKey(part))
            {
                _parts[part] += amount;
            }
            else
            {
                _parts[part] = amount;
            }

            EventBus.Publish(new InventoryChangedEvent(part, _parts[part]));
        }

        public int GetPartCount(WeaponPartData part)
        {
            return _parts.TryGetValue(part, out var count) ? count : 0;
        }

        public IReadOnlyDictionary<WeaponPartData, int> GetAllParts()
        {
            return _parts;
        }

        public void ClearAll()
        {
            _parts.Clear();
            EventBus.Publish(new InventoryChangedEvent(null, 0));
        }
    }
}