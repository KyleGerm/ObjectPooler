using System;
using UnityEngine;

namespace KylesUnityLib.Pooling
{
 
    /// <summary>
    /// Generic Enum containing class which can hold GameObjects in a Pool<br/>
    /// Enums used in this Pooler should always start with a value of 0 and increase in value by 1
    /// </summary>
    /// <typeparam name="T">Type of enum to be used in this pool class</typeparam>
    public class Pooler<T> where T : Enum
    {
        private GameObjectPool[] _poolArr;

        /// <summary>
        /// The Pool which will be accessed the most.
        /// Defaults to the first value of the enum
        /// </summary>
        public GameObjectPool HotPath { get; private set; }

        private GameObjectPool this[T type] => _poolArr[Convert.ToInt32(type)];
        public Pooler()
        {
            int enumLength = Enum.GetValues(typeof(T)).Length;
            _poolArr = new GameObjectPool[enumLength];
            for (int i = 0; i < _poolArr.Length; i++)
            {
                _poolArr[i] = new GameObjectPool();
            }
        }
        /// <summary>
        /// Returns a single available Gameobject
        /// </summary>
        /// <param name="prefabType">Which Pool to search</param>
        /// <param name="resizable">Can a new GameObject be created if none can be found?</param>
        /// <returns></returns>
        public IPoolable GetObject(T prefabType, bool resizable) => this[prefabType].GetObject(resizable);


        /// <summary>
        /// Returns an array of Gameobjects from the Specified Pool.
        /// If the specified amount cannot be created, returns null.
        /// 
        /// <para>⚠️ This method allocates memory. Prefer <see cref="RequestMultiple(T, bool, ref IPoolable[])"/> for zero-allocation usage.</para>
        ///</summary>
        /// <param name="amount">Number of GameObjects to return</param>
        /// <param name="prefabType">Which pool to search</param>
        /// <param name="resizable">Can more objects be made to return the amount specified?</param>
        /// <returns></returns>
        public IPoolable[] RequestMultiple(int amount, T prefabType, bool resizable) => this[prefabType].RequestMultiple(amount, resizable);

        /// <summary>
        /// <para>Fills the array with GameObjects from the specified pool. Returns true if the array was successfully populated. Zero Allocation</para>
        /// <para>The array will still be partially populated even if false is returned. 
        /// Null checks should be used if false is returned, and the array was empty when passed in</para>
        /// </summary>
        /// <param name="prefabType">Which pool to search</param>
        /// <param name="resizable"><para>Can more objects be made to return the amount specified?</para>
        /// <para>Allocation will happen if true, and new instances are created</para></param>
        /// <param name="buffer">The buffer given to populate</param>
        /// <returns></returns>
        public bool RequestMultiple(T prefabType, bool resizable, ref IPoolable[] buffer) => this[prefabType].RequestMultiple(resizable, ref buffer);

        /// <summary>
        /// Returns an array of Components other than GameObject.<br/>
        /// If the Component is not part of the PrefabType specified, returns null.
        /// <para>This assumes these objects are in use. They must explicitly be returned before they can be used again.</para>
        ///  <para>⚠️ This method allocates memory. Prefer <see cref="RequestMultiple{C}(T, ref C[])"/> for zero-allocation usage.</para>
        /// </summary>
        /// <typeparam name="C">Type of Component to search for</typeparam>
        /// <param name="amount">Number of Components to return</param>
        /// <param name="prefabType">Which pool to search</param>
        /// <returns></returns>
        public C[] RequestMultiple<C>(int amount, T prefabType) where C : Component => this[prefabType].RequestMultiple<C>(amount);

        /// <summary>
        /// Returns an array of Components other than GameObject Non-Allocating.<br/>
        /// Buffer will contain any elements written to it, even if the buffer was not filled
        /// <para>This assumes these objects are in use. They must explicitly be returned before they can be used again.</para>
        /// </summary>
        /// <typeparam name="C">Type of Component to search for</typeparam>
        /// <param name="componentList">Buffer to be filled</param>
        /// <param name="prefabType">Which pool to search</param>
        /// <returns>True if buffer was completely filled</returns>
        public bool RequestMultiple<C>(T prefabType,ref C[] componentList) where C : Component => this[prefabType].RequestMultiple(ref componentList);

        /// <summary>
        /// Takes a Key value enum, an object to pool, and a number of objects to make.
        /// if the object does not inherit the IPoolable interface, the pool will not be made. 
        /// </summary>
        /// <param name="listType"></param>
        /// <param name="obj"></param>
        /// <param name="size"></param>
        public void GenerateList(T listType, in GameObject obj, int size,int maxSize ,Action<GameObject> action = null) => _poolArr[Convert.ToInt32(listType)] = GameObjectPool.Create(obj,size, maxSize,action);


        /// <summary>
        /// Returns all elements in a specific PoolType 
        /// </summary>
        /// <param name="listType"></param>
        public void ReturnAll(T listType) => this[listType].ReturnAll();

        /// <summary>
        /// Destroys all GameObjects in the list, and Removes the list from the Pool
        /// </summary>
        /// <param name="listType"></param>
        public void DestroyList(T listType, Action<GameObject> action = null) => this[listType].DestroyList(action);

        /// <summary>
        /// Removes the Pool from the list, and hands back the objects
        /// </summary>
        /// <param name="listType"></param>
        public GameObject[] RemoveListButDontDestroy(T listType) => this[listType].RemoveListButDontDestroy();

        /// <summary>
        /// Validate all Pools
        /// </summary>
        public void Validate()
        {
            for (int i = 0; i < _poolArr.Length; i++)
            {
                if (!_poolArr[i].Active) continue;
                _poolArr[i].Validate();
            }
        }

        public void ReturnAll()
        {
           foreach(var pool in _poolArr)
            {
                pool.ReturnAll();
            }
        }

        public void SetHotPath(T enumType) => HotPath = this[enumType];

        public int SizeOfPool(T enumType) => this[enumType].SizeOfPool;

        /// <summary>
        /// Reduce a pool by an amount
        /// </summary>
        /// <param name="enumType"></param>
        /// <param name="amount"></param>
        public void Reduce(T enumType, int amount) => this[enumType].Reduce(amount);

        /// <summary>
        /// Resizes the pool to the given size. Pooler will handle creation or destruction of 
        /// objects to accomodate the new size.<br/>
        /// Will not work with an uninitialised pool.
        /// </summary>
        /// <param name="enumType"></param>
        /// <param name="newSize"></param>
        public void Resize(T enumType, int newSize) => this[enumType].Resize(newSize);
    }
}
//TODO: Create a sampling solution to enable automatic resizing over time based on usage