**Factory\<T>**
===========

The `Factory<T>` class is used by `Pool<T>` to dynamically create new pooled objects.
This allows the pool to construct instances of any type without requiring a default constructor.  
By letting the user define how objects should be created, the pool can support everything from simple `new()` 
calls to complex multi‑step construction involving dependency injection, static method calls, calculations, or component decoration.  

A `Factory<T>` simply wraps a `Func<T>` that knows how to create an instance of `T`.
To construct a factory, pass in any function that returns a new `T` - this can be a method group, a lambda expression, 
or a lambda that forms a closure by capturing external variables.

Simple Factory creation  
------

```csharp
Factory<MyClass> myFactory = new Factory<MyClass>(() => new MyClass());
```

The Factory can now create new instances of `T` when needed by calling:  


```csharp
MyClass newInstamce = myFactory.CreateNewObject();
```
The factory can later have its creation updated, so long as it returns `T`:  

```csharp
myFactory.DefineCreation(() => new MyClass(x));
```  
The Factory is also used to orchestrate the destruction of the object through the pool.  
This happens in the same whay the creation is defined, but is not needed to create the factory,
as some classes do not need explicit cleanup.

More detailed instructions on how to use `Factory<T>` can be found in -> [Reference](Reference.md) 


