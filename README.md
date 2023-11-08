# SexFaces plugin for Koikatsu and Koikatsu Sunshine
Adds the ability to set custom facial expressions to be displayed during H scenes (as opposed to the ones hardcoded into the game).

Also extends the range of facial expressions accessible from the character editor.

Originally forked from ManlyMarco's [KK_SkinEffects plugin](https://github.com/ManlyMarco/KK_SkinEffects), but modified beyond recognition.

## Installation
1. Install [KKAPI](https://github.com/ManlyMarco/KKAPI) and its dependencies.
1. Download the ZIP from [the latest release](https://github.com/Sauceke/KK_SexFaces/releases) and extract it into your game directory.
1. Enjoy

## Usage
Expressions can be added individually for each character, using the ❤ -> Sex Faces menu inside the Character Editor. You can set a maximum of 10 expressions for each combination of experience and H scene event. You don't have to add a face for every combination, the rest will be handled by the game as usual.

You can just use the right sidebar to change expressions if you want to, but the Sex Faces menu gives you more powerful controls. These are:

- **Pattern mixers:** Morph between any two eyebrow, eye, or mouth patterns.
  - **Ratio:** Amount of morph between the two patterns.
  - **Openness:** The *maximum* openness for the pattern. The actual openness may be smaller than this e.g. while the character is blinking or isn't talking.
- **Head:** Change the rotation of the head around all 3 axes (pitch, yaw, roll). This is relative to the game's own Head Direction option. If you set Head Direction to Camera, the character's head will always be tilted at the same angle relative to the camera.
- **Iris:** Change the size of each iris.
