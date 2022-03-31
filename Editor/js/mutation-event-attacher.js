class MutationEventAttacher {
    constructor(rootNode, xpathOrSelector, eventName, handlerFunction) {
        this.rootNode = rootNode;
        this.xpathOrSelector = xpathOrSelector;
        this.eventName = eventName;
        this.handlerFunction = handlerFunction;
        if (document.readyState == "loading") window.addEventListener("load", this.init.bind(this)); else this.init();
    }
    init() {
        this.targets = document.queryElements(this.xpathOrSelector, this.rootNode);
        this.targets.forEach( (target) => target.addEventListener(this.eventName, this.handlerFunction) );
        const observer = new MutationObserver(this.mutationCallback.bind(this));
        observer.observe(this.rootNode, { 
            childList: true,
            attributes: true,
            subtree: true
        })
    }
    mutationCallback() {
        let matchingNodes = document.queryElements(this.xpathOrSelector, this.rootNode);
        let deletedTargets = this.targets.filter( (target) => !matchingNodes.includes(target) );
        let newTargets = matchingNodes.filter( (target) => !this.targets.includes(target) );
        deletedTargets.forEach( (target) => target.removeEventListener(this.eventName, this.handlerFunction));
        newTargets.forEach( (target) => target.addEventListener(this.eventName, this.handlerFunction) );
        this.targets = this.targets.concat(newTargets).filter( (target) => !deletedTargets.includes(target) );
    }
}