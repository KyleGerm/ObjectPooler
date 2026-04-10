# public void ReplaceInvalid( )
---------------------------

Ensures all elements are valid and non-null.

This will iterate through the pool, checking if each object in the pool has an 
`IPooledObject<T>` wrapper which is not null, and a `T: Entity` which is also not null.  
If either are null, then both are discarded, and a new `T` is created by the `IFactory<T>`, and 
wrapped in a new `IPooledObject<T>`, replacing the invalid value.

This is used to ensure that all elements in the pool are valid, without needing to reduce the 
size of the pool. This does mean that new objects with a fresh state will be created if 
any elements are replaced.

Use cases for this are rare, as there should be no way for an `IPooledObject<T>` wrapper 
to be destroyed, or for the `IPooledObject<T>.Entity` reference to point to a null
object.  
If there is a unique way in which your `T` object evaluates its equality to `null`, e.g in 
Unity, where a GameObject has a fake `null` value if it has been lined up for destruction, 
then using `ReplaceInvalid` may be needed.

Example:
```csharp
myPool.ReplaceInvalid();
```

In cases where object state is important, and knowing the state of an object is 
more crucial than how many are available, then altering the `IFactory<T>` creation method 
should be used prior to calling this method, or you should use `RemoveInvalid` instead.

Next -> [RemoveInvalid](RemoveInvalid.md)

[Back to Reference](../Reference.md)
