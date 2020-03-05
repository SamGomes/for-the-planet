
# install.packages("ggplot2", repos = 'http://cran.us.r-project.org')
# install.packages("multcomp", repos = 'http://cran.us.r-project.org')
# install.packages("nlme", repos = 'http://cran.us.r-project.org')
# install.packages("pastecs", repos = 'http://cran.us.r-project.org')
# install.packages("reshape", repos = 'http://cran.us.r-project.org')
# install.packages("tidyverse", repos = 'http://cran.us.r-project.org')
# install.packages("sjPlot", repos = 'http://cran.us.r-project.org')
# install.packages("sjmisc", repos = 'http://cran.us.r-project.org')
# install.packages("jsonlite", repos = 'http://cran.rstudio.com/')
# install.packages("stringr", repos = 'http://cran.rstudio.com/')

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


# plot game balance
gameresultslog <- read.csv(file="input/gameresultslog.csv", header=TRUE, sep=",")
totalGameResults <- data.frame(matrix(ncol = 0, nrow = max(gameresultslog$num_played_rounds)))
for(i in  seq(from=1, to=max(gameresultslog$num_played_rounds), by=1)) {
  totalGameResults$roundsNum[i] <- i #assume the id of the group is the first id of the players
  totalGameResults$num_played_rounds[i] <- length(which(gameresultslog$num_played_rounds >= i))/length(gameresultslog$num_played_rounds)  #assume the id of the group is the first id of the players
}
plot <- ggplot(totalGameResults, aes(x = totalGameResults$roundsNum , y=totalGameResults$num_played_rounds)) + geom_line(stat="identity") 
plot <- plot + ylim(0, 1.0)
plot <- plot + labs(x = "num_played_rounds", y = "Survived Games (%)") + theme(axis.text=element_text(size = 15), axis.title=element_text(size = 15, face = "bold")) #+ scale_x_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
suppressMessages(ggsave(sprintf("plots/OutcomeFrequencies.png"), height=6, width=10, units="in", dpi=500))



# plot strategies
strategieslog <- read.csv(file="input/strategies.csv", header=TRUE, sep=",")
agg <- aggregate(playerCurrInvestEnv ~ playerType*currRoundId , strategieslog , mean)
plot <- ggplot(agg, aes(x = agg$currRoundId, y=agg$playerCurrInvestEnv, group=agg$playerType, color=agg$playerType)) 
plot <- plot + geom_line(stat="identity")
plot <- plot + geom_point(aes(color=agg$playerType)) 
plot <- plot + labs(x = "Curr Round Id", y = "Cooperation Investment", color="Player Type") + theme(axis.text = element_text(size = 15), axis.title = element_text(size = 15, face = "bold")) #+ scale_group_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
plot <- plot + ylim(0, 5.0)
suppressMessages(ggsave(sprintf("plots/StratsEnv.png"), height=6, width=10, units="in", dpi=500))


# plot state
agg <- aggregate(envState ~ playerType*currRoundId , strategieslog , mean)
plot <- ggplot(agg, aes(x = agg$currRoundId, y=agg$envState, group=agg$playerType, color=agg$playerType))
plot <- plot + geom_line(stat="identity")
plot <- plot + geom_point(aes(color=agg$playerType)) 
plot <- plot + labs(x = "Curr Round Id", y = "Env State", color="Player Type") + theme(axis.text = element_text(size = 15), axis.title = element_text(size = 15, face = "bold")) #+ scale_group_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
# plot <- plot + ylim(0, 1.0)
suppressMessages(ggsave(sprintf("plots/EnvState.png"), height=6, width=10, units="in", dpi=500))


# print(colnames(strategieslog))
agg <- aggregate(playerEconState ~ playerType*currRoundId , strategieslog , mean)
plot <- ggplot(agg, aes(x = agg$currRoundId, y=agg$playerEconState, group=agg$playerType, color=agg$playerType))
plot <- plot + geom_line(stat="identity")
plot <- plot + geom_point(aes(color=agg$playerType)) 
plot <- plot + labs(x = "Curr Round Id", y = "Econ State", color="Player Type") + theme(axis.text = element_text(size = 15), axis.title = element_text(size = 15, face = "bold")) #+ scale_group_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
# plot <- plot + ylim(0, 1.0)
suppressMessages(ggsave(sprintf("plots/EconState.png"), height=6, width=10, units="in", dpi=500))


