
using System;
using System.Buffers;
using System.Linq;
using UnityEngine;
using KylesUnityLib.Internal.Pooling;

namespace KylesUnityLib.Pooling
{
    /// <summary>
    /// Pool with added functionality for GameObjects
    /// </summary>
    public class GameObjectPool
    {
        private bool _active;
        private PoolIdentifier[] _pool;
        private int _poolCount;
        private GameObject _template;
        private ulong[] _objMask;
        private ulong _chunkMask;
        /// <summary>
        /// Maximum set size for the pool.<br/>
        /// Cannot be changed after creation.
        /// </summary>
        public readonly int MaxSize;
        private Action<GameObject>? _creationAction;
        /// <summary>
        /// Current number of objects in the pool
        /// </summary>
        public int SizeOfPool => _poolCount;
        /// <summary>
        /// Active status of the pool.<br/>
        /// Determines if the pool is in a usable state
        /// </summary>
        public bool Active => _active;

        /// <summary>
        /// Gives an uninitialized GameObjectPool back with a defined max size.<br/>
        /// Pool will stay inactive until <see cref="GenerateList(in GameObject, int, Action{GameObject})"></see> is called
        /// </summary>
        /// <param name="maxSize"></param>
        public GameObjectPool(int maxSize)
        {
            //ensures the arraySize can house all objects up to a maxSize
            MaxSize = Math.Min(maxSize, 4096);
            _objMask = new ulong[(MaxSize + 63) / 64];
            _objMask = null!;
            _template = null!;
            _pool = null!;
        }
        internal GameObjectPool()
        {
            _active = false;
            _objMask = null!;
            _template = null!;
            _pool = null!;
        }

        /// <summary>
        /// Creates a new GameObjectPool, generates a pool, and hands back the object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="size"></param>
        /// <param name="maxSize"></param>
        /// <param name="onCreation"></param>
        /// <returns></returns>
        public static GameObjectPool Create(in GameObject obj, int size, int maxSize, Action<GameObject> onCreation = null!)
        {
            if (obj == null)
            {
                Logger.LogWarning("obj is null, ensure the object used for the pool is not null before generating a pool.");
                return null!;
            }
            if(maxSize <= 0)
            {
                Logger.LogWarning("maxSize must be gretaer than 0. Cannot create the pooler.");
                return null!;
            }
            if(size >  maxSize) size = maxSize;
            var pool = new GameObjectPool(maxSize);
            
            if(pool.GenerateList(obj, size,onCreation))
                return pool;

            Logger.LogWarning("Pooler was unable to initialize. Check parameters and try again.");
            return null!;
        }
        /// <summary>
        /// Takes an object to pool, and a number of objects to make.
        /// </summary>
        /// <param name="obj">Object templae to be used in this pool</param>
        /// <param name="size">Initial size of pool</param>
        /// <param name = "onCreation">Any action to be performed on object creation.<br/>This will also be applied to future objects created, to ensure each object starts in the same state.</param>
        public bool GenerateList(in GameObject obj, int size, Action<GameObject> onCreation = null!)
        {
            if (_active) {
                Logger.LogWarning("Pooler is already in use, and has objects. Either destroy the pool using DestroyList(), or take ownership of the objects using RemoveListButDontDestroy()");
                return false; 
            }
            if(obj == null)
            {
                Logger.LogWarning("obj is null, ensure the object used for the pool is not null before generating a pool.");
                return false;
            }
            size = Math.Max(0, size);
     
           _pool = new PoolIdentifier[(size + 63) / 64 * 64];
            _template = obj;
            _poolCount = 0;
            _chunkMask = 0;
            _creationAction = onCreation;
            Array.Clear(_objMask, 0, _objMask.Length);
            for (int i = 0; i < size; i++)
            {
                CreateNewPooledObject(false, out _); 
            }
            _active = _poolCount > 0;

            if(!_active) return false;

            for(int i = 0;i < _objMask.Length; i++)
            {
                if(_objMask[i] != 0)
                    _chunkMask |= 1UL << i;  
            }
            return true;
        }

        /// <summary>
        /// Returns a single available IPoolable
        /// </summary>
        /// <param name="resizable">Can a new GameObject be created if none can be found?</param>
        /// <returns>Available IPoolable, or null if none can be found</returns>
        public IPoolable GetObject(bool resizable)
        {
            if(!_active) return null!;

            if (_chunkMask != 0 && GetNextAvailable(DeBruijn.TrailingZeroCount(_chunkMask)) is IPoolable obj)
                return obj;

            if (resizable && CreateNewPooledObject(true, out IPoolable newObj))
                return newObj ;

            return null!;
        }

