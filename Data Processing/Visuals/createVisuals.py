from GameCounter import GameCounter
from matplotlib import pyplot as plt
from Result import Result
from Team import Team
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
def loadAllFiles(team:Team, directory:str, binCount:int, auditedVersion:int, runningVersion:int):
    allGameData:dict = {}
    files = []

    if (auditedVersion < runningVersion):
        files = os.listdir(f'{directory} - V{auditedVersion}')
    else:
        files = os.listdir(directory)

    files.sort()

    versionFiles = [f for f in files if not isFileOtherGameVersionArchived(directory, auditedVersion, f)]
    teamFiles = [f for f in versionFiles if f.startswith(str(team.value))]

    while len(teamFiles) >= binCount:
        workList = teamFiles[:binCount]
        teamFiles = teamFiles[binCount:]

        gameSet = GameCounter()

        for file in workList:
            gameOutcome = loadFile(team, directory, file, auditedVersion)

            if (gameOutcome != None):
                gameSet.CountResult(gameOutcome)

        gameSetKey = len(allGameData)
        allGameData[gameSetKey] = gameSet
    
    return allGameData

def isFileOtherGameVersionArchived(archivePath:str, currentVersion:int, fileName:str):
    retFileExists:bool = False

    for version in range(1, currentVersion):
        otherArchive = f'{archivePath} - V{version}'
        potentialOtherName = f'{otherArchive}\{fileName}'
        retFileExists = os.path.exists(potentialOtherName)

        if retFileExists:
            break

    return retFileExists

# Loads the specific file and converts from json to an object
def loadFile(team:Team, directory:str, fileName:str, version:int):
    fullPath = f'{directory}\{fileName}'
    file = open(fullPath, "r")
    data = json.load(file)

    outcome:Result = None

    gameSetVersion:int = 1
    if ('Version' in data):
        gameSetVersion = int(data['Version'])

    if gameSetVersion == version:
        lastState = data['States'][-1:][0]
        
        outcome:Result = Result.Lost
        if (lastState['RedTeamScore'] == lastState['BlueTeamScore']):
            outcome = Result.Tied
        elif (lastState['RedTeamScore'] > lastState['BlueTeamScore'] and team == Team.Red):
            outcome = Result.Won

        file.close()
    
    return outcome

def chartGameData(team:Team, binCount:int, degrees:int, gameData:dict, version:int):
    binGroup = [bin * binCount for bin in range(len(gameData))]
    wins = [v.GetValue(Result.Won) for v in list(gameData.values())]
    losses = [v.GetValue(Result.Lost) for v in list(gameData.values())]
    ties = [v.GetValue(Result.Tied) for v in list(gameData.values())]

    winningColor = 'red'
    losingColor = 'blue'
    winningTeamName = getTeamName(Team.Red) + ' Team'
    losingTeamName = getTeamName(Team.Blue) + ' Team'

    if (team == Team.Blue):
        winningTeamName = getTeamName(Team.Blue) + ' Team'
        losingTeamName = getTeamName(Team.Red) + ' Team'
        winningColor = 'blue'
        losingColor = 'red'

    # https://www.askpython.com/python-modules/matplotlib/plot-multiple-datasets-scatterplot
    fix, ax = plt.subplots()
    ax.scatter(binGroup, wins, label=winningTeamName, marker='o', color=winningColor)
    ax.scatter(binGroup, losses, label=losingTeamName, marker='s', color=losingColor)
    ax.scatter(binGroup, ties, label='Ties', marker='^', color='gray')

    plt.plot(binGroup, np.polyval(np.polyfit(binGroup, wins, degrees), binGroup), color=winningColor)
    plt.plot(binGroup, np.polyval(np.polyfit(binGroup, losses, degrees), binGroup), color=losingColor)
    plt.plot(binGroup, np.polyval(np.polyfit(binGroup, ties, degrees), binGroup), color='gray')

    ax.legend()
    ax.set_xlabel('Epochs')
    ax.set_ylabel('Outcomes')
    ax.set_title(f'Number of Wins by Team - V{version}')

    plt.show()

if (__name__ == "__main__"):
    archivedDataDirectory:str = "D:\Code\GVSU-Enhancing-Game-AI-With-ML\Data Processing\Raw Data\Archive"
    runningVersion:int = 5
    team = Team.Red

    auditedVersion:int = 5
    degrees:int = 1
    binCount:int = 1

    gameData = loadAllFiles(team, archivedDataDirectory, binCount, auditedVersion, runningVersion)
    chartGameData(team, binCount, degrees, gameData, auditedVersion)
