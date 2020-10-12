using UnityEngine;

namespace van
{
    public enum eTerrainBase{Base,Grass}
    public enum eTerrainObj{None,Tree,Hill}
    public enum eLocation{None,City,Village}
    
    public class SmallMapGenerator
    {
        
        //生成地形
        public eTerrainBase getTerrain(int x, int y)
        {
            if (nextRandom < 2)
                return eTerrainBase.Grass;
            return eTerrainBase.Base;
        }

        //生成地形物体
        public eTerrainObj getTerrainObject(int x, int y)
        {
            if(nextRandom>30)
                return eTerrainObj.None;
            if (nextRandom > 40)
                return eTerrainObj.Hill;
            return eTerrainObj.Tree;
        }
        
        //生成具体的
        public eLocation getLocation(int x, int y)
        {
            if (nextRandom > 2)
                return eLocation.None;
            if (nextRandom > 20)
                return eLocation.Village;
            return eLocation.City;
        }

        private float nextRandom=>Random.Range(0, 100f);
        
    }
}