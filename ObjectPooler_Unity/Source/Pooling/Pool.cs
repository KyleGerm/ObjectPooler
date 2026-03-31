using KylesUnityLib.Factory;
using KylesUnityLib.Internal.Pooling;
using System;
using System.Runtime.CompilerServices;


namespace KylesUnityLib.Pooling
{
    /// <summary>
    /// Base Pool. Can hold any class object
    /// </summary>
    /// <typeparam name="T">Type of object to be pooled</typeparam>
    public  class Pool<T> where T : class ,IInjectable<T>
    {
        private bool _active;
        private PoolIdentifier<T>[] _pool;
        private int _poolCount;
        private readonly ulong[] _objMask;
        private ulong _chunkMask;
        /// <summary>
        /// The max size of the pool set at creation.<br/>
        /// Can not be changed.
        /// </summary>
        public readonly int MaxSize;
        /// <summary>
        /// The number of objects in the pool
        /// </summary>
        public int SizeOfPool => _poolCount;
        /// <summary>
        /// Determines if the pool is in a usable state
        /// </summary>
        public bool Active => _active;
        private readonly IFactory<T> _factory;
        public IFactory<T> Factory => _factory;
       
        /// <summary>
        /// Creates an uninitialized Pool object<br/>
        /// <see cref="GenerateList(int)"/> Must be called before the pool will be usable and active
        /// </summary>
        /// <param name="maxSize">The max size of the pool. Cannot be changed after creation</param>
        /// <param name="factory">Is used to create pooled objects</param>
        /// <exception cref="ArgumentNullException"></exception>
        public Pool(int maxSize, IFactory<T> factory)
        {
            CheckMaxSizeArgument(maxSize, nameof(maxSize));
            if (factory == null) throw new ArgumentNullException(typeof(Factory<T>).FullName, "Factory is null. Define a valid factory and try again");
            factory.ValidateFactory();
            MaxSize = maxSize;
            _objMask = new ulong[(MaxSize + 63) / 64];
            _factory = factory;
            _pool = null!;
        }
        internal Pool()
        {
            _active = false;
            _factory = null!;
            _pool = null!;
            _objMask = null!;
        }

        /// <summary>
        /// Creates a new Pool, generates a pool of size, and returns the object
        /// </summary>
        /// <param name="factory">Object which defines how to create and destroy the pooled objects</param>
        /// <param name="size">Inital pool size to generate</param>
        /// <param name="maxSize">Maximum size of this pool. Cannot be changed after creation</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Ensures factory is not null</exception>
        /// <exception cref="PoolInitializationException">Throws if pool did not initialize as expected</exception>
        public static Pool<T> Create(IFactory<T> factory, int size, int maxSize)
        {
            #region Validation
            if (factory == null) throw new ArgumentNullException(typeof(IFactory<T>).FullName, "Factory is null. Define a valid factory and try again");
            factory.ValidateFactory();
            CheckMaxSizeArgument(maxSize,nameof(maxSize));
            #endregion

            Pool<T> pool = new Pool<T>(maxSize,factory);

            try
            {
                pool.GenerateList(size);
                return pool;
            }
            catch (Exception ex)
            {
                throw new PoolInitializationException("Pooler was unable to initialize. Check parameters and try again.",ex);
            }
        }
        /// <summary>
        /// Generates a pool with a given size.<br/>
        /// Cannot be used if pool is already active.<br/>
        /// Current pool must be removed or destroyed before creating a new one.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        /// <exception cref="ActivePoolOverwriteException"></exception>
        public bool GenerateList(int size)
        {
            if (_active)
            {
                throw new ActivePoolOverwriteException("Pooler is already in use, and has objects. Either destroy the pool using DestroyList(), or take ownership of the objects using RemoveListButDontDestroy()");
            }
            size = Math.Max(0, size);
            size = Math.Min(size, MaxSize);
            _pool = new PoolIdentifier<T>[(int)((size + 63) / 64) * 64];
            _poolCount = 0;
            _chunkMask = 0;
            Array.Clear(_objMask, 0, _objMask.Length);
            for (int i = 0; i < size; i++)
            {
                CreateNewPooledObject(false, out _);
            }
            _active = _poolCount > 0;

            if (!_active) return false;

            for (int i = 0; i < _objMask.Length; i++)
            {
                if (_objMask[i] != 0)
                {
                    _chunkMask |= 1UL << i;
                }
            }
            return true;
        }

        
        /// <summary>
        /// Returns a single available IPoolable
        /// </summary>
        /// <param name="resizable">Can a new GameObject be created if none can be found?</param>
        /// <returns>Available IPoolable, or null if none can be found</returns>
        public IPoolable<T> GetObject(bool resizable = false)
        {
            if (!_active) throw new PoolIsInactiveException("Pool is inactive or not in a usable state. Check Pool Setup before calling GetObject");

            if (_chunkMask != 0 && GetNextAvailable(DeBruijn.TrailingZeroCount(_chunkMask)) is IPoolable<T> obj)
                return obj;

            if (resizable && CreateNewPooledObject(true, out IPoolable<T> newObj))
                return newObj;

            throw new PoolExhaustedException("Pool is exhausted and resizing is not allowed. Ensure pool size is suitable for object request frequency, or that resizing is enabled.");
        }

