// Auto-generated replacement by ChatGPT (simple runtime registry for items)
using UnityEngine;
using System.Collections.Generic;

namespace CyberLife.Combat
{
    public class ItemRegistry : MonoBehaviour
    {
        public List<ItemDef> preloadItems = new List<ItemDef>();
        static Dictionary<string, ItemDef> _map = new Dictionary<string, ItemDef>();

        void Awake()
        {
            foreach (var it in preloadItems)
            {
                if (it == null || string.IsNullOrEmpty(it.itemId)) continue;
                _map[it.itemId] = it;
            }
        }

        public static T Get<T>(string id) where T : ItemDef
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (_map.TryGetValue(id, out var def)) return def as T;
            return null;
        }
    }
}