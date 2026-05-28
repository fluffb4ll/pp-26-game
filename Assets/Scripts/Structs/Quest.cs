using System;
using UnityEngine;

namespace Structs
{
    public enum QuestType
    {
        Kill,
        PickUp,
        Insert,
        Collect,
        Travel,
        Buy
    }
    
    [Serializable]
    public struct Quest
    {
        public string description;
        public GameObject objective;
        public QuestType type;
    }
}