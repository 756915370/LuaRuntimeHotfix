
local PlayerMove = BaseClass("PlayerMove")

local function Start(self)
    local gameObject = CS.UnityEngine.GameObject.Find("Cube")
    self.gameObject = gameObject
    self.transform = gameObject.transform
    self.rigidbody = gameObject:GetComponent(typeof(CS.UnityEngine.Rigidbody))
end

local function Update(self,deltaTime)
    local verticalInput = CS.UnityEngine.Input.GetAxis("Vertical")
    local movement = self.transform.forward * verticalInput * -10 * deltaTime
    self.rigidbody:MovePosition(self.rigidbody.position + movement)
end

local function OnReload(self)
    print('call onReload from: ',self.__cname)
    if self.__ctype == ClassType.class then
        print("this is a class not a instance")
        for k,v in pairs(self.instances) do
            print("call instance reload: ",k)
            if v.OnReload ~= nil then
                v:OnReload()
            end
        end
    else
        if self.__ctype == ClassType.instance then
            print("this is a instance")
            GameMain.playerFunc = self.TestFunc
        end
    end
end

local count = 1
PlayerMove.Start = Start
PlayerMove.Update = Update
PlayerMove.TestFunc = function()
    --count = count + 1
    --print("count:",count)
    --print("热更")
    print("热更2")
end
PlayerMove.OnReload = OnReload
return PlayerMove