# IPooledObject\<T>
-------------------

`Pool<T>` creates instances of `IPooledObject<T>` which wraps 
each created instance of `T`.  
This exposes some helpful members which tie the `T` instance to the pool,
and allow `T` to give the wrapper custom return actions. 

------------------------------------------------------

- <span style="color:#1AFF94; font-Size:16px;">**T**</span> Entity :  
The wrapped `T` instance.
- <span style="color:#F2D300; font-Size:16px;">**ReturnToPool**</span>( ) :  
Returns the object to the pool so it can be reused and passed to another caller.
Add to the `OnReturn` property to include your own behaviours when `ReturnToPool` is called.
- <span style="color:#1AFF94; font-Size:16px;">**Action**</span> OnReturn : 
An object specific action which is run each time `ReturnToPool` is called. Add to this 
action to give custom behaviours which reset state.

- <span style="color:#1AFF94; font-Size:16px;">**IPooledObject\<T>**</span>.
<span style="color:#F2D300; font-Size:16px;">**ReturnAll**</span>( ) :   
This static helper method returns a collection of `IPooledObject<T>` objects to the pool.
This can be used in three different ways:

```csharp
IPoolable<MyClass> pooledObjects1 = pool.RequestMultiple(10);
IPoolable<MyClass> pooledObjects2 = pool.RequestMultiple(10);

//Can return individual objects in a single call
IPoolable<MyClass>.ReturnAll( pooledObjects1[0], pooledObjects2[1] );

//Can return a collection of IPooledObjects by calling the extension method
pooledObjects1.ReturnAll();

//Can return multiple collections by calling the static method
IPooledObject<MyClass>.ReturnAll( pooledObjects1, pooledObjects2);
```
Internally, this helper method iterates over the collection and calls `ReturnToPool` on 
each item. This simply hides the boilerplate.

[Back to Reference](../Reference.md)