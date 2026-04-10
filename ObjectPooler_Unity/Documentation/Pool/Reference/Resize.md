# public void Resize( int )
--------------------------

`Resize` allows you to directly control the size of a pool which
has already been generated.  
The `int: amount` passed to `Resize` will be the target size of the pool.  
If `amount` is 0, the pool will be destroyed, and the `Pool<T>` object will
be set to inactive.  
If `amount` is larger than the `MaxSize` of the pool, the pool will only grow
to `MaxSize` objects.  

`Resize` can be used to grow or shrink the pool, and uses the `IFactory<T>` given 
on construction to create objects, or clean them before clearing its reference to 
the object, so it can be collected.

Example:
```csharp
Pool<MyClass> myPool = Pool<MyClass>.Create(new Factory<MyClass>(() => new MyClass()), 10, 50);
int size = myPool.SizeOfPool; // 10

myPool.Resize(15);
size = myPool.SizeOfPool; //15

myPool.Resize(500);
size = myPool.SizeOfPool; //maxSize was set to 50, so SizeOfPool is capped at 50

myPool.Resize(0); //Pool is destroyed. myPool is inactive

// This will throw a PoolIsInactiveExeption, since the pool was destroyed when resized to 0
IPooledObject<MyClass> myObject = myPool.GetObject()
```

*Note: Using Resize when the pool is inactive will throw 
[<span style="color:#0AAF31; font-Size:16px;">**PoolIsInactiveException**</span>](Exceptions.md).
The pool must be explicitly regenerated using [`GenerateList`](GenerateList.md) before it can be used again, 
so that internal state of the pool can be syncronized correctly, and it is clear that new objects have been created.*

Next -> [DestroyList](DestroyList.md)

[Back to Reference](../Reference.md)