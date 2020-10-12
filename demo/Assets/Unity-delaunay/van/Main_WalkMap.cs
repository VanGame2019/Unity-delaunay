using System;
using System.Collections.Generic;
using UnityEngine;

namespace van
{
    public class Main_WalkMap:MonoBehaviour
    {
        private SmallMapGenerator Generator;
        
        public List<Sprite> spriteList;
        public SpriteRenderer sample;
        
        private void Awake()
        {
            Generator = new SmallMapGenerator();
            createMap();
        }

        int w = 100;
        int h = 100;
        private void createMap()
        {
            
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    var terrain = Generator.getTerrain(i, j);
                    var terrainObj = Generator.getTerrainObject(i, j);
                    var location = Generator.getLocation(i, j);

                    if (terrain == eTerrainBase.Grass)
                    {
                        var sp = Instantiate(sample);
                        sp.sprite = spriteList[0];
                        sp.transform.position = getPosition(i,j);
                        sp.sortingOrder = 0;
                    }

                    switch (terrainObj)
                    {
                        case eTerrainObj.None:
                            break;
                        case eTerrainObj.Tree:
                        {
                            if(location!=eLocation.None)
                                break;
                            var sp = Instantiate(sample);
                            sp.sprite = spriteList[5];
                            sp.transform.position = getPosition(i,j);
                            sp.sortingLayerName = "Object";
                            sp.sortingOrder = -j*100;
                        }
                           
                            break;
                        case eTerrainObj.Hill:
                        {
                            if(location!=eLocation.None)
                                break;
                            var sp = Instantiate(sample);
                            sp.sprite = spriteList[3];
                            sp.transform.position = getPosition(i,j);
                            sp.sortingLayerName = "Object";
                            sp.sortingOrder = -j*100;
                        }
                            
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    switch (location)
                    {
                        case eLocation.None:
                            break;
                        case eLocation.City:
                        {
                            var sp = Instantiate(sample);
                            sp.sprite = spriteList[1];
                            sp.transform.position = getPosition(i,j);
                            sp.sortingLayerName = "Object";
                            sp.sortingOrder = -j*100+1;
                        }
                            break;
                        case eLocation.Village:
                        {
                            var sp = Instantiate(sample);
                            sp.sprite = spriteList[4];
                            sp.transform.position = getPosition(i,j);
                            sp.sortingLayerName = "Object";
                            sp.sortingOrder = -j*100+1;
                        }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        private Vector3 getPosition(int x, int y)
        {
            var xGap = 1f;
            var yGap = 0.75f;
            var _x = (-w / 2 + x) * xGap;
            var _y = (-h / 2 + y) * yGap;
            
            if (y % 2 == 0)
            {
                _x += xGap / 2;
            }
            return new Vector3(_x,0,_y);
        }
        
    }
}