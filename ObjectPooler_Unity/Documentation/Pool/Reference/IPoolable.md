# IPoolable\<T>
---------------

This interface holds the contract which `IPooledObject<T>` needs 
to be able to operate on a `T` instance, and is the requirement of a `T` instance to 
be a pooled object.

-  <span style="color:#F2D300; font-Size:16px;">**InjectPoolable**</span>
( <span style="color:#1AFF94; font-Size:16px;">**IPooledObject\<T>**</span> ) :  
This is used by the `T` object to pass its own return logic into the `IPooledObject<T>` wrapper.  
When `IPooledObject<T>.ReturnToPool` is called, any `T` specific return actions passed to the wrapper 
through this method will be run. Use this to restore state to a `T` object, so it is in a predictable 
state when it is passed out again.

```csharp
public class MyClass : IPoolable<MyClass>{
	public int Value = 0;
	public bool IsActive;
	private void ResetValue(){
		Value = 0;
	}

	private void ResetInactive(){
		IsActive = false;
	}

	public void InjectPoolable(IPooledObject<MyClass> wrapper){
		wrapper.OnReturn += ResetValue;
		wrapper.OnReturn += ResetInactive;
	}
}
```
It is recommended not to cache the reference to the `IPooledObject<T>`, as it may prevent garbage collection
when the object it disposed. The wrapper does not have any useful properties to the `T` object unless you want 
a way to return the object through `T`. 

[Back to Reference](../Reference.md)