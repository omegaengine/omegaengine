---
uid: OmegaEngine.Foundation.Collections
summary: Specialized collection types used to limit allocations.
---
These collections are designed for hot rendering paths where minimizing garbage-collector pressure matters. They reuse buffers rather than allocating new arrays on every update, which is important for maintaining stable frame times.
