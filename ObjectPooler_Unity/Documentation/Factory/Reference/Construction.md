# **public Factory\<T>( FactoryCreationMethod\<T> )**
-------------------------------------

This is the constructor for Factory\<T>.  
The factory must be supplied with a valid T returning method, to be able to create new instances of T.  

There are three main ways of setting up the factory, each with different reasons for usage, but any will create 
a valid factory. 

Lambda definition
-----------------
The easiest and fastest way to create a Factory, is by supplying a lambda expression.  

A lambda is ideal when the construction of T is self‑contained and does not 
rely on external state. This makes it fast, predictable, and easy to read.
 

**Simple construction**  
```csharp
Factory<MyClass> myFactory = new Factory<MyClass>(() => new MyClass());
```
Lambdas can also contain multi‑step setup logic, 
as long as all required values are created inside the lambda:  

```csharp
// Lambda expression used in factory creation
Factory<MyClass> myFactory = new Factory<MyClass>(
    () =>
    {
        int x = 10;
        Dependacy injectible = new Dependancy();
        MyClass instance = new MyClass(x, injectible);
  
        return instance;
    }
);
```

This keeps the construction logic local to the factory and avoids capturing external variables.  
(Closures - lambdas that do capture external state - are covered in the next section.)

Closure definition
------------------  
A closure looks identical to a lambda expression, but behaves differently when it captures variables from the surrounding scope.  
In the earlier example, all values were created inside the lambda, so nothing was captured.  
A closure is created when the factory’s creation method uses variables defined outside the lambda.  
This allows the factory to reference objects that may change over time, such as a shared Dependancy instance:  

```csharp
//Dependancy created ouside the lambda
Dependancy shared = new Dependancy();

Factory<MyClass> myFactory = new Factory<MyClass>(
() => 
    {
        int x = 10;
        MyClass instance = new MyClass(x, shared);
        return instance;
    }
);

```  

Closures capture variables, not values. This means that if the variable changes after the closure is defined, 
the closure will see the updated value when it runs. This behaviour can be powerful, 
but it can also introduce subtle bugs if the captured state changes unexpectedly.
Use closures with care when the external variables are mutable or shared across multiple systems.

```csharp
//x is created externally 
int x = 10;

//Factory is made 
Factory<MyClass> myFactory = new Factory<MyClass>(
() => 
    {
        Dependancy injected = new Dependancy();
        //x is captured, not the value

        MyClass instance = new MyClass(x, injected);
        return instance;
    }
);

x = 15;
//now when Factory creates a new instance, T will be created with x having a value of 15, not 10 
```  


Method definition
-----------------
Passing a method group to the constructor is often the easiest approach to understand.
Functionally, it behaves the same as a lambda or closure, but placing the creation
logic inside a named method can make the code easier to read, test, and maintain.
This approach is useful when:
- the construction logic is long enough that it doesn’t belong inline
- the creation process is reused elsewhere
- you want to avoid capturing external variables accidentally
- you prefer explicit, named behaviour over inline expressions


```csharp
public MyClass CreateNewInstance()
{
    MyClass instance = new MyClass();
    // any other logic
    return instance;
}

Factory<MyClass> myFactory = new Factory<MyClass>(CreateNewInstance);
```  
Using a method group keeps the factory definition clean and makes 
the creation logic discoverable and self‑documenting, especially in larger systems. 

The constructor will call [ValidateFactory](Validate.md) after assigning the given delegate to the factory.  
If the delegate is null, this will cause an ArgumentNullException to be thrown.

Next -> [**CreateNewObject()**](CreateNewObject.md)

[**Back to Reference**](..\Reference.md) 
