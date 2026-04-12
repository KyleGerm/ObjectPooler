# Exceptions
-------------
In the default implementation of Factory\<T>, the only exception which will
be thrown is <span style="color:#0AAF31; font-Size:18px;"> **ArgumentNullException**</span>.  
This is thrown in [ValidateFactory](Validate.md), where the creation delegate has been supplied a null value, 
meaning the Factory cannot perform even its most basic operation.

Users implementing custom validation of the result of  
[CreateNewObject](CreateNewObject.md) may choose to throw 
<span style="color:#0AAF31; font-size:18px;"><strong>NullObjectConstructionException</strong></span>  
to indicate that the creation delegate returned a null object.


*Note: Factory\<T> cannot safely throw  <span style="color:#0AAF31; font-Size:16px;"> **NullObjectConstructionException**</span>
as there is no garunteed safe cleanup of an object after creation. Since only the factory would own this object, and therefore 
would be the responsibility of the factory to safely clean up the object, a test for null values cannot be performed.  

Instead, Pool\<T> will throw  <span style="color:#0AAF31; font-Size:16px;"> **NullObjectConstructionException**</span> 
in the case that the object handed back to the pool is null.*
 
 [**Back to Reference**](../Reference.md) 
