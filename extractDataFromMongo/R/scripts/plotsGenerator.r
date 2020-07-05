
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
suppressMessages(library(reshape))
suppressMessages(library(dplyr))
suppressMessages(library(car))



gameresultslog <- read.csv(file="input/gameresultslog.csv", header=TRUE, sep=",")


# plot game balance
roundsNumL <- c()
num_played_roundsL <- c()
playerNamesL <- c()

gameresultslog <- gameresultslog[gameresultslog$playerName != "BALANCED-COOPERATOR" & gameresultslog$playerName != "BALANCED-DEFECTOR" ,]
gameresultslog$adversary <-""
gameresultslog[grepl("DEF", gameresultslog$playerName),]$adversary <- as.numeric(0)
gameresultslog[grepl("COOP", gameresultslog$playerName),]$adversary <- as.numeric(1)


computeStats <- function(subfolder, depVar, indepVar, filename){

	out <- shapiro.test(depVar[0:5000]) #normality test
	# sphericityTest = mauchly.test(depVar)

	if(out$p.value > 0.05){
		out <- summary(aov(depVar ~ indepVar))
	}else{
		out <- summary(friedman.test(depVar, indepVar))
	}

	filePath <- sprintf("stats/%s/%s_test.txt", subfolder, filename)
	capture.output(out, file = filePath)
}



