# Gruntimaps MapServer RESTful API

The new RESTful API can be navigated from its initial endpoint of `/api`. It aims for compliance with the HATEOAS model.
 
In the examples, the server is referred to as `www.mymapserver.com`.

As a general rule, non-terminal endpoints return an array of links to other endpoints. Typically those links contain a link to the endpoint (named `href`), a relationship indicator named `rel` (e.g. `self`, `collection`, `item`), the method to be used when invoking the endpoint (e.g. `GET`, `POST`, `PATCH`, etc), and a `title` which identifies the purpose of that link.

## Root

**URL** : `/api`

**Parameters**: none

### Example Success Response
    200 OK

```json
{
    "links":[
        {
            "href":"https://www.mymapserver.com/api",
            "rel":"self",
            "method":"GET",
            "title":"self"
        },
        {
            "href":"https://www.mymapserver.com/api/layers",
            "rel":"collection",
            "method":"GET",
            "title":"layers"
        },
        {
            "href":"https://www.mymapserver.com/api/fonts",
            "rel":"collection",
            "method":"GET",
            "title":"fonts"
        },
        {
            "href":"https://www.mymapserver.com/api/sprites",
            "rel":"collection",
            "method":"GET",
            "title":"sprites"
        }
    ]
}
```

### Example Error Responses
None by design.

## Layer List
**URL** : `/api/layers`

**Parameters**: none

This call returns a list of all of the layers configured on the server. 

### Example Success Response
    200 OK

```json
{
    "links":[
        {
            "href":"https://www.mymapserver.com/api/layers",
            "rel":"self",
            "method":"GET",
            "title":"self"
        },
        {
            "href":"https://www.mymapserver.com/api/layers/a_layer",
            "rel":"collection",
            "method":"GET",
            "title":"A description of this layer"
        },
        {
            "href":"https://www.mymapserver.com/api/layers/another_layer",
            "rel":"collection",
            "method":"GET",
            "title":"The description for the second layer"
        }
    ]
}
```
## Layer Details
**URL** : `/api/layers/a_layer`

**Parameters**: The layer identifier is passed in the URL

This call returns the various data elements available for the specified layer. 

### Example Success Response

    200 OK

```json
{
    "links":[
        {
            "href":"https://www.mymapserver.com/api/layers/a_layer",
            "rel":"self",
            "method":"GET",
            "title":"self"
        },{
            "href":"https://www.mymapserver.com/api/layers/a_layer/source",
            "rel":"item",
            "method":"GET",
            "title":"source"
        },{
            "href":"https://www.mymapserver.com/api/layers/a_layer/style",
            "rel":"item",
            "method":"GET",
            "title":"style"
        },{
            "href":"https://www.mymapserver.com/api/layers/a_layer/mappack",
            "rel":"item",
            "method":"GET",
            "title":"mappack"
        },{
            "href":"https://www.mymapserver.com/api/layers/a_layer/tiles",
            "rel":"item",
            "method":"GET",
            "title":"tiles"
        },{
            "href":"https://www.mymapserver.com/api/layers/a_layer/grid",
            "rel":"item",
            "method":"GET",
            "title":"grid"
        },{
            "href":"https://www.mymapserver.com/api/layers/a_layer/metadata",
            "rel":"item",
            "method":"GET",
            "title":"metadata"
        }    
    ]
}
```
### Example Error Responses
    404 Not Found
```json
{
    "name":"RESOURCE_NOT_FOUND",
    "message":"The specified resource does not exist",
    "information_link":null,
    "details":[
        {
            "field":"service",
            "issue":"Service does not exist"
        }
    ]
}
```

## Layer Source JSON
**URL** : `/api/layers/a_layer/source`

**Parameters**: The layer identifier is passed in the URL

This call returns the MapBox source JSON component for the specified layer. 

### Example Success Response

    200 OK
```json
{
    "tilejson": "2.0.0",
    "name": "a_layer",
    "description": "A description of this layer",
    "version": "2",
    "scheme": "xyz",
    "tiles": [
        "https://www.mymapserver.com/api/layers/a_layer/tiles&x={x}&y={y}&z={z}"
    ],
    "minzoom": 0.0,
    "maxzoom": 14.0,
    "bounds": [
        152.629485,
        -28.217714,
        153.551436,
        -26.969239
    ],
    "center": [
        153.094482,
        -27.244862,
        14.0
    ],
    "type": "vector",
    "format": "pbf"
}
```

## Layer Style JSON
**URL** : `/api/layers/a_layer/style`

**Parameters**: The layer identifier is passed in the URL

This call returns the MapBox style JSON component for the specified layer. 

### Example Success Response

    200 OK
```json
[
  {
    "id": "a_layer",
    "layout": {},
    "minzoom": 6,
    "paint": {
      "fill-extrusion-color": {
        "property": "Zone_ZONE_",
        "type": "categorical",
        "stops": [
          [ "0", "white" ],
          [ "1", "royalblue" ],
          [ "2", "yellow" ],
          [ "3", "darkcyan" ],
          [ "4", "orchid" ],
          [ "5", "darkorange" ],
          [ "6", "forestgreen" ],
          [ "7", "palevioletred" ],
          [ "8", "rosybrown" ],
          [ "9", "darkkhaki" ],
          [ "10", "turquoise" ]
        ]
      },
      "fill-extrusion-height": {
        "property": "Roof_HEI_1",
        "type": "identity"
      },
      "fill-extrusion-opacity": 0.6
    },
    "source": "a_layer",
    "source-layer": "a_layer",
    "type": "fill-extrusion"
  }
]
```

