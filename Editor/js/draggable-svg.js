class DraggableSVG {
    constructor(rootNode, draggableSelector, coordinateSelectors, grid) {
        this.rootNode = rootNode;
        this.draggableSelector = draggableSelector;
        this.coordinateSelectors = coordinateSelectors;
        if (!grid) grid = { "x": 1, "y": 1 };
        if (!grid.offset) grid.offset = { "x": 0, "y": 0 }
        this.grid = grid;
        new MutationEventAttacher(rootNode, draggableSelector, "mousedown", this.mousedown.bind(this));
        new MutationEventAttacher(rootNode, "xpath:.", "mousemove", this.mousemove.bind(this));
        new MutationEventAttacher(rootNode, "xpath:.", "mouseup", this.mouseup.bind(this));
        new MutationEventAttacher(rootNode, "xpath:.", "mouseleave", this.mouseleave.bind(this));
    }
    getMousePosition(evt) {
        let CTM = evt.target.queryElement("xpath:./ancestor-or-self::*[local-name() = 'svg']").getScreenCTM();
        let result = {
            x: (evt.clientX - CTM.e) / CTM.a,
            y: (evt.clientY - CTM.f) / CTM.d
        };
        return result;
    }

    mousedown(evt) {
        let target = evt.target.queryElements("xpath:./ancestor-or-self::*").intersect(this.rootNode.queryElements(this.draggableSelector)).pop();
        this.rootNode.drag = {
            "origin": this,
            "target": target,
            "dragStarted": false,
            "mouseDownPosition": this.getMousePosition(evt),
            "startCoordinates": {
                "x": this.coordinateSelectors.x.map( (selector) => target.queryElements(selector).map( (attr) => parseInt(attr.value) )).flat(),
                "y": this.coordinateSelectors.y.map( (selector) => target.queryElements(selector).map( (attr) => parseInt(attr.value) )).flat(),
            }
        };
    }
    mousemove(evt) {
        if (!this.rootNode.drag) return;
        if (this != this.rootNode.drag.origin) return;
        let target = this.rootNode.drag.target;
        let position = this.getMousePosition(evt);
        let delta = {
            "x": position.x - this.rootNode.drag.mouseDownPosition.x,
            "y": position.y - this.rootNode.drag.mouseDownPosition.y,
        };
        if (!evt.ctrlKey) {
            delta.x = Math.round((this.rootNode.drag.startCoordinates.x[0] + delta.x + this.grid.offset.x ) / this.grid.x) * this.grid.x - this.grid.offset.x - this.rootNode.drag.startCoordinates.x[0];
            delta.y = Math.round((this.rootNode.drag.startCoordinates.y[0] + delta.y + this.grid.offset.y ) / this.grid.y) * this.grid.y - this.grid.offset.y - this.rootNode.drag.startCoordinates.y[0];
        }
        if (!this.rootNode.drag.dragStarted) {
            const dragstart = new DragEvent("dragstart", {
                "clientX": evt.clientX,
                "clientY": evt.clientY,
                "layerX": evt.layerX,
                "layerY": evt.layerY,
                "ctrlKey": evt.ctrlKey,
                "altKey": evt.altKey,
                "shiftKey": evt.shiftKey,
                "metaKey": evt.metaKey,
                "buttons": evt.buttons,
                "button": evt.button,
                "bubble": true,
                "cancelable": true,

            });
            target.dispatchEvent(dragstart);
            this.rootNode.drag.dragStarted = true;
        }
        const dragevent = new DragEvent("drag", {
            "clientX": evt.clientX,
            "clientY": evt.clientY,
            "layerX": evt.layerX,
            "layerY": evt.layerY,
            "ctrlKey": evt.ctrlKey,
            "altKey": evt.altKey,
            "shiftKey": evt.shiftKey,
            "metaKey": evt.metaKey,
            "buttons": evt.buttons,
            "button": evt.button,
            "bubble": true,
            "cancelable": true,

        });
        target.dispatchEvent(dragevent);
        let newCoordinates = {
            "x": this.rootNode.drag.startCoordinates.x.map( (x) => x + delta.x ),
            "y": this.rootNode.drag.startCoordinates.y.map( (y) => y + delta.y ),
        };
        this.coordinateSelectors.x.map( (selector) => target.queryElements(selector) ).flat().forEach( (attr, i) => attr.value = newCoordinates.x[i]);
        this.coordinateSelectors.y.map( (selector) => target.queryElements(selector) ).flat().forEach( (attr, i) => attr.value = newCoordinates.y[i]);
    }
    mouseup(evt) {
        if (!this.rootNode.drag) return;
        if (this != this.rootNode.drag.origin) return;
        delete this.rootNode.drag;
    }
    mouseleave(evt) {
        if (!this.rootNode.drag) return;
        if (this != this.rootNode.drag.origin) return;
        let target = this.rootNode.drag.target;
        this.coordinateSelectors.x.map( (selector) => target.queryElements(selector) ).flat().forEach( (attr, i) => attr.value = this.rootNode.drag.startCoordinates.x[i]);
        this.coordinateSelectors.y.map( (selector) => target.queryElements(selector) ).flat().forEach( (attr, i) => attr.value = this.rootNode.drag.startCoordinates.y[i]);
        const dragevent = new DragEvent("drag", {
            "clientX": evt.clientX,
            "clientY": evt.clientY,
            "layerX": evt.layerX,
            "layerY": evt.layerY,
            "ctrlKey": evt.ctrlKey,
            "altKey": evt.altKey,
            "shiftKey": evt.shiftKey,
            "metaKey": evt.metaKey,
            "buttons": evt.buttons,
            "button": evt.button,
            "bubble": true,
            "cancelable": true,

        });
        target.dispatchEvent(dragevent);
        delete this.rootNode.drag;
    }
}