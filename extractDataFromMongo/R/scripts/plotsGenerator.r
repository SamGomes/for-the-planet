
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


df1 <- read.csv(file="input/AllVsAll/gameresultslog.csv", header=TRUE, sep=",")
df2 <- read.csv(file="input/AllVsAll/gameresultslog(2).csv", header=TRUE, sep=",")
df3 <- read.csv(file="input/AllVsAll/gameresultslog(3).csv", header=TRUE, sep=",")
df4 <- read.csv(file="input/AllVsAll/gameresultslog(4).csv", header=TRUE, sep=",")

gameresultslog <- rbind(df1,df2,df3,df4)

# plot game balance
roundsNumL <- c()
num_played_roundsL <- c()
playerTypesL <- c()

levels(gameresultslog$playerType)[levels(gameresultslog$playerType) == "AI-EMOTIONAL-CONSTRUCTIVE-COLLECTIVIST"] <- "Constructive-Collectivist"
levels(gameresultslog$playerType)[levels(gameresultslog$playerType) == "AI-EMOTIONAL-CONSTRUCTIVE-INDIVIDUALISTIC"] <- "Constructive-Individualist"
levels(gameresultslog$playerType)[levels(gameresultslog$playerType) == "AI-EMOTIONAL-DISRUPTIVE-COLLECTIVIST"] <- "Disruptive-Collectivist"
levels(gameresultslog$playerType)[levels(gameresultslog$playerType) == "AI-EMOTIONAL-DISRUPTIVE-INDIVIDUALISTIC"] <- "Disruptive-Individualist"

getMCTS <- function(mood)
{
	numMctsSteps = 0
	if(mood >= -10 && mood < -3.)
	{ 
		numMctsSteps = 3;
	}
	else if (mood >= -3 && mood <= 3)
	{
		numMctsSteps = 2;
	}
	else if (mood > 3 && mood <= 10) 
	{
		numMctsSteps = 1;
	}
	return(numMctsSteps)
}


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
plotsHeatMapMcts <- c()


labelSize <- 15.5


endGames <- subset(gameresultslog, gameresultslog$gameState == "VICTORY" | gameresultslog$gameState == "LOSS")



# plot survival rates
agg <- gameresultslog  %>% count(roundId, playerType)
agg2 <- endGames  %>% count(playerType)
colnames(agg) <- c("roundId", "playerType", "survived")
colnames(agg2) <- c("playerType", "total")
aggJ <- full_join(agg,agg2)
aggJDiv <- aggJ$survived / aggJ$total
aggJDiv[is.na(aggJDiv)] <- 0
aggJ$survivalRate <- aggJDiv

plotSRate <- ggplot(aggJ, aes(x = roundId, y=survivalRate*100, color=playerType)) + scale_color_npg()
plotSRate <- plotSRate + geom_line(stat = "identity", size=1.2)
plotSRate <- plotSRate + ylim(0, 100.0)

plotSRate <- plotSRate + ggtitle("All VS. All")


write.csv2(aggJ, "output/SurvivalRates_all.csv")

plotSRate <- plotSRate + labs(x = "Round Num", y = "Survival Rate (%)\n") + theme(plot.title = element_text(size=labelSize*1.4), legend.text = element_text(size=labelSize), plot.margin=margin(10,30,10,30), axis.text=element_text(size = 15), axis.title=element_text(size = 15, face = "bold"), legend.title = element_blank(), legend.position = 'bottom') + guides(col = guide_legend(ncol = 2)) 
plotSRate <- plotSRate + scale_x_continuous(breaks=c(1, 5, 10, 15, 20))
plotsSRate[[length(plotsSRate)+1]] <- plotSRate



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

wonGames <- winners  %>% count(playerType)
playedGames <- endGames  %>% count(playerType)
colnames(wonGames) <- c("playerType", "wins")
colnames(playedGames) <- c("playerType", "total")
ratio <- full_join(wonGames, playedGames)

