
# install.packages("multcomp", dep = TRUE, repos = 'http://cran.us.r-project.org')
# install.packages("nlme", dep = TRUE, repos = 'http://cran.us.r-project.org')
# install.packages("pastecs", dep = TRUE, repos = 'http://cran.us.r-project.org')
# install.packages("reshape", dep = TRUE, repos = 'http://cran.us.r-project.org')
# install.packages("tidyverse", dep = TRUE, repos = 'http://cran.us.r-project.org')
# install.packages("sjPlot", dep = TRUE, repos = 'http://cran.us.r-project.org')
# install.packages("sjmisc", dep = TRUE, repos = 'http://cran.us.r-project.org')
# install.packages("jsonlite", dep = TRUE, repos = 'http://cran.rstudio.com/')
# install.packages("stringr", dep = TRUE, repos = 'http://cran.rstudio.com/')
# install.packages("Rmisc", dep=TRUE, repos = "http://cran.us.r-project.org")
# install.packages("egg", dep=TRUE, repos = "http://cran.us.r-project.org")
# install.packages("ggpubr", dep=TRUE, repos = "http://cran.us.r-project.org")


suppressMessages(library(ggplot2))
suppressMessages(library(Rmisc))
suppressMessages(library(reshape))
suppressMessages(library(dplyr))
suppressMessages(library(ggpubr))
suppressMessages(library(car))


library("ggsci")



gameresultslog <- read.csv(file="input/gameresultslog.csv", header=TRUE, sep=",")


# plot game balance
roundsNumL <- c()
num_played_roundsL <- c()
playerNamesL <- c()

gameresultslog <- subset(gameresultslog, gameresultslog$playerName != "BALANCED-COOPERATOR" & gameresultslog$playerName != "BALANCED-DEFECTOR")
gameresultslog$adversary <-""
gameresultslog[grepl("DEF", gameresultslog$playerName),]$adversary <- as.numeric(0)
gameresultslog[grepl("COOP", gameresultslog$playerName),]$adversary <- as.numeric(1)

levels(gameresultslog$playerName)[levels(gameresultslog$playerName) == "AI-EMOTIONAL-CONSTRUCTIVE-COLLECTIVIST_VS_DEF"] <- "Constructive-Collectivist"
levels(gameresultslog$playerName)[levels(gameresultslog$playerName) == "AI-EMOTIONAL-CONSTRUCTIVE-INDIVIDUALISTIC_VS_DEF"] <- "Constructive-Individualist"
levels(gameresultslog$playerName)[levels(gameresultslog$playerName) == "AI-EMOTIONAL-DISRUPTIVE-COLLECTIVIST_VS_DEF"] <- "Disruptive-Collectivist"
levels(gameresultslog$playerName)[levels(gameresultslog$playerName) == "AI-EMOTIONAL-DISRUPTIVE-INDIVIDUALISTIC_VS_DEF"] <- "Disruptive-Individualist"
levels(gameresultslog$playerName)[levels(gameresultslog$playerName) == "RANDOM_CMP_VS_DEF"] <- "RANDOM"


levels(gameresultslog$playerName)[levels(gameresultslog$playerName) == "MCTS_1_VS_COOP"] <- "MCTS (depth = 1)"
levels(gameresultslog$playerName)[levels(gameresultslog$playerName) == "MCTS_2_VS_COOP"] <- "MCTS (depth = 2)"
levels(gameresultslog$playerName)[levels(gameresultslog$playerName) == "MCTS_3_VS_COOP"] <- "MCTS (depth = 3)"
levels(gameresultslog$playerName)[levels(gameresultslog$playerName) == "MCTS_4_VS_COOP"] <- "MCTS (depth = 4)"
levels(gameresultslog$playerName)[levels(gameresultslog$playerName) == "MCTS_5_VS_COOP"] <- "MCTS (depth = 5)"


