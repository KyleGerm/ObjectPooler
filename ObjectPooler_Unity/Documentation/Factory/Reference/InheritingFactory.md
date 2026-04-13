# Inheriting `Factory<T>`
------------------------

If you want to encapsulate more complex logic inside a dedicated factory class, 
you can inherit from `Factory<T>`. This gives you full access to the factory’s internal structure 
and allows you to override any part of its behaviour.  
`Pool<T>` only depends on the `IFactory<T>` interface, so you are free to either:
- implement `IFactory<T>` directly, or
- inherit from `Factory<T>` for convenience and built‑in lifecycle handling.  

Both approaches are fully compatible with Pool\<T>.  


Inheriting `Factory<T>`:  
----------------------
`Factory<T>` requires a creation delegate so it can construct new instances of `T`.  
Because of this, any class inheriting `Factory<T>` must call a base constructor that supplies a valid creation method.  

A simple specialisation might look like this:

```csharp
class MyClassFactory : Factory<MyClass>{

//Define a default constructor to call the base constructor, and define creation in base
     public MyClassFactory() : base(() => new MyClass())
    {
        //Use this to create any extra logic
        DefineCleanup(instance => instance.Delegate = null);
    }

    //Because MyClassFactory will always contain a Cleanup action unless DefineCleanup is called again, 
    //specialised validation can be defined

    public override void ValidateFactory() {
            if(_creationAction == null) throw ArgumentNullException("Creation delegate is null");
            MyClass test = CreateNewObject();
            if(test == null) throw new NullObjectConstructionException("Tested instance of MyClass returned null");
            DisposeObject(test);
        }
}
```
Here:
- The derived constructor supplies a default creation method to the base class  
- The factory is now specialised for producing MyClass instances
- You can still redefine creation or cleanup later using [DefineCreation](DefineCreation.md) and [DefineCleanup](DefineCleanup.md),
just like a normal `Factory<T>`.  
This preserves the flexibility of `Factory<T>` while allowing you to encapsulate custom behaviour in a dedicated class.

As earlier mentioned, every method can be overridden, but to maintain the flexibility the `Factory<T>` provides, it is recommended to 
leave these as they are, unless a significant reason to change them arises, such as changing the logic in [ValidateFactory](Validate.md).

Implementing `IFactory<T>` example:
---------------------------------

In this example, the same behaviour is defined as the example above, however since `IFactory<T>` is implemented, 
each method must be explicitly defined:

```csharp
   public class MyClassFactory : IFactory<MyClass>
    {
        
        public MyClass CreateNewObject() {
            return new MyClass();
        }

        //DisposeObject can directly hold the logic for sanitising an object
        public void DisposeObject(MyClass disposed) {
            disposed.Dependancy = null;
        }

        //Since a specialised implementation of IFactory can garuntee a way to prepare
        //object for disposal, a more thorough Validation can exist

        public void ValidateFactory() {
            if(_creationAction == null) throw ArgumentNullException("Creation delegate is null");
            MyClass test = CreateNewObject();
            if(test == null) throw new NullObjectConstructionException("Tested version of MyClass returned null");
            DisposeObject(test);
        }
    }
```
While this does leave you free to create your own implementation, this breaks the flexibility of a default `Factory<T>`,
unless care is taken to design it in a similar way.

The freedom is given to the user to best shape their factory in a way they see fit, while still being compatible with Pool\<T>.

Next -> [IFactory\<T>](IFactory.md)

[**Back to Reference**](../Reference.md) 
