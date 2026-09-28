# Basements

Expand into the earth with a placeable basement.

**Compatible with Valheim 1.0 Deep North Update.**

## Features

- Adds **Basement** to the hammer's **Misc.** build category.
- Costs 200 Stone and 100 Wood, and requires a nearby Stonecutter.
- Supports nested basements; the default limit is five levels.
- Allows unrestricted building inside the basement.
- Returns its construction materials when dismantled after its interior is clear.

## Install

**Recommended:** install with [Gale](https://hexium.gg/mod-manager). Gale supports both the Hexium and Thunderstore catalogs and can import existing mod-manager profiles.

**Manual:** install [BepInExPack Valheim](https://valheim.hexium.gg/mods/denikson/BepInExPack_Valheim), then place `Basements.dll` in `Valheim/BepInEx/plugins/`.

## Configuration

The config file is `BepInEx/config/com.rolopogo.Basement.cfg`.

For an in-game settings menu, optionally install [BepInEx Configuration Manager](https://valheim.hexium.gg/mods/Azumatt/Official_BepInEx_ConfigurationManager) and press F1.

| Setting | Default | Description |
| --- | ---: | --- |
| `Lock Configuration` | `true` | When enabled, server administrators control synchronized settings. |
| `Max nested basements` | `5` | The maximum number of basement levels that can be placed inside one another. |

Settings reload when the config file changes. Basements uses ServerSync: install the same package on a server when you want the server configuration to apply to every player. ServerSync is included in Basements; do not install a separate `ServerSync.dll`.

## Compatibility

Basements works alongside [QuickTeleport](https://valheim.hexium.gg/mods/OdinPlus/QuickTeleport). QuickTeleport changes portal and dungeon travel timing; it does not change basement placement or construction behavior.

## Troubleshooting

- If Basement is missing from the hammer, confirm that `Basements.dll` is under `BepInEx/plugins/`, then look in the hammer's **Misc.** category and check the BepInEx log for `Basements`.
- If nesting stops earlier than expected, check `Max nested basements` in the config file or Configuration Manager.
- When `Lock Configuration` is enabled, change synchronized settings on the server.

## Videos

- [Video 1](https://streamable.com/t5cizh)
- [Video 2](https://streamable.com/bix98w)

## Support

Need help? Find Elg in the [Odin Plus Discord](https://discord.gg/mbkPcvu9ax).

## Liked the mod?

Consider showing some love with a coffee on Ko-fi:

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/C0C6S21AN)

> This helps with tooling costs, as well as keeping me able to update the mods frequently and creating new mods and features quickly.
