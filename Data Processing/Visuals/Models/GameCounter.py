import Result

class GameCounter:
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