
using JetBrains.Annotations;
using KylesUnityLib.Factory;
using KylesUnityLib.Pooling;
using System;

namespace KylesUnityLib.Internal.Pooling
{
    internal class PoolIdentifier<T> : IPooledObject<T>  where T : class , IPoolable<T>
    {
        internal int ChunkIndex { get; private set; }
        internal ulong BitMask { get; private set; }
       
        public T Entity { get; private set; }

        public event Action? OnReturn;
        internal event Action<PoolIdentifier<T>>? notifyPool = null!;
        private FactoryCleanupMethod<T>? _cleanupMethod;
        private Action? _returnMethod;
        internal PoolIdentifier(T pooledObj)
        {
            Entity = pooledObj;
            pooledObj.InjectPoolable(this);
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
            _cleanupMethod = null;
            _returnMethod = null;
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
            _cleanupMethod = null;
            Dispose();
        }

        internal void PrepareForDisposal(FactoryCleanupMethod<T> cleanup)
        {
            _cleanupMethod = cleanup;
            _returnMethod = DisposalReturn;
        }
    }

}
