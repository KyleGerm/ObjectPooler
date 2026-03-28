using KylesUnityLib.Factory;


namespace KylesUnityLib.Pooling.Tests
{
    public class Factory_Tests
    {

        [Fact]
        public void FactoryCanBeCreated()
        {
            Factory<BasicPoolingClass> fac = new(() => new BasicPoolingClass());
            var result = Record.Exception(() => fac.ValidateFactory());
            Assert.Null(result);
        }

        [Fact]
        public void Validate_ThrowsCorrectly()
        {
            Factory<BasicPoolingClass> fac = new(null!);
            var result = Record.Exception(() => fac.ValidateFactory());
            Assert.NotNull(result);
        }

        [Fact]
        public void Obj_CreatedIsNotNull()
        {
            Factory<BasicPoolingClass> fac = new(() => new BasicPoolingClass());
            BasicPoolingClass obj = fac.CreateNewObject ();
            Assert.NotNull(obj);
        }
        [Fact]
        public void Obj_Constructor_is_Used()
        {
            int value = 1;
            Factory<BasicPoolingClass> fac = new(() => new BasicPoolingClass() { Value = value});
            BasicPoolingClass test = fac.CreateNewObject();
            Assert.Equal(value,test.Value);
        }
        [Fact]
        public void FactoryObjDefinitionCanBeChanged_AndIsUsed()
        {
            Factory<BasicPoolingClass> fac = new(() => new BasicPoolingClass());
            var test = fac.CreateNewObject();
            Assert.Equal(0, test.Value);
           static BasicPoolingClass NewConstructor()
            {
                return new BasicPoolingClass()
                {
                    Value = 10
                };
            }
            fac.DefineCreation(NewConstructor);
            BasicPoolingClass newObj = fac.CreateNewObject(); 
            Assert.Equal(10, newObj.Value);
        }
        [Fact]
        public void CreatedObjectsAreUnique()
        {
            Factory<BasicPoolingClass> fac = new(() => new BasicPoolingClass() { Value = 10 });
            var test = fac.CreateNewObject();
            var newObj = fac.CreateNewObject();
            Assert.NotEqual(test, newObj);
        }
        [Fact]
        public void NullCleanUpActionWillBeIgnored()
        {
            Factory<BasicPoolingClass> fac = new(() => new BasicPoolingClass() { Value = 10 });
            var test = fac.CreateNewObject();
            Assert.Equal(10, test.Value);
            fac.DisposeObject(test);
            Assert.Equal(10, test.Value);
        }
        [Fact]
        public void FactoryCleanUpActionCanBeDefined()
        {
            Factory<BasicPoolingClass> fac = new(() => new BasicPoolingClass() { Value = 10 });
            var test = fac.CreateNewObject();
            Assert.Equal(10, test.Value);
            fac.DefineCleanupAction(obj => obj.Value = 0);
            fac.DisposeObject(test);
            Assert.Equal(0, test.Value);
        }

        [Fact]
        public void NullPassedToDefineCreationWillBeIgnored()
        {
            Factory<BasicPoolingClass> fac = new(() => new BasicPoolingClass() { Value = 10 });
            var shouldBeNull = Record.Exception(() => fac.ValidateFactory());
            Assert.Null(shouldBeNull);
            var test = fac.CreateNewObject();
            Assert.Equal(10, test.Value);
            fac.DefineCreation(null!);
            var shouldStillBeNull = Record.Exception(() => fac.ValidateFactory());
            Assert.Null(shouldStillBeNull);
            var Obj_AfterNullDefinition = fac.CreateNewObject();
            Assert.Equal(10, Obj_AfterNullDefinition.Value);
        }

        [Fact]
        public void CleanUpActionCanBeChanged_AndIsUsed()
        {
            //Create object, Define cleanup
            Factory<BasicPoolingClass> fac = new(() => new BasicPoolingClass() { Value = 10 });
            var test = fac.CreateNewObject();
            Assert.Equal(10, test.Value);
            fac.DefineCleanupAction(obj => obj.Value = 0);

            //Assert
            fac.DisposeObject(test);
            Assert.Equal(0, test.Value);

            //Create new object, Define new cleanup
            var newObj = fac.CreateNewObject();
            test.LinkedClass = newObj;
            fac.DefineCleanupAction(x => {
                x.Value = x.LinkedClass!.Value;
                x.LinkedClass.Value = 0;
                x.LinkedClass = null;
            });

            //Assert 2
            fac.DisposeObject(test);
            Assert.Equal(10, test.Value);
            Assert.Equal(0, newObj.Value);
            Assert.Null(test.LinkedClass);
        }
      
    }

 
}