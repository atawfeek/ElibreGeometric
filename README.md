# Non-Convex Polygons Boolean Operatons
Windows based application using .NET technologies to perform boolean operations on 2D non-convex polygons.

# Summary
Recently, I got an exciting chance to work on geometric algorithms, and to execute different operations on polygons especially the non-convex ones via .Net application wrapping the required algorithm.

One of the commonly used algorithms to perform different operations like intersection, union, subtraction and others on polygons is Vatti clipping algorithm.

This app uses "A generic solution to polygon clipping" library whihc is an extension of Bala Vatti's clipping algorithm.

# User Story
- As a user, I want to randomly generate two non-convex polygons.
- Verify that the following acceptance criteria are met:
-	I can generate two non-convex polygons successfully at a time
-	I can perform one of the following main operations on the generated polygons
    * Intersection
    * Subtraction
    * Union
-	I can save the generated polygons after applying the chosen operation as an image to my disk.

# Technologies
- Asp.Net Core
- Layerd Architecture  
  - Domain 
  - Presentation   
  - Test
- Dependency Injection: **Unity**  
- Unit Test: **NUnit & Moq**
- Considered Design Patterns:
  - Lazy Loading Pattern
  - Simple Factory Pattern
  - RIP Polymorphism Pattern
- Considered Object-Oriented Principles
  - Single Responsibility
  - Open-Closed Principle

# Libraries
•	"A generic solution to polygon clipping" library

# Algorithms
•	Vatti clipping algorithm