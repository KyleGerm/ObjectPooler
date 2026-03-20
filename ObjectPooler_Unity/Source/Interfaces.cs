

using System;
using UnityEngine;

namespace KylesUnityLib.Pooling
{
    /// <summary>
    /// Qualifies the class to be part of a pool 
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// An 
        /// </summary>
        event Action onReturn;
        /// <summary>
        /// Returns the item to the pool it belongs to.
        /// <para>This should only be called when the object will no longer be used by its current owner</para>
        /// </summary>
        void ReturnToPool();

        GameObject gameObject { get; }
    }

    /// <summary>
    /// Non Generic methods for the Pooler classes 
    /// </summary>
    internal interface IPoolingAbstractor
    {

        /// <summary>
        /// Returns Everything in all pools
        /// </summary>
        void ReturnAll();
        /// <summary>
        /// Checks through each pool, and removes any invalid values 
        /// </summary>
        void Validate();
    }
}
