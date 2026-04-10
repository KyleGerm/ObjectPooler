# public IPooledObject\<T>[] RequestMultiple(int , bool = false)
---------------------------------------------------------------

`RequestMultiple` is a convenience method which will return an array of `int: size` objects.
This is a good method to use for times where you know that a number of objects are needed, and 
retreiving them in one go is preferred.  
The performance of this method is comparitively faster than `GetObject` on a per object
basis. Multiple objects can be retrieved in one loop, with one call, instead of over multiple calls,
which accounts for most of the time it takes for `GetObject` to return a value.

The boolean parameter has a default value of false. Set this to true if you wish to allow the pool to create 
new instances if there are none available.

Internally, this method utilises `RequestMultiple( ref IPooledObject<T> )` which is non allocating, and a 
faster call. Use this instead if possible.

Example Usage:

```csharp
IPooledObject<MyClass>[] polledObjects = myPool.RequestMultiple(10)
```

`RequestMultiple` will throw [<span style="color:#0AAF31; font-Size:16px;">**PoolExhaustedException**</span>](Exceptions.md)
if it cannot return an array of the requested size.  
`RequestMultiple` will throw [<span style="color:#0AAF31; font-Size:16px;">**PoolIsInactiveException**</span>](Exceptions.md) 
if the pool size is 0, or [`GeneratePool`](GeneratePool.md) has not been called.

Next -> [RequestMultiple : Non-allocating](RequestMultiple_NonAlloc.md)

[Back to Reference](../Reference.md)