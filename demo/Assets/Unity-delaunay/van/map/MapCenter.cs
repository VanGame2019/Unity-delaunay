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

        public Bounds bounds;
        public float BoundsSize => bounds.size.x;
        
        private Color _color = Color.black;
        public Color color
        {
            get
            {
                if (this.boss == null)
                {
                    if (this._color == Color.black)
                    {
                        this._color = Random.ColorHSV(0.5f, 1f, 0.5f, 1f, 0.5f, 1f, 0.5f, 0.5f);
                    }
                    return _color;
                }

                return this.boss.color;
            }
        }

        private Color _colorWithoutAlpha= Color.black;
        public Color colorWithoutAlpha
        {
            get
            {
                if (this._colorWithoutAlpha == Color.black)
                {
                    this._colorWithoutAlpha = new Color();
                    this._colorWithoutAlpha.r = this.color.r;
                    this._colorWithoutAlpha.g = this.color.g;
                    this._colorWithoutAlpha.b = this.color.b;
                    this._colorWithoutAlpha.a = 1;
                }

                return this._colorWithoutAlpha;
            }
        }
        
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