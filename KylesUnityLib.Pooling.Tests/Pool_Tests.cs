using KylesUnityLib.Factory;
using KylesUnityLib.Internal.Pooling;
using System.Reflection;

namespace KylesUnityLib.Pooling.Tests
{
    public class Pool_Tests
    {
        [Fact]
        public void PoolIsCreatedSuccessfullyWithDefaultConstructor()
        {
            Pool<BasicPoolingClass> pool = new();
            Assert.NotNull(pool);
        }

        [Fact]
        public void DefaultConstructedPoolCannotBeUsed()
        {
            Pool<BasicPoolingClass> pool = new();
            var obj = pool.TryGetObject(out var _);
            Assert.False(obj);
            Assert.False(pool.Active);
        }

        [Fact]
        public void Pool_ParamConstructor_CannotBeUsed()
        {
            Pool<BasicPoolingClass> pool = new();
            var obj = pool.TryGetObject(out var _);
            Assert.False(obj);
            Assert.False(pool.Active);
        }

        [Fact]
        public void StaticConstructorCanBeUsed()
        {
            Pool<BasicPoolingClass> pool = Pool<BasicPoolingClass>.Create(new Factory<BasicPoolingClass>(() => new()), 5, 10);
            Assert.True(pool.Active);
            Assert.NotNull(pool.GetObject());
            Assert.True(pool.TryGetObject(out var _));
        }

        [Fact]
        public void PoolInitializedWithSizeOfZeroIsInactive()
        {
            bool active = false;
            var err = Record.Exception(() => {
                var pool = Pool<BasicPoolingClass>.Create(new Factory<BasicPoolingClass>(() => new()), 0, 10);
                active = pool.Active;
            });
            Assert.Null(err);
            Assert.False(active);
        }

        [Fact]
        public void NullFactorySuppliesWillThrow()
        {
            var errOnCreate = Record.Exception(() => {
                Pool<BasicPoolingClass>.Create(null!, 0, 10);
            });
            var errOnConstruct = Record.Exception(() => {
                var pool = new Pool<BasicPoolingClass>(10, null!);
            });
            Assert.NotNull(errOnConstruct);
            Assert.NotNull(errOnCreate);
            Assert.IsType<ArgumentNullException>(errOnCreate);
            Assert.IsType<ArgumentNullException>(errOnConstruct);
        }

        [Fact]
        public void EmptyFactorySuppliesWillThrow()
        {
            var errorOnEmptyFactoryConstructor = Record.Exception(() =>
            {
                Factory<BasicPoolingClass> factory = new(null!);
                Pool<BasicPoolingClass>.Create(factory, 0, 10);
            });

            Assert.NotNull(errorOnEmptyFactoryConstructor);
            Assert.IsType<ArgumentNullException>(errorOnEmptyFactoryConstructor);
        }

        [Fact]
        public void MaxSizeInitializedCorrectly()
        {
            var pool = new Pool<BasicPoolingClass>(20, new Factory<BasicPoolingClass>(() => new()));
            Assert.Equal(20, pool.MaxSize);
            pool = Pool<BasicPoolingClass>.Create(new Factory<BasicPoolingClass>(() => new()), 2, 30);
            Assert.Equal(30, pool.MaxSize);
        }

        [Fact]
        public void MaxSizeRespectedWhenCreatingNewObjects()
        {
            var pool = new Pool<BasicPoolingClass>(20, new Factory<BasicPoolingClass>(() => new()));
            pool.GenerateList(20);
            for (int i = 0; i < 25; i++)
            {
                pool.TryGetObject(out IPooledObject<BasicPoolingClass> obj, true);
            }
            Assert.Equal(20, pool.SizeOfPool);
        }

