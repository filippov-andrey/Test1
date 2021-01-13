using System;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;

namespace ConsoleApp3
{
    [RPlotExporter]
    public class Program
    {
        [Benchmark]
        public int SortAndXor()
        {
            var array = new[]
            {
                6, 1, 1, 4, 4, 3, 3, 5, 5, 6, 2, 2, 20, 20, 30, 30, 40, 40, 50, 50, 60, 60, 70, 70, 80, 80, 90, 90, 100,
                100, 110, 110, 120, 120, 130, 130, 140, 140, 150, 150, 160, 160, 170, 100500, 180, 180, 190, 190,
                200, 200, 300, 300, 400, 400, 500, 500, 600, 600, 700, 700, 800, 800, 900, 900, 1000, 1000, 170
            };

            return array.Aggregate(0, (current, element) => current ^ element);
        }

        [Benchmark]
        public int AvxSortXor()
        {
            var array = new[]
            {
                6, 1, 1, 4, 4, 3, 3, 5, 5, 6, 2, 2, 20, 20, 30, 30, 40, 40, 50, 50, 60, 60, 70, 70, 80, 80, 90, 90, 100,
                100, 110, 110, 120, 120, 130, 130, 140, 140, 150, 150, 160, 160, 170, 100500, 180, 180, 190, 190,
                200, 200, 300, 300, 400, 400, 500, 500, 600, 600, 700, 700, 800, 800, 900, 900, 1000, 1000, 170
            };

            return array.Aggregate(0, (current, element) => current ^ element);
        }

        static unsafe int ElementCount(int element, int[] source)
        {
            var vectorSize = 256 / 8 / 4;
            var mask = stackalloc int[vectorSize];

            for (var j = 0; j < vectorSize; j++)
            {
                mask[j] = element;
            }

            var vectorMask = System.Runtime.Intrinsics.X86.Avx.LoadVector256(mask);
            var accVector = Vector256<int>.Zero;

            int i;
            var array = source;

            fixed (int* ptr = array)
            {
                for (i = 0; i < array.Length - vectorSize; i += vectorSize)
                {
                    var v = System.Runtime.Intrinsics.X86.Avx.LoadVector256(ptr + i);
                    var areEqual = Avx2.CompareEqual(v, vectorMask);
                    accVector = Avx2.Subtract(accVector, areEqual);
                }
            }

            var result = 0;

            Avx.Store(mask, accVector);

            for (var j = 0; j < vectorSize; j++) result += mask[j];

            for (; i < array.Length; i++)
            {
                if (array[i] == element) result++;
            }

            return result;
        }

        [Benchmark]
        public int AvxCount()
        {

            var array = new[]
            {
                6, 1, 1, 4, 4, 3, 3, 5, 5, 6, 2, 2, 20, 20, 30, 30, 40, 40, 50, 50, 60, 60, 70, 70, 80, 80, 90, 90, 100,
                100, 110, 110, 120, 120, 130, 130, 140, 140, 150, 150, 160, 160, 170, 100500, 180, 180, 190, 190,
                200, 200, 300, 300, 400, 400, 500, 500, 600, 600, 700, 700, 800, 800, 900, 900, 1000, 1000, 170
            };

            return array.FirstOrDefault(element => ElementCount(element, array) is 1);
        }

        [Benchmark]
        public int AvxWithDistinct()
        {

            var array = new[]
            {
                6, 1, 1, 4, 4, 3, 3, 5, 5, 6, 2, 2, 20, 20, 30, 30, 40, 40, 50, 50, 60, 60, 70, 70, 80, 80, 90, 90, 100,
                100, 110, 110, 120, 120, 130, 130, 140, 140, 150, 150, 160, 160, 170, 100500, 180, 180, 190, 190,
                200, 200, 300, 300, 400, 400, 500, 500, 600, 600, 700, 700, 800, 800, 900, 900, 1000, 1000, 170
            };

            var distinct = array.Distinct();

            return distinct.FirstOrDefault(element => ElementCount(element, array) is 1);
        }

        private static void Main()
        {
            var p = new Program();
            Console.WriteLine(p.SortAndXor());
            Console.ReadKey();
        }
    }
}
