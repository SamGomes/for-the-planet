mongoexport --host Cluster0-shard-0/cluster0-shard-00-00-nfksn.mongodb.net:27017,\
cluster0-shard-00-01-nfksn.mongodb.net:27017,cluster0-shard-00-02-nfksn.mongodb.net:27017 \
--ssl --username admin \
--password admin \
--authenticationDatabase admin \
--db fortheplanetlog \
--collection fortheplanetlogs \
--type csv --out gameresultslog.csv \
\
-f sessionId,gameId,generation,playerName,playerType,playerTookFromCP,playerGain,envState,\
gameState,envState,playerEconState,playerName,playerType,\
playerInvestEcon,playerInvestEnv,playerInvHistEcon,playerInvHistEnv,\
\
strongestEmotionType,strongestEmotionIntensity,\
activeEmotions_Hate,activeEmotions_Reproach,\
activeEmotions_Shame,activeEmotions_Anger,\
activeEmotions_Remorse,activeEmotions_Distress,\
activeEmotions_Fear,activeEmotions_Disappointment,\
activeEmotions_FearConfirmed,activeEmotions_Pity,\
activeEmotions_Resentment,activeEmotions_Love,\
activeEmotions_Admiration,activeEmotions_Pride,\
activeEmotions_Gratitude,activeEmotions_Gratification,\
activeEmotions_Joy,activeEmotions_Hope,\
activeEmotions_Relief,activeEmotions_Satisfaction,\
activeEmotions_Gloating,activeEmotions_HappyFor,\
mood

mv gameresultslog.csv ./R/input;
cd R;
./compileData