        [Fact]
        public void PoolConstructedManuallyCannotBeUsed_UntilGenerateListCalled()
        {
            var pool = new Pool<BasicPoolingClass>(10, new Factory<BasicPoolingClass>(() => new()));
            Assert.False(pool.Active);
            Assert.False(pool.TryGetObject(out IPooledObject<BasicPoolingClass> _));
            var throwsGetObjectFalse = Record.Exception(() => pool.GetObject(false));
            var throwsGetObjectTrue = Record.Exception(() => pool.GetObject(true));
            Assert.NotNull(throwsGetObjectFalse);
            Assert.NotNull(throwsGetObjectTrue);
            Assert.IsType<PoolIsInactiveException>(throwsGetObjectFalse);
            Assert.IsType<PoolIsInactiveException>(throwsGetObjectTrue);

            pool.GenerateList(5);
            Assert.True(pool.Active);
            throwsGetObjectFalse = Record.Exception(() => pool.GetObject(false));
            throwsGetObjectTrue = Record.Exception(() => pool.GetObject(true));
            Assert.Null(throwsGetObjectFalse);
            Assert.Null(throwsGetObjectTrue);
            Assert.True(pool.TryGetObject(out IPooledObject<BasicPoolingClass> _));
        }

        [Fact]
        public void RequestMultipleAllocatingWillWork()
        {
            var pool = Pool<BasicPoolingClass>.Create(new Factory<BasicPoolingClass>(() => new()), 10, 10);
            IPooledObject<BasicPoolingClass>[] poolables = pool.RequestMultiple(10, false);
            Assert.True(poolables.Length == 10);
            foreach (var poolable in poolables)
            {
                Assert.NotNull(poolable);
                Assert.NotNull(poolable.Entity);
            }
        }
        [Fact]
        public void RequestMultipleNonAllocWillWork()
        {
            IPooledObject<BasicPoolingClass>[] poolables = new IPooledObject<BasicPoolingClass>[10];
            var pool = Pool<BasicPoolingClass>.Create(new Factory<BasicPoolingClass>(() => new()), 10, 10);

            Assert.True(pool.RequestMultiple(ref poolables));
            foreach (var poolable in poolables)
            {
                Assert.NotNull(poolable);
                Assert.NotNull(poolable.Entity);
            }
        }

        [Fact]
        public void RequestingTooManyWillThrow()
        {
            var pool = Pool<BasicPoolingClass>.Create(new Factory<BasicPoolingClass>(() => new()), 10, 10);
            var result = Record.Exception(() =>
            {
                IPooledObject<BasicPoolingClass>[] poolables = pool.RequestMultiple(11, false);
            });
            Assert.NotNull(result);
            Assert.IsType<PoolExhaustedException>(result);
        }

        [Fact]
        public void RequestingTooManyNonAllocWillReturnFalseButWillPopulate()
        {
            IPooledObject<BasicPoolingClass>[] poolables = new IPooledObject<BasicPoolingClass>[20];
            var pool = Pool<BasicPoolingClass>.Create(new Factory<BasicPoolingClass>(() => new()), 10, 10);
            Assert.False(pool.RequestMultiple( ref poolables));
            for (int i = 0; i < poolables.Length; i++)
            {
                if (i < 10)
                {
                    Assert.NotNull(poolables[i]);
                    Assert.NotNull(poolables[i].Entity);
                }
                else
                {
                    Assert.Null(poolables[i]);
                }
            }
        }

        [Fact]
        public void ReturnAllWorksSuccessfully()
        {
            IPooledObject<BasicPoolingClass>[] poolables = new IPooledObject<BasicPoolingClass>[10];
            var pool = Pool<BasicPoolingClass>.Create(new Factory<BasicPoolingClass>(() => new()), 10, 10);
            pool.RequestMultiple( ref poolables);

            Assert.Throws<PoolExhaustedException>(() => pool.RequestMultiple(2, false));
            pool.ReturnAll();
            IPooledObject<BasicPoolingClass>[] secondPoolables = [];
            Assert.Null(Record.Exception(() => secondPoolables = pool.RequestMultiple(10, false)));

            for (int i = 0; i < secondPoolables.Length; i++)
            {
                Assert.Equal(poolables[i], secondPoolables[i]);
                Assert.Equal(poolables[i].Entity, secondPoolables[i].Entity);
            }
        }

