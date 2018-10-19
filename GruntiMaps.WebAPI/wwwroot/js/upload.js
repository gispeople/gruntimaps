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

$(document).ready(function() {

    $.get("/api/fonts",
        function(fonts) {
            for (let k of fonts.links) {
                const option = new Option(k.title, k.href);
                $("#font-choices").append($(option));
            }
        });

    // enable the form's colorpickers
    $(".color-picker").colorpicker();

    // create preview map
    //    mapboxgl.accessToken = "pk.eyJ1IjoieHBzZ3JlZW4iLCJhIjoiY2l3ZTB1YjRyMGFqZDJ5cGVicTBiemU5dyJ9.Ed1tY97mZHFeKhEnHoRc7w";
    mapboxgl.config.REQUIRE_ACCESS_TOKEN = false;
    var map = new mapboxgl.Map({
        container: "map",
        style: {
            "version": 8,
            "sources": {},
            "layers": [],
            "glyphs": "/api/fonts/{fontstack}/{range}"
        },
        center: [151, -29],
        zoom: 1
    });
    document.map = map;

    const zlc = new ZoomLevelControl();
    map.addControl(zlc, "top-right");

    const nav = new mapboxgl.NavigationControl();
    map.addControl(nav, "top-left");

    // manage map sizing to match window size
    function coordMapSize() {
        $("#map").height($(window).height() - $("nav").height() - $("#instructions").height() - 101);
        map.resize();
    }

    coordMapSize();
    $(window).on("resize", coordMapSize);

    // create map layer styling objects
    var lineLayer = {
        'id': "line",
        'source': "source",
        'type': "line",
        'paint': {
            'line-color': "#000",
            'line-opacity': 1,
            'line-width': 2
        }
    };
    var fillLayer = {
        'id': "fill",
        'source': "source",
        'type': "fill",
        'paint': {
            'fill-opacity': 0.2,
            'fill-color': "lightblue"
            // ,'fill-outline-color': "blue"
        }
    };
    var circleLayer = {
        'id': "circle",
        'source': "source",
        'type': "circle",
        'paint': {
            'circle-radius': 5,
            'circle-color': "blue",
            'circle-opacity': 1,
            'circle-stroke-width': 1,
            'circle-stroke-opacity': 1,
            'circle-stroke-color': "red"
        }
    };
    var symbolLayer = {
        'id': "symbol",
        'source': "source",
        'type': "symbol",
        'minZoom': 0,
        'layout': {
            'text-field': "",
            'text-size': 12,
            "text-font": ["Open Sans Regular"]
        }
    };

    function changeColor(e) {
        const tmp = e.target.name.split("-"); // first part will be layer type
        if (e.target.name === "symbol-text-color") {
            map.setPaintProperty("symbol", "text-color", e.target.value);
        } else if (e.target.name === "symbol-text-halo-color") {
            map.setPaintProperty("symbol", "text-halo-color", e.target.value);
        } else {
            map.setPaintProperty(tmp[0], e.target.name, e.target.value);
        }
    }

    function changeNumber(e) {
        const tmp = e.target.name.split("-"); // first part will be layer type
        if (e.target.name === "symbol-text-size") { // handle special cases
            map.setLayoutProperty("symbol", "text-size", Number(e.target.value));
        } else if (e.target.name === "symbol-text-halo-width") {
            map.setPaintProperty("symbol", "text-halo-width", Number(e.target.value));
        } else if (e.target.name === "symbol-text-halo-blur") {
            map.setPaintProperty("symbol", "text-halo-blur", Number(e.target.value));
        } else if (e.target.name === "symbol-min-zoom") {
            map.setLayerZoomRange("symbol", Number(e.target.value), 20);
        } else { // otherwise treat it as a paint property
            map.setPaintProperty(tmp[0], e.target.name, Number(e.target.value));
        }
    }

    $(".mb-number").on("change", changeNumber);

    $(".mb-color").on("change", changeColor);

    $("#btn-add-label").on("click",
        function() {
            const fieldname = $("#label-choices").val();
            const currentVal = $("#text-field-input").val();
            $("#text-field-input").val(currentVal + "{" + fieldname + "}");
            map.setLayoutProperty("symbol", "text-field", $("#text-field-input").val());
        });

    $("#font-choices").on("change",
        function() {
            map.setLayoutProperty("symbol", "text-font", [$("#font-choices").val()]);
        });

    $("#text-field-input").on("change",
        function() {
            map.setLayoutProperty("symbol", "text-field", $("#text-field-input").val());
        });

    $("#fill-polygons").on("change",
        function() {
            $("#fill-color-group").hide();
            // $("#fill-outline-color-group").hide();
            $("#fill-opacity-group").hide();
        });
    $("#fill-on").on("click",
        function() {
            $("#fill-color-group").show();
            // $("#fill-outline-color-group").show();
            $("#fill-opacity-group").show();
        });
    $("#fill-group input:radio").on("change",
        function() {
            if ($("#fill-group input:radio:checked").val() === "false") {
                $("#fill-color-group").hide();
                $("#fill-opacity-group").hide();
                map.setLayoutProperty(fillLayer.id, "visibility", "none");
            } else {
                $("#fill-color-group").show();
                $("#fill-opacity-group").show();
                map.setLayoutProperty(fillLayer.id, "visibility", "visible");
            }
        });
    $("#text-allow-overlap-group input:radio").on("change",
        function() {
            map.setLayoutProperty(symbolLayer.id,
                "text-allow-overlap",
                $("#text-allow-overlap-group input:radio:checked").val() === "true");
        });
    $("#text-justify-group input:radio").on("change",
        function() {
            map.setLayoutProperty(symbolLayer.id, "text-justify", $("#text-justify-group input:radio:checked").val());
        });

    var layerid;

    // $("#layer-name").on("change", function (evt){
    //     if ($("#layer-name").val()!="") $("#layername-hint").hide();
    //     else $("#layername-hint").show();
    // });

    $("#files").on("change",
        function() {
            var file = this.files[0];

            // reset state
            $("#message").removeClass().text("");
            $("#countAndType").text("");
            $("#filename-hint").show();
            $("#layername-hint").show();
            if (map.getLayer("fill")) map.removeLayer("fill");
            if (map.getLayer("line")) map.removeLayer("line");
            if (map.getLayer("circle")) map.removeLayer("circle");
            if (map.getLayer("symbol")) map.removeLayer("symbol");
            if (map.getSource("source")) map.removeSource("source");
            $("#attribs").hide();
            $("#name-group").prop("hidden", true);
            $("#upload").prop("hidden", true);
            $("#label-choices").empty();
            $("#text-field-input").val("");

            if (!file) {
                // no file now selected, so nothing further to do.
                return;
            }
            $("#filename-hint").hide();
            const fr = new FileReader();
            fr.onload = function(e) {
                var f = JSON.parse(e.target.result);

                if (f.crs === undefined) {
                    $("#message").removeClass().addClass("bg-info").text("File has no defined CRS, assuming WGS84.");
                } else if (f.crs.properties.name !== "urn:ogc:def:crs:OGC:1.3:CRS84")
                    $("#message").removeClass().addClass("bg-warning")
                        .text("WARNING: CRS does not appear to be WGS84!");
                if (f.name) {
                    layerid = f.name;
                } else {
                    // get rid of any extension/s
                    layerid = file.name.split(".")[0];
                }

                var symbols = false;

                var gt;
                var feat = f.features;
                for (var x = 0; x < feat.length; x++) {
                    var geo = feat[x].geometry;
                    var prop = feat[x].properties;
                    var k;
                    var option;
                    if (prop && !symbols) {
                        for (k in Object.keys(prop)) {
                            option = new Option(Object.keys(prop)[k], Object.keys(prop)[k]);
                            $("#label-choices").append($(option));
                        }
                        symbols = true;
                    }
                    gt = geo.type;
                    if (gt === "GeometryCollection") {
                        var geoms = geo.geometries;
                        for (var y = 0; y < geoms.length; y++) {
                            var geo1 = geoms[y];
                            var prop1 = geo1.properties;
                            if (prop1 && !symbols) {
                                for (k in Object.keys(prop1)) {
                                    option = new Option(k, k);
                                    $("#label-choices").append($(option));
                                }
                                symbols = true;
                            }
                            gt = geoms[y].type;
                        }
                    }
                }
                var bounds = geojsonExtent(f);
                map.fitBounds([[bounds[0], bounds[1]], [bounds[2], bounds[3]]]);
                if (map.getSource("source") === undefined)
                    map.addSource("source", { "type": "geojson", "data": f });
                else {
                    /* we already had a source, which means we already had a map, 
                     * so we'll remove any existing layers, and recreate the source.
                     */
                    if (map.getLayer("fill")) map.removeLayer("fill");
                    if (map.getLayer("line")) map.removeLayer("line");
                    if (map.getLayer("circle")) map.removeLayer("circle");
                    if (map.getLayer("symbol")) map.removeLayer("symbol");
                    map.removeSource("source");
                    map.addSource("source", { "type": "geojson", "data": f });
                }

                if (gt === "MultiPolygon" || gt === "Polygon") {
                    // polygon labels sit in the middle
                    symbolLayer.layout["text-offset"] = [0, 0];
                    $("#countAndType").text(f.features.length + " Polygons");
                    map.addLayer(lineLayer);
                    $("#line-color-input").val(lineLayer.paint["line-color"]);
                    $("#line-opacity").val(lineLayer.paint["line-opacity"]);
                    $("#line-width").val(lineLayer.paint["line-width"]);
                    map.addLayer(fillLayer);
                    $("#fill-opacity").val(fillLayer.paint["fill-opacity"]);
                    $("#fill-color-input").val(fillLayer.paint["fill-color"]);
                    $("#line-tab").addClass("active in").show();
                    $("#fill-tab").removeClass("active in").show();
                    $("#point-tab").removeClass("active in").hide();
                    $("#line-panel").addClass("active in");
                    $("#fill-panel").removeClass("active in");
                    $("#point-panel").removeClass("active in");
                    // $("#fill-outline-color-input").val("blue");
                } else if (gt === "MultiLineString" || gt === "LineString") {
                    // line labels sit above the line
                    symbolLayer.layout["text-offset"] = [0, -1];
                    $("#countAndType").text(f.features.length + " Lines");
                    map.addLayer(lineLayer);
                    $("#line-color-input").val(lineLayer.paint["line-color"]);
                    $("#line-opacity").val(lineLayer.paint["line-opacity"]);
                    $("#line-width").val(lineLayer.paint["line-width"]);
                    $("#line-tab").addClass("active in").show();
                    $("#fill-tab").removeClass("active in").hide();
                    $("#point-tab").removeClass("active in").hide();
                    $("#line-panel").addClass("active in");
                    $("#fill-panel").removeClass("active in");
                    $("#point-panel").removeClass("active in");
                } else if (gt === "MultiPoint" || gt === "Point") {
                    // point labels sit above the point
                    symbolLayer.layout["text-offset"] = [0, -1];
                    $("#countAndType").text(f.features.length + " Points");
                    map.addLayer(circleLayer);
                    $("#circle-radius").val(circleLayer.paint["circle-radius"]);
                    $("#circle-color-input").val(circleLayer.paint["circle-color"]);
                    $("#circle-opacity").val(circleLayer.paint["circle-opacity"]);
                    $("#circle-stroke-width").val(circleLayer.paint["circle-stroke-width"]);
                    $("#circle-stroke-opacity").val(circleLayer.paint["circle-stroke-opacity"]);
                    $("#circle-stroke-color-input").val(circleLayer.paint["circle-stroke-color"]);
                    $("#line-tab").removeClass("active in").hide();
                    $("#fill-tab").removeClass("active in").hide();
                    $("#point-tab").addClass("active in").show();
                    $("#line-panel").removeClass("active in");
                    $("#fill-panel").removeClass("active in");
                    $("#point-panel").addClass("active in");
                }

                if (symbols) {
                    map.addLayer(symbolLayer);
                    $("#symbol-text-size").val(symbolLayer.layout["text-size"]);
                    $("#font-choices").val(symbolLayer.layout["text-font"]);
                    $("#symbol-min-zoom").val(symbolLayer.minZoom);
                    $("#label-tab").removeClass("active in").show();
                } else {
                    $("#label-tab").removeClass("active in").hide();
                }

                $("#attribs").show();
                $("#name-group").prop("hidden", false);
                $("#upload").prop("hidden", false);

            };
            fr.readAsText(file);
        }); // end #files.change handler

    $("#upload").on("click",
        function() {
            const fileUpload = $("#files").get(0);
            const files = fileUpload.files;
            const layerdescription = $("#layer-name").val();
            const data = new FormData();
            for (let i = 0; i < files.length; i++) {
                data.append(files[i].name, files[i]);
            }

            const layers = [];

            // we need to set some of the layer properties slightly differently for the vector tile source.
            // in particular, the layer's id needs changing, we need to set the new source name,
            // and we need to set the source-layer property (which can't be set for a GeoJSON layer)
            const thisFill = map.getLayer("fill");
            if (thisFill !== undefined && thisFill !== null) {
                thisFill.id = layerid + "-fill";
                thisFill.source = layerid;
                thisFill["source-layer"] = layerid;
                layers.push(thisFill.serialize());
            }
            const thisLine = map.getLayer("line");
            if (thisLine !== undefined && thisLine !== null) {
                thisLine.id = layerid + "-line";
                thisLine.source = layerid;
                thisLine["source-layer"] = layerid;
                layers.push(thisLine.serialize());
            }
            const thisCircle = map.getLayer("circle");
            if (thisCircle !== undefined && thisCircle !== null) {
                thisCircle.id = layerid + "-circle";
                thisCircle.source = layerid;
                thisCircle["source-layer"] = layerid;
                layers.push(thisCircle.serialize());
            }
            const thisSymbol = map.getLayer("symbol");
            if (thisSymbol !== undefined && thisSymbol !== null) {
                thisSymbol.id = layerid + "-symbol";
                thisSymbol.source = layerid;
                thisSymbol["source-layer"] = layerid;
                layers.push(thisSymbol.serialize());
            }
            data.append("json", JSON.stringify(layers));
            data.append("layerDescription", layerdescription);
            data.append("layerId", layerid);
            data.append("instanceId", "server");
            $.ajax({
                type: "POST",
                url: "/import/UploadHandler",
                contentType: false,
                processData: false,
                data: data,
                success: function(message) {
                    if (message.result === "success") {
                        $("#message").removeClass().addClass("bg-success");
                    } else {
                        $("#message").removeClass().addClass("bg-danger");

                    }
                    $("#message").text(message.message);
                },
                error: function() {
                    $("#message").removeClass().addClass("bg-danger").text("There was an error uploading files!");
                }
            });
        }); // end #upload.click handler
});