# public void Reduce( int )
---------------------------

Reduce the size of a pool by `int: amount`.

This will shrink the size of the pool by calling any dispose method set 
in the `IFactory<T>` passed in on creation, and then rleasing its reference 
to the object.  
If the value of `amount` is zero, this is a no-op.  
If the value of `amount` is equal to or more than the size of the pool, the pool
is destroyed, and the `Pool<T>` object will become inactive until [`GenerateList`](GenerateList.md)
is called again.  

`Reduce` will remove objects from the end of the pool, so this is a LIFO operation.

Example:

```csharp
//make a pool with size of 30
myPool.GenerateList(30);

int size = myPool.SizeOfPool; //30

myPool.Reduce(5); //reduce by 5

size = myPool.SizeOfPool; //25

```

This operation is functionally equivalent to `Resize( SizeOfPool - amount)`, but if the aim is specifically to 
reduce the size of the pool, this avoids one level of indirection and communicates intent more clearly.

Next -> [Resize](Resize.md)

[Back to Reference](../Reference.md)