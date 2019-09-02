using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication5
{
    class Program
    {
        public const int Repetitions = 10;
        public static long ArraySize = 10000000;

        public static int[] A;
        public static int[] B;
        public static int[] C;

       static  List<Tuple<string, Action>> Methods = new List<Tuple<string, Action>>();

        static void Main(string[] args)
        {


            string aFileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().FullName), "eval.csv");

            File.Delete(aFileName);


            Methods.Add(new Tuple<string, Action>("Plain Add 2", PlainAdd2));
            Methods.Add(new Tuple<string, Action>("Plain Add", PlainAdd));
            Methods.Add(new Tuple<string, Action>("Plain Add with const length" , PlainAddWidthConstLength));
            Methods.Add(new Tuple<string, Action>("Plain Add unsafe", PlainAddUnsafe));
            Methods.Add(new Tuple<string, Action>("Plain Add unsafe const Length", PlainAddUnsafeConstLength));
            Methods.Add(new Tuple<string, Action>("Plain Add unsafe const Length (Index)", PlainAddUnsafeConstLengthIndexed));
            Methods.Add(new Tuple<string, Action>("Plain Add unsafe const Length with block", PlainAddUnsafeConstLengthWithBlock));
            Methods.Add(new Tuple<string, Action>("Plain Add unsafe const Length with block index", PlainAddUnsafeConstLengthWithBlockIndexed));
            //     Methods.Add(new Tuple<string, Action>("Plain Add Par For", PlainParallelFor));
            Methods.Add(new Tuple<string, Action>("Plain Add ParFor (Partitioner)", PlainParallelForPartitioner));
            Methods.Add(new Tuple<string, Action>("Plain Add ParFor (Partitioner, Unsafe)", PlainParallelForPartitionerUnsafe));
          //  Methods.Add(new Tuple<string, Action>("System.Numerics.Vector<>", Add_Vector));



            List<long> ArraySizes = new List<long>();
            for (int i = 10; i < 10000000; i+=1000000)
            {
                ArraySizes.Add(i);
            }

        
            File.AppendAllLines(aFileName, new string[] { ";" + string.Join(";", ArraySizes) });




            foreach (var aMethod in Methods)
            {
                List<string> aValues = new List<string>();

                foreach(var arsz in ArraySizes)
                {
                    ArraySize = arsz;

                    A = new int[ArraySize];
                    B = new int[ArraySize];
                    C = new int[ArraySize];

                    for (int i = 0; i < A.Length; i++)
                    {
                        A[i] = i;
                        B[i] = i;
                        C[i] = i;
                    }




                    long aElapsed = Run(aMethod.Item1, aMethod.Item2);
                    aValues.Add(aElapsed.ToString());
                }

                File.AppendAllLines(aFileName, new string[] {aMethod.Item1+ ";" + string.Join(";", aValues) });
            }

            Console.ReadKey();
        }

        public static void PlainAdd2()
        {
            for (int i = 0; i < A.Length; i++)
            {
                C[i] = Math.Add<int>(A[i], B[i]);
            }
        }

        public static void PlainAdd()
        {
            for (int i = 0; i < A.Length; i++)
            {
                C[i] = A[i] + B[i];
            }
        }

        public static void PlainAddWidthConstLength()
        {
            int aLength = A.Length;

            for (int i = 0; i < aLength; i++)
            {
                C[i] = A[i] + B[i];
            }
        }


        public static void PlainAddUnsafe()
        {
            unsafe
            {          
                fixed (int* apinned = A, bpinned = B, cpinned = C)
                {
                    int* a = apinned;
                    int* b = bpinned;
                    int* c = cpinned;

                    for (int i = 0; i < A.Length; i++, a++, b++, c++)
                    {
                        *c = *a + *b;
                    }
                }
            }
        }

        public static void PlainAddUnsafeConstLength()
        {
            unsafe
            {
                int aLength = A.Length;
              
                fixed (int* apinned = A, bpinned = B, cpinned = C)
                {
                    int* a = apinned;
                    int* b = bpinned;
                    int* c = cpinned;

                    for (int i = 0; i < aLength; i++, a++, b++, c++)
                    {
                        *c = *a + *b;
                    }
                }
            }
        }

        public static void PlainAddUnsafeConstLengthIndexed()
        {
            unsafe
            {
                int aLength = A.Length;

                fixed (int* apinned = A, bpinned = B, cpinned = C)
                {
                    int* a = apinned;
                    int* b = bpinned;
                    int* c = cpinned;

                    for (int i = 0; i < aLength; i++)
                    {
                        c [i]= a[i] + b[i];
                    }
                }
            }
        }

        public static void PlainAddUnsafeConstLengthWithBlock()
        {
            unsafe
            {
                int aLength = A.Length;
                int nBlock = aLength / 10;
                int nRest = aLength % 10;


                fixed (int *apinned = A, bpinned=B, cpinned = C)
                {
                    int* a = apinned;
                    int* b = bpinned;
                    int* c = cpinned;

                    for (int i = 0; i < nBlock; i++, a+=10, b+=10, c+=10)
                    {
                        *(c + 0) = *(a + 0) + *(b + 0);
                        *(c + 1) = *(a + 1) + *(b + 1);
                        *(c + 2) = *(a + 2) + *(b + 2);
                        *(c + 3) = *(a + 3) + *(b + 3);
                        *(c + 4) = *(a + 4) + *(b + 4);
                        *(c + 5) = *(a + 5) + *(b + 5);
                        *(c + 6) = *(a + 6) + *(b + 6);
                        *(c + 7) = *(a + 7) + *(b + 7);
                        *(c + 8) = *(a + 8) + *(b + 8);
                        *(c + 9) = *(a + 9) + *(b + 9);
                    }


                    for (int i = 0; i < nRest; i++, a++, b++, c++)
                    {
                        *c = *a + *b;
                    }
                }
            }
        }


        public static void PlainAddUnsafeConstLengthWithBlockIndexed()
        {
            unsafe
            {
                int aLength = A.Length;
                int nBlock = aLength / 10;
                int nRest = aLength % 10;


                fixed (int* apinned = A, bpinned = B, cpinned = C)
                {
                    int* a = apinned;
                    int* b = bpinned;
                    int* c = cpinned;

                    for (int i = 0; i < nBlock; i++, a += 10, b += 10, c += 10)
                    {
                        c[0] = a[0] + b[0];
                        c[1] = a[1] + b[1];
                        c[2] = a[2] + b[2];
                        c[3] = a[3] + b[3];
                        c[4] = a[4] + b[4];
                        c[5] = a[5] + b[5];
                        c[6] = a[6] + b[6];
                        c[7] = a[7] + b[7];
                        c[8] = a[8] + b[8];
                        c[9] = a[9] + b[9];
                    }


                    for (int i = 0; i < nRest; i++, a++, b++, c++)
                    {
                        *c = *a + *b;
                    }
                }
            }
        }

        public static void PlainParallelFor()
        {
            unsafe
            {
                int aLength = A.Length;
                int nBlock = aLength / 10;
                int nRest = aLength % 10;

                Parallel.For(0, A.Length, i =>
                {
                    C[i] = A[i] + B[i];
                });
            }
        }

        public static void PlainParallelForPartitioner()
        {

            var aPart = Partitioner.Create(0, A.Length);

            Parallel.ForEach(aPart, x =>
             {
                 for (int i = x.Item1; i < x.Item2; i++)
                 {
                     C[i] = A[i] + B[i];
                 }
             });
        }

        public static void PlainParallelForPartitionerUnsafe()
        {

            var aPart = Partitioner.Create(0, A.Length);

            Parallel.ForEach(aPart, x =>
             {
                     unsafe
                 {
                     int aLength = A.Length;

                     fixed (int* apinned = A, bpinned = B, cpinned = C)
                     {

                         for (int i = x.Item1; i < x.Item2; i++)
                         {
                             cpinned[i] = apinned[i] + bpinned[i];
                         }
                     }
                 }
             });
        }

       

        static long Run(string type_in, Action action)
        {
            Console.Write("{0:30} ({1}): ", type_in, ArraySize);

            Stopwatch w = Stopwatch.StartNew();

            
            for (int i = 0; i < Repetitions; i++)
            {
                Array.Clear(C, 0, C.Length);
                action();
            }



            w.Stop();



            long aElapsedMilliseconds = w.ElapsedMilliseconds / Repetitions;

            Console.WriteLine("{0}ms", aElapsedMilliseconds);

            for (int i = 0; i < A.Length; i++)
            {
                int c = A[i] + B[i];

                if (c != C[i])
                {
                    Console.WriteLine("   Error ");
                    break;
                }

            }

            return aElapsedMilliseconds;
        }
    }
}
