rm -r plots/*

Rscript scripts/plotsGenerator.r
cd plots
mkdir AllvsAll
mv *.png AllvsAll
cd ..

Rscript scripts/plotsGeneratorDefault.r FALSE
cd plots
mkdir MCTS 
mv *.png MCTS
cd ..

Rscript scripts/plotsGeneratorDefault.r TRUE
cd plots
mkdir Affect 
mv *.png Affect


