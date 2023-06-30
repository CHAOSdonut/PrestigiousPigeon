# Rise Of The Trickster

UNITY Version - 2021.3.19f1

### Creating a new Entity prefab step by step
1. Create and name Empty GameObject
2. Ensure positions and rotation are 0. 
3. Add 3D Model to Scene (if Model is still default imported object, right click Model -> Prefab -> Unpack)
4. Move the GameObject (with the 3D model inside) to the Prefab folder.
5. Open newly created Prefab -> Select the 3D Model
6. In the top right, select the Z-Axis view
7. Change the 3D Model rotation to look toward the camera
8. Add the enemy script to the GameObject.
9. Add the Healthbar prefab to the GameObject and set position above 3D Model
10. Add HealthBar reference to the entity script.

### Fix the animation bug where model implodes when animation plays
1. Put FBX in the prefab
2. Make avatar from FBX
3. Add avatar to the animation
