

using System;
using UnityEngine;
using KylesUnityLib.Factory;
using System.Runtime.CompilerServices;

namespace KylesUnityLib.Pooling
{
    /// <summary>
    /// Qualifies the class to be part of a pool 
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// OnReturn is run when the object is returned to the pool. Use this to add object specific return behaviour
        /// </summary>
        event Action OnReturn;
        /// <summary>
        /// Returns the item to the pool it belongs to.
        /// <para>This should only be called when the object will no longer be used by its current owner</para>
        /// </summary>
        void ReturnToPool();
        /// <summary>
        /// The gameObject contained within the pool
        /// </summary>
        GameObject GameObject { get; }
    }

    /// <summary>
    /// Qualifies the class to be part of a pool 
    /// </summary>
    public interface IPooledObject<T>
    {
        /// <summary>
        /// OnReturn is run when the object is returned to the pool. Use this to add object specific return behaviour
        /// </summary>
        event Action OnReturn;
        /// <summary>
        /// Returns the item to the pool it belongs to.
        /// <para>This should only be called when the object will no longer be used by its current owner</para>
        /// </summary>
        void ReturnToPool();
       /// <summary>
       /// The pooled object
       /// </summary>
        T Entity { get; }
    }

    public interface IPoolable<T>
    {
        /// <summary>
        /// Allows the <see cref="IPooledObject{T}"/> to inject itself into its <see cref="T"/> instance.<br/>
        /// This allows <see cref="T"/> to access the wrapper and add behaviours to it
        /// </summary>
        /// <param name="poolable"></param>
        void InjectPoolable(IPooledObject<T> poolable);
    }
   

}
