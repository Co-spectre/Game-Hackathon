# System Patterns

## Architectural Patterns

- Player state machine: `PlayerController` keeps movement, dash, attacking, and stagger states separate so combat logic can interrupt movement cleanly.
- Room encounter loop: `RoomController` controls room entry, door lock, wave spawning, and reward unlock flow.
- Event-driven combat progression: `WaveSpawner` raises a completion event instead of making every system poll enemy count.
- Realm transition: `JapanPortal` handles lighting and position changes when the player moves between the Nordic and Japan sections.

## Design Patterns

- Singleton service: `CameraJuiceManager` is used as a global impact-feedback service for shake and hit stop.
- Editor generation pattern: `NordicWorldBuilder` creates a full world layout from one editor command for fast iteration.
- Component-based combat: damage, health, and UI are split across separate MonoBehaviours instead of one large script.

## Common Idioms

- Use Rigidbody forces for movement and dash so combat can be tuned independently from animation.
- Use trigger volumes for room entry, campfires, and portals.
- Use visual feedback on hits: shake, stop, floating numbers, and door locking.
- Keep era identity in lighting, materials, and landmark silhouettes so each biome reads quickly.