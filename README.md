# Survival Game
Set on a procedurally generating island, the player needs to build a base to fend off monsters from the sea, while repairing a plane to escape the island.

## Details
Terrain details (Trees, rocks, grass, etc.) are generated randomly and stored in a quad tree for fast spacial lookups. Objects far away are rendered using GPU instancing for better performance, while closer objects are instanciated as actual game objects, allowing the player to interact with them for collisions/resource collection.
The use of GPU instancing allows for significantly denser details without sacrificing performance, while the quad tree allowed for much more efficient culling based on where they player is looking.

https://github.com/user-attachments/assets/25305719-a27e-4947-92dc-42a9977af490

## Collecting
Trees are able to be harvested for logs, and the logs can be processed down to planks for building

https://github.com/user-attachments/assets/fca4f94d-b3e0-4600-b0c0-8434fd5f41c1

## Building

There is a basic building system set up, using a socket based approach. Each building part placed creates new anchors for the next object to snap to. These anchors are stored in a quad tree for faster spacial lookups

https://github.com/user-attachments/assets/4e5a58d5-d54e-4de9-b2d8-5d2347c4c12f

## Map
A simple map allows the player to navigate around the island

<img src="https://github.com/user-attachments/assets/27fb5880-c43b-4ef0-9b4f-c92879bf2bc3" width="500">
