if (!Array.prototype.intersect) Array.prototype.intersect = function(arr) {
    return this.filter( (v) => arr.includes(v) );
}