ratio[is.na(ratio)] <- 0
ratio$winRatio <- ratio$wins / ratio$total

plotWRate <- ggplot(ratio, aes(x = playerType , y=winRatio*100, color=playerType, fill=playerType)) + scale_color_npg() + scale_fill_npg()
plotWRate <- plotWRate + geom_bar(stat = "identity")


plotWRate <- plotWRate + ggtitle("All VS. All")

write.csv2(ratio, "output/WinRounds_all.csv")

plotWRate <- plotWRate + labs(x = "Player Type", y = "Win Rate (%)\n") + theme(plot.title = element_text(size=labelSize*1.4), legend.text = element_text(size=labelSize), plot.margin=margin(10,30,10,30), axis.ticks = element_blank(), axis.text.x = element_blank(), axis.text=element_text(size = 15), axis.title=element_text(size = 15, face = "bold"), legend.title = element_blank(), legend.position = 'bottom') + guides(col = guide_legend(ncol = 2))
plotsWRate[[length(plotsWRate)+1]] <- plotWRate



# plot final econs
endGamesWin <- subset(endGames, endGames$gameState == "VICTORY")
# endGamesWin[endGamesWin$gameState == "LOSS" ,]$playerEconState <- 0

agg <- aggregate(playerEconState ~  playerType, endGamesWin, mean)

plotFinalEcons <- ggplot(agg, aes(x = playerType , y=playerEconState*100, color=playerType, fill=playerType)) + scale_color_npg() + scale_fill_npg()
plotFinalEcons <- plotFinalEcons + geom_bar(stat = "identity")
plotFinalEcons <- plotFinalEcons + labs(x = "Player Type", y = "Avg. Final Econs (%)\n") 

plotFinalEcons <- plotFinalEcons + ggtitle("All VS. All")

write.csv2(agg, "output/Econs_all.csv")

plotFinalEcons <- plotFinalEcons + theme(plot.title = element_text(size=labelSize*1.4), legend.text = element_text(size=labelSize), plot.margin=margin(10,30,10,30), axis.ticks = element_blank(), axis.text.x = element_blank()) + theme(axis.text=element_text(size = 15), axis.title=element_text(size = 15, face = "bold"), legend.title = element_blank(), legend.position = 'bottom') + guides(col = guide_legend(ncol = 2))
plotsFinalEcons[[length(plotsFinalEcons)+1]] <- plotFinalEcons



# plot strategies
agg <- gameresultslog
agg <- aggregate(playerInvestEnv ~ playerType*roundId , agg , mean)

plotStrategies <- ggplot(agg, aes(x = roundId, y=playerInvestEnv, group=playerType, color=playerType, alpha=aggJ$survivalRate)) + scale_color_npg()
plotStrategies <- plotStrategies + geom_line(stat = "identity", size=2.0, guide=FALSE)
plotStrategies <- plotStrategies + geom_point(stat = "identity", size=3.0) + scale_alpha(range = c(0.3, 1), guide=FALSE)
# plotStrategies <- plotStrategies + ylim(0, 5.0)

plotStrategies <- plotStrategies + ggtitle("All VS. All")

write.csv2(agg, "output/Strategies_all.csv")

plotStrategies <- plotStrategies + labs(x = "Round Id", y = "Avg. Cooperation Investment\n", color="Player Type") + theme(plot.title = element_text(size=labelSize*1.4), legend.text = element_text(size=labelSize), plot.margin=margin(10,30,10,30), axis.text = element_text(size = 15), axis.title = element_text(size = 15, face = "bold"), legend.title = element_blank(), legend.position = 'bottom') + guides(col = guide_legend(ncol = 2))  
plotStrategies <- plotStrategies + ylim(0.0, 5.0)
plotStrategies <- plotStrategies + scale_x_continuous(breaks=c(1, 5, 10, 15, 20))
plotsStrategies[[length(plotsStrategies)+1]] <- plotStrategies



