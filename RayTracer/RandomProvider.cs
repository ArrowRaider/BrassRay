using System;
using System.Threading;

namespace BrassRay.RayTracer
{
    public static class RandomProvider
    {
        [field: ThreadStatic]
        public static Random Random { get; private set; }

        public static Random InitRandom()
        {
            if (Random != null) return Random;
            var seed = DateTime.Now.Ticks + Thread.CurrentThread.ManagedThreadId * 13;
            return Random = new Random((int)seed);
        }
    }
}
