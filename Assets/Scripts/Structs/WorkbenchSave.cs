using System;
using Brainrot;
using UnityEngine;

namespace Structs
{
    [Serializable]
    public struct WorkbenchSave
    {
        public float baseProduce;
        public float produceStoreCap;
        public float storedProduce;
        public Position position;
        public Rotation rotation;

        public bool hasBrainrot;
        public BrainrotType type;
        public Rarity rarity;
        public float brainrotLifeTime;
        public float brainrotProduce;

        public WorkbenchSave(float baseProduce, float produceStoreCap, float storedProduce,
            Position position, Rotation rotation, bool hasBrainrot, 
            BrainrotType type = 0, Rarity rarity = 0, float brainrotLifeTime = 0f, float brainrotProduce = 0f)
        {
            this.storedProduce = storedProduce;
            this.produceStoreCap = produceStoreCap;
            this.baseProduce = baseProduce;
            this.position = position;
            this.rotation = rotation;
            
            this.hasBrainrot = hasBrainrot;
            this.type = type;
            this.rarity = rarity;
            this.brainrotLifeTime = brainrotLifeTime;
            this.brainrotProduce = brainrotProduce;
        }
    }
}