        /// <summary>
        /// Attempts to return a single IPoolable
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="resizeable"></param>
        /// <returns>False if pool is exhausted, or the pool is inactive<br/>
        /// True if an object is found</returns>
        public bool TryGetObject(out IPoolable<T> obj, bool resizeable = false)
        {
            if (!_active)
            {
                obj = null!;
                return false;
            }

            try
            {
                obj = GetObject(resizeable);
                return true;
            }
            catch(PoolExhaustedException) {
                obj = null!;
                return false;
            }
        }

        /// <summary>
        /// Returns an array of IPoolable.
        /// If the specified amount cannot be created, returns null.
        /// 
        /// <para>⚠️ This method allocates memory. Prefer <see cref="RequestMultiple(bool, ref IPoolable{T}[])"/> for zero-allocation usage.</para>
        ///</summary>
        /// <param name="amount">Number of IPoolable to return</param>
        /// <param name="resizable">Can more objects be made to return the amount specified?</param>
        /// <returns></returns>
        public IPoolable<T>[] RequestMultiple(int amount, bool resizable)
        {
            IPoolable<T>[] list = new IPoolable<T>[amount];

            if (!_active) throw new PoolIsInactiveException("Pool is inactive or not in a usable state. Check Pool Setup before calling RequestMultiple");

            if(RequestMultiple(resizable,ref list))
            {
                return list;
            }

            throw new PoolExhaustedException("Pool is exhausted and resizing is not allowed. Ensure pool size is suitable for object request frequency, or that resizing is enabled.");
        }

        /// <summary>
        /// <para>Fills the array with IPoolable objects. Returns true if the array was successfully populated. Zero Allocation</para>
        /// <para>The array will still be partially populated even if false is returned. 
        /// Null checks should be used if false is returned, and the array was empty when passed in</para>
        /// </summary>
        /// <param name="resizable"><para>Can more objects be made to return the amount specified?</para>
        /// <para>Allocation will happen if true, and new instances are created</para></param>
        /// <param name="buffer">The buffer given to populate</param>
        /// <returns></returns>
        public bool RequestMultiple(bool resizable, ref IPoolable<T>[] buffer)
        {
            int amount = buffer.Length;
            int populatedSlots = 0;
            if (_active)
            {
                populatedSlots = PopulateBuffer(buffer);

                if (populatedSlots < amount && resizable)
                {
                    while (populatedSlots < amount && _poolCount < MaxSize && CreateNewPooledObject(true, out IPoolable<T> obj))
                    {
                        buffer[populatedSlots++] = obj;
                    }
                }
            }

            return populatedSlots == amount;
        }

