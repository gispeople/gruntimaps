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
function utilBindAll(fns, context) {
    fns.forEach((fn) => {
        if (!context[fn]) {
            return;
        }
        context[fn] = context[fn].bind(context);
    });
};

function domCreate(tagName, className, container) {
    const el = window.document.createElement(tagName);
    if (className) el.className = className;
    if (container) container.appendChild(el);
    return el;
};

function domRemove(node) {
    if (node.parentNode) {
        node.parentNode.removeChild(node);
    }
};

var TiltControl = function(defaultMapMode) {
    if (defaultMapMode === 0 || defaultMapMode === undefined) {
        this._tilt = false;
    }
    else {
        this._tilt = true;
    }
    utilBindAll(["_onClickTilt","_updatePitch"], this);
    this._className = "mapboxgl-ctrl";
}

TiltControl.prototype.getDefaultPosition = function() {
    return "top-left";
}

TiltControl.prototype.onAdd = function(map) {
    this._map = map;
    this._mapContainer = this._map.getContainer();
    this._container = domCreate("div", "mapboxgl-ctrl mapboxgl-ctrl-group");
    this._setupUI();
    return this._container;
}

TiltControl.prototype.onRemove = function() {
    this._map.off("pitchend", this._updatePitch);
    this._tiltButton.removeEventListener("click", this._onClickTilt);
    domRemove(this._container);
    this._map = (null);
}

TiltControl.prototype._setupUI = function() {
    const button = this._tiltButton = domCreate("button", "tilt-icon", this._container);
    if (this._tilt === true) {
        button.textContent = "3D";
    } else {
        button.textContent = "2D";
    }
    button.setAttribute("aria-label", "Toggle 3D");
    button.type = "button";
    this._tiltButton.addEventListener("click", this._onClickTilt);
    this._map.on("pitchend", this._updatePitch);
}

TiltControl.prototype._updatePitch = function() {
    if (this._map.getPitch() === 0) {
        this._tilt = false;
        this._tiltButton.textContent = "2D";
    } else {
        this._tilt = true;
        this._tiltButton.textContent = "3D";
    }
}

TiltControl.prototype._onClickTilt = function() {
    if (this._tilt) {
        this._tiltButton.textContent = "2D";
        this._map.easeTo({ pitch: 0 });
    } else {
        this._tiltButton.textContent = "3D";
        this._map.easeTo({ pitch: 60 });
    }
    this._tilt = !this._tilt;
}