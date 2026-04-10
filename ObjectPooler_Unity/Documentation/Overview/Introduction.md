Introduction
===========
This project is a high‑performance, generic object pooling framework designed for
both general use and specialised extension through its base implementation.  

Built as an extensible architecture, it replaces ad‑hoc pooling scripts and centralises the 
core pooling logic, allowing developers to quickly prototype a pooling system or tailor it for specific use cases.  

The framework is ready to use out of the box and includes all the features needed to safely and efficiently manage
the lifecycle of pooled objects. It supports custom behaviour injection, controlled creation and destruction,
and even full ownership of pooled instances when required.  

Designed to be type-safe, predictable, and easy to integrate, the system provides a consistent API for spawning,
recycling, and managing pooled objects across any Unity project.  

Why This Exists  
---------------  
Many Unity developers believe that a professional, well‑designed pooling solution is unnecessary because 
“you can build a pooler in an hour.”  
While this is technically true, these quick implementations often become repetitive boilerplate, 
lack architectural thought, and are rarely optimised for long‑term use.  
A dedicated pooling framework removes that burden and provides a consistent, efficient solution every time it’s needed.  

Ad‑hoc pooling systems created without proper design tend to become tightly coupled, allocate memory frequently, 
and rely on patterns that are difficult to decouple once the project grows.  
This leads to unnecessary garbage collection, reduced performance, and rigid structures that are hard to maintain or extend.  
A central, generic pooling system solves these problems by offering consistent behaviour, predictable results, 
and flexible customisation. This framework has been optimised for fast retrieval, batch returns (both allocating and non‑allocating),
safe development‑time operations, and a high‑performance release path. It also includes self‑sanitising behaviour 
to safely release unused objects and prevent memory leaks.  

Where Should This Be Used
-------------------------
Although this framework can be used in any project that requires object pooling, it truly shines in high-frequency environments such as Unity.  
Its ability to return results in as little as 0.002ms means that even at 120 FPS, a single pool operation consumes only a tiny fraction of the frame budget. 
This makes it ideal for systems that demand consistent, low‑latency performance.  
The modular architecture aligns naturally with Unity’s component‑driven design. Factories, pool behaviours, and lifecycle events can all be customised 
to match the needs of a specific project or object type. Because the framework is fully generic, it can be used for GameObjects, Components, 
particle systems, managers, and even editor tools such as bulk placement utilities.  
In practice, there are very few limits to how this system can be applied. The pool is simply the foundation -
what you build on top of it is entirely up to your project’s needs.

Quick Example  
-------------
Below is a quick example of how to create, a pool, retreive an object, and return it to the pool. More information on each process will be given
further into the documentation:

```csharp
// Create a factory for the pooled type
var factory = new Factory<PoolingClass>(() => new PoolingClass());

// Create a pool with an initial size and a maximum size
var pool = Pool<PoolingClass>.Create(factory, initialSize: 10, maxSize: 50);

// Retrieve an object from the pool
IPoolable<PoolingClass> pooled = pool.GetObject();

// Use the pooled entity
PoolingClass instance = pooled.Entity;

// Return it to the pool when finished
pooled.ReturnToPool();
```

Next: [**Index**](Index.md)  