# plot mood
moodlog <- gameresultslog[!is.na(gameresultslog$mood),]
moodAvg <- aggregate(mood ~ playerType*roundId , moodlog , mean)
plotMood <- ggplot(moodAvg, aes(x = moodAvg$roundId, y=moodAvg$mood, color=moodAvg$playerType, alpha=aggJ$survivalRate)) 
plotMood <- plotMood + scale_color_npg() + scale_alpha(range = c(0.3, 1), guide=FALSE)

# plotMood <- plotMood + geom_hline(aes(yintercept = -3), linetype = "dashed", size = 0.8)
# plotMood <- plotMood + geom_hline(aes(yintercept = 3), linetype = "dashed", size = 0.8)

plotMood <- plotMood + geom_line(stat = "identity", size=2.0) 
plotMood <- plotMood + geom_point(stat = "identity", size=3.0, guide=FALSE)
plotMood <- plotMood + labs(x = "Curr Round Id", y = "Mood\n", color="Player Type") + theme(plot.title = element_text(size=labelSize*1.4), legend.text = element_text(size=labelSize), plot.margin=margin(10,30,10,30), axis.text = element_text(size = 15), axis.title = element_text(size = 15, face = "bold"), legend.title = element_blank(), legend.position = 'bottom') + guides(col = guide_legend(ncol = 2))  

plotMood <- plotMood + ggtitle("All VS. All")

write.csv2(moodAvg, "output/Mood_all.csv")

plotMood <- plotMood + ylim(-10.0, 10.0)
plotMood <- plotMood + scale_x_continuous(breaks=c(1, 5, 10, 15, 20))
plotsMood[[length(plotsMood)+1]] <- plotMood




# plot heatmap for mcts level
# use mood log to get the mcts levels
mctsLog <- moodlog
mctsLog <- cbind(mctsLog, mcts = mapply(getMCTS, mctsLog$mood))
res <- mctsLog %>% count(roundId, playerType, mcts)


resAgg <- aggregate(n ~ playerType * roundId, res , sum)
colnames(res) <- c("roundId", "playerType", "mcts", "nMcts")
test <- merge(res, resAgg, by=c("roundId", "playerType"))
test$heat <- (test$nMcts / test$n)*100

# maybe use % instead of number of occurrences
# adjust the scale
mycol <- rgb(2, 144, 161, max = 255, alpha = 100)
mycol2 <- rgb(2, 144, 161, max = 255, alpha = 255)

margin = margin(0,0,0,0)

plotHeatMapMcts <- ggplot(test, aes(x=roundId, y=mcts, fill=heat)) + 
labs(x = "Round Id", y = "MCTS operating Depth\n", fill=" Relative\nFreq. (%)\n") + 
geom_tile(aes(), colour = "transparent") + scale_fill_gradient(low = mycol[1], high = mycol2[1], limits = c(0,100)) + 
theme(plot.title = element_text(size=labelSize*1.4), legend.text = element_text(size=labelSize*1.4), plot.margin=margin, panel.spacing=unit(1.5, "lines"), axis.text = element_text(size = labelSize*1.4), axis.title = element_text(size = labelSize*1.4, face = "bold"), legend.title=element_text(size = labelSize*1.4)) + 
scale_y_reverse() + 
facet_wrap(playerType ~ ., ncol=1) + 
theme(strip.text.x = element_text(size = labelSize*1.2))
# + theme(strip.text.y = element_text(size = labelSize*1.4))
plotHeatMapMcts <- plotHeatMapMcts + ggtitle("All VS. All")
plotHeatMapMcts <- plotHeatMapMcts + scale_x_continuous(breaks=c(1, 5, 10, 15, 20))

plotsHeatMapMcts[[length(plotsHeatMapMcts)+1]] <- plotHeatMapMcts


