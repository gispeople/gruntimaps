﻿/*

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
$(".validate").on("click", function (event) {
    var layerId = event.target.id;
    console.log("validate %s called", layerId);
    var errors = "";
    $.getJSON(`/api/layers/${layerId}/source`)
        .done(function (source) {
            console.log("source=%o", source);
            $.getJSON(`/api/layers/${layerId}/style`)
                .done(function (style) {
                    console.log("style=%o", style);
                    $.getJSON(`/api/layers/${layerId}/metadata`)
                        .done(function (data) {
                            var autogenerated = false;
                            console.log("data=%o", data);
                            // check that style references to source are correct
                            if (style === null || style.length === 0) {
                                errors += "no style found, ";
                                console.log("style not found for service");
                                if (data.tilestats === undefined || data.tilestats.layerCount === undefined || data.tilestats.layerCount === 0) {
                                    errors += "no tile stats data so can't generate a style either\n";
                                    console.log("no tile stats data available");
                                } else {
                                    for (let layerStats of data.tilestats.layers) {
                                        console.log("Layer %s has %i %s geometries and there are %i attributes", layerStats["layer"], layerStats["count"], layerStats["geometry"], layerStats["attributeCount"]);
                                    }
                                }
                            } else if (style[0].metadata && style[0].metadata.gruntimaps && style[0].metadata.gruntimaps.autogenerated) {
                                console.log("autogenerated");
                                autogenerated = true;
                            } else {
                                for (let thisStyle of style) {
                                    const srcMismatch = thisStyle.source !== source.name;
                                    if (srcMismatch) errors += "style source name mismatch\n";
                                    console.log("style source name matches source name: %s", srcMismatch ? "FAILED" : "OK");
                                    if (thisStyle["source-layer"] !== undefined) {
                                        const dataMismatch = data.vector_layers.find(function(e) {
                                                return e.id === thisStyle["source-layer"];
                                            }) ===
                                            undefined;
                                        if (dataMismatch) errors += "data source layer name mismatch\n";
                                        console.log("data layer name matches source layer (%s) name: %s",
                                            thisStyle["source-layer"],
                                            dataMismatch ? "FAILED" : "OK");
                                    }
                                }
                            }
                            if (errors === "") { errors = "OK"; }
                            if (autogenerated) {
                                errors += " (autogenerated style)";
                            }
                            event.target.text = errors;
                        });
                });
        });
});