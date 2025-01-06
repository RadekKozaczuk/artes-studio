# artes-studio

Refactorization
- input-related logic moved to SC_Input class
- simplified singleton - we now assume init happens in Awake and usage in Start and later
- score is moved to a dedicated class (SC_UI) and reacts to changes instead of being updated every frame (observer pattern)
- updates are now more centralized f.e. Gem is updated from GameBoard, this is not only faster but also gives us more control over the execution order
- a lot of tiny simplifications for example sometimes it is easier to pass x and y to a function instead of creating a Vector2Int just to pass the same information
- Swapping Gems now have a dedicated method for better readability, testability, and reusability
- Getting a random Gem also has a dedicated method for better readability, testability, and reusability
- some variables were moved to SC_Variables
- effects are now in variables 
- gem has only one prefab and the texture is assigned on spawn
- Information about whether the element is in motion is now stored in game logic instead of in the gem element itself

Optimization
- transform caching
- removed GetComponent (references are now stored normally)
- better null checks
- matches are now stored in an array which is faster and easier to understand
- Get Random piece greatly stabilised (the numer of iterations is now constant and there is no risk of falling over) as well as improved performance
- the entire board is now one collider and the click position is mapped to the element in the array
- gems are now pooled (reused) instead of being destroyed and instantiated over and over again
- FastDistance2D method reduces the amount of casting normally done by Vector2.Distance
- CheckForBombs greately optimized (a lot of redundant code)

New Features:
- the bomb has now a diamond-shape explosion, this feature is easy to enable/disable as it is only a single additional check
- 4-piece matches are now correctly identified (5 and more elements also classify as a 4-piece match)
- The piece that created a 4-piece match turns into a bomb now
- The created bomb inherits the color from the color that created the match
- Bomb delay added by using coroutines (existing architecture)
