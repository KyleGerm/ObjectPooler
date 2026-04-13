using KylesUnityLib.Internal.Pooling;


namespace KylesUnityLib.Pooling.Tests
{
    public class PoolIdentifier_Tests
    {
        [Fact]
        public void PoolIdentifierCreationIsNotNull()
        {
            PoolIdentifier<BasicPoolingClass> poolIdentifier = new(new BasicPoolingClass());
            Assert.NotNull(poolIdentifier);
        }
        [Fact]
        public void PoolIdentifierWrappedObjectShows()
        {
            BasicPoolingClass pool = new();
            PoolIdentifier<BasicPoolingClass> poolIdentifier = new(pool);
            Assert.NotNull(poolIdentifier.Entity);
            Assert.Equal(pool, poolIdentifier.Entity);
        }
        [Fact]
        public void IdentifyingValuesAreSetCorrectly()
        {
            PoolIdentifier<BasicPoolingClass> poolIdentifier = new(new BasicPoolingClass());
            poolIdentifier.SetIdentifier(5, 10U);
            Assert.Equal(5, poolIdentifier.ChunkIndex);
            Assert.Equal(10U, poolIdentifier.BitMask);
        }
        [Fact]
        public void OnReturnActionIsSetAndUsed()
        {
            int testingValue = 5;
            PoolIdentifier<BasicPoolingClass> poolIdentifier = new(new BasicPoolingClass());
            poolIdentifier.OnReturn += () => {
                testingValue += 5;
                poolIdentifier.SetIdentifier(3, 6U);
            };
            Assert.Equal(5, testingValue);
            Assert.Equal(0, poolIdentifier.ChunkIndex);
            Assert.Equal(0U, poolIdentifier.BitMask);

            poolIdentifier.ReturnToPool();
            Assert.Equal(10, testingValue);
            Assert.Equal(3, poolIdentifier.ChunkIndex);
            Assert.Equal(6U, poolIdentifier.BitMask);
        }
        [Fact]
        public void NullOnReturnIsIgnored()
        {
            PoolIdentifier<BasicPoolingClass> poolIdentifier = new(new BasicPoolingClass());
            var result = Record.Exception(() => poolIdentifier.ReturnToPool());
            Assert.Null(result);
        }

        [Fact]
        public void NotifyPoolEventIsSetAndUsed()
        {
            PoolIdentifier<BasicPoolingClass> poolIdentifier = new(new BasicPoolingClass());
            poolIdentifier.notifyPool += (identifier) =>
            {
                identifier.SetIdentifier(5, 10U);
                identifier.Entity.Value = 15;
            };

            Assert.Equal(0, poolIdentifier.Entity.Value);
            Assert.Equal(0, poolIdentifier.ChunkIndex);
            Assert.Equal(0U, poolIdentifier.BitMask);

            poolIdentifier.ReturnToPool();
            Assert.Equal(15, poolIdentifier.Entity.Value);
            Assert.Equal(5, poolIdentifier.ChunkIndex);
            Assert.Equal(10U, poolIdentifier.BitMask);
        }

       /* [Fact]
        public void NotifyPoolIsClearedAfterUse()
        {
            bool hasBeenChanged = false;
            PoolIdentifier<BasicPoolingClass> poolIdentifier = new(new BasicPoolingClass());
            poolIdentifier.notifyPool += (identifier) =>
            {
                hasBeenChanged = true;
            };
            poolIdentifier.ReturnToPool();
            Assert.True(hasBeenChanged);
            hasBeenChanged = false;
            poolIdentifier.ReturnToPool();
            Assert.False(hasBeenChanged);
        }*/
        [Fact]
        public void ClearEventsRemovesActions()
        {
            bool notifyPoolHasBeenRun = false;
            bool onReturnHasBeenRun = false;
            PoolIdentifier<BasicPoolingClass> poolIdentifier = new(new BasicPoolingClass());
            poolIdentifier.notifyPool += (identifier) =>
            {
                notifyPoolHasBeenRun = true;
            };
            poolIdentifier.OnReturn += () =>
            {
                onReturnHasBeenRun = true;
            };
            poolIdentifier.ClearEvents();
            poolIdentifier.ReturnToPool();
            Assert.False(notifyPoolHasBeenRun);
            Assert.False(onReturnHasBeenRun);
        }
        [Fact]
        public void DisposeCleansReferences()
        {
            bool notifyPoolHasBeenRun = false;
            bool onReturnHasBeenRun = false;
            PoolIdentifier<BasicPoolingClass> poolIdentifier = new(new BasicPoolingClass());
            poolIdentifier.notifyPool += (identifier) =>
            {
                notifyPoolHasBeenRun = true;
            };
            poolIdentifier.OnReturn += () =>
            {
                onReturnHasBeenRun = true;
            };
            poolIdentifier.Dispose();
            Assert.False(notifyPoolHasBeenRun);
            Assert.False(onReturnHasBeenRun);
            Assert.Null(poolIdentifier.Entity);
        }
      /*  [Fact]
        public void OnReturn_DoesNotRunTwice()
        {
            int notifyPoolHasBeenRun = 0;
            int onReturnHasBeenRun = 0;
            PoolIdentifier<BasicPoolingClass> poolIdentifier = new(new BasicPoolingClass());
            poolIdentifier.notifyPool += (identifier) =>
            {
                notifyPoolHasBeenRun += 1;
            };
            poolIdentifier.OnReturn += () =>
            {
                onReturnHasBeenRun += 1;
            };
            poolIdentifier.ReturnToPool();
            poolIdentifier.ReturnToPool();
            Assert.Equal(1, notifyPoolHasBeenRun);
            Assert.Equal(1, onReturnHasBeenRun);

        }*/

        [Fact]
        public void ActionsAreRunInCorrectOrder()
        {
            List<string> id = [];
            PoolIdentifier<BasicPoolingClass> poolIdentifier = new(new BasicPoolingClass());
            poolIdentifier.notifyPool += (identifier) =>
            {
                id.Add("notifyPool");
            };
            poolIdentifier.OnReturn += () =>
            {
                id.Add("onReturn");
            };
            poolIdentifier.ReturnToPool();
            Assert.Equal(["onReturn", "notifyPool"], id.ToArray());
        }
    }
}
