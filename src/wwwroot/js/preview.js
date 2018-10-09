/*

Copyright 2016, 2017, 2018 GIS People Pty Ltd

This file is part of GruntiMaps.

GruntiMaps is free software: you can redistribute it and/or modify it under 
the terms of the GNU Affero General Public License as published by the Free
Software Foundation, either version 3 of the License, or (at your option) any
later version.

GruntiMaps is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR
A PARTICULAR PURPOSE. See the GNU Affero General Public License for more 
details.

You should have received a copy of the GNU Affero General Public License along
with GruntiMaps.  If not, see <https://www.gnu.org/licenses/>.

*/
$(document).ready(function () {

    $.get("/api/fonts", function (fonts) {
        for (let k of fonts.content) {
            const option = new Option(k.name, k.links.href);
            $("#font-choices").append($(option));
        }
    });


    function loadLayers(source) {
        if (source.metadata === undefined) return;
        $.get(source.metadata.gruntimaps.styles, function (styles) {
            for (let style of styles) {
                if (map.getLayer(style.id) === undefined) {
                    let thisType = style.type;
                    let mapStyle = map.getStyle();
                    let existingLayerOfType = mapStyle.layers.find(function(e) { return e.type === thisType; });
                    if (existingLayerOfType !== undefined && existingLayerOfType !== null) map.addLayer(style, existingLayerOfType.id); else map.addLayer(style);
                    map.setLayoutProperty(style.id, "visibility", "visible");
                    const layer = document.createElement("a");
                    layer.href = "#";
                    layer.className = "active";
                    layer.textContent = style.id;
                    layer.onclick = function (e) {
                        const clickedLayer = this.text;
                        e.preventDefault();
                        e.stopPropagation();
                        const visibility = map.getLayoutProperty(clickedLayer, "visibility");
                        if (visibility === "visible") {
                            map.setLayoutProperty(clickedLayer, "visibility", "none");
                            this.className = "";
                        } else {
                            this.className = "active";
                            map.setLayoutProperty(clickedLayer, "visibility", "visible");
                        }
                    };
                    var layerGroup = document.getElementById(style.source);
                    //if (layerGroup === null) {
                    //    layerGroup = document.createElement("div");
                    //    layerGroup.id = style.source;
                    //    layerGroup.textContent = style.source
                    //    const menu = document.getElementById("menu");
                    //    menu.appendChild(layerGroup);
                    //}
                    layerGroup.appendChild(layer);
//                    const menu = document.getElementById("menu");
//                    menu.appendChild(layer);
                }

                //                    layergroup.appendChild(link);                                

            }
        });
    }

    // create preview map
    mapboxgl.config.REQUIRE_ACCESS_TOKEN = false;
    var host = window.location.protocol + "//" + window.location.host;
    map = new mapboxgl.Map({
        container: "map",
        style: {
            "version": 8,
            "sources": {},
            "layers": [],
            "glyphs": host + "/api/fonts/{fontstack}/{range}",
            "sprite": host + "/sprites/satellite-v9"
        },
        center: [-120, 21],
        zoom: 2
    });

    //document.map = map;

    const zlc = new ZoomLevelControl();
    map.addControl(zlc, "bottom-left");

    const nav = new mapboxgl.NavigationControl();
    map.addControl(nav, "bottom-left");

    map.on("load", function (e) {
        map.addSource("empty",
            {
                "type": "geojson",
                "data": { "type": "Feature", "properties": { "title": "empty" }, "geometry": null }
            });
        map.addLayer({ "type": "background", "id": "background-placeholder", "source": "empty", "layout": { "visibility": "none"} });
        map.addLayer({ "type": "raster", "id": "raster-placeholder", "source": "empty", "layout": { "visibility": "none"} });
        map.addLayer({ "type": "fill", "id": "fill-placeholder", "source": "empty", "layout": { "visibility": "none"} });
        map.addLayer({ "type": "fill-extrusion", "id": "fill-extrusion-placeholder", "source": "empty", "layout": { "visibility": "none"} });
        map.addLayer({ "type": "line", "id": "line-placeholder", "source": "empty", "layout": { "visibility": "none"} });
        map.addLayer({ "type": "circle", "id": "circle-placeholder", "source": "empty", "layout": { "visibility": "none"} });
        map.addLayer({ "type": "symbol", "id": "symbol-placeholder", "source": "empty", "layout": { "visibility": "none"} });
        map.addLayer({ "type": "heatmap", "id": "heatmap-placeholder", "source": "empty", "layout": { "visibility": "none"} });
        map.addLayer({ "type": "hillshade", "id": "hillshade-placeholder", "source": "empty", "layout": { "visibility": "none"} });
        $.get("/api/layers", function (layers) {
            for (let l of layers.content) {
                $.get(l.links.href, function (layerProps) {
                    var source = layerProps.source;
                    var style = layerProps.style;
                    $.get(source, function (src) {
                        src.metadata = { gruntimaps: { styles: style } };
                        map.addSource(src.name, src);
                    });
                });
            }
        });
    });
    map.on("sourcedata",
        function (e) {
            // console.log(e);
            // if (e.isSourceLoaded) {
            var layerGroup = document.getElementById(e.sourceId);
            if (layerGroup === null) {
                layerGroup = document.createElement("div");
                layerGroup.id = e.sourceId;
                layerGroup.textContent = e.source.description;
                layerGroup.className = "active";
                layerGroup.onclick = function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    for (let c of e.target.children) {
                        console.log(c);
                        if (map.getLayoutProperty(c.text, "visibility") === "visible") {
                            map.setLayoutProperty(c.text, "visibility", "none");
                            layerGroup.className = "";
                        } else {
                            map.setLayoutProperty(c.text, "visibility", "visible");
                            layerGroup.className = "active";
                        }
                    }
                    //const visibility = map.getLayoutProperty(clickedLayer, "visibility");
                    //if (visibility === "visible") {
                    //    map.setLayoutProperty(clickedLayer, "visibility", "none");
                    //    this.className = "";
                    //} else {
                    //    this.className = "active";
                    //    map.setLayoutProperty(clickedLayer, "visibility", "visible");
                    //}
                };

                const menu = document.getElementById("menu");
                menu.appendChild(layerGroup);
            }

            loadLayers(e.source);
            // }

        });

    map.on("mousemove", function (e) {
        var properties = map.queryRenderedFeatures(e.point);
        if (properties.length !== 0) {
            var props = [];
            for (var prop of properties) {
                props.push(prop.properties);
            }
            document.getElementById("properties").innerHTML = JSON.stringify(props, null, 2);
        }
    });
});
