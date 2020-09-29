using System.Collections.Generic;
using UnityEngine;

namespace van.map
{
    public class MapCenter
    {
        public Vector2 position;
        
        public List<MapCenter> neighbours = new List<MapCenter>();
        public List<MapEdge> edges = new List<MapEdge>();

        public bool isOcean = false;
        
        public MapCenter(Vector2 p)
        {
            this.position = p;
        }
    }
}