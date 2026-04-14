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
    public  class Pool<T> where T : class ,IPoolable<T>
    {
        private bool _disposed;
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
            if (_active || _disposed)
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
        /// Returns a single available IPooledObject
        /// </summary>
        /// <param name="resizeable">Can a new GameObject be created if none can be found?</param>
        /// <returns>Available IPooledObject, or null if none can be found</returns>
        public IPooledObject<T> GetObject(bool resizeable = false)
        {
            if (!_active) throw new PoolIsInactiveException("Pool is inactive or not in a usable state. Check Pool Setup before calling GetObject");
            int nextChunk = DeBruijn.TrailingZeroCount(_chunkMask);
            if (_chunkMask != 0 && GetNextAvailable(ref nextChunk) is IPooledObject<T> obj)
                return obj;

            if (resizeable && CreateNewPooledObject(true, out IPooledObject<T> newObj))
                return newObj;

            throw new PoolExhaustedException("Pool is exhausted and resizing is not allowed. Ensure pool size is suitable for object request frequency, or that resizing is enabled.");
        }

        /// <summary>
        /// Attempts to return a single IPooledObject
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="resizeable"></param>
        /// <returns>False if pool is exhausted, or the pool is inactive<br/>
        /// True if an object is found</returns>
        public bool TryGetObject(out IPooledObject<T> obj, bool resizeable = false)
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
        /// Returns an array of IPooledObject.
        /// If the specified amount cannot be created, returns null.
        /// 
        /// <para>⚠️ This method allocates memory. Prefer <see cref="RequestMultiple(bool, ref IPooledObject{T}[])"/> for zero-allocation usage.</para>
        ///</summary>
        /// <param name="amount">Number of IPooledObject to return</param>
        /// <param name="resizable">Can more objects be made to return the amount specified?</param>
        /// <returns></returns>
        public IPooledObject<T>[] RequestMultiple(int amount, bool resizable = false)
        {
            IPooledObject<T>[] list = new IPooledObject<T>[amount];

            if (!_active) throw new PoolIsInactiveException("Pool is inactive or not in a usable state. Check Pool Setup before calling RequestMultiple");

            if(RequestMultiple(ref list, resizable))
            {
                return list;
            }

            throw new PoolExhaustedException("Pool is exhausted and resizing is not allowed. Ensure pool size is suitable for object request frequency, or that resizing is enabled.");
        }

        /// <summary>
        /// <para>Fills the array with IPooledObject objects. Returns true if the array was successfully populated. Zero Allocation</para>
        /// <para>The array will still be partially populated even if false is returned. 
        /// Null checks should be used if false is returned, and the array was empty when passed in</para>
        /// </summary>
        /// <param name="resizable"><para>Can more objects be made to return the amount specified?</para>
        /// <para>Allocation will happen if true, and new instances are created</para></param>
        /// <param name="buffer">The buffer given to populate</param>
        /// <returns></returns>
        public bool RequestMultiple(ref IPooledObject<T>[] buffer, bool resizable = false)
        {
            int amount = buffer.Length;
            int populatedSlots = 0;
            if (_active)
            {
                populatedSlots = PopulateBuffer(ref buffer);

                if (populatedSlots < amount && resizable)
                {
                    while (populatedSlots < amount && _poolCount < MaxSize && CreateNewPooledObject(true, out IPooledObject<T> obj))
                    {
                        buffer[populatedSlots++] = obj;
                    }
                }
            }

            return populatedSlots == amount;
        }
        private int PopulateBuffer(ref IPooledObject<T>[] span)
        {
            int count = 0;
            int amount = span.Length;
           

            while (_chunkMask != 0 && count < amount)
            {
                int availableChunk = DeBruijn.TrailingZeroCount(_chunkMask);
                ref ulong chunk = ref _objMask[availableChunk];
                int baseIdx = availableChunk << 6;
                PoolIdentifier<T>[] pool = _pool;
                while (chunk != 0 && count + 4 <= amount)
                {
                    int i0 = DeBruijn.TrailingZeroCount(chunk);
                    chunk &= chunk - 1;
                    span[count++] = pool[baseIdx + i0];

                    if (chunk == 0) break;

                    int i1 = DeBruijn.TrailingZeroCount(chunk);
                    chunk &= chunk - 1;
                    span[count++] = pool[baseIdx + i1];

                    if (chunk == 0) break;

                    int i2 = DeBruijn.TrailingZeroCount(chunk);
                    chunk &= chunk - 1;
                    span[count++] = pool[baseIdx + i2];

                    if (chunk == 0) break;

                    int i3 = DeBruijn.TrailingZeroCount(chunk);
                    chunk &= chunk - 1;
                    span[count++] = pool[baseIdx + i3];
                }
                if(chunk == 0 && count < amount)
                {
                     availableChunk = DeBruijn.TrailingZeroCount(_chunkMask);
                    chunk = ref _objMask[availableChunk];
                     baseIdx = availableChunk << 6;
                }
                while (chunk != 0 && count < amount)
                {
                    int i = DeBruijn.TrailingZeroCount(chunk);
                    span[count++] = _pool[baseIdx + i];
                  
                    chunk &= chunk - 1; 
                }
                if (chunk == 0) _chunkMask &= ~(1UL << availableChunk);
            }
            return count;
        }
        private IPooledObject<T> GetNextAvailable(ref int availableChunk)
        {
            ref ulong mask = ref _objMask[availableChunk];
            if (mask == 0) return null!;

            int chunkIndex =  DeBruijn.TrailingZeroCount(mask);

            ulong bitMask = 1UL << chunkIndex;
            mask &= ~bitMask;
            int currentChunk = availableChunk;
            if (mask == 0)
            {
                _chunkMask &= ~(1UL << availableChunk);
                availableChunk = DeBruijn.TrailingZeroCount(_chunkMask);
            }
            int objectIndex = (currentChunk * 64) + chunkIndex;
            return _pool[objectIndex];
        }

        /// <summary>
        /// Returns all objects to the pool.
        /// </summary>
        public void ReturnAll()
        {
            if (!_active) return;

            for (int block = 0; block < _objMask.Length; block++)
            {
                ulong mask = _objMask[block];

                // Case 1: all inactive → skip
                if (mask == ulong.MaxValue)
                    continue;

                // Case 2: all active → return all 64 quickly
                if (mask == 0)
                {
                    int start = block * 64;
                    int count = Math.Min(64, _poolCount - start);

                    for (int i = 0; i < count; i++)
                        _pool[start + i].ReturnToPool();

                    continue;
                }

                // Case 3: mixed bits → check each bit
                for (int bit = 0; bit < 64; bit++)
                {
                    int index = block * 64 + bit;
                    if (index >= _poolCount)
                        return;

                    bool isActive = (mask & (1UL << bit)) == 0;
                    if (isActive)
                        _pool[index].ReturnToPool();
                }
            }
        }

        private bool CreateNewPooledObject(bool setActive, out IPooledObject<T> newIdent)
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
                identifier.notifyPool += ReturnToPool;
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
        /// Increasing the pool size will create new objects using <see cref="IFactory{T}.CreateNewObject"/>.<br/>
        /// The time it takes for this operation to complete is dependant on how many new objects are created, and the 
        /// complexity of the object construction per object.</para>
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
        /// Destroys all objects in the pool<br/>
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
        public void ReplaceInvalid()
        {
            if (!_active) return;
            if (SizeOfPool == 0)
            {
                _active = false;
                return;
            }
            for (int i = 0; i < SizeOfPool; i++)
            {
                if (_pool[i] != null && _pool[i].Entity != null)
                {
                    continue;
                }
                bool wasAvailable = (_objMask[i >> 6] & (1UL << (i & 63))) != 0;
                if (!wasAvailable)
                {
                    _pool[i]?.PrepareForDisposal(_factory.DisposeObject);
                }
                else
                {
                    _pool[i]?.Dispose();
                }
                    
                InsertNewPooledObjectAt(i);

                _objMask[i >> 6] |= (1UL << (i & 63));
                _chunkMask |= 1U << (i >> 6);

            }
        }

        /// <summary>
        /// Removes any invalid objects in the pool, and shrinks the pool down to the new valid size.
        /// </summary>
        public void RemoveInvalid()
        {
            if (!_active) return;
            if (_poolCount == 0)
            {
                _active = false;
                return;
            }

            int i = 0;
            while (i < _poolCount)
            {
                var wrapper = _pool[i];
                // Check if invalid
                if(wrapper != null && wrapper.Entity != null)
                {
                    i++;
                    continue;
                }
                int last = _poolCount - 1;
                int newIndex = i;
               ref ulong maskChunkForNew = ref _objMask[newIndex >> 6];
                ulong newObjMask = 1UL << (newIndex & 63);
                bool elementToBeRemovedWasAvailable = (maskChunkForNew & newObjMask) != 0;
                //chooses appropriate disposal if wrapper is not null
                if (wrapper != null)
                {
                    if (!elementToBeRemovedWasAvailable)
                        wrapper.PrepareForDisposal(_factory.DisposeObject);
                    else
                        wrapper.Dispose();
                }
                // Move last element into this slot
        
                var lastWrapper = _pool[last];
                _poolCount--;
                // If we just removed the last element, continue
                if (i == last)
                {
                    _pool[i] = null!;
                    _objMask[0] = 0;
                    continue;
                }
                // Otherwise, move last element down
                _pool[i] = lastWrapper;
                _pool[last] = null!;
                bool lastElementWasAvailable = (_objMask[last >> 6] & (1UL << (last & 63))) != 0;
                //set the new bit to 1 or 0 based on if it was available
                if (lastElementWasAvailable)
                    maskChunkForNew |= newObjMask;
                else
                    maskChunkForNew &= ~newObjMask;

                //set the last bit to 0
                _objMask[last >> 6] &= ~(1UL << (last & 63));
            }
            _chunkMask = 0;
            if (_poolCount == 0)
            {
                _active = false;
                return;
            }
            for (int mask = 0; mask < _objMask.Length; mask++)
            {
                if (_objMask[mask] != 0)
                {
                    _chunkMask |= (1U << mask);
                }
            }
            // Reassign identifiers
            for (int j = 0; j < _poolCount; j++)
            {
                int chunkIndex = j >> 6;
                ulong bitMask = 1UL << (j & 63);
                _pool[j].SetIdentifier(chunkIndex, bitMask);
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
        /// <summary>
        /// Closes down the pool, Calls disposal on all inactive objects, 
        /// delegates disposal to active objects, and prepares for Garbage Collection. <br/>
        /// POOL CANNOT BE USED AFTER THIS IS CALLED 
        /// </summary>
        public void ShutDownPool()
        {
            if(_disposed) return;

            for (int i = 0; i < _poolCount; ++i)
            {
                // Check whether the bit for object i is set to 1 in the mask.

                bool isInactive = (_objMask[(int)i / 64] & (1UL << (i & 63))) != 0;

                if (isInactive)
                {
                    _factory.DisposeObject(_pool[i].Entity);
                    _pool[i].Dispose();
                }
                else
                {
                    _pool[i].PrepareForDisposal(_factory.DisposeObject);
                }
            }

            _chunkMask = 0;
            Array.Clear(_objMask, 0, _objMask.Length);
            _poolCount = 0;
            _pool = null!;
            _disposed = true;
            _active = false;
        }
    }
}
