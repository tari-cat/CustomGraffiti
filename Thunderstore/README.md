# Custom Graffiti
This mod for _Bomb Rush Cyberfunk_ allows you to add custom graffiti to BRC by adding images to folders. No more manual Unity texture asset patching!

The mod supports **.jpg** and **.png** files, with .png recommended so your graffiti can have transparency.

## How to add Graffiti

### Start the game once with the Mod installed

1.  Install the Mod, preferably using r2modman.
2.  Start the game.
3.  Load a save. This should create the `%AppData%\r2modmanPlus-local\BombRushCyberfunk\profiles\<profile>\BepInEx\CustomGraffiti\<sizes>` folders.
4.  Look in your BepInEx mod/profile folder. In r2modman, go to `Settings > Profiles > Browse profile folder` to take you there (use `Browse data Folder` if you are not using profiles).

### Preparing Images

In the `CustomGraffiti` folder, you can place your images into four different folders (`Small`, `Medium`, `Large`, and `ExtraLarge`), which change the size of your graffiti.

For reference, the graffiti dimensions used by the original graffiti textures are (in pixels):

| Folder       | Dimensions    | Aspect Ratio |
| ------------ | ------------- | ------------ |
| `Small`      | 256 x 256 px  | 1:1          |  
| `Medium`     | 512 x 512 px  | 1:1          |  
| `Large`      | 1024 x 512 px | 2:1          |  
| `ExtraLarge` | 2048 x 512 px | 4:1*         |  

* \* `ExtraLarge` spray locations may get scrunched into narrower spaces, such as the locations in the Hideout. If you particularly care about your graffiti's aspect ratio, the results of `ExtraLarge` may come out looking weird.

### Setting Your Combo

`Small` graffiti will be randomized by default and do not require a combo. These are also known as the character-specific graffiti, and having any will cause **all** character-specific graffiti to be replaced.

For `Medium`, `Large`, and `ExtraLarge` graffiti, the file name needs to start with your desired combo. The file format for graffiti is as follows where `XXXXXX` is your desired combo in **unique** numbers:

| Folder        | Expected Format    | Valid #  | Permutations |
| ------------- | ------------------ | -------- | ------------ |
| `Medium/`     | `XXXX_Name.png`    | `1234`   | 24           |
| `Large/`      | `XXXXX_Name2.png`  | `12345`  | 120          |
| `ExtraLarge/` | `XXXXXX_Name3.png` | `123456` | 720          |

For example: `1423_My Medium Graffiti.png`. The combo targets are mapped to `1` at the top, incrementing clockwise. The center dot is ignored. This is why `Medium` has 4 digits even though there are 5 on screen with the center. `1423` one would be Up, Left, Right, Down, return to Center.

Another example: `123456_Something Extra Large.png` would create a combo that draws a hexagon for `ExtraLarge` starting at the top, going clockwise, then cutting a wedge by returning to the center dot after `6`.

Replacement example: if you do not like "VoodooBoy", you can create a `Large` graffiti with `15234_Something that Features BRC More Prominently.png`.

Remember, custom graffiti files ...

*  **will** override the original graffiti in the base game if the combo sequence matches.
*  **can** have duplicate combos, and they will be randomized.
*  **cannot** have duplicate **names**.
*  **must have** combo, underscore, and graffiti name for `Medium`, `Large`, and `ExtraLarge` Graffiti! The **underscore is required**, but the rest of your name can have spaces.

## Known Issues

* Having any `Small` graffiti **will effectively disable all character-specific graffiti**. If you want to customize character-specific graffiti, that is the job of another mod designed to handle character-specific settings (for example, DripRemix).
*  **Removed custom graffiti will render as an off-white rectangle**. Disabling the mod also will cause this.
*  While this mod is SlopCrew/Multiplayer safe — **tricks have no multiplier for new custom graffiti**.
*  If your `M/L/XL` graffiti shows up in the in-game phone menu, but the **game crashes when you try to inspect it**, you may have formatted your file name incorrectly:
   *  Are all of your digits unique and within range?
   *  Is there an underscore between the combo sequence and name?
   *  Do you have a **duplicate name**? Combos can be duplicated to randomize graffiti, but the "name" of the graffiti after the underscore should be unique.
   *  Did you put in the correct number of digits? 4 digits for Medium, 5 for Large, and 6 for ExtraLarge (the center does not count)!

## How to report issues

Please go to the [GitHub](https://github.com/tari-cat/CustomGraffiti) page to make an issue. Thank you!
