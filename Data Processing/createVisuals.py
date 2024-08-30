import json
import os
from enum import Enum
from matplotlib import pyplot

class Team(Enum):
    Blue = 0
    Red = 1
    Neutral = 2

class Result(Enum):
    Won = 1
    Lost = 2
    Tied = 3

class gameCounter:
    def __init__(self):
        self.won:int = 0
        self.lost:int = 0
        self.tied:int = 0

    def CountResult(self, result:Result):
        if (result == Result.Won):
            self.won = self.won + 1
        elif (result == Result.Lost):
            self.lost = self.lost + 1
        else:
            self.tied = self.tied + 1

    def GetValue(self, result:Result):
        retValue = 0

        if (result == Result.Won):
            retValue = self.won
        elif (result == Result.Lost):
            retValue = self.lost
        else:
            retValue = self.tied

        return retValue

# Loads a team's raw data from the provided directory.
def loadAllFiles(team:Team, directory:str, binCount:int):
    allGameData:dict = {}

    files = os.listdir(directory)
    files.sort()
    teamFiles = [f for f in files if f.startswith(str(team.value))]

    while len(teamFiles) >= binCount:
        workList = teamFiles[:binCount]
        teamFiles = teamFiles[binCount:]

        gameSet = gameCounter()

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

# https://www.askpython.com/python-modules/matplotlib/plot-multiple-datasets-scatterplot
def chartGameData(gameData:dict):
    binGroup = range(len(gameData))
    wins = [v.GetValue(Result.Won) for v in list(gameData.values())]
    losses = [v.GetValue(Result.Lost) for v in list(gameData.values())]
    ties = [v.GetValue(Result.Tied) for v in list(gameData.values())]

    # https://www.askpython.com/python-modules/matplotlib/plot-multiple-datasets-scatterplot
    fix, ax = pyplot.subplots()
    ax.scatter(binGroup, wins, label='Wins', marker='*', color='g')
    ax.scatter(binGroup, losses, label='Losses', marker='s', color='r')
    ax.scatter(binGroup, ties, label='Ties', marker='v', color='b')
    ax.legend()
    ax.set_xlabel('X-axis')
    ax.set_ylabel('Y-axis')
    ax.set_title('Multiple Datasets Scatter Plot')
    pyplot.show()

if (__name__ == "__main__"):
    team = Team.Red
    archivedDataDirectory:str = "D:\Code\GVSU-Enhancing-Game-AI-With-ML\Data Processing\Raw Data\Archive"
    binCount:int = 10

    gameData = loadAllFiles(team, archivedDataDirectory, binCount)
    chartGameData(gameData)
