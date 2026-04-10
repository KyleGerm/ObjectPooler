# Pool\<T>.Create(IFactory<T>, int, int)
----------------------------------

`Pool<T>.Create` is a static constructor that will create and initialise a new pool, 
and hand it back to the caller.  
It does this by calling the [constructor](Construction.md) and [`GenerateList`](GenerateList.md), before returning the pool.
This is simply a convenience wrapper, and nothing special is added, except using a try catch to throw a 
[<span style="color:#0AAF31; font-Size:16px;">**PoolInitialisationException**</span>](Exceptions.md) in the case the 
pool creation was unsuccessful.

Call has three parameters:

- [`IFactory<T>`](../../Factory/Reference/IFactory.md)`factory`: The factory given to the pooler to create new objects, and sanitise objects
ready for Garbage Collection.

- `int size`: The size of the pool to be created on initialisation.

- `int maxSize`: The maximum size of the pool, set for the duration of its lifetime.

This can be called using:

```csharp
Pool<Myclass> myPool = Pool<MyClass>.Create(
new Factory<MyClass>(() => new MyClass()), //Factory
				10,        //size
				50);       //maxSize
```

*Note: `size` can be larger than `maxSize` with no side effects, but the size of the pool will only be 
`maxSize` large.*

Next -> [GenerateList](GenerateList.md)

[Back to Reference](../Reference.md)