var Plugin = {

    insertOneDB: function (currSessionId, currGameId , generation , playerName , playerType, playerTookFromCP, playerGain, envState) {
      var MongoClient = require('mongodb').MongoClient;
      var uri = 'mongodb+srv://admin:admin@fortheplanet-ya50d.mongodb.net/test?retryWrites=true&w=majority';
      var dbName = 'ForThePlanet';

      MongoClient.connect(uri, function(err, client) {
        if(err) {
            console.log('Error occurred while connecting to MongoDB Atlas...\n',err);
        }
        console.log('Connected...');
        var col = client.db(dbName).collection('fortheplanetlogs');

        var data = {'currSessionId':currSessionId,
        "currGameId":currGameId,
        "generation":generation,
        "playerName":playerName,
        "playerType":playerType,
        "playerTookFromCP":playerTookFromCP,
        "playerGain":playerGain,
        "envState":envState,
        };

        col.insertOne(data);
        client.close();
        });
    },


};
mergeInto(LibraryManager.library, Plugin);