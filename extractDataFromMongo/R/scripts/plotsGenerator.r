
# install.packages("multcomp", dep = TRUE, repos = 'http://cran.us.r-project.org')
# install.packages("nlme", dep = TRUE, repos = 'http://cran.us.r-project.org')
# install.packages("pastecs", dep = TRUE, repos = 'http://cran.us.r-project.org')
# install.packages("reshape", dep = TRUE, repos = 'http://cran.us.r-project.org')
# install.packages("tidyverse", dep = TRUE, repos = 'http://cran.us.r-project.org')
# install.packages("sjPlot", dep = TRUE, repos = 'http://cran.us.r-project.org')
# install.packages("sjmisc", dep = TRUE, repos = 'http://cran.us.r-project.org')
# install.packages("jsonlite", dep = TRUE, repos = 'http://cran.rstudio.com/')
# install.packages("stringr", dep = TRUE, repos = 'http://cran.rstudio.com/')
# install.packages("ggplot2", dep=TRUE, repos = "http://cran.us.r-project.org")


suppressMessages(library(ggplot2))
suppressMessages(library(multcomp))
suppressMessages(library(nlme))
suppressMessages(library(pastecs))
suppressMessages(library(reshape))
suppressMessages(library(tidyverse))
suppressMessages(library(sjPlot))
suppressMessages(library(sjmisc))
suppressMessages(library(jsonlite))
suppressMessages(library(stringr))


gameresultslog <- read.csv(file="input/gameresultslog.csv", header=TRUE, sep=",")


# plot game balance
endGames <- gameresultslog[gameresultslog$gameState == "VICTORY" | gameresultslog$gameState == "LOSS" ,]
roundsNumL <- c()
num_played_roundsL <- c()
playerNamesL <- c()

# gameresultslog <- gameresultslog[gameresultslog$playerName != "BALANCED-COOPERATOR" & gameresultslog$playerName != "BALANCED-DEFECTOR" ,]
gameresultslog <- gameresultslog[gameresultslog$playerName != "BALANCED-DEFECTOR"  & gameresultslog$playerName != "RANDOM_CMP_VS_DEF"  & gameresultslog$playerName != "MCTS_VS_DEF" ,]
# gameresultslog <- gameresultslog[gameresultslog$playerName != "BALANCED-COOPERATOR" & gameresultslog$playerName != "RANDOM_CMP_VS_COOP"  & gameresultslog$playerName != "MCTS_VS_COOP" ,]

playerNames = unlist(distinct(gameresultslog, playerName))
print(playerNames)
# playerNames = c("AI-RANDOM","AI-EMOTIONAL-CONSTRUCTIVE-COLLECTIVIST","AI-EMOTIONAL-CONSTRUCTIVE-INDIVIDUALISTIC","AI-EMOTIONAL-DISRUPTIVE-COLLECTIVIST", "AI-EMOTIONAL-DISRUPTIVE-INDIVIDUALISTIC")
k <- 0
for(j in  seq(from=1, to=length(playerNames), by=1)) {
	gameresultsOfJ <- endGames[endGames$playerName == playerNames[j],]
	if(length(rownames(gameresultsOfJ)) > 0){
		for(i in  seq(from=1, to=max(gameresultsOfJ$roundId), by=1)) {
		  roundsNumL[k] <- i #assume the id of the group is the first id of the players
		  num_played_roundsL[k] <- length(which(gameresultsOfJ$roundId >= i))/length(gameresultsOfJ$roundId)  #assume the id of the group is the first id of the players
		  playerNamesL[k] <- playerNames[j]
		  k = k + 1
		}
	}
}
totalGameResults <- data.frame(roundsNumL,num_played_roundsL,playerNamesL)
plot <- ggplot(totalGameResults, aes(x = totalGameResults$roundsNumL , y=totalGameResults$num_played_roundsL, color=playerNamesL )) + geom_line(stat = "summary", fun.y = "mean")#  + facet_grid(playerNamesL ~ .)
plot <- plot + ylim(0, 1.0)
plot <- plot + labs(x = "num_played_rounds", y = "Survived Games (%)") + theme(axis.text=element_text(size = 15), axis.title=element_text(size = 15, face = "bold")) #+ scale_x_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
suppressMessages(ggsave(sprintf("plots/OutcomeFrequencies.png"), height=6, width=10, units="in", dpi=500))




# plot strategies
agg <- aggregate(playerInvestEnv ~ playerName*envState , gameresultslog , mean)
plot <- ggplot(agg, aes(x = agg$envState, y=agg$playerInvestEnv, group=agg$playerName, color=agg$playerName)) 
plot <- plot + geom_line(stat="identity")
plot <- plot + geom_point(aes(color=agg$playerName)) 
plot <- plot + labs(x = "Curr Round Id", y = "Cooperation Investment", color="Player Type") + theme(axis.text = element_text(size = 15), axis.title = element_text(size = 15, face = "bold")) #+ scale_group_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
plot <- plot + ylim(0, 5.0)
suppressMessages(ggsave(sprintf("plots/StratsEnv.png"), height=6, width=10, units="in", dpi=500))


