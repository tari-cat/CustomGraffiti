## Custom Graffiti
This mod adds custom graffiti in BRC by simply adding images to folders.
The mod supports .jpg and .png files.

### How to add Graffiti
In the mod folder, `BepInEx/plugins/CustomGraffiti`, there's a `Graffiti` folder.
In the `Graffiti` folder, you can place your images into four different folders, which change the size of your graffiti.

Custom graffiti will override the original graffiti in the base game. Custom graffiti with duplicate combos will be randomized. 
`Small` graffiti will be randomized by default and do not require a combo.

For `Medium`, `Large`, `ExtraLarge`, graffiti, the file name needs to start with your desired combo.

The file format for graffiti is as follows where `XXXXXX` is your desired combo:
`Medium`: `XXXX_Example Name.png`
`Large`: `XXXXX_Large Graffiti Name.jpg`
`ExtraLarge`: `XXXXXX_Extra Large Graffiti Name.png`

Please note that the underscore is required for `Medium`, `Large`, and `ExtraLarge` Graffiti!
Avoid using duplicate names as well.

### Known Issues
Removed custom graffiti will render as a gray square.
While this mod is SlapCrew/Multiplayer safe -- tricks have no multiplier for new custom graffiti.

### How to report issues
Please go to the [GitHub](https://github.com/tari-cat/CustomGraffiti) page to make an issue. Thank you!
