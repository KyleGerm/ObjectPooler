

using System;
using System.Collections.Generic;

namespace KylesUnityLib.Pooling
{
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

        /// <summary>
        /// Return all objects given, back to the pool
        /// </summary>
        /// <param name="pooled"></param>
        static void ReturnAll(params IPooledObject<T>[] pooled)
        {
            foreach(var returnable in pooled)
            {
                returnable?.ReturnToPool();
            }
        }

        /// <summary>
        /// Returns all objects in collections given, to the pool
        /// </summary>
        /// <param name="pooled"></param>
        static void ReturnAll(params IEnumerable<IPooledObject<T>>[] pooled)
        {
            foreach(var e in pooled)
            {
                foreach( var returnable in e)
                    { returnable?.ReturnToPool(); }
            }
        }
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
   
    public static class IPooledObjectExtensions
    {
        /// <inheritdoc cref="IPooledObject{T}.ReturnAll(IPooledObject{T}[])" />
        public static void ReturnAll<T>(this IPooledObject<T>[] collection)
        {
            IPooledObject<T>.ReturnAll(collection);
        }
    }

}
