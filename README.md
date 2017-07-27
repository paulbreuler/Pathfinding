# Pathfinding
SEE WIKI FOR MORE INFO

This is an opensource project that is a personal exploration of A* for grid based pathfinding. I'm working on this as part of hackathons and free time as a way to continue exploring my passion outside of my day job.

Goals:
 - Create an algorithm that can locate a target and avoid obstacles such as non-traversable objects, objects/materials that buff or debuff movement and moving obstacles.
 - Allow for dynamic and efficient updates to path as obstacles move or the target moves.
 
Stretch Goal:
 - Remove grid size limit and make algorithm build grid only around units based in a larger world container. 
 - Have grid build using varying grid block sizes on low to high precision based on locality. e.g. if target is far away we only need an approximate area of low density to locate. As we move closer increase precision.

See wiki for reference/research material

A* algorithm based off YouTube tutorial by Sebastian Lague
https://www.youtube.com/watch?v=-L-WgKMFuhE&list=PLFt_AvWsXl0cq5Umv3pMC9SPnKjfp9eGW

His original tutorial was made into a GitHub repo that can be found at:
https://github.com/SebLague/Pathfinding