        [Fact]
        public void ResizingWorksSuccessfully()
        {
            var pool = Pool<BasicPoolingClass>.Create(new Factory<BasicPoolingClass>(() => new()), 10, 50);
            int newSize = 50;
            IPooledObject<BasicPoolingClass>[] poolables = [];
            pool.Resize(newSize);
            Assert.Equal(50, pool.SizeOfPool);
            poolables = pool.RequestMultiple(newSize, false);
            foreach (var item in poolables)
            {
                Assert.NotNull(item);
                Assert.NotNull(item.Entity);
            }
            newSize = 10;
            pool.Resize(newSize);
            pool.ReturnAll();
            Assert.Equal(newSize, pool.SizeOfPool);
            poolables = pool.RequestMultiple(newSize, false);
            foreach (var item in poolables)
            {
                Assert.NotNull(item);
                Assert.NotNull(item.Entity);
            }
        }

        [Fact]
        public void ReducingPoolWithActiveEntitiesWillNotCorruptMask()
        {
            int numberOfReturns = 0;
            IPooledObject<BasicPoolingClass>[] poolables = new IPooledObject<BasicPoolingClass>[50];
            var pool = Pool<BasicPoolingClass>.Create(new Factory<BasicPoolingClass>(() => new()), 50, 50);
            pool.RequestMultiple( ref poolables);
            foreach (var item in poolables)
            {
                item.OnReturn += () => numberOfReturns++;
            }
            int reduceAmount = 17;
            pool.Reduce(reduceAmount);
            pool.ReturnAll();
            Assert.Equal(50 - reduceAmount, numberOfReturns);
            Assert.Throws<PoolExhaustedException>(() => pool.RequestMultiple((50 - reduceAmount) + 1, false));
        }

        [Fact]
        public void ReduceByPoolSizeOrMoreWillDestroyList_AndWillNotCorrupt()
        {
            int numberOfReturns = 0;
            IPooledObject<BasicPoolingClass>[] poolables = new IPooledObject<BasicPoolingClass>[50];
            var pool = Pool<BasicPoolingClass>.Create(new Factory<BasicPoolingClass>(() => new()), 50, 50);
            pool.RequestMultiple( ref poolables);
            foreach (var item in poolables)
            {
                item.OnReturn += () => numberOfReturns++;
            }
            pool.Reduce(50);
            pool.ReturnAll();
            Assert.Equal(0, numberOfReturns);
            Assert.False(pool.Active);
            pool.GenerateList(30);
            poolables = new IPooledObject<BasicPoolingClass>[30];
            pool.RequestMultiple(ref poolables);
            foreach (var item in poolables)
            {
                item.OnReturn += () => numberOfReturns++;
            }
            pool.Reduce(50);
            pool.ReturnAll();
            Assert.Equal(0, numberOfReturns);
            Assert.False(pool.Active);
        }

        [Fact]
        public void DestroyListInactivatesPool()
        {
            var pool = Pool<BasicPoolingClass>.Create(new Factory<BasicPoolingClass>(() => new()), 50, 50);
            Assert.True(pool.Active);
            Assert.Equal(50, pool.SizeOfPool);
            pool.DestroyList();
            Assert.False(pool.Active);
            Assert.Equal(0, pool.SizeOfPool);
            Assert.Throws<PoolIsInactiveException>(() => pool.GetObject());
        }