## Layer Offline Map Pack
**URL** : `/api/layers/a_layer/mappack`

**Parameters**: The layer identifier is passed in the URL

This call returns an offline map pack for the specified layer. 

### Example Success Response

    200 OK
**Content** : a zip file containing the stylesheet and the mbtiles file for this layer.

### Example Error Response
    400 Bad Request
```json
{
    "name":"INVALID_REQUEST",
    "message":"Request is not well-formed, syntactically incorrect, or violates schema",
    "information_link":null,
    "details":[
        {
            "field":"service",
            "issue":"Service name must be supplied"
        }
    ]
}
````
    404 Not Found
```json
{
    "name":"RESOURCE_NOT_FOUND",
    "message":"The specified resource does not exist",
    "information_link":null,
    "details":[
        {
            "field":"service",
            "issue":"Service does not exist"
        }
    ]
}
```

## Layer Tiles request
**URL** : `/api/layers/a_layer/tiles?x={x}&y={y}&z={z}`

**Parameters**: The layer identifier is passed in the URL, plus `x`, `y` and `z` coordinates.

This call returns the appropriate tile for the slippy coordinates `x`, `y` and `z` for the specified layer. 

The tile data returned could be PNG or PBF depending on the type of data stored in the MapBox Tile database.

### Example Success Response
    200 OK
**Content** : A PNG tile or a PBF(MVT) tile depending on the data type stored in the layer.

## Layer Grid request
**URL** : `/api/layers/a_layer/grid?x={x}&y={y}&z={z}`

**Parameters**: The layer identifier is passed in the URL, plus `x`, `y` and `z` coordinates.

This call returns the appropriate grid data for the slippy coordinates `x`, `y` and `z` for the specified layer. 

### Example Success Response
    200 OK
**Content** : JSON grid data for this tile.

## Font List
**URL** : `/api/fonts`

**Parameters**: none

This call returns the list of fonts available on the server. 

### Example Success Response
    200 OK

```json
{
    "links":[
        {
            "href":"https://www.mymapserver.com/api/fonts/A+Font+Name",
            "rel":"collection",
            "method":"GET",
            "title":"A Font Name"
        },
        {
            "href":"https://www.mymapserver.com/api/fonts/A+Different+Font",
            "rel":"collection",
            "method":"GET",
            "title":"A Different Font"
        },
        {
            "href":"https://www.mymapserver.com/api/fonts",
            "rel":"self",
            "method":"GET",
            "title":"self"
        }
    ]
}
```
## Font Details
**URL** : `/api/fonts/A+Font+Name`

**Parameters**: the font name, passed in through the URL

This call returns a list of the supported glyph ranges for the specified font. 

### Example Success Response

    200 OK
```json
{
    "links":[
        {
            "href":"https://www.mymapserver.com/api/fonts/A+Font+Name/9216-9471",
            "rel":"item",
            "method":"GET",
            "title":"A Font Name, glyphs 9216-9471"
        },{
            "href":"https://www.mymapserver.com/api/fonts/A+Font+Name/9472-9727",
            "rel":"item",
            "method":"GET",
            "title":"A Font Name, glyphs 9472-9727"
        },{
            "href":"https://www.mymapserver.com/api/fonts/A+Font+Name/9728-9983",
            "rel":"item",
            "method":"GET",
            "title":"A Font Name, glyphs 9728-9983"
        },{
            "href":"https://www.mymapserver.com/api/fonts/A+Font+Name/9984-10239",
            "rel":"item",
            "method":"GET",
            "title":"A Font Name, glyphs 9984-10239"
        },{
            "href":"https://www.mymapserver.com/api/fonts/A+Font+Name",
            "rel":"self",
            "method":"GET",
            "title":"self"
        }
    ]
}
```

## Sprites List
**URL** : `/api/sprites`

**Parameters**: none

The `href` entries returned from this call are intended to be passed into a MapBox stylesheet - the MapBox client uses them as the base part of URLs that it constructs to retrieve individual sprites. (It adds `.json` to retrieve the list of sprites in the set, `.png` to retrieve the normal size sprite, `@2x.png` to retrieve the double-sized sprites and `@4x.png` to retrieve the quadruple-sized sprites. Arguably these could be presented as collection members for each sprite set but they are not needed for current purposes.)

### Example Success Response

    200 OK
```json
{
    "links":[
        {
            "href":"https://www.mymapserver.com/sprites/basic-v9",
            "rel":"item",
            "method":"GET",
            "title":"basic-v9"
        },{
            "href":"https://www.mymapserver.com/sprites/bright-v9",
            "rel":"item",
            "method":"GET",
            "title":"bright-v9"
        },{
            "href":"https://www.mymapserver.com/sprites/satellite-v9",
            "rel":"item",
            "method":"GET",
            "title":"satellite-v9"
        },{
            "href":"https://www.mymapserver.com/api/sprites",
            "rel":"self",
            "method":"GET",
            "title":"self"
        }
    ]
}
```

