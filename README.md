# Unity-Determinism

A MonoBehaviour-based framework for **deterministic** simulation in Unity.  
Provides custom physics, rollback reconciliation, and simple netcode for perfectly synced multiplayer or replay systems.

---

[# The resource I prepared about Determinism (TR)](https://docs.google.com/document/d/11yvE1Nc9JV4Hny2ZjhqLcvGwzmrQMk_sci2YPZWR_L4/edit?tab=t.o68404mqj8hf)

## 📦 Features

- Fixed-timestep game loop with custom `TimeManager`  
- Deterministic RNG (seed-based)  
- State snapshot & restore for replay/rollback  
- Simple replay playback system  
- *(Planned)* Custom physics, rollback & UDP netcode  

---

## 🗺️ Roadmap

### ✅ Completed

- Project scaffolding and folder structure  
- Fixed-timestep loop  
- Basic `MonoBehaviour`-based position & velocity updates  
- Snapshot & restore infrastructure (serialization)  
- Seeded deterministic RNG  
- Simple replay playback system  
- Initial test scenes: top-down movement & input recording  

### 🚧 To Do

1. **Custom Physics Engine**  
   - [ ] `RigidBody` & `Collider` classes (no built-in Rigidbody)  
   - [ ] AABB & circle collision detection + resolution  
   - [ ] Collision manifold calculation  
   - [ ] Stable integration (Verlet / semi-implicit Euler)  
   - [ ] Unit tests for physics subsystems  

2. **Rollback Mechanism**  
   - [ ] State backup strategy (full snapshot vs delta)  
   - [ ] Rewind & re-simulate algorithm  
   - [ ] Divergence detection & reporting  
   - [ ] Seed & snapshot management per packet  

3. **Networking (Netcode)**  
   - [ ] Lightweight UDP messaging layer  
   - [ ] Input-only packet format (frame index + input)  
   - [ ] Client-side prediction & loss recovery  
   - [ ] Integrate rollback with networked inputs  

4. **Testing & CI**  
   - [ ] Cross-platform floating-point deviation tests  
   - [ ] Automated regression suite (CI playback tests)  
   - [ ] Performance profiling & optimizations  

5. **Documentation & Examples**  
   - [ ] `CONTRIBUTING.md` guidelines  
   - [ ] XML doc comments for public APIs  
   - [ ] “Hello Determinism” tutorial scene  
   - [ ] Network + rollback demo scenes  

6. **Advanced (Optional)**  
   - [ ] Multithreading with thread-safe queues  
   - [ ] Burst Compiler & Native Collections  
   - [ ] In-game rollback debug overlay  