        [Fact]
        public void ReplaceInvalidCanRepairBrokenElements()
        {
            static ulong GetChunkMask<T>(Pool<T> pool) where T : class, IPoolable<T>
            {
                var field = typeof(Pool<T>)
                    .GetField("_chunkMask", BindingFlags.NonPublic | BindingFlags.Instance);

                return (ulong)field!.GetValue(pool)!;
            }
            //Accesses _bitMask member variable of Pool
            static ulong[] GetObjMask<T>(Pool<T> pool) where T : class, IPoolable<T>
            {
                var field = typeof(Pool<T>)
                    .GetField("_objMask", BindingFlags.NonPublic | BindingFlags.Instance);

                return (ulong[])field!.GetValue(pool)!;
            }
            Factory<BasicPoolingClass> fac = new(() => new());
            Pool<BasicPoolingClass> pool = Pool<BasicPoolingClass>.Create(fac, 50, 50);
            var pooled = pool.RequestMultiple(50, false);
            foreach (var item in pooled)
            {
                ((PoolIdentifier<BasicPoolingClass>)item).Dispose();
                Assert.Null(item.Entity);
            }
            pool.ReturnAll();
            pool.ReplaceInvalid();
            Assert.Equal(1U, GetChunkMask(pool));
            Assert.Equal(0x003FFFFFFFFFFFFUL, GetObjMask(pool)[0]);
;            pooled = pool.RequestMultiple(50, false);
            foreach (var item in pooled)
            {
                Assert.NotNull(item.Entity);
            }
        }
        [Fact]
        public void RemoveListButDontDestroyWillGiveBackValidObjects()
        {
            Factory<BasicPoolingClass> fac = new(() => new());
            Pool<BasicPoolingClass> pool = Pool<BasicPoolingClass>.Create(fac, 50, 50);
            BasicPoolingClass[] baseObject = pool.RemoveListButDontDestroy();
            foreach (var item in baseObject)
            {
                Assert.NotNull(item);
                Assert.IsType<BasicPoolingClass>(item);
            }
            Assert.False(pool.Active);
            Assert.Equal(0, pool.SizeOfPool);
        }
        [Fact]
        public void GenerateListOnActivePoolThrows_InternalStateUnchanged()
        {
            //1. Test Generate on active pool throws
            var pool = Pool<BasicPoolingClass>.Create(new Factory<BasicPoolingClass>(() => new()), 10, 10);
            Assert.True(pool.Active);
            Assert.Equal(10, pool.SizeOfPool);
            Assert.Throws<ActivePoolOverwriteException>(() => pool.GenerateList(20));
            //2. Pool preserves inital state
            Assert.True(pool.Active);
            Assert.Equal(10, pool.SizeOfPool);
            //3. Pool is still usable
            for (int i = 0; i < 10; i++)
            {
                Assert.NotNull(pool.GetObject());
            }
            Assert.Throws<PoolExhaustedException>(() => pool.GetObject());
            pool.ReturnAll();
        }
        [Fact]
        public void PoolObjMask_And_ChunkMask_AreSynchronisedCorrectly()
        {
            //Accesses _chunkMask member variable of Pool
           static ulong GetChunkMask<T>(Pool<T> pool) where T : class ,IPoolable<T>
            {
                var field = typeof(Pool<T>)
                    .GetField("_chunkMask", BindingFlags.NonPublic | BindingFlags.Instance);

                return (ulong)field!.GetValue(pool)!;
            }
            //Accesses _bitMask member variable of Pool
            static ulong[] GetObjMask<T>(Pool<T> pool) where T : class ,IPoolable<T>
            {
                var field = typeof(Pool<T>)
                    .GetField("_objMask", BindingFlags.NonPublic | BindingFlags.Instance);

                return (ulong[])field!.GetValue(pool)!;
            }
            //When pool is created, bitMask and chunkMask are created correctly
            var pool = Pool<BasicPoolingClass>.Create(new Factory<BasicPoolingClass>(() => new()), 64, 64);
            Assert.Equal(1U, GetChunkMask(pool));
            var objMask = GetObjMask(pool);
            Assert.True(objMask.Length == 1);
            Assert.Equal(UInt64.MaxValue, objMask[0]);
            //When all objects are removed, both masks are cleared
            IPooledObject<BasicPoolingClass>[] poolables = pool.RequestMultiple(64, false);
            Assert.Equal(0U, GetChunkMask(pool));
            objMask = GetObjMask(pool);
            Assert.Equal(0U, objMask[0]);
            //When an arbitrary object is returned, the correct bit is restored and chunkMask reflects availabilty
            int objIndexTest = 27;
            poolables[objIndexTest].ReturnToPool();
            Assert.Equal((ulong)(1 << objIndexTest), GetObjMask(pool)[0]);
            Assert.Equal(1U , GetChunkMask(pool));
        }

