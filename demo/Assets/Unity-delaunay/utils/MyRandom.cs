using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ETModel
{
    public class MyRandom
    {

        public static uint seed = 12;
        private const uint max = 2147482648;
        private const uint mul = 69069;
        private const uint add = 0;


        public static uint resetRandomSeed(int s = -1)
        {
            if (s == -1)
            {
                seed = createRandomSeed();
            }
            else
            {
                seed = (uint) s;
            }

            return seed;
        }

        public static uint createRandomSeed()
        {
            return (uint) (Random.value * (max - 1) + 1);
        }
        
        public static float random()
        {
            seed = (seed * mul + add) & (max - 1);
            return (float) seed / max;
        }

        /// <summary>
        /// 包括a,不包括b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int Range(int a, int b)
        {
            var r = random();
            int v = a + (int) ((b - a) * r);
            return v;
        }

        /// <summary>
        /// 包括a,不包括b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Range(float a, float b)
        {
            var r = random();
            float v = a + (b - a) * r;
            return v;
        }
    }
}