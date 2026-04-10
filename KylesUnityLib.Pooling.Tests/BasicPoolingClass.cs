

namespace KylesUnityLib.Pooling.Tests
{

    public class BasicPoolingClass : IPoolable<BasicPoolingClass>
    {
        public int Value { get; set; }
        public BasicPoolingClass? LinkedClass { get; set; }

        public void InjectPoolable(IPooledObject<BasicPoolingClass> poolable)
        {
            poolable.OnReturn += ReturnToPool;
        }

        public void ReturnToPool()
        {
            Value = 0;
        }
    }
}
