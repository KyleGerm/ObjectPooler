# IFactory\<T>
--------------

`IFactory<T>` is the interface used by `Pool<T>` to access the public method it needs to 
interact with the implementing factory.

Its properties are as follows:

-  <span style="color:#F2D300; font-Size:16px;">**CreateNewObject**</span>() :  
	Constructs and returns a new instance of `T`.  
	This method must always return a valid, non-null object.
 
	
-  <span style="color:#F2D300; font-Size:16px;">**DisposeObject**</span>( <span style="color:#1AFF94; font-Size:16px;">**T**</span> ) :  
	Performs any cleanup required before the object is released and becomes  
	eligible for garbage collection.  
	Implementations only need to provide logic here if the object holds  
	references or resources that must be cleared.


-  <span style="color:#F2D300; font-Size:16px;">**ValidateFactory**</span>() :  
	 Validates that the factory is correctly configured and able to produce  
	  objects as expected.  
	  Implementations of `Factory<T>` or custom factories may extend this  
	  validation to enforce additional rules.


Next -> [Exceptions](Exceptions.md)
	
[**Back to Reference**](../Reference.md)
