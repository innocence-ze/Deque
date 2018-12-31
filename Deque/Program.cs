using System;
using System.Collections.Generic;

namespace Deque
{
    class Program
    {
        static void Main(string[] args)
        {
            List<int> list = new List<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);
            Deque<int> deque = new Deque<int>(list);
            Console.WriteLine(100);
            Console.WriteLine(deque);

            deque.EnHead(0);
            Console.WriteLine(101);
            Console.WriteLine(deque);

            deque.EnTail(5);
            Console.WriteLine(102);
            Console.WriteLine(deque);

            deque.DeHead();
            Console.WriteLine(103);
            deque.TrimExcess();
            Console.WriteLine(deque);

            deque.DeTail();
            Console.WriteLine(104);
            Console.WriteLine(deque);
            
            Console.WriteLine(105);
            Console.WriteLine(deque.PeekHead());
            
            Console.WriteLine(106);
            Console.WriteLine(deque.PeekTail());

            Console.WriteLine(107);
            foreach (var v in deque)
            {
                Console.WriteLine(v);
            }

            Console.WriteLine(108);
            deque.Clear();
            deque.TrimExcess();
            Console.WriteLine(deque);

            deque.EnTail(3);
            deque.EnTail(4);
            deque.EnHead(2);
            deque.EnHead(1);
            Console.WriteLine(109);
            Console.WriteLine(deque);
            Console.ReadKey();
        }
    }
}
