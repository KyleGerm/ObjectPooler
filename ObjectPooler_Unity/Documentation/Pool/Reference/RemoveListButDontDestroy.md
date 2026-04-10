# public T[] RemoveListButDontDestroy( )
----------------------------------

Removes all references to objects, and hands back the array of `T` objects to the caller. 

`RemoveListButDontDesroy` does the following in order:

- Adds the `T` Entities to an array to be returned   
- Prepares the `IPooledObject` wrappers for Garbage Collection, and removes its
reference to the wrapper.
- Clears all internal markers used for tracking objects in the pool
- Deactivates the pool.
- Returns `T[]` objects to the caller.

Example:
```csharp
MyClass[] objectsRemovedFromPool = myPool.RemoveListButDontDestroy();

bool myPoolIsActive = myPool.Active; //false
```

After this call, the returned objects are no longer managed by the pool, and 
should be treated as independant `T` objects.

*Note: Because the `IPoolable<T>` wrapper injects itself into the `IPooledObject<T>` on creation, 
a `T` instance may cache a reference to its wrapper. Holding a reference to the wrapper prevents the 
Garbage Collector from disposing this object, therefore it is strongly recommended to either:*
- not cache the `IPooledObject` wrapper   
 or 
- manage the reference properly by dropping it if the object is removed from the pool

Next -> [ReplaceInvalid](ReplaceInvalid.md)

[Back to Reference](../Reference.md)