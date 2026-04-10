# Exceptions
------------

The following exceptions are designed to give the caller 
an informative context as to why something may not be working in 
relation to the pool.   
Exceptions are as follows:

- <span style="color:#0AAF31; font-Size:24px;">**PoolInitialisationException**</span>  
This will throw in `Pool<T>.Create`, where control has reached
the end of scope without being able to return a valid pool instance.
This will contain the original exception, which may be any of the other 
exceptions in this list.  
This normally means parameters have not been given valid values, so check your 
setup and retry.

- <span style="color:#0AAF31; font-Size:24px;">**ActivePoolOverwriteException**</span>  
Will throw specifically when calling [`GenerateList`](GenerateList.md) on an already active pool.  
Since the pool is responsible for managing the lifetime of pooled objects, it cannot 
overwrite objects which have been created, and not scheduled for cleanup. Calling 
`GenerateList` on an active pool is unexpected, and should be brought to the callers attention.  
Use [`Resize`](Resize.md) if managing the size of the pool is your intention.

- <span style="color:#0AAF31; font-Size:24px;">**PoolIsInactiveException**</span>  
Throws when an inactive pool is used to manage objects.  
[`GetObject`](GetObject.md), [`RequestMultiple - Allocating`](RequestMultiple_Alloc.md), and [`Resize`](Resize.md)
will throw if used when inactive. Other methods are safe from this exception, 
as they do not expect an object, or ask for one to be created.

- <span style="color:#0AAF31; font-Size:24px;">**PoolExhaustedException**</span>  
Will throw when the caller has asked for, and expects an object back, and the pool cannot provide it.   
[`GetObject`](GetObject.md) and [`RequestMultiple - Allocating`](RequestMultiple_Alloc.md) will throw if the 
pool has run out of inactive objects to return, without being able to create new ones, or if the pool has run out 
of objects to return and cannot make a new object as it is already at maximum size.  
Safe methods can be used which will not throw this exception such as [`TryGetObject`](TryGetObject.md) or 
[`RequestMultiple - Non Allocating`](RequestMultiple_NonAlloc.md). These return a boolean false on failure instead,
and catch <span style="color:#0AAF31; font-Size:14px;">**PoolExhaustedException**</span> and 
<span style="color:#0AAF31; font-Size:14px;">**PoolIsInactiveException**</span> internally

[Back to Reference](../Reference.md)