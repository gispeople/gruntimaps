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

        $.get(source.metadata.gruntimaps.styles, function(styles) {
            for (let t in styles) {
                if (styles.hasOwnProperty(t)) {
                    if (map.getLayer(styles[t].id) === undefined) {
                        map.addLayer(styles[t]);
                        map.setLayoutProperty(styles[t].id, "visibility", "visible");
                        const link = document.createElement("a");
                        link.href = "#";
                        link.className = "active";
                        link.textContent = styles[t].id;
                        link.onclick = function(e) {
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
                        const layers = document.getElementById("menu");
                        layers.appendChild(link);
                    }

                    layergroup = document.getElementById(styles[t].source);
                    if (layergroup===null) { 
                        layergroup = document.createElement("div");
                        layergroup.id = styles[t].source;
                        const menu = document.getElementById("menu"); 
                        menu.appendChild(layergroup);
                    }
//                    layergroup.appendChild(link);                                
                }
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

    map.on("load", function(e) {
        $.get("/api/layers", function(layers) {
            console.log(layers);
            for (let l of layers.content) {
                $.get(l.links.href, function(layerProps) {
                    console.log(layerProps);
                    var source = layerProps.source;
                    var style = layerProps.style;
                    $.get(source, function(src) {
                        src.metadata = { gruntimaps: { styles: style } };
                        map.addSource(src.name, src);
                    });
                });
            }
        });
    });
    map.on("sourcedata",
        function(e) {
            // console.log(e);
            // if (e.isSourceLoaded) {
            loadLayers(e.source);
            // }

        });

    map.on("mousemove", function(e) {
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
