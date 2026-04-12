# **public virtual void DefineCreation(FactoryCreationMethod\<T> )**
--------------------------------------------------------------------

DefineCreation is used to redefine how the factory creates new objects after the Factory\<T>
has already been constructed.  
Calling this method replaces the existing creation delegate - it is not an additive or
chained process.
By default, DefineCreation will ignore null values.  
However, if the supplied method returns null, the factory will still accept it. 
The factory assumes that you are providing a valid creation method and does not
attempt to validate the returned object.


Call DefineCreation with:  
```csharp
myFactory.DefineCreation(() => new MyClass());
```
DefineClass can be supplied with a creation method in the same way as [creating the Factory](Construction.md).  

Next -> [**DefineCleanup( FactoryCleanUpMethod\<T> )**](DefineCleanup.md) 

[**Back to Reference**](../Reference.md) 
