from matplotlib import pyplot as plt
import json
import os
import numpy as np

def getTeamName(team:Team):
    retName = ""

    if (team == Team.Red):
        retName = "Red"
    elif (team == Team.Blue):
        retName = "Blue"
    else:
        retName = "Neutral"

    return retName 

# Loads a team's raw data from the provided directory.
def loadAllFiles(team:Team, directory:str, binCount:int):
    allGameData:dict = {}

    files = os.listdir(directory)
    files.sort()
    teamFiles = [f for f in files if f.startswith(str(team.value))]

    while len(teamFiles) >= binCount:
        workList = teamFiles[:binCount]
        teamFiles = teamFiles[binCount:]

        gameSet = GameCounter()

        for file in workList:
            gameOutcome = loadFile(team, directory, file)
            gameSet.CountResult(gameOutcome)

        gameSetKey = len(allGameData)
        allGameData[gameSetKey] = gameSet
    
    return allGameData

# Loads the specific file and converts from json to an object
def loadFile(team:Team, directory:str, fileName:str):
    fullPath = f'{directory}\{fileName}'
    file = open(fullPath, "r")
    data = json.load(file)
    
    lastState = data['States'][-1:][0]
    
    outcome:Result = Result.Lost
    if (lastState['RedTeamScore'] == lastState['BlueTeamScore']):
        outcome = Result.Tied
    elif (lastState['RedTeamScore'] > lastState['BlueTeamScore'] and team == Team.Red):
        outcome = Result.Won

    file.close()
    return outcome

def chartGameData(team:Team, binCount:int, gameData:dict):
    binGroup = [bin * binCount for bin in range(len(gameData))]
    wins = [v.GetValue(Result.Won) for v in list(gameData.values())]
    losses = [v.GetValue(Result.Lost) for v in list(gameData.values())]
    ties = [v.GetValue(Result.Tied) for v in list(gameData.values())]

    teamName:str = getTeamName(team)

    # https://www.askpython.com/python-modules/matplotlib/plot-multiple-datasets-scatterplot
    fix, ax = pyplot.subplots()
    ax.scatter(binGroup, wins, label='Wins', marker='o', color='green')
    ax.scatter(binGroup, losses, label='Losses', marker='s', color='red')
    ax.scatter(binGroup, ties, label='Ties', marker='^', color='gray')
    ax.legend()
    ax.set_xlabel('Epochs')
    ax.set_ylabel('Outcomes')
    ax.set_title(f'{teamName} Performance')
    pyplot.show()

if (__name__ == "__main__"):
    team = Team.Red
    archivedDataDirectory:str = "D:\Code\GVSU-Enhancing-Game-AI-With-ML\Data Processing\Raw Data\Archive"
    binCount:int = 1

    gameData = loadAllFiles(team, archivedDataDirectory, binCount)
    chartGameData(team, binCount, gameData)
