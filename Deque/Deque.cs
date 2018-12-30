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
        /// <summary>
        /// 数组最小增大长度
        /// </summary>
        private const int minGrow = 4;
        private const int shrinkThreshold = 32;
        /// <summary>
        /// 增长速率
        /// </summary>
        private const int growFactor = 200;
        /// <summary>
        /// 默认初始大小
        /// </summary>
        private const int defaultCapacity = 4;        
        #endregion

        #region valid
        /// <summary>
        /// 储存数据的一维数组
        /// </summary>
        private T[] array;
        /// <summary>
        /// 数据的头位置，方括号中的数
        /// </summary>
        private int head = 0;    
        /// <summary>
        /// 数组的尾位置，方括号中的数加一
        /// </summary>
        private int tail = 0;       
        /// <summary>
        /// 储存的数据数量
        /// </summary>
        private int size = 0;    
        /// <summary>
        /// 修改次数
        /// </summary>
        private int version = 0;
        /// <summary>
        /// 多线程使用
        /// </summary>
        private object syncRoot;
        /// <summary>
        /// 空数组
        /// </summary>
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
            if(size == array.Length)
            {
                int newCapacity = (int)(array.Length * (long)growFactor / 100);
                if (newCapacity < array.Length + minGrow)
                    newCapacity = array.Length + minGrow;
                SetCapacity(newCapacity);
            }
            array[tail] = item;
            tail = (tail + 1) % array.Length;
            size++;
            version++;
        }

        public void EnHead(T item)
        {
            if (size == array.Length)
            {
                int newCapacity = (int)(array.Length * (long)growFactor / 100);
                if (newCapacity < array.Length + minGrow)
                    newCapacity = array.Length + minGrow;
                SetCapacity(newCapacity);
            }
            head = (head - 1 + array.Length) % array.Length;
            array[head] = item;
            size++;
            version++;
        }

        public T DeTail()
        {
            if(size == 0)
            {
                throw new InvalidOperationException("EmptyDeque");
            }
            int removeIndex = (tail + array.Length - 1) % array.Length;
            T remove = array[removeIndex];
            array[removeIndex] = default;
            tail = removeIndex;
            size--;
            version++;
            return remove;
        }

        public T DeHead()
        {
            if (size == 0)
            {
                throw new InvalidOperationException("EmptyDeque");
            }
            T remove = array[head];
            array[head] = default;
            head = (head + 1) % array.Length;
            size--;
            version++;
            return remove;
        }

        public void Clear()
        {
            if (head < tail)
            {
                Array.Clear(array, head, size);
            }
            else
            {
                Array.Clear(array, head, array.Length - head);
                Array.Clear(array, 0, tail);
            }
        }

        public bool Contains(T item)
        {
            int index = head;
            int count = size;
            EqualityComparer<T> c = EqualityComparer<T>.Default;
            while (count-- > 0)
            {
                if(item == null)
                {
                    if(array[index] == null)
                    {
                        return true;
                    }
                }
                else if(array[index] != null && c.Equals(array[index],item))
                {
                    return true;
                }
                index = (index + 1) % array.Length;
            }
            return false;
        }

        public void CopyTo(T[] targetArray, int arrayIndex)
        {
            if(targetArray == null)
            {
                throw new ArgumentNullException("targetArray");
            }
            if(arrayIndex < 0 || arrayIndex > targetArray.Length)
            {
                throw new ArgumentOutOfRangeException("arrayIndex");
            }
            //目标数组长度
            int arrLen = targetArray.Length;
            if(arrLen - arrayIndex < size)
            {
                throw new ArgumentException("don't have enough space");
            }
            //可以拷贝的数量
            int numToCopy = (arrLen - arrayIndex < size) ? (arrLen - arrayIndex) : size;
            if(numToCopy == 0)
            {
                return;
            }
            //从deque头到数组尾端的长度
            int firstPart = (array.Length - head < numToCopy) ? array.Length - head : numToCopy;
            Array.Copy(array, head, targetArray, arrayIndex, firstPart);
            numToCopy -= firstPart;
            if (numToCopy > 0)
            {
                Array.Copy(array, 0, targetArray, arrayIndex + array.Length - head, numToCopy);
            }
        }

        public T PeekTail()
        {
            if(size == 0)
            {
                throw new InvalidOperationException("EmptyDeque");
            }
            return array[(tail - 1 + array.Length) % array.Length];
        }

        public T PeekHead()
        {
            if (size == 0)
            {
                throw new InvalidOperationException("EmptyDeque");
            }
            return array[head];
        }

        public T[] ToArray()
        {
            T[] arr = new T[size];
            if(size == 0)
            {
                return arr;
            }
            if (head < tail)
            {
                Array.Copy(array, head, arr, 0, size);
            }
            else
            {
                Array.Copy(array, head, arr, 0, array.Length - head);
                Array.Copy(array, 0, arr, array.Length - head, tail);
            }
            return arr;
        }

        public void TrimExcess()
        {
            int threshold = (int)(array.Length * 0.9);
            if (size < threshold)
            {
                SetCapacity(size);
            }
        }

        #endregion

        #region privateMethodTODO
        private void SetCapacity(int capacity)
        {
            T[] newArray = new T[capacity];
            if(head<tail)
            {
                Array.Copy(array, head, newArray, 0, size);
            }
            else
            {
                Array.Copy(array, head, newArray, 0, array.Length - head);
                Array.Copy(array, 0, newArray, array.Length - head, tail);
            }
            array = newArray;
            head = 0;
            tail = size;
            version++;
        }
        #endregion

        #region structTODO
        #endregion

    }
}
