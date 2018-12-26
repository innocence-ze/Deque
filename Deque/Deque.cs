using System;
using System.Text;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Permissions;

namespace System.Collections.Generic
{
    class Deque<T> : ICollection,IEnumerable<T>
    {
        #region const
        private const int minGrow = 4;
        private const int shrinkThreshold = 32;
        private const int growFactor = 200;
        private const int defaultCapacity = 4;        
        #endregion

        #region valid
        private T[] array;
        private int head = 0;       // First valid element in the deque
        private int tail = 0;       // Last valid element in the deque
        private int size = 0;       // Number of elements.
        private int version = 0;
        private object syncRoot;
        private static readonly T[] emptyArray = new T[0];
        #endregion

        #region constructor
        public Deque()
        {
            array = emptyArray;
        }

        public Deque(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException("capacity", "The capacity must be greater than 0.");
            array = new T[capacity];
            head = 0;
            tail = 0;
            size = 0;
        }

        public Deque(IEnumerable<T> collection,bool enTail = true)
        {
            if (collection == null)
                throw new ArgumentNullException("collection is null");
            array = new T[defaultCapacity];
            size = 0;
            version = 0;
            if(enTail)
            {
                using (IEnumerator<T> ie = collection.GetEnumerator())
                {
                    while (ie.MoveNext())
                    {
                        EnTail(ie.Current);
                    }
                }
            }
            else
            {
                using (IEnumerator<T> ie = collection.GetEnumerator())
                {
                    while (ie.MoveNext())
                    {
                        EnHead(ie.Current);
                    }
                }
            }
        }
        #endregion

        #region property
        public int Count
        {
            get
            {
                return size;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                if (syncRoot == null)
                {
                    Threading.Interlocked.CompareExchange<object>(ref syncRoot, new object(), null);
                }
                return syncRoot;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }
        #endregion

        #region interfaceTODO
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region publicMethodTODO
        public void EnTail(T item)
        {

        }

        public void EnHead(T item)
        {

        }

        public T DeTail()
        {
            return default;
        }

        public T DeHead()
        {
            return default;
        }

        public void Clear()
        {

        }

        public bool Contains(T item)
        {
            return default;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {

        }

        public T PeekTail()
        {
            return default;
        }

        public T PeekHead()
        {
            return default;
        }

        public T[] ToArray()
        {
            return default;
        }

        public void TrimExcess()
        {
            
        }

        #endregion

        #region privateMethodTODO
        #endregion

        #region structTODO
        #endregion

    }
}
