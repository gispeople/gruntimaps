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
                if (window.map.getLayer(style.id) === undefined) {
                    const thisType = style.type;
                    const mapStyle = window.map.getStyle();
                    const existingLayerOfType = mapStyle.layers.find(function(e) { return e.type === thisType; });
                    if (existingLayerOfType !== undefined && existingLayerOfType !== null) window.map.addLayer(style, existingLayerOfType.id); else window.map.addLayer(style);
                    window.map.setLayoutProperty(style.id, "visibility", "visible");
                    const layer = document.createElement("a");
                    layer.href = "#";
                    layer.className = "active";
                    layer.textContent = style.id;
                    layer.onclick = function (e) {
                        const clickedLayer = this.text;
                        e.preventDefault();
                        e.stopPropagation();
                        const visibility = window.map.getLayoutProperty(clickedLayer, "visibility");
                        if (visibility === "visible") {
                            window.map.setLayoutProperty(clickedLayer, "visibility", "none");
                            this.className = "";
                        } else {
                            this.className = "active";
                            window.map.setLayoutProperty(clickedLayer, "visibility", "visible");
                        }
                    };
                    const layerGroup = document.getElementById(style.source);

                    layerGroup.appendChild(layer);
                }
            }
        });
    }

    // create preview map
    mapboxgl.config.REQUIRE_ACCESS_TOKEN = false;
    const host = window.location.protocol + "//" + window.location.host;
    window.map = new mapboxgl.Map({
        container: "map",
        style: {
            "version": 8,
            "sources": {},
            "layers": [],
            "glyphs": host + "/api/fonts/{fontstack}/{range}",
            "sprite": host + "/sprites/satellite-v9"
        },
        center: [-0.13, 51.52],
        zoom: 8
    });

    const nav = new mapboxgl.NavigationControl();
    window.map.addControl(nav, "top-left");

    const fsc = new mapboxgl.FullscreenControl();
    window.map.addControl(fsc, "top-left");

    //const inf = new InfoControl();
    //window.map.addControl(inf, "top-left");

    const sc = new mapboxgl.ScaleControl();
    window.map.addControl(sc, "bottom-left");

    // const att = new mapboxgl.AttributionControl({customAttribution: "Contains OS data &copy; Crown copyright and database rights 2018<br>Served using <a href='https://www.gruntimaps.com'>GruntiMaps</a>"});
    // window.map.addControl(att, "bottom-right");

    const zlc = new ZoomLevelControl();
    window.map.addControl(zlc, "bottom-right");

    window.map.on("load", function () {
        // the following empty layers are there to allow the other layers to be added in a predictable order.
        // 
        window.map.addSource("empty",
            {
                "type": "geojson",
                "data": { "type": "Feature", "properties": { "title": "empty" }, "geometry": null }
            });
        window.map.addLayer({ "type": "background", "id": "background-placeholder", "source": "empty", "layout": { "visibility": "none"} });
        window.map.addLayer({ "type": "raster", "id": "raster-placeholder", "source": "empty", "layout": { "visibility": "none"} });
        window.map.addLayer({ "type": "hillshade", "id": "hillshade-placeholder", "source": "empty", "layout": { "visibility": "none"} });
        window.map.addLayer({ "type": "fill", "id": "fill-placeholder", "source": "empty", "layout": { "visibility": "none"} });
        window.map.addLayer({ "type": "fill-extrusion", "id": "fill-extrusion-placeholder", "source": "empty", "layout": { "visibility": "none"} });
        window.map.addLayer({ "type": "line", "id": "line-placeholder", "source": "empty", "layout": { "visibility": "none"} });
        window.map.addLayer({ "type": "circle", "id": "circle-placeholder", "source": "empty", "layout": { "visibility": "none"} });
        window.map.addLayer({ "type": "symbol", "id": "symbol-placeholder", "source": "empty", "layout": { "visibility": "none"} });
        window.map.addLayer({ "type": "heatmap", "id": "heatmap-placeholder", "source": "empty", "layout": { "visibility": "none"} });
        $.get("/api/layers", function (layers) {
            for (let l of layers.content) {
                $.get(l.links.href, function (layerProps) {
                    const source = layerProps.source;
                    var style = layerProps.style;
                    $.get(source, function (src) {
                        src.metadata = { gruntimaps: { styles: style } };
                        window.map.addSource(src.name, src);
                    });
                });
            }
        });
    });
    window.map.on("sourcedata",
        function (e) {
            if (e.sourceDataType === "metadata") {
                var layerGroup = document.getElementById(e.sourceId);
                if (layerGroup === null) {
                    layerGroup = document.createElement("div");
                    layerGroup.id = e.sourceId;
                    layerGroup.textContent = e.source.description;
                    layerGroup.className = "active";
                    layerGroup.onclick = function(f) {
                        f.preventDefault();
                        f.stopPropagation();
                        for (let c of f.target.children) {
                            console.log(c);
                            if (window.map.getLayoutProperty(c.text, "visibility") === "visible") {
                                window.map.setLayoutProperty(c.text, "visibility", "none");
                                layerGroup.className = "";
                            } else {
                                window.map.setLayoutProperty(c.text, "visibility", "visible");
                                layerGroup.className = "active";
                            }
                        }
                    };

                    const menu = document.getElementById("menu");
                    menu.appendChild(layerGroup);
                }

                loadLayers(e.source);
            }
        }
    );
});
