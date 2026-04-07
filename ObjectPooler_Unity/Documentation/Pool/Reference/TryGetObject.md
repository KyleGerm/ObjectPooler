# public bool TryGetObject( out IPooledObject\<T>, bool = false )
---------------------------------------------------------

Use `TryGetObject` as a safe way to get an object, and have a convenient boolean 
return to determine if the out parameter is safe to use. 

Functionally, `TryGetObject` works in the same way as `GetObject`, the only difference is this method 
is prepared to catch any instance of [<span style="color:#0AAF31; font-Size:16px;">**PoolExhaustedException**</span>](Exceptions.md)
or [<span style="color:#0AAF31; font-Size:16px;">**PoolIsInactiveException**</span>](Exceptions.md), and will instead return false. 

The boolean parameter, like `GetObject` is to allow resizing of the pool if 
no objects are available. By default, this is false.

Example usage:

```csharp
IPooledObject<MyClass> myInstance = null;

if(myPool.TryGetObject( out myInstance )){
	myInstance.Entity.DoSomeWork();
	myInstance.ReturnToPool();
}
```

*Note: This method will still throw if any undefined Exceptions occur. It is only equipped to 
catch [<span style="color:#0AAF31; font-Size:16px;">**PoolExhaustedException**</span>](Exceptions.md),
and will exit early if the pool is inactive, effectively preventing a [<span style="color:#0AAF31; font-Size:16px;">**PoolIsInactiveException**</span>](Exceptions.md).
If this method throws, the cause should be investigated.*

Next -> [RequestMultiple : Allocating ](RequestMultiple_Alloc.md)

[Back to Reference](../Reference.md)