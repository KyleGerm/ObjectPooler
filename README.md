This readme is unfinished. An updated version will be supplied shortly 
   
----------------  



Performance
-----------
This pool is designed for high‑performance scenarios where object creation
and destruction would otherwise cause unnecessary allocations and GC pressure.

Key performance characteristics:

- Zero allocations during steady‑state usage  
  All objects are pre‑allocated and reused. No runtime instantiation unless
  the pool is exhausted and given permission.

- O(1) object retrieval  
  GetObject() and RequestMultiple() operate in constant time.

- Minimal overhead on return  
  Returning objects to the pool is a lightweight flag operation, with optional
  callbacks for custom cleanup logic.

Limitations
-----------
- A single **Pool<T>** can hold a maximum of 4096 objects.  
  This limit comes from the internal flag system, which uses a 64×64 bit
  structure to track object availability. If you need more than 4096 objects,
  you can create multiple pools and maage through an array. A multiple pool implementation will be released in the future 

Requirements
------------
- netstandard2.1 

