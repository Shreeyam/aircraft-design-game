using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Extensions
{
    public static class LinqExtensions
    {
        public static IEnumerable<float> Linspace(float min, float max, float n)
        {
            var step = ((max - min) / (n - 1));

            for (int i = 0; i < n + 1; i++)
            {
                yield return min + step * i;
                
                 
            }
        }

        public static IEnumerable<TResult> Zip<TA, TB, TResult>(
    this IEnumerable<TA> seqA, IEnumerable<TB> seqB, Func<TA, TB, TResult> func)
        {
            if (seqA == null) throw new ArgumentNullException("seqA");
            if (seqB == null) throw new ArgumentNullException("seqB");

            using (var iteratorA = seqA.GetEnumerator())
            using (var iteratorB = seqB.GetEnumerator())
            {
                while (iteratorA.MoveNext() && iteratorB.MoveNext())
                {
                    yield return func(iteratorA.Current, iteratorB.Current);
                }
            }
        }
    }
}
