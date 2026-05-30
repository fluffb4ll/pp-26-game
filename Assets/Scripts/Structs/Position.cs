using UnityEngine;

namespace Structs
{
    public struct Position
    {
        public float x;
        public float y;
        public float z;

        public Position(Vector3 position)
        {
            x = position.x;
            y = position.y;
            z = position.z;
        }
    }
}