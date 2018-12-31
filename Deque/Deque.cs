using System.Text;

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
        /// <summary>
        /// 初始化Deque类的新实例，该实例为空并且具有默认初始容量
        /// </summary>
        public Deque()
        {
            array = emptyArray;
        }

        /// <summary>
        ///  初始化Deque类的新实例，该实例为空并且具有指定的初始容量
        /// </summary>
        /// <param name="capacity">Deque可包含的初始元素数</param>
        /// <exception cref="ArgumentOutOfRangeException"> capacity小于0</exception>
        public Deque(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException("capacity", "The capacity must be greater than 0.");
            array = new T[capacity];
            head = 0;
            tail = 0;
            size = 0;
        }

        /// <summary>
        ///  初始化Deque类的新实例，该实例包含从指定集合复制的元素并且具有足够的容量来容纳所复制的元素。
        /// </summary>
        /// <param name="collection">其元素被复制到Deque中</param>
        /// <param name="enTail">是否从尾部添加，默认为true</param>
        /// <exception cref="ArgumentNullException">collection为null</exception>
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
        /// <summary>
        /// 获取Deque中包含的元素数。
        /// </summary>
        /// <returns>Deque中包含的元素数</returns>
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

        #region interface
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (array.Rank != 1)
            {
                throw new ArgumentException("array is not an array with rank 1");
            }

            if (array.GetLowerBound(0) != 0)
            {
                throw new ArgumentException("array must have lower bound zero");
            }

            if (index < 0 || index > array.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            int arrLen = array.Length;

            if (arrLen - index < size)
            {
                throw new ArgumentException("no enough space to store items");
            }

            int numToCopy = (arrLen - index < size) ? arrLen - index : size;

            if (numToCopy == 0)
            {
                return;
            }

            try
            {
                int firstPart = (this.array.Length - head < numToCopy) ? this.array.Length - head : numToCopy;
                Array.Copy(this.array, head, array, index, firstPart);
                numToCopy -= firstPart;
                if (numToCopy > 0)
                {
                    Array.Copy(this.array, 0, array, index + this.array.Length - head, numToCopy);
                    Array.Copy(this.array, 0, array, index + this.array.Length - head, numToCopy);
                }
            }

            catch (ArrayTypeMismatchException)
            {
                throw new ArgumentException("targetArray is not the right type");
            }
        }

        #endregion

        #region publicMethod
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
            head = 0;
            tail = 0;
            size = 0;
            version++;
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

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < array.Length; i++)
            {
                if(i == head)
                {
                    sb.AppendLine(array[i].ToString() + "  (head)");
                }
                else if (i == (tail - 1 + array.Length) % array.Length)
                {
                    sb.AppendLine(array[i].ToString() + "  (tail)");
                }
                else
                {
                    sb.AppendLine(array[i].ToString());
                }
            }
            return sb.ToString();
        }
        #endregion

        #region privateMethod
        private void SetCapacity(int capacity)
        {
            T[] newArray = new T[capacity];
            if (size > 0)
            {
                if (head < tail)
                {
                    Array.Copy(array, head, newArray, 0, size);
                }
                else
                {
                    Array.Copy(array, head, newArray, 0, array.Length - head);
                    Array.Copy(array, 0, newArray, array.Length - head, tail);
                }
            }
            array = newArray;
            head = 0;
            tail = size;
            version++;
        }

        internal T GetElement(int i)
        {
            return array[(head + i) % array.Length];
        }
        #endregion

        #region struct
        public struct Enumerator : IEnumerator<T>, IEnumerator
        {
            #region valid
            private Deque<T> deque;
            private int index;   //-1 not start   -2 ended/disposed
            private int version;
            private T currentElement;
            #endregion
            
            #region constructor
            public Enumerator(Deque<T> q)
            {
                deque = q;
                index = -1;
                version = deque.version;
                currentElement = default;
            }
            #endregion


            #region interface
            public T Current
            {
                get
                {
                    if(index < 0)
                    {
                        if(index == -1)
                        {
                            throw new InvalidOperationException("Enum not start");
                        }
                        else
                        {
                            throw new InvalidOperationException("Enum ended");
                        }
                    }
                    return currentElement;
                }
            }     

            object IEnumerator.Current
            {
                get
                {
                    if (index < 0)
                    {
                        if (index == -1)
                        {
                            throw new InvalidOperationException("Enum not start");
                        }
                        else
                        {
                            throw new InvalidOperationException("Enum ended");
                        }
                    }
                    return currentElement;
                }
            }

            public void Dispose()
            {
                index = -2;
                currentElement = default;
            }

            public bool MoveNext()
            {
                if(version != deque.version)
                {
                    throw new InvalidOperationException("deque fail version");
                }
                if(index == -2)
                {
                    return false;
                }
                index++;
                if(index == deque.size)
                {
                    index = -2;
                    currentElement = default;
                    return false;
                }
                currentElement = deque.GetElement(index);
                return true;
            }

            public void Reset()
            {
                if (version != deque.version)
                {
                    throw new InvalidOperationException("deque fail version");
                }
                index = -1;
                currentElement = default;
            }
            #endregion
        }
        #endregion
    }
}
