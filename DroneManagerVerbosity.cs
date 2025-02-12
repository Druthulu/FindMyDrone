using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

public class DMV_Init : IModApi
{
    //private string serverChatName = "Server";
    public static List<EntityCreationData> OnlinePlayersDroneList = new List<EntityCreationData>();

    public void InitMod(Mod _modInstance)
    {
        Log.Out(" Loading Patch: " + base.GetType().ToString());
        ModEvents.ChatMessage.RegisterHandler(new global::Func<ClientInfo, EChatType, int, string, string, List<int>, bool>(this.ChatMessage));
        Harmony harmony = new Harmony(base.GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    // check messages to see if someone requested drone location
    public bool ChatMessage(ClientInfo cInfo, EChatType type, int senderId, string msg, string mainName, List<int> recipientEntityIds)
    {
        //LO("Chat message detected");
        if (!string.IsNullOrEmpty(msg) && cInfo != null)
        {
            //LO("Chat message detected");
            if (msg == "/drone")
            {
                //LO($"drone command was used by cinfo.entityId: {cInfo.entityId}, senderId: {senderId} onlineplayerdronelist.Count: {OnlinePlayersDroneList.Count}");
                DroneManager.Instance.Save();
                bool found = false;
                for (int i = 0; i < OnlinePlayersDroneList.Count; i++)
                {
                    //LO("iterating through onlineplayers drone list");
                    EntityCreationData onlineDrone = OnlinePlayersDroneList[i];
                    if (onlineDrone.belongsPlayerId == senderId)
                    {
                        //onlineDrone.pos.ToCultureInvariantString();
                        var x = (int)onlineDrone.pos.x;
                        var z = (int)onlineDrone.pos.z;
                        string xdir = "";
                        string zdir = "";
                        if (x < 0)
                        {
                            // west
                            xdir = "W";
                            x *= -1;
                        }
                        else
                        {
                            // east
                            xdir = "E";
                        }
                        if (z < 0)
                        {
                            // south
                            zdir = "S";
                            z *= -1;
                        }
                        else
                        {
                            // north
                            zdir = "N";
                        }
                        string location = $"{x} {xdir}, {z} {zdir}";
                        string message = string.Format(Localization.Get("drone_loc", false), location);
                        sayToServer(message, senderId);
                        found = true;
                    }
                }
                if (!found)
                {
                    string message = string.Format(Localization.Get("drone_none", false));
                    sayToServer(message, senderId);
                }
                //DMV_Init.OnlinePlayersDroneList.Clear(); // clear list for next use
                return false;
            }
            return true;
        }
        return true;
    }
    private void sayToServer(string msg, int playerId)
    {
        List<int> reciept = new List<int>();
        reciept.Add(playerId);
        GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, msg, reciept, EMessageSender.None);
    }

}


[HarmonyPatch(typeof(DroneManager), "write")]
public static class DroneManagerVerbosity
{
    public static void Postfix(DroneManager __instance)
    {
        // Local chat message data
        DMV_Init.OnlinePlayersDroneList.Clear();

        // Server Console changes
        List <EntityCreationData> list = new List<EntityCreationData>();
        __instance.GetDrones(list);
        //Log.Out($"[Drone Manager Verbosity] list count {list.Count}");
        for (int i = 0; i < list.Count; i++)
        {
            // Local chat message data
            DMV_Init.OnlinePlayersDroneList.Add(list[i]);
            
            // Server Console changes
            EntityCreationData entityCreationData = list[i];
            try
            {
                EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(entityCreationData.belongsPlayerId) as EntityPlayer;

                Log.Out("[Drone Manager Verbosity] Online Players' Drone Locations: {0}, {1}", new object[]
                {
                    entityPlayer.entityName,
                    entityCreationData.pos.ToCultureInvariantString()
                });
            }
            catch
            {
                //Dont log offline drone data
                
                Log.Out("[Drone Manager Verbosity] Offline Players' Drone Locations: drone entity id {0}, position {1}", new object[]
               {
                    entityCreationData.id,
                    entityCreationData.pos.ToCultureInvariantString()
               });
            }
           
        }
    }
}
