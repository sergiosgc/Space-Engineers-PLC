Node.prototype.templateNode = function(deep, assignments) {
    let result = this.cloneNode(deep);
    result.removeAttributeNS(null, "id");
    for (const [path, value] of Object.entries(assignments)) {
        let match;
        if (match = path.match(/^xpath:(?<pre>.*)\/@(?<attribute>[A-Za-z_][-A-Za-z0-9.]*)$/)) {
            let pre = match.groups.pre;
            let attribute = match.groups.attribute;
            if (pre == "") pre = "/";
            if (typeof(value) == null) {
                result.queryElements("xpath:" + pre).forEach( (elm) => elm.removeAttribute(attribute));
            } else if (typeof(value) == "object" && attribute == "class") {
                result.queryElements("xpath:" + pre).forEach( (elm) => value.forEach( (classVal) => elm.classList.add(classVal)) );
            } else if (typeof(value) == "object" && attribute == "style") {
                result.queryElements("xpath:" + pre).forEach( (elm) => {
                    for (const [styleKey, styleValue] of Object.entries(value)) elm.style[styleKey] = styleValue;
                });
            } else {
                result.queryElements("xpath:" + pre).forEach( (elm) => elm.setAttributeNS(null, attribute, value));
            }
            continue;
        }
        result.queryElements(path).forEach( (node) => {
            if (node.nodeType == Node.ELEMENT_NODE) {
                while (node.firstChild) node.removeChild(node.firstChild);
                if (typeof(value) == "string") node.appendChild(document.createTextNode(value)); else node.appendChild(value);
            }
        });
    }
    return result;
}