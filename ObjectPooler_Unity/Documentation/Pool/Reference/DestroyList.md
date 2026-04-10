# public void DestroyList( Action\<T> = null )
---------------------------------------------

All instances of `T` contained in the pool will be prepared for Garbage Collection by running
`IFactory<T>.DisposeObject` on each object, and releasing its reference to the object.

An `Action<T>` can be given as a parameter if you wish any other actions to be taken on each object
as the pool is being destroyed. 

Example:
```csharp
MyClass myClass = new MyClass();

myPool.DestroyList(disposed => myClass.Value += disposed.Value);
```  

The `Action<T>` given will be run before any clean up methods, so the current state of each
object is preserved for any actions passed into the method.

If `DestroyList` is called when the pool is inactive, the method becomes a no-op.

`Resize` and `Reduce` each use `DestroyList` if the requirement is met, but an action cannot be passed
into these methods. Use `DestroyList` exclusively if any other operations are needed, or pass the
action into `IFactory<T>.DefineCleanup`.

Next -> [RemoveListButDontDestroy](RemoveListButDontDestroy.md)

[Back to Reference](../Reference.md)