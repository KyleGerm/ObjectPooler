#  Pool\<T> Reference
---------------------

Below are more detailed explanations on the public methods of Pool\<T>.  
Click on the links to lean more.

**Pool\<T>**  
~   where T : class , IPoolable\<T>

Properties:
-----------

- int: SizeOfPool:  
How many elements are currently in the pool
- int: MaxSize:  
The maximum size the pool can reach
- bool: Active  
If the pool is active, and able to be used
- IFactory\<T>: Factory  
The factory passed into the pool on creation

Methods:
--------
- [**Pool( int, IFactory\<T>**)](Reference/Construction.md)
- [**Pool\<T>.Create(IFactory\<T>, int, int )**](Reference/Create.md)
- [**GenerateList( int )**](Reference/GenerateList.md)
- [**GetObject( bool = false )**](Reference/GetObject.md)
- [**TryGetObject( bool = false )**](Reference/TryGetObject.md)
- [**RequestMultiple(int, bool = false )**](Reference/RequestMultiple_Alloc.md)
- [**RequestMultiple(ref IPooledObject\<T>\[ \], bool = false )**](Reference/RequestMultiple_NonAlloc.md)
- [**Reduce( int )**](Reference/Reduce.md)
- [**Resize( int )**](Reference/Resize.md)
- [**ReturnAll( )**](Reference/ReturnAll.md)
- [**RemoveInvalid( )**](Reference/RemoveInvalid.md)
- [**ReplaceInvalid( )**](Reference/ReplaceInvalid.md)
- [**ShutDownPool( )**](Reference/ShutDownPool.md)
- [**DestroyList( Action\<T> = null)**](Reference/DestroyList.md)
- [**RemoveListButDontDestroy( )**](Reference/RemoveListButDontDestroy.md)

 [**Exceptions**](Reference/Exceptions.md)
 -----------------------------------------
