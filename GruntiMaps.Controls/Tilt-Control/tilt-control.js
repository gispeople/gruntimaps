function utilBindAll(fns, context) {
    fns.forEach((fn) => {
        if (!context[fn]) {
            return;
        }
        context[fn] = context[fn].bind(context);
    });
};

function domCreate(tagName, className, container) {
    var el = window.document.createElement(tagName);
    if (className) el.className = className;
    if (container) container.appendChild(el);
    return el;
};

function domRemove(node) {
    if (node.parentNode) {
        node.parentNode.removeChild(node);
    }
};

var TiltControl = function TiltControl(defaultMapMode) {
    if (defaultMapMode === 0 || defaultMapMode === undefined) {
        this._tilt = false;
    }
    else {
        this._tilt = true;
    }
    utilBindAll(['_onClickTilt','_updatePitch'], this);
    this._className = 'mapboxgl-ctrl';
}

TiltControl.prototype.getDefaultPosition = function getDefaultPosition() {
    return 'top-left';
}

TiltControl.prototype.onAdd = function onAdd(map) {
    this._map = map;
    this._mapContainer = this._map.getContainer();
    this._container = domCreate('div', 'mapboxgl-ctrl mapboxgl-ctrl-group');
    this._setupUI();
    return this._container;
}

TiltControl.prototype.onRemove = function onRemove() {
    this._map.off('pitchend', this._updatePitch);
    this._tiltButton.removeEventListener("click", this._onClickTilt);
    domRemove(this._container);
    this._map = (null);
}

TiltControl.prototype._setupUI = function() {
    var button = this._tiltButton = domCreate('button', "tilt-icon", this._container);
    if (this._tilt === true) {
        button.textContent = "3D";
    } else {
        button.textContent = "2D";
    }
    button.setAttribute("aria-label", "Toggle 3D");
    button.type = 'button';
    this._tiltButton.addEventListener('click', this._onClickTilt);
    this._map.on('pitchend', this._updatePitch);
}

TiltControl.prototype._updatePitch = function _updatePitch() {
    if (this._map.getPitch() === 0) {
        this._tilt = false;
        this._tiltButton.textContent = "2D";
    } else {
        this._tilt = true;
        this._tiltButton.textContent = "3D";
    }
}

TiltControl.prototype._onClickTilt = function _onClickTilt() {
    if (this._tilt) {
        this._tiltButton.textContent = "2D";
        this._map.easeTo({ pitch: 0 });
    } else {
        this._tiltButton.textContent = "3D";
        this._map.easeTo({ pitch: 60 });
    }
    this._tilt = !this._tilt;
    $(document).trigger("mapModeChange", this._tilt);
}