        [Fact]
        public void RequestMultipleOnInactivePoolWillThrow()
        {
            //1. Constructor tests
            var pool = new Pool<BasicPoolingClass>();
            Assert.Throws<PoolIsInactiveException>(() => pool.RequestMultiple(11, false));

            pool = new Pool<BasicPoolingClass>(10,new Factory<BasicPoolingClass>(() => new()));
            Assert.Throws<PoolIsInactiveException>(() => pool.RequestMultiple(11, false));
            pool.GenerateList(0);
            Assert.Throws<PoolIsInactiveException>(() => pool.RequestMultiple(11, false));

            pool = Pool<BasicPoolingClass>.Create(new Factory<BasicPoolingClass>(() => new()), 0, 10);
            Assert.Throws<PoolIsInactiveException>(() => pool.RequestMultiple(11, false));
            //Throws after list is destroyed
            pool.GenerateList(10);
            Assert.True(pool.Active);
            pool.DestroyList();
            Assert.Throws<PoolIsInactiveException>(() => pool.RequestMultiple(11, false));
        }
        [Fact]
        public void RequestMultiple_ResizableTrue_PerformsAsExpected()
        {
            var pool = Pool<BasicPoolingClass>.Create(new Factory<BasicPoolingClass>(() => new()), 5, 15);
            Assert.Equal(5, pool.SizeOfPool);
            IPooledObject<BasicPoolingClass>[] poolables = new IPooledObject<BasicPoolingClass>[6];
            Assert.True(pool.RequestMultiple(ref poolables,true));
            Assert.Equal(6, pool.SizeOfPool);
            foreach(var item in poolables)
            {
                Assert.NotNull(item);
                Assert.NotNull(item.Entity);
            }
            IPooledObject<BasicPoolingClass>[] poolables2 = new IPooledObject<BasicPoolingClass>[15];
            Assert.False(pool.RequestMultiple(ref poolables2, true));
            Assert.Equal(15, pool.SizeOfPool);
            for(int i = 0; i < poolables2.Length; i++)
            {
                if(i < 9)
                {
                    Assert.NotNull(poolables2[i]);
                    Assert.NotNull(poolables2[i].Entity);
                }
                else
                {
                    Assert.Null(poolables2[i]);
                }
            }
        }
        [Fact]
        public void ResizeRespectsInactiveState()
        {
            var pool = Pool<BasicPoolingClass>.Create(new Factory<BasicPoolingClass>(() => new()), 0, 10);
            Assert.Throws<PoolIsInactiveException>(() => pool.Resize(5));
            Assert.False(pool.Active);
            Assert.Equal(0, pool.SizeOfPool);
            pool = new();
            Assert.Throws<PoolIsInactiveException>(() => pool.Resize(5));
            Assert.False(pool.Active);
            Assert.Equal(0, pool.SizeOfPool);
            pool = new(10, new Factory<BasicPoolingClass>(() => new()));
            Assert.Throws<PoolIsInactiveException>(() => pool.Resize(5));
            Assert.False(pool.Active);
            Assert.Equal(0, pool.SizeOfPool);
            pool.GenerateList(10);
            pool.Resize(5);
            Assert.Equal(5,pool.SizeOfPool);
            Assert.True(pool.Active);
            pool.DestroyList();
            Assert.Throws<PoolIsInactiveException>(() => pool.Resize(5));
            Assert.False(pool.Active);
            Assert.Equal(0, pool.SizeOfPool);
            pool.GenerateList(10);
            pool.Resize(74);
            Assert.Equal(10,pool.SizeOfPool);
            Assert.True(pool.Active);
            pool.Resize(0);
            Assert.Throws<PoolIsInactiveException>(() => pool.Resize(5));
            Assert.False(pool.Active);
            Assert.Equal(0, pool.SizeOfPool);
        }

