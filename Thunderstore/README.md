## Custom Graffiti
This mod adds custom graffiti in BRC by simply adding images to folders.
The mod supports .jpg and .png files.

### How to add Graffiti
In the mod folder, `BepInEx/plugins/CustomGraffiti`, there's a `Graffiti` folder.
In the `Graffiti` folder, you can place your images into four different folders (`Small`, `Medium`, `Large`, and `ExtraLarge`), which change the size of your graffiti.

`Small` graffiti will be randomized by default and do not require a combo.

For `Medium`, `Large`, and `ExtraLarge` graffiti, the **file name needs to start with your desired combo**.

The file format for graffiti is as follows where `XXXXXX` is your desired combo in **unique** numbers: <br/>
`Medium`: `XXXX_Name.png` <br/>
`Large`: `XXXXX_Name.png` <br/>
`ExtraLarge`: `XXXXXX_Name.png` <br/>

For example: `162534_My Graffiti.png`

Please note that the underscore between your combo and graffiti name is required for `Medium`, `Large`, and `ExtraLarge` Graffiti! <br/>
Avoid using duplicate names as well. <br/>
Custom graffiti will override the original graffiti in the base game. Custom graffiti with duplicate combos will be randomized.

### Known Issues
**Removed custom graffiti will render as a gray/white square**. <br/>
While this mod is SlopCrew/Multiplayer safe -- **tricks have no multiplier for new custom graffiti**. <br/>

### How to report issues
Please go to the [GitHub](https://github.com/tari-cat/CustomGraffiti) page to make an issue. Thank you!
