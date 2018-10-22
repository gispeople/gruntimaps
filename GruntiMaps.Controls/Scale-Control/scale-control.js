function GruntiMapsScaleControl(options) {
	this._options = options;
	utilBindAll(["_onMove"], this);
}

GruntiMapsScaleControl.prototype.getDefaultPosition = function getDefaultPosition() {
	return "bottom-left";
};

GruntiMapsScaleControl.prototype._onMove = function _onMove() {
	updateScale(this._map, this._imperialContainer, this._options, "imperial");
	updateScale(this._map, this._metricContainer, this._options, "metric");
};

function utilBindAll(fns, context) {
	fns.forEach((fn) => {
		if (!context[fn]) {
			return;
		}
		context[fn] = context[fn].bind(context);
	});
}

function domCreate(tagName, className, container) {
	var el = window.document.createElement(tagName);
	if (className) el.className = className;
	if (container) container.appendChild(el);
	return el;
}

function domRemove(node) {
	if (node.parentNode) {
		node.parentNode.removeChild(node);
	}
}
GruntiMapsScaleControl.prototype.onAdd = function onAdd(map) {
	this._map = map;
	this._container = domCreate("div", "mapboxgl-ctrl", map.getContainer());
	this._metricContainer = domCreate("div", "scale-metric", this._container);
	this._imperialContainer = domCreate("div", "scale-imperial", this._container);
	this._map.on("move", this._onMove);
	this._onMove();
	return this._container;
};

GruntiMapsScaleControl.prototype.onRemove = function onRemove() {
	domRemove(this._imperialContainer);
	domRemove(this._metricContainer);
	domRemove(this._container);
	this._map.off("move", this._onMove);
	this._map = (undefined);
};

function updateScale(map, container, options, unit) {
	var maxWidth = options && options.maxWidth || 100;
	var y = map._container.clientHeight / 2;
	var maxMeters = getDistance(map.unproject([0, y]), map.unproject([maxWidth, y]));
	if (unit === "imperial") {
		var maxFeet = 3.2808 * maxMeters;
		if (maxFeet > 5280) {
			var maxMiles = maxFeet / 5280;
			setScale(container, maxWidth, maxMiles, "mi");
		} else {
			setScale(container, maxWidth, maxFeet, "ft");
		}
	} else {
		setScale(container, maxWidth, maxMeters, "m");
	}
} 

function setScale(container, maxWidth, maxDistance, unit) {
	var distance = getRoundNum(maxDistance);
	var ratio = distance / maxDistance;
	if (unit === "m" && distance >= 1000) {
		distance = distance / 1000;
		unit = "km";
	}
	container.style.width = (maxWidth * ratio) + "px";
	container.innerHTML = distance + unit;
}

function getDistance(latlng1, latlng2) {
	var R = 6371000;
	var rad = Math.PI / 180,
		lat1 = latlng1.lat * rad,
		lat2 = latlng2.lat * rad,
		a = Math.sin(lat1) * Math.sin(lat2) +
            Math.cos(lat1) * Math.cos(lat2) * Math.cos((latlng2.lng - latlng1.lng) * rad);
	var maxMeters = R * Math.acos(Math.min(a, 1));
	return maxMeters;
}

function getRoundNum(num) {
	var pow10 = Math.pow(10, (("" + (Math.floor(num)))).length - 1);
	var d = num / pow10;
	d = d >= 10 ? 10 : d >= 5 ? 5 : d >= 3 ? 3 : d >= 2 ? 2 : 1;
	return pow10 * d;
}
