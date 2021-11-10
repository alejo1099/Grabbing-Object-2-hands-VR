# Grabbing Objects with 2 Controls in VR
This code allow the user rotate a object with 1 or 2 points of reference, these points can be the positions of the controls of an VR application and allow the user rotate objects with both them.
We can also use the code for following a target without losing the orientation of the observing object.
To understand the idea behind this code, we must solve "the problem of the target".

## The problem of the target
This problem consists in the question, how do we can rotate an object that follow a target without losing oientation?
Imagine a cube like our object, this cube follows a target that can move towards everywhere, for example, if the target start forward of the cube, after that it goes up, it passes over our cube, and finally the target lowers to the height of the cube. The cube would should keeping look at target, but upside down.

![image](https://user-images.githubusercontent.com/26011921/141204766-ed523792-99c3-422b-a111-20f56ac6b115.png)

To calculate that, we rotate the cube 180 degrees around its X axis(remember, that if we turn the cube around its X axis, we rotate vertically, if Y axis horizontally and Z axis we turn sideways).
