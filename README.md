# NKRsys FPP CC
An open-source project. NeKoRoSYS's First Person Perspective Character Controller is focused on creating the best lightweight yet feature-rich non-Rigidbody first person perspective character controller solution for Unity3D. Feel free to use it as a template for your project, but crediting this repo would be really appreciated!

Other than having the purpose of giving a great starting point/template for implementing a character controller to make your game actually playable, this repository also provides a good way to structure a Unity project using Assembly Definitions. (All the more reason for one to actually check this Unity project instead of just importing the scripts to somewhere else!)

<br>

## Before You Use
Before you use this repo as a template in any means, this project is currently being actively developed and many things are subject to change. Kindly star the repo before using, forking or cloning, please!

<br>

## Table of Contents
This README will be pretty lengthy in the near future. So, if you want to skip over some stuff or just want to cut straight to the chase, this one's for you!

| Table of Contents               |
| ------------------------------- |
| [Features](#features)           |
| [Key Notes](#key-notes)         |
| [Sponsorship](#sponsorship)     |
| [Contribution](#contribution)   |
| [Credits](#credits)             |

<br>

## Planned Upcoming Features and Updates
There will be more releases as I refactor old and unhealthy code to make better iterations of existing features, and introduce completely new ones!
### Asset Store Release
This repo will be released to the general public once it's deemed stable, production-ready, and bug-free.
### Possible Rebrand
In account for a * planned * third person feature.

<br>

## Features
### [Documentation](https://github.com/NeKoRoSYS/NKRsys-FPP-CC/wiki)
### Settings and Preferences
This character controller comes with adjustable values and changeable control types that may suit your needs. You can select between toggling or holding crouch, inverting the camera controller's mouse input Y axis, and more. It even has a save and load system!
### Future-proof Input Management
This asset uses Unity's New Input System package, which helps make event-based player input possible with the help of InputAction.CallbackContext events. This improves the performance of the game loop because all the inputs are edge-triggered and does not run on every frame. It also makes handling cross-platform input less troubling, with few adjustments here and there, you'll be able to make the controls work for mobile and consoles!
### Utilizes UnityEvents
Specific UnityEvents get triggered depending on the current state or action of the character controller.
### Implements Finite-state Machine Theory
Not only does it organizes the codebase for the movement system, it even makes it easily scalable and modified.
### Player Footsteps and Sound Effects
This project comes with a bonus in-house script that manages player footsteps and jump sound effects.
### Coyote Jumping
This feature gives the controller extra time to make jumping off edges easier.
### Slope Movement
A character controller will not be fully-functional if it doesn't have slope movement! This makes sure that going up or down on slanted surfaces will be seamless and would not cause any bumpy movement. This asset also uses some funky magic that makes the controller snap perfectly on the ground and on the slopes.
### Advanced Crouching
This asset provides a crouching system with advanced logic and smoothing.
### Movement Smoothing
This asset is using the built-in Character Controller component as a base, this means we have to manually simulate the gravity, the feeling of inertia, and friction unlike in Rigidbodies where physics already happen without extra coding. With that, this asset already comes with a quasi-physics engine that solves all our problems.
### Movement Tagging
This feature makes it so that landing from a fall or a jump will temporarily slow down the character controller.
### Camera Headbobbing
Camera Headbobbing is separated into two sub-features that we'll call move bobbing and land bobbing. **Move Bobbing** is when the camera noticeably shakes in a pre-set pattern that makes walking and running more immersive. **Land Bobbing** is when the camera indents itself downwards as the controller lands from a fall, giving that satisfying <i>oomph</i> that adds up to the immersion. Both of these features are good for realistic shooters and horror games!
### Camera Side-Movement Tilting
This tilts the camera towards the direction relative to the X axis of the controller's movement input. It gives off an effect similar to games like Quake and Half-Life.
### Camera Controller Smoothing
This project provides an adjustable float value that can be used to make the camera controller more cinematic when toned down.

<br>

## Key Notes
- This project has mobile support, meaning that have I manually set it to 60 FPS or otherwise Unity will set it to 30 FPS. (Check the Start() method at UISettings if you want to disable it)
- **Prefabs** are located at <i>Assets\Project\Runtime\Prefabs\.</i>
- **In order for Player Audio to work**, make sure you have tags that correspond with the footstep sound effects you're trying to add. For example:
  - _Material/Wood_
  - _Material/Stone_
  - _Material/Grass_
  - _Material/Metal_<br>
My implementation of footsteps requires an array of strings (your tags) found at MaterialManager.cs and multiple AudioClip arrays inside an array (yeah I did not stutter, arrays in an array). **More info at PlayerAudio.cs, but here's a rundown - Make sure the indexes of the AudioClip arrays match the indexes of the strings at MaterialManager. (eg materialTags[0] = Material/Wood, then footsteps[0].audioClip should have wooden footstep sound effects.**

<br>

## Sponsorship
I'd really appreciate it if someone were to donate me some cash. I am an aspiring software and game developer that currently do stuff solo, and I need funding to motivate me to do a lot better on my tasks so that I could deliver way better content. Donating is not a must, but it will be immensely cherished and appreciated!

<br>

## Contribution
Something's wrong with the code or you know better workarounds and alternatives? You can either make an issue or a pull request. It will be very much appreciated!

<br>

## Credits
- This project would not have been possible without the help of wonderful people at [Samyam](https://www.youtube.com/@samyam)'s [Discord Server](https://discord.com/invite/B9bjMxj)!
