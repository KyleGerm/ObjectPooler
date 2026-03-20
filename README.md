Unity References
----------------
This project references UnityEngine DLLs from a local Unity installation.
Because Unity installs in different locations on different machines, the
reference paths in the .csproj file may not match your setup.

If the Unity references appear missing:

1. Open the project in Visual Studio.
2. Remove the broken UnityEngine references under "References".
3. Right-click "References" -> "Add Reference…" -> "Browse".
4. Navigate to your Unity installation:
   <UnityInstall>/Editor/Data/Managed/UnityEngine/
5. Re-add the required UnityEngine DLLs.

After updating the references, the project will build normally.

Example Usage
----------------
1. Create the **GameObjectPool** using:
   ```csharp
    GameObjectPool pool = GameObjectPool.Create(in GameObject obj, int size, int maxSize);
   ```
    This will create a new pool for you, populate the pool, and hand the created pooler back to you.
  
    Alternatively, you can use:
   ```csharp
    GameObjectPool pool = new GameObjectPool(int maxSize);  
    pool.GenerateList(in GameObject obj, int size);
   ```
    Note the pool will not be usable until GenerateList is called, which the static method Create does implicitly.

    An optional parameter allows you to add any GameObject logic to be applied to each GameObject on creation.  
    (e.g., health, damage, or other setup logic).

3. Use:
     -  GetObject() to retreive an object
     -  RequestMultiple() to retreive an array of objects
     -  RequestMultiple\<T\>() to retreive an array of objects as Components.
       
     The pooler will return all GameObjects as IPoolable, but the gameObject can be accessed through this interface.  
     IPoolable exposes IPoolable.ReturnToPool(), which can be called externally, or managed through the pooled object.

4. GameObjects can control their own return to the pool, but must search for an IPoolable component on creation.
     From there, IPoolable.ReturnToPool() should be called to flag this object as returned.
     Extra logic can be injected into ReturnToPool() by adding a callback to IPoolable.onReturn.

   ```csharp
   class Example : MonoBehaviour{
     IPoolable pooled;
      int health = 100;
     Start()
     {
         if(gameObject.TryGetComponent(out pooled))
         {
           pooled.onReturn += InjectedLogic;
         }
     }

     void Update()
     {
        if(health <= 0)
       {
         Return();
       }
     }
   
     void Return()
     {
       pooled?.ReturnToPool();
     }

     void InjectedLogic()
     {
       //Any behaviour to be completed before the object is returned goes here
     }
   }
   ```

   This means instances of the same object can exist inside and outside the pool, and will only subscribe to pooling logic if they are part of a pool.
   Else they will behave as normal without being controlled by a pool, and without having to inherit pooling logic.
   
----------------  


**Pooler\<T\>** can be used to manage a collection of GameObjectPoolers, with an enum as an identifier.  
This allows you to have multiple pools, held in a central class, but used through a single reference.    
To create Pooler\<T\>, you need to create an enum with enough values to hold the number of pools you wish the Pooler to have.  
The Pooler will create pools for each value of the enum, so be careful to use only as many enum values as you need.  

1. Create the Pooler\<T\> using:
   ```csharp  
    Pooler<MyEnum> pooler = new Pooler<MyEnum>();  
   ```  

2. Generate the pools for each value of the enum with the following:
   ```csharp  
    pooler.GenerateList(MyEnum.value, in GameObject obj, int size,int maxSize);
   ```
   Since the pooler owns the pools, the created pool will not be handed back to you, and must be accessed through the pooler.

4. A HotPath can be made for circumstances where one pool is used more frequently than others. This can be updated as many times as needed with:
   ```csharp 
    pooler.SetHotPath(MyEnum.value);
   ```
     and accessed with:
   ```csharp
     pooler.HotPath.*();
   ```
   - The HotPath can be cached locally if needed.
  
 6. All Pooler methods match GameObjectPooler methods for minimum friction.

Performance
-----------
This pooler is designed for high‑performance scenarios where object creation
and destruction would otherwise cause unnecessary allocations and GC pressure.

Key performance characteristics:

- Zero allocations during steady‑state usage  
  All objects are pre‑allocated and reused. No runtime instantiation unless
  the pool is exhausted and given permission.

- O(1) object retrieval  
  GetObject() and RequestMultiple() operate in constant time.

- Optional HotPath access  
  When one pool is used more frequently than others, the HotPath provides
  a direct reference to the most active pool, avoiding enum lookups.

- Minimal overhead on return  
  Returning objects to the pool is a lightweight flag operation, with optional
  callbacks for custom cleanup logic.

Requirements
------------
- Unity 2022.3 or later recommended  
  (The pooler uses only mature APIs such as GameObject, Instantiate, Destroy, and GetComponent,
   so it should work on most Unity versions.)

- .NET Framework 4.7.2  
  Required for building the DLL in Visual Studio.

- NuGet Dependencies (for DLL builds only):
  - System.Memory
  - System.Buffers
  - System.Runtime.CompilerServices.Unsafe

These dependencies are required only when building the DLL externally.
They are **not** required when using the source files directly inside a Unity project,
as Unity provides its own implementations of these APIs.

Limitations
-----------
- A single **GameObjectPool** can hold a maximum of 4096 objects.  
  This limit comes from the internal flag system, which uses a 64×64 bit
  structure to track object availability. If you need more than 4096 objects,
  you can create multiple pools or use **Pooler\<T\>** to manage them.

Requirements
------------
- Unity 2022.3 or later recommended  
  (The pooler uses only mature APIs such as GameObject, Instantiate, Destroy, and GetComponent,
   so it should work on most Unity versions.)

- .NET Framework 4.7.2  
  Required for building the DLL in Visual Studio.

- NuGet Dependencies (for DLL builds only):
  - System.Memory
  - System.Buffers
  - System.Runtime.CompilerServices.Unsafe

These dependencies are required only when building the DLL externally.
They are **not** required when using the source files directly inside a Unity project,
as Unity provides its own implementations of these APIs.
