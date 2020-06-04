const MongoClient = require('mongodb').MongoClient;

const test = require('assert');

// Connection url

const url = 'mongodb://admin:admin@fortheplanet-ya50d.mongodb.net/test?retryWrites=true&w=majority';

// Database Name

const dbName = 'ForThePlanet';

// Connect using MongoClient

MongoClient.connect(url, function(err, client) {

// Create a collection we want to drop later

const col = client.db(dbName).collection('fortheplanetlogs');

// Show that duplicate records got dropped

col.find({}).toArray(function(err, items) {

test.equal(null, err);

test.equal(4, items.length);

client.close();

});

});