 # Pool\<T>
 ----------

 The `Pool<T>` class can be used to hold a collection of objects, and be used to dynamically return
 either a single `T` object, or a collection of `T` objects.

 `Pool<T>` also has the ability to create new instances of `T` independently through the use of an `IFactory<T>`,
 supplied on creation by the user. This is an explicit permission which must be alllowed in each call of 
 a `GetObject`, or `RequestMultiple`. By default, the permission is denied to maintain predictability to the user.

 The `Pool<T>` can be constructed most simply through the static constructor `Create`:

 ```csharp
Pool<MyClass> myPool = Pool<MyClass>.Create(new Factory<MyClass>(() => new(), 10, 15));
 ```
 `Create` has a method signure as follows: `Pool<T>.Create(IFactory<T> fac, int size, int maxSize)`,
 where fac is the [factory](..\Factory\Overview.md) supplied, size is the size of the initial pool generated, 
 and maxSize is the maximum size the pool is allowed to grow to through the lifetime of the pool.

The pool can now be used by calling: 
```csharp
IPoolable<MyClass> pooled = myPool.GetObject(resizeable: false);
```
The inner entity can be accessed by calling:  
```csharp
MyClass entity = pooled.Entity;
```
and can be returned by calling:
```csharp
pooled.ReturnToPool();
```

It should be noted that either a reference to the `IPoolable<T>` given by the pool should be kept in order to 
return the object to the pool externally, or the contained object should be given the ability to either return to 
the pool themselves, or expose a method to be returned externally also.  
This is a requirement of the interface which must be implemented for `T` to be an accepted type of `Pool<T>`.

