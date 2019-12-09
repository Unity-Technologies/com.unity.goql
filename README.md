GoQL: GameObject Query Language
-------------------------------

GoQL provides a syntax and API for specifying criteria, and then searching the GameObject hierarchy
for the set of gameobjects that match those criterua.
    
    using Unity.GoQL;
    var query = "*GameObject*"; //matches all gameobjects that have "GameObject" in their name.
    var goqlMachine = new GoQLExecutor();
    var instructions = Parser.Parse(query);
    var selection = goqlMachine.Execute(instructions);
    
The syntax is best illustrated with examples.



Select all root objects:

    /

Select all objects who have a name beginning with "Quad":

    Quad*

Select second audio source component in children of all objects who have a name beginning with "Quad":

    Quad*/<AudioSource>[1]


Select all gameobjects that have a Transform and a AudioSource component: 

    <Transform, AudioSource>


Select the first 3 children of all objects that are a child of a renderer component and have "Audio" in their name: 

    <Renderer>/*Audio*[0:3]


From each object start has a name starting with "Quad" and who has a parent named "Cube", from all children that have an AudioSource component select the last one: 

    Cube/Quad*/<AudioSource>[-1]