        /// <summary>
        /// Returns an array of IPoolable.
        /// If the specified amount cannot be created, returns null.
        /// 
        /// <para>⚠️ This method allocates memory. Prefer <see cref="RequestMultiple(bool, ref IPoolable[])"/> for zero-allocation usage.</para>
        ///</summary>
        /// <param name="amount">Number of IPoolable to return</param>
        /// <param name="resizable">Can more objects be made to return the amount specified?</param>
        /// <returns></returns>
        public IPoolable[] RequestMultiple(int amount, bool resizable)
        {
            int count = 0;
            IPoolable[] list = new IPoolable[amount];

            if (!_active) return null!;
            
            count = PopulateBuffer(list);

            if (count < amount && resizable)
            {
                while (count < amount && _poolCount < MaxSize && CreateNewPooledObject(true, out IPoolable obj))
                {
                    list[count++] = obj;
                }
            }
    
            return count == amount ? list : null!;
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
        public bool RequestMultiple(bool resizable, ref IPoolable[] buffer)
        {
            int amount = buffer.Length;
            int populatedSlots = 0;
            if (_active)
            {
                populatedSlots = PopulateBuffer(buffer);

                if (populatedSlots < amount && resizable)
                {
                    while (populatedSlots < amount && _poolCount < MaxSize && CreateNewPooledObject(true, out IPoolable obj) )
                    {
                        buffer[populatedSlots++] = obj;
                    }
                }
            }

            return populatedSlots == amount;
        }

        /// <summary>
        /// Returns an array of Components.<br/>
        /// If the Component is not part of the original object, returns null.
        /// <para>This assumes these objects are in use. They must explicitly be returned before they can be used again.</para>
        ///  <para>⚠️ This method allocates memory. Prefer <see cref="RequestMultiple{C}(ref C[])"/> for zero-allocation usage.</para>
        /// </summary>
        /// <typeparam name="C">Type of Component to search for</typeparam>
        /// <param name="amount">Number of Components to return</param>
        /// <returns></returns>
        public C[] RequestMultiple<C>(int amount) where C : Component
        {
            C[] componentList = new C[amount];
            if (RequestMultiple(ref componentList))
                return componentList;

            return null!;
        }

        /// <summary>
        /// Returns an array of Components Non-Allocating.<br/>
        /// Buffer will contain any elements written to it, even if the buffer was not filled
        /// <para>This assumes these objects are in use. They must explicitly be returned before they can be used again.</para>
        /// </summary>
        /// <typeparam name="C">Type of Component to search for</typeparam>
        /// <param name="componentList">Buffer to be filled</param>
        /// <returns>True if buffer was completely filled</returns>
        public bool RequestMultiple<C>(ref C[] componentList) where C : Component
        {
            int count = 0;
            int amount = componentList.Length;
            if(!_template.TryGetComponent(out C _)) return false; 
            if (_active)
            {
                var poolables = ArrayPool<IPoolable>.Shared.Rent(amount);
                count = PopulateBuffer(poolables.AsSpan(0, amount));

                for (int i = 0;i < count; i++)
                {
                    componentList[i] = poolables[i].GameObject.GetComponent<C>();
                }
                ArrayPool<IPoolable>.Shared.Return(poolables, clearArray: false);

            }
            return count == amount;
        }

        private int PopulateBuffer(Span<IPoolable> span)
        {
            int count = 0;
            int amount = span.Length;
            if (_chunkMask == 0) return count;
            int availableChunk = DeBruijn.TrailingZeroCount(_chunkMask);

            for ( ; count < amount; count++)
            {
                span[count] = GetNextAvailable(availableChunk);

                if (_objMask[availableChunk] == 0 )
                {
                    if (_chunkMask == 0)  break; 
                    
                    availableChunk = DeBruijn.TrailingZeroCount(_chunkMask);
                }
            }
            return count;
        }
       
        private IPoolable GetNextAvailable(int availableChunk)
        {
            if(_objMask[availableChunk] == 0) return null!;

            int chunkIndex = DeBruijn.TrailingZeroCount(_objMask[availableChunk]);

            ulong bitMask = 1UL << chunkIndex;
            _objMask[availableChunk] &= ~bitMask;

            if (_objMask[availableChunk] == 0)
                _chunkMask &= ~(1UL << availableChunk);
            int objectIndex = availableChunk * 64 + chunkIndex;
            _pool[objectIndex].notifyPool += ReturnToPool;
            return _pool[objectIndex];
        }

        /// <summary>
        /// Returns all objects to the pool.
        /// </summary>
        public void ReturnAll()
        {
            if (!_active) return; 
            foreach (var obj in _pool)
            {
                obj.ReturnToPool();
            }
        }
        private bool CreateNewPooledObject(bool setActive, out IPoolable newIdent)
        {
            GameObject obj = UnityEngine.Object.Instantiate(_template);
            obj.SetActive(false);
            PoolIdentifier identifier = obj.AddComponent<PoolIdentifier>();


            if (AddToPool(identifier))
            {
                //divide by 64
                int chunkIndex = _poolCount >> 6;
                //Fast Mod bitshift
                ulong bitMask = 1UL << (_poolCount & 63);
                identifier.SetIdentifier(chunkIndex, bitMask);
                newIdent = identifier;
                _creationAction?.Invoke(obj);
                if (setActive)
                    _objMask[chunkIndex] &= ~bitMask;
                else
                {
                    _objMask[chunkIndex] |= bitMask;
                    _chunkMask |= 1UL << chunkIndex;
                }
                return true;
            }

            newIdent = null!;
            UnityEngine.Object.Destroy(obj);
            return false;
        }

       private bool AddToPool(PoolIdentifier identifier)
        {
            if(_poolCount >= _pool.Length)
            {
                if (_pool.Length < MaxSize)
                {
                    int newSize = Math.Min(_poolCount + 64, MaxSize);
                    Array.Resize(ref _pool, newSize);
                }
                else return false;
            }
            _pool[_poolCount++] = identifier;
            return true;
        }

        internal void ReturnToPool(PoolIdentifier identifier)
        {
            _objMask[identifier.ChunkIndex] |= identifier.BitMask;
            _chunkMask |= 1UL << identifier.ChunkIndex;
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
            if (!_active || sizeDiff == 0) return;

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
        /// Pool will be destroyed if amount is larger than the size of the pool
        /// </summary>
        /// <param name="amount"></param>
        public void Reduce(int amount)
        {
     
            if (amount >= _poolCount)
            {
                DestroyList();
                return;
            }
           
            int newCapacity = SizeOfPool - amount;
            for (int i = SizeOfPool - 1; i >= newCapacity; i--)
            {
                _objMask[_pool[i].ChunkIndex] &= ~_pool[i].BitMask;
                if(_objMask[_pool[i].ChunkIndex] == 0)
                    _chunkMask &= ~(1UL << _pool[i].ChunkIndex);
                _pool[i].ClearEvents();
                UnityEngine.Object.Destroy(_pool[i].gameObject);
                _pool[i] = null!;
                _poolCount--;
            }
        }

        /// <summary>
        /// Destroys all GameObjects in the list<br/>
        /// Pool will be set to inactive until a new pool is generated
        /// </summary>
        /// <param name="action"></param>
        public void DestroyList(Action<GameObject> action = null!)
        {
            if (_active && _poolCount > 0)
            {
                _active = false;
                foreach (var obj in _pool)
                {
                    if (obj == null) continue;
                    action?.Invoke(obj.gameObject);
                    obj.ClearEvents();
                    UnityEngine.Object.Destroy(obj.gameObject);
                }
                for (int i = 0; i < _objMask.Length; i++) _objMask[i] = 0;
                _chunkMask = 0;
                _poolCount = 0;
                _template = null!;
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
                _active = false;
            for (int i = 0; i < SizeOfPool; i++)
            {
                if (_pool[i] == null)
                    InsertNewPooledObjectAt(i);
            } 
        }
        private void InsertNewPooledObjectAt(int index)
        {
            GameObject obj = UnityEngine.Object.Instantiate(_template);
            obj.SetActive(false);
            var identifier = obj.AddComponent<PoolIdentifier>();

            //divide by 64
            int chunkIndex = index >> 6;
            //Fast Mod bitshift
            ulong bitMask = 1UL << (index & 63);
            identifier.SetIdentifier(chunkIndex, bitMask);

            _objMask[chunkIndex] |= bitMask;
            _chunkMask |= 1UL << chunkIndex;

            _pool[index] = identifier;
        }

        /// <summary>
        /// Shuts down the pool, and hands back the GameObjects
        /// </summary>
        /// <returns></returns>
        public GameObject[] RemoveListButDontDestroy()
        {
            if (_pool == null || _poolCount == 0) return Array.Empty<GameObject>();
            GameObject[] gameObjects = _pool.Select(x => x.gameObject).ToArray();
            //Removes PoolIdentifier component from GameObjects
            for(int i = 0; i < _poolCount;i++)
            {
                if (_pool[i] != null)
                    UnityEngine.Object.Destroy(_pool[i]);
               
                _pool[i] = null!;
            }
            _poolCount = 0;
            _active = false;
            _template = null!;
            for (int i = 0; i < _objMask.Length; i++) _objMask[i] = 0;
            _chunkMask = 0;
            return gameObjects;
        }

        /// <summary>
        /// Removes Creation actions for future object creations.<br/>
        /// Be aware this means the pool will contain objects with mixed state unless you address this first.
        /// </summary>
        public void ClearCreationAction() => _creationAction = null!;
        /// <summary>
        /// Changes the Creation action for future object creations.<br/>
        /// Be aware this means the pool will contain objects with mixed state unless you address this first.
        /// </summary>
        public void SetCreationAction(Action<GameObject> onCreation) => _creationAction = onCreation;
        /// <summary>
        /// Adds an action to the Creation Actions for future object creations.<br/>
        /// Be aware this means the pool will contain objects with mixed state unless you address this first.
        /// </summary>
        public void AddCreationAction(Action<GameObject> onCreation) => _creationAction += onCreation;
        /// <summary>
        /// Removes an action from the Creation action for future object creations.<br/>
        /// Be aware this means the pool will contain objects with mixed state unless you address this first.
        /// </summary>
        public void RemoveCreationAction(Action<GameObject> onCreation) => _creationAction -= onCreation;
    }
}
