# public void ReturnAll( ) 
--------------------------

Returns all currently active objects to the pool.

`Pool<T>` will force‑return every object that is active (i.e., 
currently checked out and not yet returned). 
Inactive objects are unaffected. This method is intended as a 
pool‑level override and should be used with care.

Once an object has been returned-whether individually or via 
`ReturnAll`-it should be considered inactive. The pool may run 
cleanup or validation logic, and the object may be reused immediately.  
**Do not continue using any references you obtained before calling `ReturnAll`.** 
Instead, request a fresh object using [`GetObject`](GetObject.md).

Example:
```csharp
//Get a collection of objects
IPooledObject<MyClass>[] pooledObjects = myPool.RequestMultiple(10);

//Do some work on the objects
foreach(var obj in pooledObjects){
obj.Entity.DoSomeWork();
}

//Return all objects
myPool.ReturnAll();

//pool state has not been respected, and this object may differ from what 
is expected next time it is passed out
pooledObjects[3].Entity.DoSomeWork();

```

*Note: Although returning an object again after using it while 'inactive' may restore its expected state, 
this pattern is not recommended.  
Objects should only be used while they are active and the pool is aware of them. 
Once returned, whether explicitly or via `ReturnAll`,they must not be used again until 
the pool provides them through a new request.*  

Next -> [Reduce](Reduce.md)

[Back to Reference](../Reference)