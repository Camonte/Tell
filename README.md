# TELL pour TBI - Technical report

This file provides some insight about the technical implementation of the application. It is meant as a complement to the documentation provided in the individual files.

The application is live at:

[https://camonte.github.io/Tell/](https://camonte.github.io/Tell/)

[![Github Pages Webapp](https://github.com/Camonte/Tell/actions/workflows/deploy-webapp.yml/badge.svg)](https://github.com/Camonte/Tell/actions/workflows/deploy-webapp.yml)

## Acknowledgements

This project builds on the work of [Peter Krcmar](https://github.com/PeterKrcmar0) who developed the [first version of TELL pour TBI](https://github.com/PeterKrcmar0/tell-in-motion). This technical report includes his own technical report as well as specificities to this new version of TELL pour TBI.

## Project structure

Assets

--Animations

--Fonts

--Lean

--Plugins

--Prefabs

--Resources

----Dictionaries

----Icons

----Shaders

----Shapes

----Sounds

--Scenes

--Scripts

--TextMesh Pro

--WebGL Templates

...

Builds

--WebGL

The WebGL Templates folder contains all the files relative to the communincation with the secondary popup window.
  
## Launching the application
* Open a terminal in the Builds/WebGL directory.
* Launch a local web server using for exemple [http-server](https://www.npmjs.com/package/http-server).
* Open the shown URL in your preferred browser (tested working on Mozilla Firefox).

## Building / modifying the app
* Install Unity Hub.
* Install Unity 2020.3.20 with WebGL support.
* Unity Hub -> Projects -> Open.
* Select this directory.

Note that all builds must be done in the Builds directory and be named "WebGL".

### Specificity on MacOS
We encountered difficulties with building the project on MacOS due to a Python 2.7 dependency. The file `PreBuildProcessing.cs` in Assets > Scripts regulates this behaviour and allows building on MacOS. However, it is system specific and needs to be modified to set the `EMSDK_PYTHON` environment variable to the path to the Python 2.7 folder on your computer. When building on Windows, this file can be ignored and even removed. Note that we developed and tested with Python 2.7.18 and have no data on how the application behaves with other Python versions.

## External Packages
* [Lean Touch](https://assetstore.unity.com/packages/tools/input-management/lean-touch-30111): Simple unified API for handling different types of input (touch, pen, mouse). Used for drag and drop, tap, ...
* [Better Unity WebGL Template](https://github.com/greggman/better-unity-webgl-template): replaces the default template used by unity in WebGL builds.
  
## Terminology
* Finger: an input (mouse, finger, pen). It can either tap objects, or drag and drop them. Phoneme: a colored shape representing a phoneme of the French language (see Phoneme in Scripts/Models).
* Grapheme: group of letters colored with the color of the Phoneme it spells out. (see Grapheme in Scripts/Models).
* Draggable (Phoneme/Grapheme): Phoneme/Grapheme that can be picked up, dragged around, and dropped by a Finger (see Draggable in Scripts/Shapes).
* (Phoneme/Grapheme) Generator: Phoneme/Grapheme that can’t be dragged, but spawns copies of itself when tapped with a Finger. The copy is attached to the Finger (see Spawn and SpawnGrapheme in Scripts/Shapes).
* Drop Zone: a zone that can be interacted with by a Draggable object. It can react to 3 interactions: Hover: triggers every frame a Draggable is being dragged inside the drop zone. Long Hover: triggered once when a Draggable is held inside a drop zone for a short period of time without moving. Drop: triggered once when a Draggable is dropped inside the drop zone

It uses a collider to track when it is colliding with Draggables (see DropZone in Scripts/Shapes).
* Side Panel: a panel on the left or the right part of the screen which contains Generators for vowels or consonants (see SidePanel in Scripts/Shapes).
* Grid: central panel which contains the current Sentence and a series of Drop Zones where the user can drop their answer. Each game mode uses a different type of Grid, which in turn uses different types of Drop Zones to handle the Draggables the user will drop as their answer (see Grid and GridManager in Scripts/Grid).
* Word: a representation of a French word, and includes the Phonemes and Graphemes it is composed of (see Word in Scripts/Models).
* Sentence: a series of Words which is used as input and displayed on the Grid (see Sentence in Scripts/Models).

## Shapes
Shapes are stored as PNG files in the Resources/Shapes directory and loaded as sprites into Unity. We also include the upper and lower halfs of all “basic” shapes (square, circle, square with rounded corners, ...) so that we can combine any two shapes into one easily. Each sprite is then colored by the appropriate colors, which are stored in the Phoneme object (see Phoneme and ShapeManager in Scripts/Shapes).
### Card-matching games images
The images used for the card-matching games are stored as PNG in the Resources/Icons/MemoryImages and Resources/Icons/MemoryPhonemes. They have been manually mirrored horizontally so that when they undergo a flip animation in the game, they appear as they should.

## Text
For all text elements we use TextMesh Pro, the default way to render text elements in Unity. We use the Fundamental Brigade Schwer Font, available in the Fonts directory. Text meshes can only be colored with a single color, or with a linear gradient. To create a clear separation between the two colors we use two text meshes and a mask: one mesh is only visible inside the mask and one is only visible outside of it, which creates the desired effect (see ShapeManager in Scripts/Shapes).

## Merging Phonemes/Graphemes
We add a Drop Zone to all Phonemes and Graphemes. When two object collide, the one that is being dragged plays the role of the Draggable and the other one the role of the Drop Zone. Hovering previews the resulting object if the objects were to be merged. The Long Hover and Drop interactions actually generate the new merged object. When Long Hovering, the merged object is attached to the Finger that is dragging the Draggable (see Draggable, DropZoneMerge and DropZoneMergeGrapheme in Scripts/Shapes).

## Adaptative word spacing
The width of Phonemes being fixed, we need to adapt the spacing between words to accommodate for Phoneme transcriptions of words being wider than the word itself. For example, distance “b” is longer than “a” so we add some extra space before and after the word. The distance “d” is shorter than “c”, so we don’t add any extra space (see e.g. GridWithShapes in Scripts/Grid).

## Communicating with browser javascript
To communicate with the secondary popup window for teachers, we followed the [official documentation](https://docs.unity3d.com/Manual/webgl-interactingwithbrowserscripting.html) and added the browser functions which we wanted to call from the application inside the Plugins directory (see PlayMenu and SecondaryWindow in Scripts/Menu).

## Sounds
The sounds are located in the Resources/Sounds directory. The sound files that we obtained were named by the color of the Phoneme in hexadecimal (e.g. the sound for the phoneme “u a” was named _4AD6CA_930FA5.wav). We mapped all sounds to their phonetic transcription (e.g. “u a.wav”). The Windows file system is case insensitive, so the files “o.wav” and “O.wav” are considered the same and are not permitted. We added a plus (+) sign to all capital letters (“o+.wav”). Some sounds for double phonemes are missing, so we play the two sounds that compose the phoneme sequentially (see SoundManager in Scripts/Sound).
  
## Expanding Phonemes
Grapheme Generators are grouped together inside expanding Phonemes, which expand when tapped by a Finger. Normally, they expand from the center point of the Phoneme but their position is limited by the borders of the screen (vertical line) and by the y position of the highest Phoneme of the Side Panel (horizontal line) (see DropZoneExpand and SidePanel in Scripts/Shapes).

## Game progression
The progression of the game is handled by the State Manager, which keeps track of the current state such as the current Sentence, which Words have been answered and if the answers are correct. It communicates with the Grid in order to get the information from the Drop Zones used for answering (see StateManager in Scripts/Grid).
### Card-matching games
For the card matching games, the progression is handled by the Memory Control which keeps track of the state of the game and checks for correctness.

## Game configuration
The different configurations of the game (game mode, chosen variation, filtering of Phonemes based on vowels/consonnants, correction type, sound muted) are made accessible throughout the application by the Config file. It also stores the default values which are used when the application is launched (see Config in Scripts).
