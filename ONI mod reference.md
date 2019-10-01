# Mod reference

A personal document to compile some common quirks of ONI.

## Animations

* Use [kparser](https://github.com/daviscook477/kparser) (hopefully soon-to-be obsolete).
* Animations must be interval locked to 33ms, and should have at least 4 frames.
* A tile in-game is roughly 90x90, but animations can be larger than a tile.
* Animations go under `%MOD_ROOT%/anim/assets/%ANIM_NAME%/`, and consist of three files: a png, a build, and an anim file.
* When referencing in code, reference `%ANIM_NAME%_kanim`.
* UI sprites **must** be named `ui_0.png`, nothing more and nothing less. The animation must be named `ui`, again exactly. Capitalization matters.
* It's a good idea to aim for about 120x120 for a UI sprite, including margin and all.
* Klei UI sprites tend to have a small, circuar shadow beneath them.

## ObjectLayers
* ObjectLayer.Building = passable
* ObjectLayer.FoundationTile = Can be built upon

## UI
* Is gross.
* Side screens inherit SideScreen, always exist and check for whether the given object is a valid target.
