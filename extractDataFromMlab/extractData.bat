mongoexport -h ds119996.mlab.com:19996 -d fortheplanetlogs -c gameresultslog -u studyAC1 -p studyAC1 -o ./gameresultslog.csv --csv -f sessionId,currGameId,condition,outcome,env_state,playerId,pos,type,econ_state,econ_history_perc,env_history_perc,num_played_rounds
rem mongoexport -h ds119996.mlab.com:19996 -d fortheplanetlogs -c playerInvestmentslog -u studyAC1 -p studyAC1 -o ./playerInvestmentslog.csv --csv -f currSessionId,currGameId,currGameRoundId,Name,playerId,playerType,amountEnv,amountEcon
rem mongoexport -h ds119996.mlab.com:19996 -d fortheplanetlogs -c playerslog -u studyAC1 -p studyAC1 -o ./playerslog.csv --csv -f sessionId,currGameId,Id,Name,type
mongoexport -h ds119996.mlab.com:19996 -d fortheplanetlogs -c feltEmotionsLog -u studyAC1 -p studyAC1 -o ./feltEmotionsLog.csv --csv -f currSessionId,currGameId,currGameRoundId,currGamePhase,playerId,playerType,strongestEmotionType,strongestEmotionIntensity,activeEmotions_Hate,activeEmotions_Reproach,activeEmotions_Shame,activeEmotions_Anger,activeEmotions_Remorse,activeEmotions_Distress,activeEmotions_Fear,activeEmotions_Disappointment,activeEmotions_FearConfirmed,activeEmotions_Pity,activeEmotions_Resentment,activeEmotions_Love,activeEmotions_Admiration,activeEmotions_Pride,activeEmotions_Gratitude,activeEmotions_Gratification,activeEmotions_Joy,activeEmotions_Hope,activeEmotions_Relief,activeEmotions_Satisfaction,activeEmotions_Gloating,activeEmotions_HappyFor
mongoexport -h ds119996.mlab.com:19996 -d fortheplanetlogs -c strategies -u studyAC1 -p studyAC1 -o ./strategies.csv --csv -f currSessionId,currGameId,currRoundId,playerId,playerType,playerCurrInvestEcon,playerCurrInvestEnv,playerEconState,envState
mongoexport -h ds119996.mlab.com:19996 -d fortheplanetlogs -c moodLog -u studyAC1 -p studyAC1 -o ./moodLog.csv --csv -f currSessionId,currGameId,currGameCondition,currGameRoundId,currGamePhase,playerId,playerType,mood