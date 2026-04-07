# public bool RequestMultiple(ref IPooledObject\<T>, bool = false)
-----------------------------------------------------------------

`RequestMultiple` will attempt to fill the given buffer with available objects, 
and will return true or false based on the success of this. 

The boolean parameter has a default value of false. Set this to true if you wish to allow the pool to create 
new instances if there are none available.

**Important:**  
If the method returns false, this does not mean that the buffer is empty. 
The buffer will contain any objects that were successfully placed in the buffer.  
These objects will be marked as in use, and if they cannot be used, they should 
be returned by the caller.  

If it is critical that a full buffer is returned, use the allocating version 
of `RequestMultiple` as this will either return successfully, or throw.

Example Usage:

```csharp
IPooledObject<MyClass>[] myObjectBuffer = new IPooledObject<MyClass>[10];

if(myPool.RequestMultiple(ref myObjectBuffer)){
	foreach(var item in myObjectBuffer){
	 item.Entity.DoSomeWork();
	 item.ReturnToPool();
	}
}
else{
	foreach(var item in myObjectBuffer){
		if(item != null){
			item.ReturnToPool();
		}
	}
}
```

Next -> [ReturnAll](ReturnAll.md)

[Back to Reference](../Reference.md)