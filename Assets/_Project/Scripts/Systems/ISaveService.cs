using Project.Core;

namespace Project.Systems
{
    public interface ISaveService : IGameService
    {
        void Save();
        void Load();
    }
}