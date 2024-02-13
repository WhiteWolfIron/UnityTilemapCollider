Script for generating PolygonCollider2D Collider based on Unity Tilemap.
Main purpose was to use it in Editor Mode. It should be usable in runtime as well, although I didn't care about optimization.
![image](https://github.com/WhiteWolfIron/UnityTilemapCollider/assets/19747364/9feb29c6-0c6c-4ea0-b1ce-1839f7fd7d5d)

Script can generate multiple paths, but was made to generate collider through edges and right now has problem with nesting, so it might be necessary to manually remove excessive paths.
![image](https://github.com/WhiteWolfIron/UnityTilemapCollider/assets/19747364/109b509f-e461-474f-a50b-2e554a3256c1)