# plot state
# victories <- gameresultslog[gameresultslog$gameState == "VICTORY",]
# allWinRouns <- gameresultslog[gameresultslog$gameId %in% victories$gameId & gameresultslog$sessionId %in% victories$sessionId,]

agg <- aggregate(envState ~ playerName*roundId , gameresultslog , mean)
plot <- ggplot(agg, aes(x = agg$roundId, y=agg$envState, group=agg$playerName, color=agg$playerName))
plot <- plot + geom_line(stat="identity")
plot <- plot + geom_point(aes(color=agg$playerName)) 
plot <- plot + labs(x = "Curr Round Id", y = "Env State", color="Player Type") + theme(axis.text = element_text(size = 15), axis.title = element_text(size = 15, face = "bold")) #+ scale_group_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
# plot <- plot + ylim(0, 1.0)
suppressMessages(ggsave(sprintf("plots/EnvState.png"), height=6, width=10, units="in", dpi=500))


agg <- aggregate(playerEconState ~ playerName*roundId , gameresultslog , mean)
plot <- ggplot(agg, aes(x = agg$roundId, y=agg$playerEconState, group=agg$playerName, color=agg$playerName))
plot <- plot + geom_line(stat="identity")
plot <- plot + geom_point(aes(color=agg$playerName)) 
plot <- plot + labs(x = "Curr Round Id", y = "Econ State", color="Player Type") + theme(axis.text = element_text(size = 15), axis.title = element_text(size = 15, face = "bold")) #+ scale_group_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
# plot <- plot + ylim(0, 1.0)
suppressMessages(ggsave(sprintf("plots/EconState.png"), height=6, width=10, units="in", dpi=500))


# plot mood
moodlog <- gameresultslog[!is.na(gameresultslog$mood),]
plot <- ggplot(moodlog, aes(x = moodlog$roundId, y=moodlog$mood, color=playerName)) #+ facet_grid(playerName ~ .)
plot <- plot + geom_line(stat = "summary", fun.y = "mean")
plot <- plot + geom_point(aes(x = moodlog$roundId, y=moodlog$mood, color=playerName ), stat = "summary", fun.y = "mean") 
plot <- plot + labs(x = "Curr Round Id", y = "Mood", color="Player Type") + theme(axis.text = element_text(size = 15), axis.title = element_text(size = 15, face = "bold")) #+ scale_group_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
plot <- plot + ylim(-10.0, 10.0)
suppressMessages(ggsave(sprintf("plots/Mood.png"), height=6, width=10, units="in", dpi=500))



# plot emotions
feltEmotionsLog <- gameresultslog[!is.na(gameresultslog$mood),]
varsToConsider <- c()
vars <- c("activeEmotions_Hate","activeEmotions_Reproach","activeEmotions_Shame","activeEmotions_Anger","activeEmotions_Remorse","activeEmotions_Distress","activeEmotions_Fear","activeEmotions_Disappointment","activeEmotions_FearConfirmed","activeEmotions_Pity","activeEmotions_Resentment","activeEmotions_Love","activeEmotions_Admiration","activeEmotions_Pride","activeEmotions_Gratitude","activeEmotions_Gratification","activeEmotions_Joy","activeEmotions_Hope","activeEmotions_Relief","activeEmotions_Satisfaction","activeEmotions_Gloating","activeEmotions_HappyFor")
j <- 1
for(i in  seq(from=1, to=length(vars), by=1)) {
	currVar = vars[i]
	isGood = 0

	for(k in  seq(from=1, to=length(feltEmotionsLog[,currVar]), by=1)) {
		currVarValue = feltEmotionsLog[,currVar][k]
		if(currVarValue!=0){
			isGood = isGood + 1
		}
	}
	
	if(isGood > 0){
		varsToConsider[[j]] <- currVar
		j <- j + 1
	}
}
agg <- melt(feltEmotionsLog, id.vars = c("sessionId","gameId","roundId","playerId","playerName"), measure.vars = varsToConsider)
levels(agg$playerName) <- c("CC","CI","DC","DI","Rnd")
plot <- ggplot(agg, aes(x = agg$roundId, y = agg$value, color = agg$variable)) + facet_grid(playerName ~ .)
plot <- plot + geom_line(stat = "summary", fun.y = "mean")
plot <- plot + geom_point(aes(x = agg$roundId, y = agg$value, color = agg$variable), stat = "summary", fun.y = "mean") 
plot <- plot + labs(x = "Curr Round Id", y = "Max. Emotion Intensity", color="Emotion Type") + theme(axis.text = element_text(size = 15), axis.title = element_text(size = 15, face = "bold")) #+ scale_group_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
plot <- plot + ylim(0, 5.0)
suppressMessages(ggsave(sprintf("plots/Emotions.png"), height=6, width=10, units="in", dpi=500))