        [Fact]
        public void ReduceDoesNothingWhenInactive_WorksOtherwise()
        {
            //Check Reduce works as intended
            var pool = Pool<BasicPoolingClass>.Create(new Factory<BasicPoolingClass>(() => new()), 10, 10);
            pool.Reduce(3);
            Assert.True(pool.Active);
            Assert.Equal(7, pool.SizeOfPool);
            pool.Reduce(11);
            Assert.False(pool.Active);
            Assert.Equal(0, pool.SizeOfPool);
            pool.Reduce(12);
            Assert.False(pool.Active);
            Assert.Equal(0, pool.SizeOfPool);
            pool.GenerateList(10);
            Assert.True(pool.Active);
            Assert.Equal(10, pool.SizeOfPool);
            pool.Reduce(0);
            Assert.True(pool.Active);
            Assert.Equal(10, pool.SizeOfPool);

            pool = new();
            pool.Reduce(1);
            Assert.False(pool.Active);
            Assert.Equal(0, pool.SizeOfPool);

            pool = new(10, new Factory<BasicPoolingClass>(() => new()));
            pool.Reduce(2);
            Assert.False(pool.Active);
            Assert.Equal(0, pool.SizeOfPool);
            pool = Pool<BasicPoolingClass>.Create(new Factory<BasicPoolingClass>(() => new()), 0, 10);
            pool.Reduce(2);
            Assert.False(pool.Active);
            Assert.Equal(0, pool.SizeOfPool);

            pool.GenerateList(10);
            Assert.True(pool.Active);
            Assert.Equal(10, pool.SizeOfPool);
            pool.Reduce(-5);
            Assert.True(pool.Active);
            Assert.Equal(10,pool.SizeOfPool);
        }

        [Fact]
        public void RemoveInvalidWorksProperly()
        {
            static ulong GetChunkMask<T>(Pool<T> pool) where T : class, IPoolable<T>
            {
                var field = typeof(Pool<T>)
                    .GetField("_chunkMask", BindingFlags.NonPublic | BindingFlags.Instance);

                return (ulong)field!.GetValue(pool)!;
            }
            //Accesses _bitMask member variable of Pool
            static ulong[] GetObjMask<T>(Pool<T> pool) where T : class, IPoolable<T>
            {
                var field = typeof(Pool<T>)
                    .GetField("_objMask", BindingFlags.NonPublic | BindingFlags.Instance);

                return (ulong[])field!.GetValue(pool)!;
            }
            Factory<BasicPoolingClass> fac = new(() => new());
            Pool<BasicPoolingClass> pool = Pool<BasicPoolingClass>.Create(fac, 10,20);
            Assert.Equal(10, pool.SizeOfPool);
            var pooled = pool.RequestMultiple(10);
            for(int i = 0; i < pooled.Length; i++)
            {
                pooled[i].ReturnToPool();
                if (i % 2  == 0)
                {
                    ((PoolIdentifier<BasicPoolingClass>)pooled[i]).Dispose();
                }
            }
            pool.RemoveInvalid();
            Assert.Equal(5, pool.SizeOfPool);
            Assert.Equal(1U, GetChunkMask(pool));
            Assert.Equal((ulong)31, GetObjMask(pool)[0]);
            pooled = pool.RequestMultiple(5);
            foreach(var item  in pooled)
            {
                Assert.NotNull(item);
                Assert.NotNull(item.Entity);
                item.ReturnToPool();
            }
           
        }

