require("BaseClass")

local PlayerMove =  require("PlayerMove")

GameMain = {}
playerMove = {}

local function Start()
    print("GameMain start...")
    playerMove = PlayerMove.New()
    playerMove:Start()
    GameMain.playerFunc = playerMove.TestFunc
end

local function TestFunc()
    print("call from self...")
    GameMain.playerFunc()
    print("call from playermove...")
    playerMove.TestFunc()
end

function Update(deltaTime)
    playerMove:Update(deltaTime)
end

GameMain.Start = Start
GameMain.TestFunc = TestFunc

return GameMain