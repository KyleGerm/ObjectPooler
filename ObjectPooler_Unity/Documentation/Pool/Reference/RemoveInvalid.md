# public void RemoveInvalid( ) 
-------------------------------

Remove any instances in the pool which have null values.

This will iterate through the pool, checking if each object in the pool has an 
`IPooledObject<T>` wrapper which is not null or a `T: Entity` which is also not null.  
If either are null, then both are discarded, and the size of the pool is reduced. 

The order of the pool will change as a result, if any elements are found to be null.  
A description of how the order changes can be found at the bottom of this page.

This is used to ensure all elements of the pool are valid, without needing to replace 
any values. This means that the size of the pool will shrink if any elements are 
removed.

Use cases for this are rare, as there should be no way for an `IPooledObject<T>` wrapper 
to be destroyed, or for the `IPooledObject<T>.Entity` reference to point to a null
object.  
If there is a unique way in which your `T` object evaluates its equality to `null`, e.g in 
Unity, where a GameObject has a fake `null` value if it has been lined up for destruction, 
then using `RemoveInvalid` may be needed.

Example:
```csharp
myPool.RemoveInvalid();
```

In cases where pool size is important, and knowing the state of an object is
less crucial than how many are available, using [`ReplaceInvalid`](ReplaceInvalid.md)
may be a better use case, as this will not reduce pool size.

Reordering explained:
--------------------

`RemoveInvalid` will look through the pool, and search for any null values. In the case 
it find a null value, it will replace the value at position `i` with the value at 
position `SizeOfPool - 1` and retest.  
For example:
```
Pool size is 50
Object at index 30 is null

Pool[30] = Pool[SizeOfPool - 1]
```

This means that reordering is deterministic, but unpredictable if you do not know where 
null values are before reordering.

Next -> [ShutDownPool](ShutDownPool.md)

[Back to Reference](../Reference.md)