analyse <- function(gameresultslog, subfolder, adversary){

	do.call(file.remove, list(list.files(sprintf("plots/%s", subfolder), full.names = TRUE)))

	endGames <- gameresultslog[gameresultslog$gameState == "VICTORY" | gameresultslog$gameState == "LOSS" ,]

	path = sprintf("plots/%s", subfolder)
	if(!dir.exists(path)){
		dir.create(path, showWarnings = TRUE, recursive = FALSE)
	}


	# plot survival rates
 	agg <- gameresultslog  %>% count(roundId, playerName, adversary)
	agg2 <- endGames  %>% count(playerName, adversary)
	colnames(agg) <- c("roundId", "playerName", "adversary", "survived")
	colnames(agg2) <- c("playerName", "adversary", "total")
	aggJ <- full_join(agg,agg2)
	aggJDiv <- aggJ$survived / aggJ$total
	aggJDiv[is.na(aggJDiv)] <- 0
	aggJ$survivalRate <- aggJDiv
 	aggJ <- aggJ[aggJ$adversary == adversary,]

	plot <- ggplot(aggJ, aes(x = roundId, y=survivalRate, color=playerName)) 
	plot <- plot + geom_line(stat = "identity")#  + facet_grid(playerNamesL ~ .)
	plot <- plot + ylim(0, 1.0)
	plot <- plot + labs(x = "Round Num", y = "Survived Games (%)") + theme(axis.text=element_text(size = 15), axis.title=element_text(size = 15, face = "bold")) #+ scale_x_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
	suppressMessages(ggsave(sprintf("plots/%s/SurvivalRate.png", subfolder), height=6, width=10, units="in", dpi=500))

	

	# aggJ$playerName <- factor(aggJ$playerName, levels= unique(aggJ$playerName))
	# newLevels <- c("AI-EMOTIONAL-CONSTRUCTIVE-COLLECTIVIST",
	# 	"AI-EMOTIONAL-CONSTRUCTIVE-COLLECTIVIST",
	# 	"AI-EMOTIONAL-CONSTRUCTIVE-INDIVIDUALISTIC",
	# 	"AI-EMOTIONAL-CONSTRUCTIVE-INDIVIDUALISTIC",
	# 	"AI-EMOTIONAL-DISRUPTIVE-COLLECTIVIST",
	# 	"AI-EMOTIONAL-DISRUPTIVE-COLLECTIVIST",
	# 	"AI-EMOTIONAL-DISRUPTIVE-INDIVIDUALISTIC",
	# 	"AI-EMOTIONAL-DISRUPTIVE-INDIVIDUALISTIC",
	# 	"RANDOM_CMP",
	# 	"RANDOM_CMP")

	# levels(aggJ$playerName) <- newLevels



	# plot win rate and draw rate
	df <- endGames[endGames$gameState == "VICTORY",]
	agg <- aggregate(playerEconState ~ gameId, df, max)
	names(agg) <- c("gameId", "maxState")
	joined <- full_join(df, agg)
	filteredJoined <- joined[joined$playerEconState == joined$maxState,]
	cnt <- count(filteredJoined,gameId)
	joined2 <- full_join(filteredJoined, cnt)
	winners = joined2[joined2$n==1,]
	drawers = joined2[joined2$n!=1,]

	wonGames <- winners  %>% count(playerName, adversary)
	playedGames <- endGames  %>% count(playerName, adversary)
	colnames(wonGames) <- c("playerName", "adversary", "wins")
	colnames(playedGames) <- c("playerName", "adversary", "total")
	ratio <- full_join(wonGames, playedGames)

	ratio[is.na(ratio)] <- 0
	ratio$winRatio <- ratio$wins / ratio$total
 	ratio <- ratio[ratio$adversary == adversary,]

	plot <- ggplot(ratio, aes(x = playerName , y=winRatio, fill=playerName)) 
	plot <- plot + geom_bar(stat = "identity")#  + facet_grid(playerNamesL ~ .)
	plot <- plot + ylim(0, 1.0)
	plot <- plot + labs(x = "Player Type", y = "WinRate (%)") 
	plot <- plot + theme(axis.ticks = element_blank(), axis.text.x = element_blank())
	suppressMessages(ggsave(sprintf("plots/%s/WinRate.png", subfolder), height=6, width=10, units="in", dpi=500))


	# df <- endGames[endGames$gameState == "VICTORY",]
	df <- endGames
	agg <- aggregate(playerEconState ~ adversary + playerName, df, mean)
	agg <- agg[agg$adversary == adversary,]
	plot <- ggplot(agg, aes(x = playerName , y=playerEconState, fill=playerName)) 
	plot <- plot + geom_bar(stat = "identity")#  + facet_grid(playerNamesL ~ .)
	# plot <- plot + ylim(0, 1.0)
	plot <- plot + labs(x = "Player Type", y = "Avg. Final Econs (%)") 
	plot <- plot + theme(axis.ticks = element_blank(), axis.text.x = element_blank())
	suppressMessages(ggsave(sprintf("plots/%s/EconLevels.png", subfolder), height=6, width=10, units="in", dpi=500))


	# ratio$playerName <- factor(ratio$playerName, levels= unique(ratio$playerName))
	# newLevels <- c("AI-EMOTIONAL-CONSTRUCTIVE-COLLECTIVIST",
	# 		"AI-EMOTIONAL-CONSTRUCTIVE-COLLECTIVIST",
	# 		"AI-EMOTIONAL-CONSTRUCTIVE-INDIVIDUALISTIC",
	# 		"AI-EMOTIONAL-CONSTRUCTIVE-INDIVIDUALISTIC",
	# 		"AI-EMOTIONAL-DISRUPTIVE-COLLECTIVIST",
	# 		"AI-EMOTIONAL-DISRUPTIVE-COLLECTIVIST",
	# 		"AI-EMOTIONAL-DISRUPTIVE-INDIVIDUALISTIC",
	# 		"AI-EMOTIONAL-DISRUPTIVE-INDIVIDUALISTIC",
	# 		"RANDOM_CMP",
	# 		"RANDOM_CMP")

	# levels(ratio$playerName) <- newLevels



	# plot strategies
	agg <- gameresultslog
	agg <- agg[agg$adversary == adversary,]
	agg <- aggregate(playerInvestEnv ~ playerName*envState , agg , mean)
	plot <- ggplot(agg, aes(x = agg$envState, y=agg$playerInvestEnv, group=agg$playerName, color=agg$playerName)) 
	plot <- plot + stat_summary_bin(fun.y='mean', bins=20, geom='point', aes(color=agg$playerName))
	plot <- plot + stat_summary_bin(fun.y='mean', bins=20, geom='line', aes(color=agg$playerName))
	plot <- plot + labs(x = "Env State", y = "Cooperation Investment", color="Player Type") + theme(axis.text = element_text(size = 15), axis.title = element_text(size = 15, face = "bold")) #+ scale_group_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
	plot <- plot + xlim(0.0, 1.0) + ylim(0.0, 5.0)
	suppressMessages(ggsave(sprintf("plots/%s/StratsEnv.png", subfolder), height=6, width=10, units="in", dpi=500))

	


	# # plot state
	# agg <- aggregate(envState ~ playerName*roundId , gameresultslog , mean)
	# plot <- ggplot(agg, aes(x = agg$roundId, y=agg$envState, group=agg$playerName, color=agg$playerName))
	# plot <- plot + geom_line(stat="identity")
	# plot <- plot + geom_point(aes(color=agg$playerName)) 
	# plot <- plot + labs(x = "Curr Round Id", y = "Env State", color="Player Type") + theme(axis.text = element_text(size = 15), axis.title = element_text(size = 15, face = "bold")) #+ scale_group_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
	# plot <- plot + ylim(0, 1.0)
	# suppressMessages(ggsave(sprintf("plots/%s/EnvState.png", subfolder), height=6, width=10, units="in", dpi=500))


	# agg <- aggregate(playerEconState ~ playerName*roundId , gameresultslog , mean)
	# plot <- ggplot(agg, aes(x = agg$roundId, y=agg$playerEconState, group=agg$playerName, color=agg$playerName))
	# plot <- plot + geom_line(stat="identity")
	# plot <- plot + geom_point(aes(color=agg$playerName)) 
	# plot <- plot + labs(x = "Curr Round Id", y = "Econ State", color="Player Type") + theme(axis.text = element_text(size = 15), axis.title = element_text(size = 15, face = "bold")) #+ scale_group_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
	# plot <- plot + ylim(0, 1.0)
	# suppressMessages(ggsave(sprintf("plots/%s/EconState.png", subfolder), height=6, width=10, units="in", dpi=500))





	# plot mood
	moodlog <- gameresultslog[!is.na(gameresultslog$mood),]
 	moodlog <- moodlog[moodlog$adversary == adversary,]
	plot <- ggplot(moodlog, aes(x = moodlog$roundId, y=moodlog$mood, color=playerName)) #+ facet_grid(playerName ~ .)
	plot <- plot + geom_line(stat = "summary", fun.y = "mean")
	plot <- plot + geom_point(aes(x = moodlog$roundId, y=moodlog$mood, color=playerName ), stat = "summary", fun.y = "mean") 
	plot <- plot + labs(x = "Curr Round Id", y = "Mood", color="Player Type") + theme(axis.text = element_text(size = 15), axis.title = element_text(size = 15, face = "bold")) #+ scale_group_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
	plot <- plot + ylim(-10.0, 10.0)
	suppressMessages(ggsave(sprintf("plots/%s/Mood.png", subfolder), height=6, width=10, units="in", dpi=500))

	


	# plot emotions
	feltEmotionsLog <- gameresultslog[!is.na(gameresultslog$mood) & gameresultslog$adversary== adversary,]
 # 	feltEmotionsLog <- feltEmotionsLog[feltEmotionsLog$adversary == subfolder,]
	
	feltEmotionsLog2 <- aggregate(
		 cbind(activeEmotions_Hate,
		 activeEmotions_Reproach,
		 activeEmotions_Shame,
		 activeEmotions_Anger,
		 activeEmotions_Remorse,
		 activeEmotions_Distress,
		 activeEmotions_Fear,
		 activeEmotions_Disappointment,
		 activeEmotions_FearConfirmed,
		 activeEmotions_Pity,
		 activeEmotions_Resentment,
		 activeEmotions_Love,
		 activeEmotions_Admiration,
		 activeEmotions_Pride,
		 activeEmotions_Gratitude,
		 activeEmotions_Gratification,
		 activeEmotions_Joy,
		 activeEmotions_Hope,
		 activeEmotions_Relief,
		 activeEmotions_Satisfaction,
		 activeEmotions_Gloating,
		 activeEmotions_HappyFor)
		 ~  playerName + roundId, feltEmotionsLog, mean)
	
	varsToConsider <- c("activeEmotions_Hate","activeEmotions_Reproach","activeEmotions_Shame","activeEmotions_Anger","activeEmotions_Remorse","activeEmotions_Distress","activeEmotions_Fear","activeEmotions_Disappointment","activeEmotions_FearConfirmed","activeEmotions_Pity","activeEmotions_Resentment","activeEmotions_Love","activeEmotions_Admiration","activeEmotions_Pride","activeEmotions_Gratitude","activeEmotions_Gratification","activeEmotions_Joy","activeEmotions_Hope","activeEmotions_Relief","activeEmotions_Satisfaction","activeEmotions_Gloating","activeEmotions_HappyFor")
	agg <- melt(feltEmotionsLog, id.vars = c("roundId","playerName"), measure.vars = varsToConsider)
	agg <- agg[agg$value != 0,]
	plot <- ggplot(agg, aes(x = agg$roundId, y = agg$value, color = agg$variable)) + facet_grid(playerName ~ .)
	plot <- plot + geom_line(stat = "summary", fun.y = "mean")
	plot <- plot + geom_point(aes(x = agg$roundId, y = agg$value, color = agg$variable), stat = "summary", fun.y = "mean") 
	plot <- plot + labs(x = "Curr Round Id", y = "Max. Emotion Intensity", color="Emotion Type") + theme(axis.text = element_text(size = 5), axis.title = element_text(size = 15, face = "bold")) #+ scale_group_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
	plot <- plot + ylim(0.0, 5.0)
	suppressMessages(ggsave(sprintf("plots/%s/Emotions.png", subfolder), height=6, width=10, units="in", dpi=500))


}


analyse(gameresultslog, "COOP", 1)
analyse(gameresultslog, "DEF", 0)