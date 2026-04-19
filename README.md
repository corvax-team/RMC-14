<!-- CM14 rework: non-RMC edit marker. -->
<p align="center"> <img alt="Space Station 14" width="400" height="400" src="https://github.com/user-attachments/assets/51a5b8f3-9068-4eeb-8caa-c6c9439118d7" /></p>

CCM-14 is a repository based on RMC-14.

This is the primary repo for CCM-14.

## Links

Project links are intentionally omitted in this developer-safe build.

## Contributing

We have a [list of issues](https://github.com/corvax-team/RMC-14/issues) that need to be addressed, and anyone can take them on.

## Building

1. Clone this repo.
2. Run `RUN_THIS.py` to init submodules and download the engine.
3. Compile the solution.

[More detailed instructions on building the project.](https://docs.spacestation14.com/en/general-development/setup.html)

## Local Hosting

For local development, the default VS Code `Server` launch profile now uses `Corvax/local`.

This preset is prepared for ordinary local hosting:
- SQLite instead of PostgreSQL
- hub advertising disabled
- public infolinks cleared
- Discord OAuth disabled by default
- auth disabled for simple local testing

If you need Discord account linking locally, fill `Tools/DiscordAuth/.env` from `Tools/DiscordAuth/.env.example`,
run the auth service, and then set matching values in the server preset.

## License

Everything related to licensing is described here: [Legal.md](https://github.com/corvax-team/RMC-14/blob/master/Legal.md)

Most assets are licensed under [CC-BY-SA-3.0](https://creativecommons.org/licenses/by-sa/3.0/) unless stated otherwise.
Assets have their license and the copyright in the metadata file.
[Example](https://github.com/space-wizards/space-station-14/blob/master/Resources/Textures/Objects/Tools/crowbar.rsi/meta.json).

Note that some assets are licensed under the non-commercial [CC-BY-NC-SA 3.0](https://creativecommons.org/licenses/by-nc-sa/3.0/) or similar non-commercial licenses and will need to be removed if you wish to use this project commercially.

## RMC-14 Links

Upstream public links are intentionally omitted in this developer-safe build.

## RMC-14

RMC-14 is an asymmetric game that runs on [Robust Toolbox](https://github.com/space-wizards/RobustToolbox).

This is the primary repo for RMC-14. To prevent people forking Robust Toolbox, a content pack is loaded by the client and server.
This content pack contains everything needed to play the game on one specific server.

If you want to host or create content for RMC-14, this is the repo you need.
It contains both RobustToolbox and the content pack for development of new content packs.
