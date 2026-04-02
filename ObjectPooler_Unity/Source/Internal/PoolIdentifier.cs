
using JetBrains.Annotations;
using KylesUnityLib.Factory;
using KylesUnityLib.Pooling;
using System;
using UnityEngine;

namespace KylesUnityLib.Internal.Pooling
{
    internal class PoolIdentifier : MonoBehaviour, IPoolable 
    {

        internal int ChunkIndex { get; private set; }
        internal ulong BitMask { get; private set; }
        public event Action OnReturn = null!;
        internal event Action<PoolIdentifier> notifyPool = null!;
        public GameObject GameObject => gameObject;
        public void ReturnToPool()
        {
            OnReturn?.Invoke();
            notifyPool?.Invoke(this);
            notifyPool = null!; 
        }

        private void OnDestroy()
        {
            ClearEvents();
        }

        internal void SetIdentifier(int chunkIndex, ulong bitMask)
        {
            this.ChunkIndex = chunkIndex;
            this.BitMask = bitMask;
        }

        internal void ClearEvents()
        {
            OnReturn = null!;
            notifyPool = null!;
        }
    }


    internal class PoolIdentifier<T> : IPoolable<T>  where T : class , IInjectable<T>
    {
        internal int ChunkIndex { get; private set; }
        internal ulong BitMask { get; private set; }
       
        public T Entity { get; private set; }

        public event Action? OnReturn;
        internal event Action<PoolIdentifier<T>>? notifyPool = null!;
        private FactoryCleanupMethod<T> _cleanupMethod;
        private Action _returnMethod;
        internal PoolIdentifier(T pooledObj)
        {
            Entity = pooledObj;
            ((IInjectable<T>)pooledObj).InjectPoolable(this);
            _returnMethod = StandardReturn;
        }
        private void StandardReturn()
        {
            OnReturn?.Invoke();
            notifyPool?.Invoke(this);
            notifyPool = null;
        }
        public void ReturnToPool()
        {
            _returnMethod?.Invoke();
        }
        internal void SetIdentifier(int chunkIndex, ulong bitMask)
        {
            this.ChunkIndex = chunkIndex;
            this.BitMask = bitMask;
        }

        internal void ClearEvents()
        {
            OnReturn = null;
            notifyPool = null;
        }

        internal void Dispose()
        {
            Entity = null!;
            ClearEvents();
        }
        private void DisposalReturn()
        {
            var obj = Entity;
            Entity = null!;
            _cleanupMethod?.Invoke(obj);
            Dispose();
        }

        internal void PrepareForDisposal(FactoryCleanupMethod<T> cleanup)
        {
            _cleanupMethod = cleanup;
            _returnMethod = DisposalReturn;
        }
    }

}
