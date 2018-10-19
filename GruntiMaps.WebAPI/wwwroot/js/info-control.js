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
function InfoControl() {}

InfoControl.prototype.onAdd = function (map) {
    this._map = map;
    var container = document.createElement("pre");
    container.className = "mapboxgl-ctrl mapboxgl-ctrl-group";
    container.style += "position: absolute; top: 200px; width: 300px; overflow: auto;background: rgba(255,255,255,0.8);";
    container.id = "info-container";
    container.innerHTML = '{}';
    this._container = container;
    map.on("mousemove", function(e) {
        var properties = map.queryRenderedFeatures(e.point);
        if (properties.length !== 0) {
            var props = [];
            for (var prop of properties) {
                if (Object.keys(prop.properties).length>0)
                    props.push(prop.properties);
            }
            document.getElementById("info-container").innerHTML = JSON.stringify(props, null, 2);
        } else {
            document.getElementById("info-container").innerHTML = '[]';
        }
    });
    return container;
};

InfoControl.prototype.onRemove = function () {
    this._container.parentNode.removeChild(this._container);
    this._map.off("mousemove")
    this._map = undefined;
};
