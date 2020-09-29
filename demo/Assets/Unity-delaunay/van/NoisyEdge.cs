using System.Collections.Generic;
using UnityEngine;
using van.map;

namespace van
{
    public class NoisyEdge
    {
        public static float NOISY_LINE_TRADEOFF = 0.5f;

        private static Dictionary<MapEdge, NoisyEdge> dic = new Dictionary<MapEdge, NoisyEdge>();

        private MapEdge _mapEdge;
        public List<Vector2> path0;
        public List<Vector2> path1;

        public NoisyEdge(MapEdge mapEdge)
        {
            _mapEdge = mapEdge;
            
            buildNoisyEdge();
        }
        
        public void buildNoisyEdge()
        {
            path0 = new List<Vector2>();
            path1 = new List<Vector2>();

            var f = NOISY_LINE_TRADEOFF;
            var t = Vector2.Lerp(_mapEdge.p0, _mapEdge.belongs[0].position, f);
            var q = Vector2.Lerp(_mapEdge.p0, _mapEdge.belongs[1].position, f);
            var r = Vector2.Lerp(_mapEdge.p1, _mapEdge.belongs[0].position, f);
            var s = Vector2.Lerp(_mapEdge.p1, _mapEdge.belongs[1].position, f);

            var minLength = 1;
            if (_mapEdge.belongs[0].isOcean && _mapEdge.belongs[1].isOcean) minLength = 100;

            path0 = buildNoisyLineSegments(_mapEdge.p0, t, _mapEdge.midPoint, q, minLength);
            path1 = buildNoisyLineSegments(_mapEdge.p1, s, _mapEdge.midPoint, r, minLength);

        }

        public static List<Vector2> buildNoisyLineSegments(Vector2 AA, Vector2 BB, Vector2 CC, Vector2 DD, float minLength)
        {
            List<Vector2> points = new List<Vector2>();

            void subdivide(Vector2 A, Vector2 B, Vector2 C, Vector2 D)
            {
                if (Vector2.Distance(A, C) < minLength || Vector2.Distance(B, D) < minLength)
                {
                    return;
                }
                
                // Subdivide the quadrilateral
                float p = Random.Range(0.2f, 0.8f);
                float q = Random.Range(0.2f, 0.8f);

                // Midpoints
                var E = Vector2.Lerp(A, D, p);
                var F = Vector2.Lerp(B, C, p);
                var G = Vector2.Lerp(A, B, q);
                var I = Vector2.Lerp(D, C, q);

                // Central point
                var H = Vector2.Lerp(E, F, q);

                // Divide the quad into subquads, but meet at H
                var s = 1f - Random.Range(-0.4f, 0.4f);
                var t = 1f - Random.Range(-0.4f, 0.4f);

                subdivide(A,Vector2.Lerp(G,B,s),H,Vector2.Lerp(E,D,t));
                points.Add(H);
                subdivide(H,Vector2.Lerp(F,C,s),C,Vector2.Lerp(I,D,t));
            }
            
            points.Add(AA);
            subdivide(AA,BB,CC,DD);
            points.Add(CC);
            return points;
        }
        
        
        

        public static NoisyEdge getNoisyEdge(MapEdge mapEdge)
        {
            if (!dic.ContainsKey(mapEdge))
            {
                dic.Add(mapEdge,new NoisyEdge(mapEdge));
            }

            return dic[mapEdge];
        }
    }
}