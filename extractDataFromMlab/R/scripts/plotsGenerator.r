
suppressMessages(library(ggplot2))
suppressMessages(library(ez))
suppressMessages(library(ggplot2))
suppressMessages(library(multcomp))
suppressMessages(library(nlme))
suppressMessages(library(pastecs))
suppressMessages(library(reshape))
suppressMessages(library(tidyverse))
suppressMessages(library(sjPlot))
suppressMessages(library(sjmisc))

gameresultslog <- read.csv(file="input/gameresultslog.csv", header=TRUE, sep=",")

plot <- ggplot(gameresultslog, aes(x = gameresultslog$type, fill = gameresultslog$outcome)) + geom_bar(color="black", position="fill") 
plot <- plot + labs(x = "Player Type", y = "Frequencies (%)", fill = "Game outcome") + theme(axis.text=element_text(size = 15), axis.title=element_text(size = 15, face = "bold")) + scale_x_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
suppressMessages(ggsave(sprintf("plots/OutcomeFrequencies.png")))

gameresultslogOutcomes<-split(gameresultslog, gameresultslog$outcome)
winOutcome<-gameresultslogOutcomes$VICTORY
agg <- aggregate(pos ~ type, winOutcome, mean)
plot <- ggplot(agg, aes(x = agg$type, y=agg$pos)) + geom_bar(color="black", stat="identity", fill = "#638ad3") 
plot <- plot + labs(x = "Player Type", y = "Mean Endgame Position") + theme(axis.text = element_text(size = 15), axis.title = element_text(size = 15, face = "bold")) + scale_x_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
suppressMessages(ggsave(sprintf("plots/Positions.png")))



feltEmotionsLog <- read.csv(file="input/feltEmotionsLog.csv", header=TRUE, sep=",")
plot <- ggplot(feltEmotionsLog, aes(x = feltEmotionsLog$playerType, fill = feltEmotionsLog$emotionType)) + geom_bar(color="black", position="fill") 
plot <- plot + labs(x = "Player Type", y = "Times Emotion was Triggered", fill = "Emotion Type") + theme(axis.text=element_text(size = 15), axis.title=element_text(size = 15, face = "bold")) + scale_x_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
suppressMessages(ggsave(sprintf("plots/Emotions.png")))


agg <- aggregate(econ_history_perc ~ type, gameresultslog, mean)
print(agg)
plot <- ggplot(agg, aes(x = agg$type)) + geom_bar(fill=agg$type, y = agg$econ_history_perc ,color="black", stat="identity", fill = "#638ad3", position = "dodge")  + geom_bar(fill=agg$type, y = agg$econ_history_perc ,color="black", stat="identity", fill = "#638ad3", position = "dodge") 
plot <- plot + labs(x = "Player Type", y = "Avg. Investments (%)", fill = "Investment Target") + theme(axis.text=element_text(size = 15), axis.title=element_text(size = 15, face = "bold")) + scale_x_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
suppressMessages(ggsave(sprintf("plots/Investments.png")))
