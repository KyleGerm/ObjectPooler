# public Pool(int maxSize, IFactory<T> factory)
----------------------------------------------

This is the constructor for the `Pool<T>` class.
Use this to manually construct a new `Poll<T>`.

`maxSize` is used to set the maximum size of the pool. This is 
unchangable once the pool is created, so assignment should be given with 
regards to the maximum possible size the pool will reach through it's lifetime.  
The maximum possible size which can be given is 4096. This limit exists because 
the pool tracks object availability using a 64-bit chunk system.
In most cases, the restriction to this size should not be an issue, but in the case more are rquired,
the only possible solution is to make more than one pool.

`maxSize` is also used to manage the amount of memory used to track objects in the pool, 
and can be reduced by up to 504 bytes from largest number of objects to smallest.  
(63 x 8 bytes; One 64bit int is needed for each set of 64 objects) 
Using the maxSize argument optimally will result in the smallest memory footprint for your needed pool size.

An [`IFactory<T>`](../../Factory/Reference/IFactory.md) is needed fo the construction of the pool.  
This allows the pool to create new objects, and prepare objects for Garbage Collection.
[`Factory<T>`](../../Factory/Overview.md) is the default class for the pool, but a unique one can 
be created through inheritance, or implementing `IFactory<T>`.

Calling only the constructor does not activate the pool. The pool is not considered active
until a pool has been generated, and the pool can be safely accessed. 
In order to activate the pool after calling the constructor, [`GenerateList`](GenerateList.md) must be called:

Manual construction and activation:  

```csharp
//create a new pool with a maximum size of 64
Pool<MyClass> myPool = new Pool<MyClass>(64, new Factory<MyClass>(() => new MyClass()));

//Generate a pool with a size of 10, and activate the pool
myPool.GenerateList(10);
```

This manual construction is identical to [`Pool<T>.Create`](Create.md), which implicitly calls these steps together 
and returns an initialised pool.

Next -> [Create](Create.md)

[Back to Reference](..\Reference.md) 