levels(gameresultslog$playerName)[levels(gameresultslog$playerName) == "AI-EMOTIONAL-CONSTRUCTIVE-COLLECTIVIST_VS_COOP"] <- "Constructive-Collectivist"
levels(gameresultslog$playerName)[levels(gameresultslog$playerName) == "AI-EMOTIONAL-CONSTRUCTIVE-INDIVIDUALISTIC_VS_COOP"] <- "Constructive-Individualist"
levels(gameresultslog$playerName)[levels(gameresultslog$playerName) == "AI-EMOTIONAL-DISRUPTIVE-COLLECTIVIST_VS_COOP"] <- "Disruptive-Collectivist"
levels(gameresultslog$playerName)[levels(gameresultslog$playerName) == "AI-EMOTIONAL-DISRUPTIVE-INDIVIDUALISTIC_VS_COOP"] <- "Disruptive-Individualist"
levels(gameresultslog$playerName)[levels(gameresultslog$playerName) == "RANDOM_CMP_VS_COOP"] <- "Random"


levels(gameresultslog$playerName)[levels(gameresultslog$playerName) == "MCTS_1_VS_DEF"] <- "MCTS (depth = 1)"
levels(gameresultslog$playerName)[levels(gameresultslog$playerName) == "MCTS_2_VS_DEF"] <- "MCTS (depth = 2)"
levels(gameresultslog$playerName)[levels(gameresultslog$playerName) == "MCTS_3_VS_DEF"] <- "MCTS (depth = 3)"
levels(gameresultslog$playerName)[levels(gameresultslog$playerName) == "MCTS_4_VS_DEF"] <- "MCTS (depth = 4)"
levels(gameresultslog$playerName)[levels(gameresultslog$playerName) == "MCTS_5_VS_DEF"] <- "MCTS (depth = 5)"


gameresultslog <- gameresultslog %>% rename(
     	 Hate = activeEmotions_Hate,
     	 Reproach = activeEmotions_Reproach,
		 Shame = activeEmotions_Shame,
		 Anger = activeEmotions_Anger,
		 Remorse = activeEmotions_Remorse,
		 Distress = activeEmotions_Distress,
		 Fear = activeEmotions_Fear,
		 Disappointment = activeEmotions_Disappointment,
		 FearConfirmed = activeEmotions_FearConfirmed,
		 Pity = activeEmotions_Pity,
		 Resentment = activeEmotions_Resentment,
		 Love = activeEmotions_Love,
		 Admiration = activeEmotions_Admiration,
		 Pride = activeEmotions_Pride,
		 Gratitude = activeEmotions_Gratitude,
		 Gratification = activeEmotions_Gratification,
		 Joy = activeEmotions_Joy,
		 Hope = activeEmotions_Hope,
		 Relief = activeEmotions_Relief,
		 Satisfaction = activeEmotions_Satisfaction,
		 Gloating = activeEmotions_Gloating,
		 HappyFor = activeEmotions_HappyFor
    )


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


do.call(file.remove, list(list.files("./plots/"), full.names = TRUE))
path = "plots"
if(!dir.exists(path)){
	dir.create(path, showWarnings = TRUE, recursive = FALSE)
}

plotsSRate <- c()
plotsWRate <- c()
plotsFinalEcons <- c()
plotsStrategies <- c()
plotsMood <- c()
plotsEmotions <- c()


labelSize <- 12.5


