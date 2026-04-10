# public IPooledObject\<T> GetObject( bool = false )  
--------------------------------------

`GetObject` returns a single `IPooledObject<T>` from the pool, and marks
the object as active. This object will then not be returned with future calls until
the object has been returned via `IPooledObject<T>.ReturnToPool`, where it is marked 
as inactive, and any object return logic passed to the `IPooledObject` wrapper will run.

The method call has a parameter, `resizeable` set to false as default. This disables the
`Pool<T>`'s ability to create a new object and return it if none are available when `GetObject`
is called.

Using `GetObject` assumes you are expecting there to be an object available, and will not fail
silently if it cannot return one.  
If this happens, a 
[<span style="color:#0AAF31; font-Size:16px;">**PoolExhaustedException**</span>](Exceptions.md) will
be thrown.

If you cannot be certain there is an available object, or enough space to make a new one 
when resizing is allowed, use [`TryGetObject`](TryGetObject.md), or prepare for 
a potential throw with a try block.

```csharp
//Returns an object while resizable is false
IPooledObject<MyClass> availableObject = myPool.GetObject();

//Returns an object, resizing if needed
IPooledObject<MyClass> availableObject = myPool.GetObject(true);

//Prepares against an exception being thrown
try
{
	IPooledObject<MyClass> availableObject = myPool.GetObject();
}
catch(PoolExhaustedException){}
```

`GetObject` will also throw 
[<span style="color:#0AAF31; font-Size:16px;">**PoolIsInactiveException**</span>](Exceptions.md)
if you try to use it before [`GenerateList`](GenerateList.md) has been called, or after [`ShutDownPool`](ShutDownPool.md)
has been called, in which case the pool is unusable, and should be discarded. 

Next -> [TryGetObject](TryGetObject.md)

[Return to Reference](../Reference.md)