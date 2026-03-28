

namespace KylesUnityLib.Pooling.Tests
{

    public class BasicPoolingClass : IInjectable<BasicPoolingClass>
    {
        public int Value { get; set; }
        public BasicPoolingClass? LinkedClass { get; set; }

        public void InjectPoolable(IPoolable<BasicPoolingClass> poolable)
        {
            
        }
    }
}