analyse <- function(gameresultslog, adversary){


	endGames <- subset(gameresultslog, gameresultslog$gameState == "VICTORY" | gameresultslog$gameState == "LOSS")



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

	plotSRate <- ggplot(aggJ, aes(x = roundId, y=survivalRate*100, color=playerName)) + scale_color_npg()
	plotSRate <- plotSRate + geom_line(stat = "identity", size=1.2)
	plotSRate <- plotSRate + ylim(0, 100.0)
	
	if(adversary == 1){
		plotSRate <- plotSRate + ggtitle("VS. Cooperation-Prone Opponents")
	}else{
		plotSRate <- plotSRate + ggtitle("VS. Defection-Prone Opponents")
	}
	
	write.csv2(aggJ, sprintf("output/SurvivalRates_adversary_%d.csv", adversary))

	plotSRate <- plotSRate + labs(x = "Round Num", y = "Survived Games (%)\n") + theme(legend.text = element_text(size=labelSize), plot.margin=margin(10,30,10,30), axis.text=element_text(size = 15), axis.title=element_text(size = 15, face = "bold"), legend.title = element_blank(), legend.position = 'bottom') + guides(col = guide_legend(ncol = 2)) 
	plotsSRate[[length(plotsSRate)+1]] <<- plotSRate



	# plot win rate and draw rate
	df <- endGames[endGames$gameState == "VICTORY",]
	agg <- aggregate(playerEconState ~ sessionId*gameId, df, max)
	names(agg) <- c("sessionId", "gameId", "maxState")
	joined <- full_join(df, agg)
	filteredJoined <- joined[joined$playerEconState == joined$maxState,]
	cnt <- count(filteredJoined, sessionId, gameId) #verify
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

	plotWRate <- ggplot(ratio, aes(x = playerName , y=winRatio*100, color=playerName, fill=playerName)) + scale_color_npg() + scale_fill_npg()
	plotWRate <- plotWRate + geom_bar(stat = "identity")

	if(adversary == 1){
		plotWRate <- plotWRate + ggtitle("VS. Cooperation-Prone Opponents")
	}else{
		plotWRate <- plotWRate + ggtitle("VS. Defection-Prone Opponents")
	}

	write.csv2(ratio, sprintf("output/WinRounds_adversary_%d.csv", adversary))

	plotWRate <- plotWRate + labs(x = "Player Type", y = "WinRate (%)\n") + theme(legend.text = element_text(size=labelSize), plot.margin=margin(10,30,10,30), axis.ticks = element_blank(), axis.text.x = element_blank(), axis.text=element_text(size = 15), axis.title=element_text(size = 15, face = "bold"), legend.title = element_blank(), legend.position = 'bottom') + guides(col = guide_legend(ncol = 2))
	plotsWRate[[length(plotsWRate)+1]] <<- plotWRate



	# plot final econs
	endGamesWin <- subset(endGames, endGames$gameState == "VICTORY")
	# endGamesWin[endGamesWin$gameState == "LOSS" ,]$playerEconState <- 0

	agg <- aggregate(playerEconState ~ adversary + playerName, endGamesWin, mean)
	agg <- agg[agg$adversary == adversary,]

	plotFinalEcons <- ggplot(agg, aes(x = playerName , y=playerEconState*100, color=playerName, fill=playerName)) + scale_color_npg() + scale_fill_npg()
	plotFinalEcons <- plotFinalEcons + geom_bar(stat = "identity")
	plotFinalEcons <- plotFinalEcons + labs(x = "Player Type", y = "Avg. Final Econs (%)\n") 
	
	if(adversary == 1){
		plotFinalEcons <- plotFinalEcons + ggtitle("VS. Cooperation-Prone Opponents")
	}else{
		plotFinalEcons <- plotFinalEcons + ggtitle("VS. Defection-Prone Opponents")
	}
	

	write.csv2(agg, sprintf("output/Econs_adversary_%d.csv", adversary))

	plotFinalEcons <- plotFinalEcons + theme(legend.text = element_text(size=labelSize), plot.margin=margin(10,30,10,30), axis.ticks = element_blank(), axis.text.x = element_blank()) + theme(axis.text=element_text(size = 15), axis.title=element_text(size = 15, face = "bold"), legend.title = element_blank(), legend.position = 'bottom') + guides(col = guide_legend(ncol = 2))
	plotsFinalEcons[[length(plotsFinalEcons)+1]] <<- plotFinalEcons



	# plot strategies
	agg <- gameresultslog
	agg_Root <- agg[agg$adversary == adversary,]
	agg <- aggregate(playerInvestEnv ~ playerName*roundId , agg_Root , mean)

	plotStrategies <- ggplot(agg, aes(x = roundId, y=playerInvestEnv, group=playerName, color=playerName, alpha=aggJ$survivalRate)) + scale_color_npg()
	plotStrategies <- plotStrategies + geom_line(stat = "identity", size=2.0, guide=FALSE)
	plotStrategies <- plotStrategies + geom_point(stat = "identity", size=3.0) + scale_alpha(range = c(0.3, 1), guide=FALSE)
	# plotStrategies <- plotStrategies + ylim(0, 5.0)

	if(adversary == 1){
		plotStrategies <- plotStrategies + ggtitle("VS. Cooperation-Prone Opponents")
	}else{
		plotStrategies <- plotStrategies + ggtitle("VS. Defection-Prone Opponents")
	}

	write.csv2(agg, sprintf("output/Strategies_adversary_%d.csv", adversary))

	plotStrategies <- plotStrategies + labs(x = "Round Id", y = "Avg. Cooperation Investment\n", color="Player Type") + theme(legend.text = element_text(size=labelSize), plot.margin=margin(10,30,10,30), axis.text = element_text(size = 15), axis.title = element_text(size = 15, face = "bold"), legend.title = element_blank(), legend.position = 'bottom') + guides(col = guide_legend(ncol = 2))  
	plotsStrategies[[length(plotsStrategies)+1]] <<- plotStrategies

	

	# plot mood
	# moodlog <- gameresultslog[!is.na(gameresultslog$mood),]
 # 	moodlog <- moodlog[moodlog$adversary == adversary,]
	# moodlog <- aggregate(mood ~ playerName*roundId , moodlog , mean)
	# plotMood <- ggplot(moodlog, aes(x = moodlog$roundId, y=moodlog$mood, color=moodlog$playerName, alpha=aggJ$survivalRate)) 
	# plotMood <- plotMood + scale_color_npg() + scale_alpha(range = c(0.3, 1), guide=FALSE)
	
	# plotMood <- plotMood + geom_hline(aes(yintercept = -3), linetype = "dashed", size = 0.8)
	# plotMood <- plotMood + geom_hline(aes(yintercept = 3), linetype = "dashed", size = 0.8)

	# plotMood <- plotMood + geom_line(stat = "identity", size=2.0) 
	# plotMood <- plotMood + geom_point(stat = "identity", size=3.0, guide=FALSE)
	# plotMood <- plotMood + labs(x = "Curr Round Id", y = "Mood\n", color="Player Type") + theme(legend.text = element_text(size=labelSize), plot.margin=margin(10,30,10,30), axis.text = element_text(size = 15), axis.title = element_text(size = 15, face = "bold"), legend.title = element_blank(), legend.position = 'bottom') + guides(col = guide_legend(ncol = 2))  

	
	# if(adversary == 1){
	# 	plotMood <- plotMood + ggtitle("VS. Cooperation-Prone Opponents")
	# }else{
	# 	plotMood <- plotMood + ggtitle("VS. Defection-Prone Opponents")
	# }

	# write.csv2(moodlog, sprintf("output/Mood_adversary_%d.csv", adversary))

	# plotMood <- plotMood + ylim(-10.0, 10.0)
	# plotsMood[[length(plotsMood)+1]] <<- plotMood




	# # plot emotions
	# feltEmotionsLog <- gameresultslog[gameresultslog$adversary== adversary,]
	# feltEmotionsLog <- aggregate(
	# 	 cbind(
	# 	 	 Hate,
	# 	 	 Reproach,
	# 		 Shame,
	# 		 Anger,
	# 		 Remorse,
	# 		 Distress,
	# 		 Fear,
	# 		 Disappointment,
	# 		 FearConfirmed,
	# 		 Pity,
	# 		 Resentment,
	# 		 Love,
	# 		 Admiration,
	# 		 Pride,
	# 		 Gratitude,
	# 		 Gratification,
	# 		 Joy,
	# 		 Hope,
	# 		 Relief,
	# 		 Satisfaction,
	# 		 Gloating,
	# 		 HappyFor)
	# 	 ~  playerName + roundId, feltEmotionsLog, mean)
	
	# varsToConsider <- c("Hate", "Reproach","Shame","Anger","Remorse","Distress","Fear","Disappointment","FearConfirmed","Pity","Resentment","Love","Admiration","Pride","Gratitude","Gratification","Joy","Hope","Relief","Satisfaction","Gloating","HappyFor")
	# agg <- melt(feltEmotionsLog, id.vars = c("roundId","playerName"), measure.vars = varsToConsider)
	# agg <- agg[agg$value != 0,]
	# agg <- aggregate(value ~ playerName*roundId*variable , agg , mean)
	# agg <- merge(agg, aggJ, by=c("playerName","roundId"))


	# plotEmotions <- ggplot(agg, aes(x = agg$roundId, y = agg$value, color = agg$variable, alpha=agg$survivalRate)) + facet_grid(playerName ~ .) + scale_color_npg() + scale_alpha(range = c(0.3, 1), guide=FALSE)
	# plotEmotions <- plotEmotions + geom_line(stat = "identity", size=1.2, guide=FALSE)
	# plotEmotions <- plotEmotions + geom_point(aes(x = agg$roundId, y = agg$value, color = agg$variable), stat = "summary", fun.y = "mean", guide=FALSE) 
	# plotEmotions <- plotEmotions + labs(x = "Curr Round Id", y = "Avg. Emotion Intensity\n", color="Emotion Type") + theme(legend.text = element_text(size=labelSize*1.2), plot.margin=margin(10,30,10,30), panel.spacing=unit(1.5, "lines"), axis.text = element_text(size = 15), axis.title = element_text(size = 15, face = "bold"), legend.title = element_blank(), legend.position = 'bottom') + guides(col = guide_legend(ncol = 2))
	
	# if(adversary == 1){
	# 	plotEmotions <- plotEmotions + ggtitle("VS. Cooperation-Prone Opponents")
	# }else{
	# 	plotEmotions <- plotEmotions + ggtitle("VS. Defection-Prone Opponents")
	# }

	# write.csv2(feltEmotionsLog, sprintf("output/Emotions_adversary_%d.csv", adversary))
	# plotsEmotions[[length(plotsEmotions)+1]] <<- plotEmotions

}


