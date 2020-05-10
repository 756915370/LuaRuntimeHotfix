require("BaseClass")

local PlayerMove =  require("PlayerMove")

GameMain = {}
playerMove = {}
local playerFunc = {}

local function Start()
    print("GameMain start...")
    playerMove = PlayerMove.New()
    playerMove:Start()
    playerFunc = playerMove.TestFunc
end

local function TestFunc()
    print("call from self...")
    playerFunc()
    print("call from playermove...")
    playerMove.TestFunc()
end

function Update(deltaTime)
    playerMove:Update(deltaTime)
end

GameMain.Start = Start
GameMain.TestFunc = TestFunc

return GameMain