        private int PopulateBuffer(Span<IPoolable<T>> span)
        {
            int count = 0;
            int amount = span.Length;
            if (_chunkMask == 0) return count;
            int availableChunk = DeBruijn.TrailingZeroCount(_chunkMask);

            for (; count < amount; count++)
            {
                if (_objMask[availableChunk] == 0)
                {
                    if (_chunkMask == 0) break;

                    availableChunk = DeBruijn.TrailingZeroCount(_chunkMask);
                }

                span[count] = GetNextAvailable(availableChunk);
            }
            return count;
        }

        private IPoolable<T> GetNextAvailable(int availableChunk)
        {
            if (_objMask[availableChunk] == 0) return null!;

            int chunkIndex = DeBruijn.TrailingZeroCount(_objMask[availableChunk]);

            ulong bitMask = 1UL << chunkIndex;
            _objMask[availableChunk] &= ~bitMask;

            if (_objMask[availableChunk] == 0)
            {
                _chunkMask &= ~(1UL << availableChunk);
            }
            int objectIndex = (availableChunk * 64) + chunkIndex;
            _pool[objectIndex].notifyPool += ReturnToPool;
            return _pool[objectIndex];
        }

        /// <summary>
        /// Returns all objects to the pool.
        /// </summary>
        public void ReturnAll()
        {
            if (!_active) { return; }

            for (int i = 0; i < _poolCount; i++)
            {
                _pool[i].ReturnToPool();
            }
        }
        private bool CreateNewPooledObject(bool setActive, out IPoolable<T> newIdent)
        {
            T obj = _factory.CreateNewObject();
            if (obj == null) 
            {
                throw new NullObjectConstructionException($"{_factory.GetType().Name} returned null when creating {typeof(T).Name}.\nInspect and fix the construction method supplied.");
            }
            PoolIdentifier<T> identifier = new PoolIdentifier<T>(obj);
            if (AddToPool(identifier, out int index))
            {
                //divide by 64
                int chunkIndex = index >> 6;
                //Fast Mod bitshift
                ulong bitMask = (1UL << (index & 63));
                identifier.SetIdentifier(chunkIndex, bitMask);
                newIdent = identifier;
                if (setActive)
                {
                    _objMask[chunkIndex] &= ~bitMask;
                }
                else
                {
                    _objMask[chunkIndex] |= bitMask;
                    _chunkMask |= 1UL << chunkIndex;
                }
                return true;
            }

            identifier.Dispose();
            newIdent = null!;
            return false;
        }
        private bool AddToPool(PoolIdentifier<T> identifier, out int index)
        {
            index = _poolCount;
            if (_poolCount >= MaxSize)
                return false;

            if (_poolCount >= _pool.Length)
            {
                int newSize = Math.Min(_pool.Length + 64, MaxSize);
                Array.Resize(ref _pool, newSize);
            }
            
            _pool[_poolCount++] = identifier;
            return true;
        }
        internal void ReturnToPool(PoolIdentifier<T> identifier)
        {
            _objMask[identifier.ChunkIndex] |= identifier.BitMask;
            _chunkMask |= (1UL << identifier.ChunkIndex);
        }
        /// <summary>
        /// Resize the pool to a new size. Can increase or decrease in size.<br/>
        /// Pool will be destroyed if newSize is 0 or less.<br/>
        /// <para>⚠️ Performance Warning:<br/>
        /// Increasing the pool size will instantiate new objects, which is expensive and 
        /// can cause frame spikes if done during gameplay.<br/>Resize should be 
        /// performed during warmup, loading screens, or spread over multiple frames if large growth 
        /// is required.</para>
        /// </summary>
        /// <param name="newSize">New size of pool. New Size will be clamped to Max Size given on creation</param>
        public void Resize(int newSize)
        {
            if (newSize > MaxSize) newSize = MaxSize;
            int sizeDiff = newSize - SizeOfPool;
            if (!_active) throw new PoolIsInactiveException("Pool cannot be resized from an inactive state. You must call GenerateList to activate the pool, or create an active Pool<T> object with Pool<T>.Create");
                
            if(sizeDiff == 0) return;

            if (sizeDiff < 0)
            {
                Reduce(sizeDiff * -1);
                return;
            }

            for (int i = 0; i < sizeDiff; i++)
            {
                CreateNewPooledObject(false, out _);
            }
        }
        /// <summary>
        /// Reduce pool by an amount<br/>
        /// Pool will be destroyed if amount is larger than the size of the pool<br/>
        /// It is possible for references held to objects to be null after reducing, depending on how the object is disposed
        /// </summary>
        /// <param name="amount"></param>
        public void Reduce(int amount)
        {
            if (amount <= 0) return;
            if (amount >= _poolCount)
            {
                DestroyList();
                return;
            }

            int newCapacity = SizeOfPool - amount;
            for (int i = SizeOfPool - 1; i >= newCapacity; i--)
            {
                _objMask[_pool[i].ChunkIndex] &= ~_pool[i].BitMask;
                if (_objMask[_pool[i].ChunkIndex] == 0)
                {
                    _chunkMask &= ~(1UL << _pool[i].ChunkIndex);
                }
                _factory.DisposeObject(_pool[i].Entity);
                _pool[i].Dispose();
                _pool[i] = null!;
                _poolCount--;
            }
        }

