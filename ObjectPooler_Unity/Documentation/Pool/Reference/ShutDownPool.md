# public void ShutDownPool( )
------------------------

Use this when you want to dispose the entire Pool safely.

`ShutDownPool` will prepare every object for disposal in a safe manner, release all references
to objects within the pool, and close the pool down ready for Garbage Collection.  
This method assumes that you will release the reference to the pool after this is called, and 
cannot be used at all after calling. 

Any objects which are inactive in the pool will be prepared for Garbage Collection using 
`IFactory<T>.DisposeObject`. The `IPooledObject<T>` wrapper will be disposed
and collected, provided the user does not continue holding references to inactive objects within the pool.

Any objects which are active, i.e are checked out of the pool, and should be referenced by another object,
will be scheduled for cleanup when the `IPoolable<T>.ReturnToPool` is called. 
These objects will not go back into the pool at this point, and instead will be cleaned using
`IFactory<T>.DisposeObject`. 

After calling `Pool<T>.ShutDownPool`, a reference to `Pool<T>` does not need to be held anymore, even if references
to objects from the pool are held. The Pool can safely be disposed and collected by GC, as cleanup is deferred
to the `IFactory<T>`, and the `IPooledObject<T>` is left responsible for this action. 

`IFactory<T>` will be collected after the last `IPoolable<T> `object returned to the pool is cleaned, and its reference to 
the `IFactory<T>` is cleared.

Example:

```csharp
//prepare a buffer of size 20
IPooledObject<MyClass> pooledObjects = new PooledObject<MyClass>[20];

//get 20 objects from the pool
myPool.RequestMultiple(ref pooledObjects);

//Pool is prepared for garbage collection
myPool.ShutDownPool();

//Pool can be released safely
myPool = null;

//This can happen safely, as cleanup is deferred to the IFactory<T>
pooledObjects.ReturnAll(); 
```

After `Pool<T>.ShutDownPool` is called, and your final `IPooledObject<T>`s are returned, be sure to also clear 
your references to them, as they are the only objects now still alive at this point.

*Note: Objects handed to the caller by the factory can just be dropped and will be collected, but the cleanup action given to the 
factory will never be run. This can cause resource leaks in the case `T` holds unmanaged resources, and the cleanup
action is the only way to clear this.  
It is recommended to perform at least one of the following three solutions:**
- Explicitly Return all objects, even after the pool was shut down.
- Call [RemoveListButDontDestroy](RemoveListButDontDestroy.md) to gain explicit ownership of objects
before disposing the pool. (Cleanup is now the responsibility of the new owner of the `T` objects.)
- Enusre your `T` object implements `IDisposable` for a deterministic cleanup.

[Back to Reference](../Reference.md)