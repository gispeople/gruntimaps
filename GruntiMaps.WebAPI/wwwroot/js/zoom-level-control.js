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
function ZoomLevelControl() {}

ZoomLevelControl.prototype.onAdd = function (map) {
    this._map = map;
    var container = document.createElement("div");
    container.className = "mapboxgl-ctrl mapboxgl-ctrl-group";
    container.style += "padding: 10px; padding-top: 10px; padding-left: 10px; padding-right: 10px;";
    container.innerHTML = `<p>Zoom ${Math.round(map.getZoom() * 100 + Number.EPSILON) / 100}</p>`;
    map.on("zoomend",
        function () {
            container.innerHTML = `<p>Zoom ${Math.round(map.getZoom() * 100 + Number.EPSILON) / 100}</p>`;
        });
    return container;
};

ZoomLevelControl.prototype.onRemove = function () {
    this._container.parentNode.removeChild(this._container);
    this._map = undefined;
};