        [Fact]
        public void RemoveInvalidWillClosePool_IfZeroRemain()
        {
            static ulong GetChunkMask<T>(Pool<T> pool) where T : class, IPoolable<T>
            {
                var field = typeof(Pool<T>)
                    .GetField("_chunkMask", BindingFlags.NonPublic | BindingFlags.Instance);

                return (ulong)field!.GetValue(pool)!;
            }
            //Accesses _bitMask member variable of Pool
            static ulong[] GetObjMask<T>(Pool<T> pool) where T : class, IPoolable<T>
            {
                var field = typeof(Pool<T>)
                    .GetField("_objMask", BindingFlags.NonPublic | BindingFlags.Instance);

                return (ulong[])field!.GetValue(pool)!;
            }

            Factory<BasicPoolingClass> fac = new(() => new());
            Pool<BasicPoolingClass> pool = Pool<BasicPoolingClass>.Create(fac, 100, 101);
            Assert.Equal(100, pool.SizeOfPool);
            var pooled = pool.RequestMultiple(100);
            pooled.ReturnAll();
            for (int i = 0; i < pooled.Length; i++)
            {
                ((PoolIdentifier<BasicPoolingClass>)pooled[i]).Dispose();
            }
            pool.RemoveInvalid();
            Assert.Equal(0, pool.SizeOfPool);
            Assert.Equal(0U, GetChunkMask(pool));
            Assert.Equal(0U, GetObjMask(pool)[0]);
            Assert.False(pool.Active);
            Assert.Throws<PoolIsInactiveException>(() => pool.GetObject());
        }

        [Fact]
        public void ShutDown_ClosesPool_Successfully()
        {
            //Set up
            int cleanupsRun = 0;
            Factory<BasicPoolingClass> fac = new(() => new());
            //tracks when a cleanup is run
            fac.DefineCleanup(_ => cleanupsRun++);
            Pool<BasicPoolingClass> pool = Pool<BasicPoolingClass>.Create(fac, 100, 101);
            //avoid side effects for later in the test
            fac = null;
            //verify size
            Assert.Equal(100, pool.SizeOfPool);

            //two pools for different return behaviours
            IPooledObject<BasicPoolingClass>[] pooledToReturnBeforeShutdown = pool.RequestMultiple(50);
            IPooledObject<BasicPoolingClass>[] pooledToReturnAfterShutdown = pool.RequestMultiple(50);

            //return while pool is alive
            pooledToReturnBeforeShutdown.ReturnAll();
            //Shut down the pool. 50 cleanups should be run immediately, and the pool should close
            pool.ShutDownPool();
            Assert.Equal(50, cleanupsRun);
            Assert.False(pool.Active);

            //release the pool for GC
            pool = null;

            //Return the final 50. Cleanup is deferred to the factory, so this should still run 
            pooledToReturnAfterShutdown.ReturnAll();
            Assert.Equal(100, cleanupsRun);
        }

        [Fact]
        public void VerifySize()
        {
            Factory<BasicPoolingClass> fac = new(() => new());
            Pool<BasicPoolingClass> pool = Pool<BasicPoolingClass>.Create(fac, 100, 101);

            IPooledObject<BasicPoolingClass>[] pooled = pool.RequestMultiple(50);
            Assert.Equal(50,pooled.Length);
            for (int i = 0; i < pooled.Length; i++)
            {
                Assert.NotNull(pooled[i]);
                Assert.NotNull(pooled[i].Entity);
            }
            Assert.True(pool.RequestMultiple(ref pooled));
            for (int i = 0; i < pooled.Length; i++)
            {
                Assert.NotNull(pooled[i]);
                Assert.NotNull(pooled[i].Entity);
            }
        }

        [Fact]
        public void ReturnAllStressTest()
        {

            Factory<BasicPoolingClass> fac = new(() => new());
            Pool<BasicPoolingClass> pool = Pool<BasicPoolingClass>.Create(fac, 4001, 4001);
            IPooledObject<BasicPoolingClass>[] pooled;

            for (int i = 0; i < 500; i++)
            {
                for(int j = 0; j < 4; j++)
                {
                    pooled = pool.RequestMultiple(1000);
                }
                pool.ReturnAll();
            }
        }
    }
 }

