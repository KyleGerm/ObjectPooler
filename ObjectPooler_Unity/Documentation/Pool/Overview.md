 # Pool\<T>
 ----------

 The `Pool<T>` class can be used to hold a collection of objects, and be used to dynamically return
 either a single `T` object, or a collection of `T` objects.

 `Pool<T>` also has the ability to create new instances of `T` independently through the use of an `IFactory<T>`,
 supplied on creation by the user. Creation of new instances requires explicit permission on each call to GetObject or RequestMultiple.
 By default, the permission is denied to maintain predictability to the user.  

 To be accepted into `Pool<T>`, `T` must implement `IPoolable<T>`.  
`IPoolable<T>` allows the pool to inject an `IPooledObject<T>` wrapper into each instance.  
This gives the object a way to define custom return behaviour through `IPooledObject<T>.OnReturn`


 The `Pool<T>` can be constructed most simply through the static constructor `Create`:

 ```csharp
Pool<MyClass> myPool = Pool<MyClass>.Create(new Factory<MyClass>(() => new(), 10, 15));
 ```
 `Create` has a method signature as follows: `Pool<T>.Create(IFactory<T> fac, int size, int maxSize)`,
 where fac is the [factory](..\Factory\Overview.md) supplied, size is the size of the initial pool generated, 
 and maxSize is the maximum size the pool is allowed to grow to through the lifetime of the pool.

The pool can now be used by calling: 
```csharp
IPooledObject<MyClass> pooled = myPool.GetObject(resizeable: false);
```
The inner entity can be accessed by calling:  
```csharp
MyClass entity = pooled.Entity;
```
and can be returned by calling:
```csharp
pooled.ReturnToPool();
```

It should be noted that either a reference to the `IPooledObject<T>` given by the pool should be kept in order to 
return the object to the pool externally, or the contained object should be given the ability to either return to 
the pool themselves, or expose a method to be returned externally also. 

 You can control the size of `Pool<T>` explicitly by calling:

 ```csharp
 //This will remove 1 object from the pool
 myPool.Reduce(1);

 /* This will resize the pool in either direction
 * Resizing the pool to 0 or less will destroy the list
 * This method has the ability to either create or destroy objects depending on the difference 
 * between the pool size, and the number given to resize to. */

 myPool.Resize(15);
 ```  

 and can destroy the list using:

 ```csharp

 //Both methods here destroy the list, and make the pool unusable until
 //a new one is made with GenerateList.

 myPool.DestroyList();

 //This method destroys the list, but will give all objects in the list back instead of destroying them.

 MyClass[] returnedObjects = myPool.RemoveListButDontDestroy();
 ```  
 A full pool teardown can be called by calling:

 ```csharp
 myPool.ShutDownPool();
 ```
 this is a complete teardown, and the pool cannot be used after this is called. 
 This prepares the pool for garbage collection, and any objects inside the pool are
 prepared for disposal also.

 More information on the pool can be found in -> [Reference](Reference.md)