        /// <summary>
        /// Destroys all GameObjects in the list<br/>
        /// Pool will be set to inactive until a new pool is generated
        /// </summary>
        /// <param name="action"></param>
        public void DestroyList(Action<T> action = null!)
        {
            if (_active && _poolCount > 0)
            {
                _active = false;
                foreach (var obj in _pool)
                {
                    if (obj == null) continue;
                    action?.Invoke(obj.Entity);
                    _factory.DisposeObject(obj.Entity);
                    obj.Dispose(); 
                }
                for (int i = 0; i < _objMask.Length; i++) _objMask[i] = 0;
                _chunkMask = 0;
                _poolCount = 0;
                _pool = null!;
            }
        }

        /// <summary>
        /// Checks all elements in the pool are valid and replaces null values
        /// </summary>
        public void Validate()
        {
            if (!_active) return;
            if (SizeOfPool == 0)
            {
                _active = false;
            }
            for (int i = 0; i < SizeOfPool; i++)
            {
                if (_pool[i] == null || _pool[i].Entity == null)
                {
                    InsertNewPooledObjectAt(i);
                }
            }
        }
        private void InsertNewPooledObjectAt(int index)
        {
            
            T obj = _factory.CreateNewObject();
            PoolIdentifier<T> identifier = new PoolIdentifier<T>(obj);
            //divide by 64
            int chunkIndex = index >> 6;
            //Fast Mod bitshift
            ulong bitMask = (1UL << (index & 63));
            identifier.SetIdentifier(chunkIndex, bitMask);

            _objMask[chunkIndex] |= bitMask;
            _chunkMask |= 1UL << chunkIndex;

            _pool[index] = identifier;
        }

        /// <summary>
        /// Shuts down the pool, and hands back the objects
        /// </summary>
        /// <returns></returns>
        public T[] RemoveListButDontDestroy()
        {
            if (_pool == null || _poolCount == 0) return Array.Empty<T>();
            T[] entities = new T[_poolCount];
            for (int i = 0; i < _poolCount; i++)
            {
                entities[i] = _pool[i].Entity;
                _pool[i].Dispose();
                _pool[i] = null!;
            }
            _poolCount = 0;
            _active = false;
            for (int i = 0; i < _objMask.Length; i++) _objMask[i] = 0;
            _chunkMask = 0;
            return entities;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckMaxSizeArgument(int value, string valueName)
        {
            if (value <= 0 || value > 4096)
            {
                throw new ArgumentOutOfRangeException(valueName, "Max Size of the pool must be a value more than zero and less than 4096.");
            }
        }
    }
}
