let cytoscape = require('cytoscape');
let coseBilkent = require('cytoscape-cose-bilkent');

cytoscape.use( coseBilkent ); // register extension

module.exports = (callback, data) => {

    const cy = cytoscape();

    cy.add(data);

    cy.layout({
        name: 'cose-bilkent',
        animate: false
    }).run();

    // Access the node positions after layout
    const nodePositions = cy.nodes().map(node => ({
        id: node.id(),
        x: node.position().x,
        y: node.position().y,
    }));

    return callback(null, nodePositions);
};