# plot emotions
feltEmotionsLog <- gameresultslog
feltEmotionsLog <- aggregate(
	 cbind(
	 	 Hate,
	 	 Reproach,
		 Shame,
		 Anger,
		 Remorse,
		 Distress,
		 Fear,
		 Disappointment,
		 FearConfirmed,
		 Pity,
		 Resentment,
		 Love,
		 Admiration,
		 Pride,
		 Gratitude,
		 Gratification,
		 Joy,
		 Hope,
		 Relief,
		 Satisfaction,
		 Gloating,
		 HappyFor)
	 ~  playerType + roundId, feltEmotionsLog, mean)

varsToConsider <- c("Hate", "Reproach","Shame","Anger","Remorse","Distress","Fear","Disappointment","FearConfirmed","Pity","Resentment","Love","Admiration","Pride","Gratitude","Gratification","Joy","Hope","Relief","Satisfaction","Gloating","HappyFor")
agg <- melt(feltEmotionsLog, id.vars = c("roundId","playerType"), measure.vars = varsToConsider)
agg <- agg[agg$value != 0,]
agg <- aggregate(value ~ playerType*roundId*variable , agg , mean)
agg <- merge(agg, aggJ, by=c("playerType","roundId"))


plotEmotions <- ggplot(agg, aes(x = agg$roundId, y = agg$value, color = agg$variable, alpha=agg$survivalRate)) +
	facet_wrap(playerType ~ ., ncol=1) + 
	theme(strip.text.x = element_text(size = labelSize*1.2)) +
	 scale_color_npg() +
	 scale_alpha(range = c(0.3, 1), guide=FALSE)
plotEmotions <- plotEmotions + geom_line(stat = "identity", size=1.2, guide=FALSE)
plotEmotions <- plotEmotions + geom_point(aes(x = agg$roundId, y = agg$value, color = agg$variable), stat = "summary", fun.y = "mean", guide=FALSE) 
plotEmotions <- plotEmotions + labs(x = "Curr Round Id", y = "Avg. Emotion Intensity\n", color="Emotion Type") + theme(plot.title = element_text(size=labelSize*1.4), legend.text = element_text(size=labelSize*1.2), plot.margin=margin(10,30,10,30), panel.spacing=unit(1.5, "lines"), axis.text = element_text(size = 15), axis.title = element_text(size = 15, face = "bold"), legend.title = element_blank(), legend.position = 'bottom') + guides(col = guide_legend(ncol = 2))

plotEmotions <- plotEmotions + ggtitle("All VS. All")
plotEmotions <- plotEmotions + ylim(0.0, 10.0)
plotEmotions <- plotEmotions + scale_x_continuous(breaks=c(1, 5, 10, 15, 20))

write.csv2(feltEmotionsLog, "output/Emotions_all.csv")
plotsEmotions[[length(plotsEmotions)+1]] <- plotEmotions



ggarrange(plotlist = plotsSRate)
suppressMessages(ggsave("plots/SurvivalRate.png", height=5, width=8, units="in", dpi=500))


ggarrange(plotlist = plotsWRate)
suppressMessages(ggsave("plots/WinRate.png", height=5, width=8, units="in", dpi=500))


ggarrange(plotlist = plotsFinalEcons)
suppressMessages(ggsave("plots/EconLevels.png", height=5, width=8, units="in", dpi=500))


ggarrange(plotlist = plotsStrategies)
suppressMessages(ggsave("plots/StratsEnv.png", height=5, width=8, units="in", dpi=500))


ggarrange(plotlist = plotsMood)
suppressMessages(ggsave("plots/Mood.png", height=4, width=8, units="in", dpi=500))


ggarrange(plotlist = plotsHeatMapMcts)
suppressMessages(ggsave("plots/HeatMapMcts.png", limitsize = FALSE, height=9, width=9, units="in", dpi=500))


ggarrange(plotlist = plotsEmotions)
suppressMessages(ggsave("plots/Emotions.png", height=10, width=9, units="in", dpi=500))

