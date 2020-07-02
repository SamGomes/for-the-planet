
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


suppressMessages(library(ggplot2))

suppressMessages(library(Rmisc))
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


analyse <- function(gameresultslog, adversary){


	endGames <- gameresultslog[gameresultslog$gameState == "VICTORY" | gameresultslog$gameState == "LOSS" ,]


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

	plotSRate <- ggplot(aggJ, aes(x = roundId, y=survivalRate, color=playerName)) 
	plotSRate <- plotSRate + geom_line(stat = "identity", size=1.2)
	plotSRate <- plotSRate + ylim(0, 1.0)
	
	if(adversary == 1){
		plotSRate <- plotSRate + ggtitle("vs. COOPERATION-PRONE")
	}else{
		plotSRate <- plotSRate + ggtitle("vs. DEFECTION-PRONE")
	}
	
	plotSRate <- plotSRate + labs(x = "Round Num", y = "Survived Games (%)") + theme(axis.text=element_text(size = 15), axis.title=element_text(size = 15, face = "bold")) #+ scale_x_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
	plotsSRate[[length(plotsSRate)+1]] <<- plotSRate



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

	plotWRate <- ggplot(ratio, aes(x = playerName , y=winRatio, fill=playerName)) 
	plotWRate <- plotWRate + geom_bar(stat = "identity")
	plotWRate <- plotWRate #+ ylim(0, 1.0)
	plotWRate <- plotWRate + labs(x = "Player Type", y = "WinRate (%)") 

	if(adversary == 1){
		plotWRate <- plotWRate + ggtitle("vs. COOPERATION-PRONE")
	}else{
		plotWRate <- plotWRate + ggtitle("vs. DEFECTION-PRONE")
	}

	plotWRate <- plotWRate + theme(axis.ticks = element_blank(), axis.text.x = element_blank()) + theme(axis.text=element_text(size = 15), axis.title=element_text(size = 15, face = "bold"))
	plotsWRate[[length(plotsWRate)+1]] <<- plotWRate



	# plot final econs
	endGamesWin <- endGames
	endGamesWin[endGamesWin$gameState == "LOSS" ,]$playerEconState <- 0

	agg <- aggregate(playerEconState ~ adversary + playerName, endGamesWin, mean)
	agg <- agg[agg$adversary == adversary,]
	plotFinalEcons <- ggplot(agg, aes(x = playerName , y=playerEconState, fill=playerName)) 
	plotFinalEcons <- plotFinalEcons + geom_bar(stat = "identity")
	# plot <- plot + ylim(0, 1.0)
	plotFinalEcons <- plotFinalEcons + labs(x = "Player Type", y = "Avg. Final Econs (%)") 
	
	if(adversary == 1){
		plotFinalEcons <- plotFinalEcons + ggtitle("vs. COOPERATION-PRONE")
	}else{
		plotFinalEcons <- plotFinalEcons + ggtitle("vs. DEFECTION-PRONE")
	}
	
	plotFinalEcons <- plotFinalEcons + theme(axis.ticks = element_blank(), axis.text.x = element_blank()) + theme(axis.text=element_text(size = 15), axis.title=element_text(size = 15, face = "bold"))
	plotsFinalEcons[[length(plotsFinalEcons)+1]] <<- plotFinalEcons



	# plot strategies
	agg <- gameresultslog
	agg_Root <- agg[agg$adversary == adversary,]
	agg <- aggregate(playerInvestEnv ~ playerName*roundId , agg_Root , mean)
	aggSD <- aggregate(playerInvestEnv ~ playerName*roundId , agg_Root , sd)

	plotStrategies <- ggplot(agg, aes(x = agg$roundId, y=agg$playerInvestEnv, group=agg$playerName, color=agg$playerName, alpha=aggJ$survivalRate)) 
	plotStrategies <- plotStrategies + geom_line(stat = "identity", size=1.2)
	plotStrategies <- plotStrategies + geom_point(stat = "identity", size=2.0) + scale_alpha(range = c(0.3, 1), guide=FALSE)
	
	if(adversary == 1){
		plotStrategies <- plotStrategies + ggtitle("vs. COOPERATION-PRONE")
	}else{
		plotStrategies <- plotStrategies + ggtitle("vs. DEFECTION-PRONE")
	}

	plotStrategies <- plotStrategies + labs(x = "Round Id", y = "Cooperation Investment", color="Player Type") + theme(axis.text = element_text(size = 15), axis.title = element_text(size = 15, face = "bold")) #+ scale_group_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
	plotsStrategies[[length(plotsStrategies)+1]] <<- plotStrategies

	

	# plot mood
	moodlog <- gameresultslog[!is.na(gameresultslog$mood),]
 	moodlog <- moodlog[moodlog$adversary == adversary,]
	plotMood <- ggplot(moodlog, aes(x = moodlog$roundId, y=moodlog$mood, color=playerName)) #+ facet_grid(playerName ~ .)
	plotMood <- plotMood + geom_line(stat = "summary", fun.y = "mean", size=1.2)
	plotMood <- plotMood + geom_point(aes(x = moodlog$roundId, y=moodlog$mood, color=playerName, size=2.0), stat = "summary", fun.y = "mean") 
	plotMood <- plotMood + labs(x = "Curr Round Id", y = "Mood", color="Player Type") + theme(axis.text = element_text(size = 15), axis.title = element_text(size = 15, face = "bold")) #+ scale_group_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  

	if(adversary == 1){
		plotMood <- plotMood + ggtitle("vs. COOPERATION-PRONE")
	}else{
		plotMood <- plotMood + ggtitle("vs. DEFECTION-PRONE")
	}

	plotMood <- plotMood + ylim(-10.0, 10.0)
	plotsMood[[length(plotsMood)+1]] <<- plotMood


}


analyse(gameresultslog, 1)
analyse(gameresultslog, 0)


png("plots/SurvivalRate.png")
multiplot(plotsSRate[1], plotsSRate[2])
dev.off()

png("plots/WinRate.png")
multiplot(plotsWRate[1], plotsWRate[2])
dev.off()

png("plots/EconLevels.png")
multiplot(plotsFinalEcons[1], plotsFinalEcons[2])
dev.off()

png("plots/StratsEnv.png")
multiplot(plotsStrategies[1], plotsStrategies[2])
dev.off()

png("plots/Mood.png")
multiplot(plotsMood[1], plotsMood[2])
dev.off()

# suppressMessages(ggsave("plots/SurvivalRate.png", height=6, width=10, units="in", dpi=500))
# multiplot(plotsWRate)
# suppressMessages(ggsave("plots/WinRate.png", height=6, width=10, units="in", dpi=500))
# multiplot(plotsFinalEcons)
# suppressMessages(ggsave("plots/EconLevels.png", height=6, width=10, units="in", dpi=500))
# multiplot(plotsStrategies)
# suppressMessages(ggsave("plots/StratsEnv.png", height=6, width=10, units="in", dpi=500))
# multiplot(plotsMood)
# suppressMessages(ggsave("plots/Mood.png", height=6, width=10, units="in", dpi=500))


