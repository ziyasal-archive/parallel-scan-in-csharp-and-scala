using System;
using System.Threading.Tasks;

namespace ParallelScan
{
    class TreeResult<T>
    {
        public T Result { get; set; }
    }

    class Leaf<T> :TreeResult<T>
    {
        public int From { get; }
        public int To { get; }

        public Leaf(int @from, int to, T result)
        {
            From = @from;
            To = to;
            Result = result;
        }

    }

    class Node<T> : TreeResult<T>
    {
        public TreeResult<T> Left { get; }
        public TreeResult<T> Right { get; }

        public Node(TreeResult<T> left, T result, TreeResult<T> right)
        {
            Left = left;
            Right = right;
            Result = result;
        }
    }

    internal class Program
    {
        private static int threshold = 2;

        static async Task<Tuple<T, T>> Parallel<T>(Task<T> t1, Task<T> t2)
        {
            T[] result = await Task.WhenAll(t1, t2);

            return new Tuple<T,T>(result[0], result[1]);
        }

        static async Task Parallel(Task t1, Task t2)
        {
            await Task.WhenAll(t1, t2);
        }


        static T ReduceSegment<T>(T[] input, int left, int right, T t0, Func<T,T,T> f)
        {
            T t = t0;
            var i = left;

            while (i<right)
            {
                t = f(t, input[i]);
                i++;
            }

            return t;
        }

        static void ScanLeftSequential<T>(T[] input, int left, int right, T t0, Func<T, T, T> f, T[] output)
        {
            if (left < right)
            {
                var i = left;
                var t = t0;

                while (i<right)
                {
                    t = f(t, input[i]);
                    i++;
                    output[i] = t;
                }
            }
        }

        static TreeResult<T> Upsweep<T>(T[] input, int from, int to, Func<T, T, T> f)
        {
            if (to - from < threshold)
            {
                return new Leaf<T>(from, to,
                    ReduceSegment(input, from + 1, to, input[from], f));
            }

            var mid = @from + (to - @from) / 2;
            Tuple<TreeResult<T>, TreeResult<T>> result = Parallel(
                    Task.Run(() => Upsweep(input, @from, mid, f)),
                    Task.Run(() => Upsweep(input, mid, to, f))
                )
                .Result;

            return new Node<T>(result.Item1 /*treeLeft*/,
                f(result.Item1.Result, result.Item2.Result),
                result.Item2 /*treeRight*/);
        }


        static void Downsweep<T>(T[] input, T t0, Func<T, T, T> f, TreeResult<T> t, T[] output)
        {
            var leaf = t as Leaf<T>;
            if (leaf != null)
            {
                ScanLeftSequential(input, leaf.From, leaf.To,t0,f,output);
            }
            else if(t is Node<T>)
            {
                var node = (Node<T>) t;

                var _ = Parallel(
                        Task.Run(() => Downsweep(input,t0,f,node.Left,output)),
                        Task.Run(() => Downsweep(input,f(t0,node.Left.Result),f,node.Right, output))
                    );
            }
        }

        static void ScanLeft<T>(T[] input, T t0, Func<T, T, T> f, T[] output)
        {
            var t = Upsweep(input, 0, input.Length, f);

            Downsweep(input,t0,f,t,output);

            //prepend
            output[0] = t0;
        }

        public static void Main(string[] args)
        {
            int[] input = {1, 3, 8};
            int[] output = {0, 0, 0, 0};
            Func<int, int, int> f = (x, s) => x + s;

            ScanLeft(input, 100, f, output);

            foreach (var i in output)
            {
                Console.WriteLine(i);
            }
        }
    }
}