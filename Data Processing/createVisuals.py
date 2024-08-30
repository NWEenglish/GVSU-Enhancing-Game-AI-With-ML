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