analyse(gameresultslog, 1)
analyse(gameresultslog, 0)


ggarrange(plotlist = plotsSRate)
suppressMessages(ggsave("plots/SurvivalRate.png", height=5, width=15, units="in", dpi=500))


ggarrange(plotlist = plotsWRate)
suppressMessages(ggsave("plots/WinRate.png", height=5, width=15, units="in", dpi=500))


ggarrange(plotlist = plotsFinalEcons)
suppressMessages(ggsave("plots/EconLevels.png", height=5, width=15, units="in", dpi=500))


ggarrange(plotlist = plotsStrategies)
suppressMessages(ggsave("plots/StratsEnv.png", height=5, width=15, units="in", dpi=500))


ggarrange(plotlist = plotsMood)
suppressMessages(ggsave("plots/Mood.png", height=5, width=15, units="in", dpi=500))


ggarrange(plotlist = plotsEmotions)
suppressMessages(ggsave("plots/Emotions.png", height=9, width=15, units="in", dpi=500))


# suppressMessages(ggsave("plots/SurvivalRate.png", height=6, width=10, units="in", dpi=500))
# multiplot(plotsWRate)
# suppressMessages(ggsave("plots/WinRate.png", height=6, width=10, units="in", dpi=500))
# multiplot(plotsFinalEcons)
# suppressMessages(ggsave("plots/EconLevels.png", height=6, width=10, units="in", dpi=500))
# multiplot(plotsStrategies)
# suppressMessages(ggsave("plots/StratsEnv.png", height=6, width=10, units="in", dpi=500))
# multiplot(plotsMood)
# suppressMessages(ggsave("plots/Mood.png", height=6, width=10, units="in", dpi=500))