# plot mood
moodlog <- read.csv(file="input/moodLog.csv", header=TRUE, sep=",")
plot <- ggplot(moodlog, aes(x = moodlog$currGameRoundId, y=moodlog$mood, color = "")) + facet_grid(playerType ~ .)
plot <- plot + geom_line(stat = "summary", fun.y = "mean")
plot <- plot + geom_point(aes(x = moodlog$currGameRoundId, y=moodlog$mood), stat = "summary", fun.y = "mean") 
plot <- plot + labs(x = "Curr Round Id", y = "Mood", color="Player Type") + theme(axis.text = element_text(size = 15), axis.title = element_text(size = 15, face = "bold")) #+ scale_group_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
# plot <- plot + ylim(0, 1.0)
suppressMessages(ggsave(sprintf("plots/Mood.png"), height=6, width=10, units="in", dpi=500))



# plot emotions
feltEmotionsLog <- read.csv(file="input/feltEmotionsLog.csv", header=TRUE, sep=",")
varsToConsider <- c()
vars <- c("activeEmotions_Hate","activeEmotions_Reproach","activeEmotions_Shame","activeEmotions_Anger","activeEmotions_Remorse","activeEmotions_Distress","activeEmotions_Fear","activeEmotions_Disappointment","activeEmotions_FearConfirmed","activeEmotions_Pity","activeEmotions_Resentment","activeEmotions_Love","activeEmotions_Admiration","activeEmotions_Pride","activeEmotions_Gratitude","activeEmotions_Gratification","activeEmotions_Joy","activeEmotions_Hope","activeEmotions_Relief","activeEmotions_Satisfaction","activeEmotions_Gloating","activeEmotions_HappyFor")
j <- 1
for(i in  seq(from=1, to=length(vars), by=1)) {
	currVar = vars[i]
	isGood = 0

	for(k in  seq(from=1, to=length(feltEmotionsLog[,currVar]), by=1)) {
		currVarValue = feltEmotionsLog[,currVar][k]
		# print(currVarValue)
		if(currVarValue!=0){
			isGood = isGood + 1
		}
	}
	
	if(isGood > 0){
		varsToConsider[[j]] <- currVar
		j <- j + 1
	}
}
agg <- melt(feltEmotionsLog, id.vars = c("currSessionId","currGameId","currGameRoundId","currGamePhase","playerId","playerType"), measure.vars = varsToConsider)
levels(agg$playerType) <- c("CC","CI","DC","DI")
plot <- ggplot(agg, aes(x = agg$currGameRoundId, y = agg$value, color = agg$variable)) + facet_grid(playerType ~ .)
plot <- plot + geom_line(stat = "summary", fun.y = "mean")
plot <- plot + geom_point(aes(x = agg$currGameRoundId, y = agg$value, color = agg$variable), stat = "summary", fun.y = "mean") 
plot <- plot + labs(x = "Curr Round Id", y = "Max. Emotion Intensity", color="Emotion Type") + theme(axis.text = element_text(size = 15), axis.title = element_text(size = 15, face = "bold")) #+ scale_group_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
# plot <- plot + ylim(0, 1.0)
suppressMessages(ggsave(sprintf("plots/Emotions.png"), height=6, width=10, units="in", dpi=500))



# # emotions
# feltEmotionsLog <- read.csv(file="input/feltEmotionsLog.csv", header=TRUE, sep=",")
# plot <- ggplot(feltEmotionsLog, aes(x = feltEmotionsLog$playerType, fill = feltEmotionsLog$emotionType)) + geom_bar(color="black", position="fill") 
# plot <- plot + labs(x = "Player Type", y = "Times Emotion was Triggered", fill = "Emotion Type") + theme(axis.text=element_text(size = 15), axis.title=element_text(size = 15, face = "bold")) + scale_x_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
# suppressMessages(ggsave(sprintf("plots/EmotionsOld.png"), height=4, width=10, units="in", dpi=500))
