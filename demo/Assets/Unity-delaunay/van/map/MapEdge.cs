using System.Collections.Generic;
using UnityEngine;

namespace van.map
{
    public class MapEdge
    {
        public List<MapCenter> belongs = new List<MapCenter>();
        public Vector2 p0;
        public Vector2 p1;

        public Vector2 midPoint => Vector2.Lerp(p0, p1, 0.5f);
        public MapEdge(Vector2 _p0, Vector2 _p1)
        {
            this.p0 = _p0;
            this.p1 = _p1;
        }
        
    }
}