document.queryElements = function(expression, referenceNode) {
    if (typeof(referenceNode) == "undefined") referenceNode = document;
    if (expression.startsWith("xpath:")) return (function(xpathResult) {
            let result = [];
            for(let node = xpathResult.iterateNext(); node; node = xpathResult.iterateNext()) result.push(node);
            return result;
        })(document.evaluate(expression.substring("xpath:".length), referenceNode, null, XPathResult.UNORDERED_NODE_ITERATOR_TYPE, null));
    if (expression.startsWith("css:")) return (function(nodeList) {
            let result = [];
            nodeList.forEach( (node) => result.push(node) );
            return result;
        })(referenceNode.querySelectorAll(expression.substring("css:".length)));
    throw new Error("Expression must start with 'xpath:' or 'css:'");
}
document.queryElement = (expression, referenceNode) => document.queryElements(expression, referenceNode)[0] ?? null;
Node.prototype.queryElements = function(expression) { return this.ownerDocument.queryElements(expression, this); }
Node.prototype.queryElement = function(expression) { return this.ownerDocument.queryElement(expression, this); }