using System.Collections.Generic;
using UnityEngine;

namespace van.map
{
    public enum eCenterRank{Big,Mid,Small}
    public class MapCenter
    {
        public Vector2 position;
        
        public List<MapCenter> neighbours = new List<MapCenter>();
        public List<MapEdge> edges = new List<MapEdge>();

        public bool isOcean = false;

        public eCenterRank rank = eCenterRank.Small;
        public MapCenter boss = null;
        public List<MapCenter> underList = new List<MapCenter>();
        
        public MapCenter(Vector2 p)
        {
            this.position = p;
        }

        public List<MapCenter> getAllUnderlist()
        {
            List<MapCenter> list = new List<MapCenter>();
            checkUnderList(list,this);
            return list;
        }

        private void checkUnderList(List<MapCenter> list,MapCenter target)
        {
            list.Add(target);
            target.underList.ForEach((c)=>checkUnderList(list,c));
        }
    }
    
}