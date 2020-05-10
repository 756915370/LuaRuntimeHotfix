
local PlayerMove = BaseClass("PlayerMove")

local function Start(self)
    local gameObject = CS.UnityEngine.GameObject.Find("Cube")
    self.gameObject = gameObject
    self.transform = gameObject.transform
    self.rigidbody = gameObject:GetComponent(typeof(CS.UnityEngine.Rigidbody))
end

local function Update(self,deltaTime)
    local verticalInput = CS.UnityEngine.Input.GetAxis("Vertical")
    local movement = self.transform.forward * verticalInput * 10 * deltaTime
    self.rigidbody:MovePosition(self.rigidbody.position + movement)
end

local count = 1
PlayerMove.Start = Start
PlayerMove.Update = Update
PlayerMove.TestFunc = function()
    count = count + 1
    print("count:",count)
    print("热更")
end
return PlayerMove