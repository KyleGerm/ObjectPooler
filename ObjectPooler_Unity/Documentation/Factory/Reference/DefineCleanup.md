# **public virtual void DefineCleanup( FactoryCleanupMethod\<T> )**
-----------------------------------------------------------------

You can define any actions which need to be completed to detatch the object from any references
before destruction. In most cases, this is not required, and as such, the factory will still work 
as intended without giving an explicit definition.

In the case where all dependancies are owned by the object, explicit cleanup is not needed:

```csharp
	Factory<MyClass> myFactory = new Factory<MyClass>(CreateNewObject);

	static MyClass CreateNewObject(){
		MyClass instance = new MyClass();
		instance.dependancy = new Dependancy();
		instance.x = 10;

		return instance;
	}
```  

In this instance, because each MyClass instance owns their own reference to Dependancy, when MyClass is 
destroyed by GC, so is the contained Dependancy.

However, when using patterns such as dependency injection, cleanup may be required to prevent 
dangling references or unintended retention of shared objects:

```csharp
	
	static Dependancy shared = new Dependancy();

	//Object creation contains a reference to an external dependancy
	static MyClass CreateNewObject(){
		MyClass instance = new MyClass();
		instance.dependancy = shared;
		instance.x = 10;

		return instance;
	}

	Factory<MyClass> myFactory = new Factory<MyClass>(CreateNewObject);

	//Factory now needs a cleanup definition to clear references before destruction

	myFactory.DefineCleanUp(instance => instance.dependancy = null)
	
```

Using cleanup as a preventative measure, even when not strictly required, 
reduces the risk of memory leaks, stale references, unnecessary memory retention, 
and long‑term performance degradation.  
To be clear, the factory does not *destroy** the object.  
The factory only performs the cleanup necessary for the garbage collector to safely 
reclaim the object when it becomes unreachable.


[**Back to Reference**](..\Reference.md) 