# public void GenerateList( int )
---------------------------------

`GenerateList` must be used to activate the pool. This will use the `size` parameter
given when calling this method to generate that size pool, the pools set `MaxSize` to verify and limit 
the size of the pool, and the `Factory` to create each new instance of `T`. 

This is the core of the pool, and without this being called, the pool will remain inactive. 

When calling [`Pool<T>.Create`](Create.md), this method does not need to be called again, as it is implicitly
called during the initialisation.
If `GenerateList` is called while the pool is active, i.e. the previous
list has not been destroyed or removed from the pool, then an
[<span style="color:#0AAF31; font-Size:16px;">**ActivePoolOverwriteException**</span>](Exceptions.md)
will be thrown.  
*Note: While the pool could silently ignore the call without throwing, this would be an 
unpredictable behaviour, where the results would not immediately be known. Being 
explicit about the results of the action was chosen to stay deterministic.*

To call `GenerateList`, the `size` parameter only must be given, as `maxSize` and `factory` are supplied on
construction:

```csharp
myPool.GenerateList(10);
```
This will populate the pool with `size` elements using the construction supplied 
to `factory`, and set them to inactive.  
*Note: The pooler does not know what state the object should be in when created, so this should be part 
of the construction process given to factory, so the object is handed back to the pooler in the correct state.*

If you have generated a list, and the pool is active, but you wish to explicitly add more objects to the pool, 
you can do so using [`Resize`](Resize.md).

Next -> [GetObject](GetObject.md)

[Back to Reference](../Reference.md)