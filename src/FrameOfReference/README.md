# Frame of Reference

"Frame of Reference" is the official sample game for the OmegaEngine.

The `FrameOfReference\Game` project places the files `_portable` and `config\Settings.xml` in the build directories which together cause the game content files to be loaded from `\content\`.  
When releasing the binaries as standalone applications these files are not present and game content files are instead expected be located in a subdirectory of the installation path named `content`.

To open the Debug Console when running the sample project press `Ctrl + Alt + Shift + D`.

Command-line arguments for the sample project:

| Usage             | Description                                             |
| ----------------- | ------------------------------------------------------- |
| `/map MapName`    | Loads *MapName* in normal game mode                     |
| `/modify MapName` | Loads *MapName* in modification mode                    |
| `/benchmark`      | Executes the automatic benchmark                        |
| `/menu MapName`   | Loads *MapName* as the background map for the main menu |
