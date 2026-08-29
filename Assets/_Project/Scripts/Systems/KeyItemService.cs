using System.Collections.Generic;

namespace Project.Systems
{
    public class KeyItemService : IKeyItemService
    {
        private readonly HashSet<string> _unlockedKeys = new HashSet<string>();

        public void Initialize()
        {
            _unlockedKeys.Clear();
        }

        public void Shutdown()
        {
            _unlockedKeys.Clear();
        }

        public void Unlock(string keyId)
        {
            if (string.IsNullOrEmpty(keyId)) return;
            _unlockedKeys.Add(keyId);
        }

        public bool HasKey(string keyId)
        {
            return _unlockedKeys.Contains(keyId);
        }

        public IReadOnlyCollection<string> GetAllUnlockedKeys()
        {
            return _unlockedKeys;
        }
    }
}