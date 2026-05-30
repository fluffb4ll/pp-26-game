using UnityEngine;

namespace Structs
{
    public struct Rotation
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Rotation(Quaternion rotation)
        {
            x = rotation.x;
            y = rotation.y;
            z = rotation.z;
            w = rotation.w;
        }
    }
}