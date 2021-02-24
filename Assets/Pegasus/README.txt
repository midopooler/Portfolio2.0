PEGASUS
=======

Welcome to Pegasus, the cutscene and flythrough generation system for Unity 3D.

OVERVIEW:

Pegasus consists of a Manager, and POI (Points of Interest) that it flies through. The Manager allows you to control overall flythrough properties, and the POI allows you to control the behaviour of the camera as it flies through individual locations in your scene.

There are two types of fly throughs:
	- SINGLE SHOT fly throughs will just play through a single time and then stop;
	- LOOPED fly throughs are infinite fly throughs that will look back to the start after they have finished.

QUICKSTART:

To add a Pegasus Manager to your scene select Game Object -> Pegasus -> Add Pegasus Manager. 

To add Points of Interest (POI) to your scene hit the CTRL button and while holding it down click the LEFT Mouse Button. This will create a new point of interest at every location that you click on. You need to add two or more points of interest to get a fly through.

By default Pegasus will pick up your main camera as its target. If you would like Pegasus to control something else then drop it into the Target Object slot in the Pegasus Manager.

To Play your fly through just press the Play button. 


DEMO:

For a quick demo of Pegasus in action open up the demo scene and press Play. Check out the configuration of the manager, and then the configuration of each of the POI to see how different things work.


DETAILED DOCUMENTATION:

Detailed documentation can be found in the documentation directory, or online at http://www.procedural-worlds.com/pegasus/documentation/.