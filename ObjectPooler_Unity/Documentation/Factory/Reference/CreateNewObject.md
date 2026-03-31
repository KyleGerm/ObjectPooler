# **public virtual T CreateNewObject()**
----------------------------------------

CreateNewObject will use the creation logic supplied to the Factory either on creation,
or upated by [DefineCreation](DefineCreation.md) to create a new instance of T.

This is the simplest method to call in Factory. It can simply be called with:  

```csharp
MyClass newInstance = myFactory.CreateNewObject();
```  

The factory does not validate the result of the creation method.
It assumes that the user has supplied a valid function that returns a correctly‑constructed instance of T.
If you are unsure whether the creation method is valid, you should either:
- Perform your own null or state checks on the returned object, or
- Call [ValidateFactory](Validate.md) before using the factory.
Note that the default implementation of ValidateFactory can only confirm that the factory has a creation method. 
It cannot verify that the objects produced by CreateNewObject are valid or usable.

Next -> [**DisposeObject( T )**](DisposeObject.md) 

[**Back to Reference**](..\Reference.md) 