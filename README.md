# Slime-Simulation
Project created with Unity 2020.3

Forked from Sebastian Lague's [Slime Simulation](https://github.com/SebLague/Slime-Simulation)

Sebastian's Video here: https://www.youtusbe.com/watch?v=X-iSQQgOd1A

Based on this paper: https://uwe-repository.worktribe.com/output/980579

All I did was add mouse interactivity:

![Trypophilia](https://user-images.githubusercontent.com/45629548/114337602-aeaa8880-9b51-11eb-8e62-0af4fb966624.gif)

This simulation was done with only 250k agents, which is the video appears so granular.

On my Asus Flip 14, its also compatible with Multi-touch.   
Yes, I can just drag the slimes around with my finger. I can't stop ...


## Good Settings for Interactivity

How the slimes react to the mouse depends entirely on the slime settings. 
For example in the setting ("Trypophilia" in the repository) shown above, you can hover over a strand to make the network find a way around it, and make gestures to steer the network

* Generally, I've found its most important to have a big Sensor offset Distance of about 50 to 150, or else you may only have very small slime strands aggregate, which may be difficult to get ahold of.  
* Turn speed significantly impacts how well you can steer slimes, with more controllability by you the higher the turn speed. Lower turn speed means agents mostly try to avoid you.  
*  The Sensor Angle Spacing (together with turn speed and movement speed) seems to control the radius of the curves slimes make, with an angle spacing of 5-60 being reasonable. Lower spacing generally leads to straighter lines than large angles.

## TODOs


* make mouse click change nearby slime's species (easy)
* add setting for mouse location correction for different screen sizes (easy)
* fix WebGL color problem and host online (easy)
* add multiplayer (hard)
* add music based on activity (map regional changes to random notes) (hard)
