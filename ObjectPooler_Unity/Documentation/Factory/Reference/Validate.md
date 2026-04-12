 # **public virtual void ValidateFactory()** 
 ------------------------------------------

 ValidateFactory is used to verify that the factory has its most basic functionality intact.  
 The default implementation of ValidateFactory will only check the creation delegate is not null.  

 Because the Factory does not need a cleanup delegate to operate, and cannot ensure the safe 
 disposal of the object, ValidateFactory cannot check the output of the creation delegate,
 and therefore trusts the user has provided a valid delegate to produce a T instance. 

 The default ValidateFactory will throw an ArgumentNullException if the creation delegate is null.  
 For this reason, ValidateFactory should be wrapped in a try catch block so you can handle possible exceptions:

 ```csharp
 //Valid factory, will not throw
 Factory<MyClass> myFactory = new Factory<MyClass>(() => new MyClass());
 try{
	myFactory.ValidateFactory();
 } catch {}

 //Valid factory, produces a null object, but the delegate is assigned
  Factory<MyClass> myFactory = new Factory<MyClass>(() => null);
 try{
	myFactory.ValidateFactory();
 } catch {}

 //Invalid factory, ArgumentNullException will be thrown
  Factory<MyClass> myFactory = new Factory<MyClass>(null);
 try{
	myFactory.ValidateFactory();
 } catch {}
 ```

 In inherited versions of Factory\<T>, since all aspects of the factory's behaviour can be 
 overriden, ValidateFactory should be defined appropriately to ensure the factory is in a valid 
 state.  
   
Next -> [Inheriting Factory\<T>](InheritingFactory.md)

 [**Back to Reference**](../Reference.md) 
