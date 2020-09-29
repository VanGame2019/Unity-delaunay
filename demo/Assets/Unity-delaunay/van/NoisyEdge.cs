using System.Collections.Generic;
using UnityEngine;
using van.map;

namespace van
{
    public class NoisyEdge
    {
        public static float NOISY_LINE_TRADEOFF = 0.5f;

        private static Dictionary<MapEdge, NoisyEdge> dic = new Dictionary<MapEdge, NoisyEdge>();

        private static FastNoiseLite warpNoise;
        public static void init()
        {
            warpNoise = new FastNoiseLite();
            
            warpNoise.SetSeed(1337);
            warpNoise.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2);
            warpNoise.SetRotationType3D(FastNoiseLite.RotationType3D.None);
            warpNoise.SetDomainWarpAmp(100);
            warpNoise.SetFrequency(0.01f);
            warpNoise.SetFractalType(FastNoiseLite.FractalType.DomainWarpIndependent);
            warpNoise.SetFractalOctaves(3);
            warpNoise.SetFractalLacunarity(2);
            warpNoise.SetFractalGain(0.5f);
        }
        
        private MapEdge _mapEdge;
        public List<Vector2> path0;
        public List<Vector2> path1;

        public NoisyEdge(MapEdge mapEdge)
        {
            _mapEdge = mapEdge;
            
            buildNoisyEdge();
//            buildNoisyEdgeByWarp();
        }


        public static int count = 0;
        private static float RATIO = 10;
        private void buildNoisyEdgeByWarp()
        {
            path0 = new List<Vector2>();
            path1 = new List<Vector2>();

//            path0.Add(_mapEdge.p0);
            for (int i = 0; i <= 10; i++)
            {
                var p = Vector2.Lerp(_mapEdge.p0, _mapEdge.p1, 0.1f * i);
                var x = Mathf.Floor(p.x*RATIO);
                var y = Mathf.Floor(p.y*RATIO);
                var z = 0f;
                if (count == 0)
                {
                    Debug.Log($"before({x},{y})");
                }
                warpNoise.DomainWarp(ref x,ref y,ref z);

                if (count == 0)
                {
                    Debug.Log($"after({x},{y})");
                }
                Vector2 v = new Vector2(x/RATIO,y/RATIO);
                this.path0.Add(v);
            }
//            path0.Add(_mapEdge.p1);

            count++;
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
//                var s = 0.5f - Random.Range(-0.4f, 0.4f);
//                var t = 0.5f - Random.Range(-0.4f, 0.4f);

                var s = 0.5f;
                var t = 0.5f;

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