
# install.packages("ggplot2", repos = 'http://cran.us.r-project.org')
# install.packages("multcomp", repos = 'http://cran.us.r-project.org')
# install.packages("nlme", repos = 'http://cran.us.r-project.org')
# install.packages("pastecs", repos = 'http://cran.us.r-project.org')
# install.packages("reshape", repos = 'http://cran.us.r-project.org')
# install.packages("tidyverse", repos = 'http://cran.us.r-project.org')
# install.packages("sjPlot", repos = 'http://cran.us.r-project.org')
# install.packages("sjmisc", repos = 'http://cran.us.r-project.org')

suppressMessages(library(ggplot2))
suppressMessages(library(multcomp))
suppressMessages(library(nlme))
suppressMessages(library(pastecs))
suppressMessages(library(reshape))
suppressMessages(library(tidyverse))
suppressMessages(library(sjPlot))
suppressMessages(library(sjmisc))

gameresultslog <- read.csv(file="input/gameresultslog.csv", header=TRUE, sep=",")
strategieslog <- read.csv(file="input/strategies.csv", header=TRUE, sep=",")


# plot game balance
totalGameResults <- data.frame(matrix(ncol = 0, nrow = max(gameresultslog$num_played_rounds)))
for(i in  seq(from=1, to=max(gameresultslog$num_played_rounds), by=1)) {
  totalGameResults$roundsNum[i] <- i #assume the id of the group is the first id of the players
  totalGameResults$num_played_rounds[i] <- length(which(gameresultslog$num_played_rounds >= i))/300 #assume the id of the group is the first id of the players
}
plot <- ggplot(totalGameResults, aes(x = totalGameResults$roundsNum , y=totalGameResults$num_played_rounds )) + geom_line(stat="identity") 
plot <- plot + ylim(0, 1.0)
plot <- plot + labs(x = "num_played_rounds", y = "Survived Games (%)") + theme(axis.text=element_text(size = 15), axis.title=element_text(size = 15, face = "bold")) #+ scale_x_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
suppressMessages(ggsave(sprintf("plots/OutcomeFrequencies.png"), height=6, width=10, units="in", dpi=500))



# plot strategies
agg <- aggregate(playerCurrInvestEnv ~ playerType*currRoundId , strategieslog, mean)
# print(agg)
plot <- ggplot(agg, aes(x = agg$currRoundId, y=agg$playerCurrInvestEnv, group=agg$playerType)) + geom_line(stat="identity") 
plot <- plot + labs(x = "Cooperation Investment", y = "Curr Round Id") + theme(axis.text = element_text(size = 15), axis.title = element_text(size = 15, face = "bold")) #+ scale_group_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
suppressMessages(ggsave(sprintf("plots/Strats.png"), height=6, width=10, units="in", dpi=500))



# feltEmotionsLog <- read.csv(file="input/feltEmotionsLog.csv", header=TRUE, sep=",")
# plot <- ggplot(feltEmotionsLog, aes(x = feltEmotionsLog$playerType, fill = feltEmotionsLog$emotionType)) + geom_bar(color="black", position="fill") 
# plot <- plot + labs(x = "Player Type", y = "Times Emotion was Triggered", fill = "Emotion Type") + theme(axis.text=element_text(size = 15), axis.title=element_text(size = 15, face = "bold")) + scale_x_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
# suppressMessages(ggsave(sprintf("plots/Emotions.png"), height=4, width=10, units="in", dpi=500))


# meltedResults <- melt(gameresultslog, id.vars= c("sessionId","playerId","type"), measure.vars = c("econ_history_perc","env_history_perc"))
# colnames(meltedResults)[4] <- "target"
# colnames(meltedResults)[5] <- "investment"
# aggResults <- aggregate(investment ~ type + target , meltedResults, mean)
# plot <- ggplot(aggResults, aes(x = aggResults$type, y=aggResults$investment, fill = aggResults$target)) + geom_bar(color="black", stat="identity", position="dodge") 
# plot <- plot + scale_fill_discrete(labels = as.character(c("Economy","Environment")))
# plot <- plot + labs(x = "Player Type", y = "Avg. Investments (%)", fill = "Investment Target") + theme(axis.text = element_text(size = 15), axis.title = element_text(size = 15, face = "bold")) + scale_x_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
# suppressMessages(ggsave(sprintf("plots/Investments.png"), height=6, width=10, units="in", dpi=500))