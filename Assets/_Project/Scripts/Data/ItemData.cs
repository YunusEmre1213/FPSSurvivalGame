using UnityEngine;

namespace Project.Data
{
    public abstract class ItemData : ScriptableObject
    {
        [Header("Kimlik")]
        public string itemId;
        public string itemName = "Yeni Esya";
        public Sprite icon;
        [TextArea] public string description;

        [Header("Yiginlama")]
        [Tooltip("Bir slotta bu esyadan en fazla kac tane yiginlanabilir. 1 = yiginlanamaz (ornegin takili bir silah parcasi).")]
        public int maxStackSize = 1;
    }
}