**Please note, this repository has been deprecated, GoQL is now included in Selection Groups.**








GoQL: GameObject Query Language
-------------------------------

GoQL provides a syntax and API for specifying criteria, and then searching the GameObject hierarchy
for the set of gameobjects that match those criterua.
    
    using Unity.GoQL;
    var query = "\"*GameObject*\""; //matches all gameobjects that have "GameObject" in their name.
    var goqlMachine = new GoQLExecutor();
    var instructions = Parser.Parse(query);
    var selection = goqlMachine.Execute(instructions);
    
The syntax is best illustrated with examples.



Select all root objects:

    /

Select all objects who have a name beginning with "Quad":

    "Quad*"

Select second audio source component in children of all objects who have a name beginning with "Quad":

    "Quad*"/<t:AudioSource>[1]


Select all gameobjects that have a Transform and a AudioSource component: 

    <t:Transform, t:AudioSource>


Select the first 3 children of all objects that are a child of a renderer component and have "Audio" in their name: 

    <Renderer>/"*Audio*"[0:3]


From each object named "Cube", select children that have a name starting with "Quad", then select the last grandchildren that has an AudioComponent.

    "Cube"/"Quad*"/<t:AudioSource>[-1]

Select all gamobjects that use the material "Skin":

    <m:Skin>

