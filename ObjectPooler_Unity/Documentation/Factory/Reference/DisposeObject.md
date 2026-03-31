# **public virtual void DisposeObject( T )** 
--------------------------------------------

DisposeObject is used to prepare T instances to be reclaimed by the GC. In order for 
DisposeObject to do anything, [DefineCleanup](DefineCleanup.md) should be used to provide 
a way to clean objects.  
If no cleanup delegate is provided to the Factory, the use of DisposeObject will simply do nothing.

```csharp
//Factory is created with a construction method using an external member
Dependancy dependancy = new Dependancy();
Factory<MyClass> myFactory = new Factory<MyClass>(() => new MyClass(dependancy));


MyClass instance = myFactory.CreateNewObject();
//Because cleanup has not been defined, DisposeObject does nothing
myFactory.DisposeObject(instance);

myFactory.DefineCleanup(instance => instance.Dependancy = null);
//Now the cleanup is defined, DisposeObject will perform the delegate on any objects passed to it
myFactory.DisposeObject(instance);
```
Use DisposeObject when you are ready to release any references held by the object so that the 
garbage collector can reclaim it safely once it becomes unreachable.  

[**Back to Reference**](..\Reference.md) 
