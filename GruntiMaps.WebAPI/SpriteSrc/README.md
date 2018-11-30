# About these fill-patterns
These SVG icons can be processed using `spritezero` to create fill-patterns for use in maps.

It's remarkable that there doesn't appear to be a lot of them around because without them you can't really do pattern fills. 

Hopefully this will be the start of a useful collection of fill patterns. At this stage, it's just some left and right 45 degree hatches in a variety of colours.

Our sprite sets as served by gruntimaps include these sprites already.

## Producing the sprite files

While the process is documented, here and there, for the sake of convenience we'll list what we do to produce the sprite sets in gruntimaps. This is a quick and dirty process so far. There's bound to be a better way - feel free to add it :)

You'll need the [`mapbox-gl-styles`](https://github.com/mapbox/mapbox-gl-styles) repository and [`spritezero-cli`](https://github.com/mapbox/spritezero-cli) installed (so you will also need `node` and `npm`).

_hint_: you're probably better off installing `spritezero` explicitly first rather than letting `spritezero-cli` install it as a dependency. It seems to try to install an outdated version. So do the following:

```
npm install -g @mapbox/spritezero
npm install -g @mapbox/spritezero-cli
```

- Copy the `.svg` files into the various `sprites\*\_svg` directories (eg. `sprites\satellite-v9\_svg`)
- create a directory to hold the sprite sets: `mkdir t`
- create the various sprite sets at the appropriate sizes:
```
rem create basic v8 in normal, retina and super-retina
spritezero t\basic-v8 sprites\basic-v8\_svg\
spritezero --ratio=2 t\basic-v8@2x sprites\basic-v8\_svg\
spritezero --ratio=4 t\basic-v8@4x sprites\basic-v8\_svg\

rem create basic v9 in normal, retina and super-retina
spritezero t\basic-v9 sprites\basic-v9\_svg\
spritezero --ratio=2 t\basic-v9@2x sprites\basic-v9\_svg\
spritezero --ratio=4 t\basic-v9@4x sprites\basic-v9\_svg\

rem create bright v8 in normal, retina and super-retina
spritezero t\bright-v8 sprites\bright-v8\_svg\
spritezero --ratio=2 t\bright-v8@2x sprites\bright-v8\_svg\
spritezero --ratio=4 t\bright-v8@4x sprites\bright-v8\_svg\

rem create bright v9 in normal, retina and super-retina
spritezero t\bright-v9 sprites\bright-v9\_svg\
spritezero --ratio=2 t\bright-v9@2x sprites\bright-v9\_svg\
spritezero --ratio=4 t\bright-v9@4x sprites\bright-v9\_svg\

rem create satellite v8 in normal, retina and super-retina
spritezero t\satellite-v8 sprites\satellite-v8\_svg\
spritezero --ratio=2 t\satellite-v8@2x sprites\satellite-v8\_svg\
spritezero --ratio=4 t\satellite-v8@4x sprites\satellite-v8\_svg\

rem create satellite v9 in normal, retina and super-retina
spritezero t\satellite-v9 sprites\satellite-v9\_svg\
spritezero --ratio=2 t\satellite-v9@2x sprites\satellite-v9\_svg\
spritezero --ratio=4 t\satellite-v9@4x sprites\satellite-v9\_svg\
```

- finally, copy the results from the `t` directory into the WebAPI `wwwroot/sprites` directory.
