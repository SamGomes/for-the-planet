
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
# gameresultslogOutcomes<-split(gameresultslog, gameresultslog$outcome)
# winOutcome<-gameresultslogOutcomes$VICTORY
# lossOutcome<-gameresultslogOutcomes$LOSS

plot <- ggplot(gameresultslog, aes(x = gameresultslog$type, fill = gameresultslog$outcome)) + geom_bar(color="black", position="fill") 
plot <- plot + labs(x = "Player Type", y = "Frequencies (%)", fill = "Game outcome") + theme(axis.text=element_text(size = 15), axis.title=element_text(size = 15, face = "bold")) + scale_x_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
suppressMessages(ggsave(sprintf("plots/OutcomeFrequencies.png")))

gameresultslogOutcomes<-split(gameresultslog, gameresultslog$outcome)
winOutcome<-gameresultslogOutcomes$VICTORY
agg <- aggregate(pos ~ type, winOutcome, mean)
print(agg)
plot <- ggplot(agg, aes(x = agg$type, y=agg$pos)) + geom_bar(color="black", stat="identity") 
plot <- plot + labs(x = "Player Type", y = "Mean Endgame Position") + theme(axis.text = element_text(size = 15), axis.title = element_text(size = 15, face = "bold")) + scale_x_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
suppressMessages(ggsave(sprintf("plots/Positions.png")))

# plot <- ggplot(gameresultslog, aes(x = gameresultslog$type, fill = gameresultslog$outcome)) + geom_bar(color="black", position="fill") 
# plot <- plot + theme(axis.text=element_text(size=15), axis.title=element_text(size=15,face="bold")) + scale_x_discrete(labels = as.character(c("Constructive\nCollectivist","Constructive\nIndividualist","Disruptive\nCollectivist","Disruptive\nIndividualistic","Random")))  
# suppressMessages(ggsave(sprintf("plots/OutcomeFrequencies.png")))
