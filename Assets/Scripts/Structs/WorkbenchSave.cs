using System;
using Brainrot;
using UnityEngine;

namespace Structs
{
    [Serializable]
    public struct WorkbenchSave
    {
        public float storedProduce;
        public BrainrotObject insertedBrainrot;
        public Vector3 position;
        public Quaternion rotation;

        public WorkbenchSave(float storedProduce, BrainrotObject insertedBrainrot,
            Vector3 position, Quaternion rotation)
        {
            this.storedProduce = storedProduce;
            this.insertedBrainrot = insertedBrainrot;
            this.position = position;
            this.rotation = rotation;
        }
    }
}