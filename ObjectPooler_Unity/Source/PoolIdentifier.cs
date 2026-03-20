
using System;
using UnityEngine;

namespace KylesUnityLib.Pooling
{
    internal class PoolIdentifier : MonoBehaviour, IPoolable 
    {

        internal int chunkIndex { get; private set; }
        internal ulong bitMask { get; private set; }
        public event Action onReturn;
        internal event Action<PoolIdentifier> notifyPool;
        public void ReturnToPool()
        {
            onReturn?.Invoke();
            notifyPool?.Invoke(this);
            notifyPool = null;
        }

        private void OnDestroy()
        {
            ClearEvents();
        }

        internal void SetIdentifier(int chunkIndex, ulong bitMask)
        {
            this.chunkIndex = chunkIndex;
            this.bitMask = bitMask;
        }

        internal void ClearEvents()
        {
            onReturn = null;
            notifyPool = null;
        }
    }
}
