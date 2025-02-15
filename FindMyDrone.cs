using System.Collections.Generic;
using UnityEngine;

public class FindMyDrone : IModApi
{
    public void InitMod(Mod _modInstance)
    {
        Log.Out(" Loading Patch: " + base.GetType().ToString());
        ModEvents.ChatMessage.RegisterHandler(new global::Func<ClientInfo, EChatType, int, string, string, List<int>, bool>(this.ChatMessage));
    }

    public (bool, Vector3) FindMyDroneRegular(ClientInfo cInfo)
    {
        Log.Out($"[FindMyDrone] Client {cInfo.playerName} requested drone location");
        List<EntityCreationData> ecd_droneList = DroneManager.Instance.GetDronesList();
        for (int i = 0; i < ecd_droneList.Count; i++)
        {
            Log.Out($"[FindMyDrone] Scanning for {cInfo.playerName}'s drone");
            EntityCreationData entityCreationData = ecd_droneList[i];
            IDictionary<PlatformUserIdentifierAbs, PersistentPlayerData> players = GameManager.Instance.GetPersistentPlayerList().Players;
            if (players != null)
            {
                foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> item in players)
                {
                    if (item.Value.EntityId == cInfo.entityId)
                    {
                        Log.Out($"[FindMyDrone] Located {cInfo.playerName}'s player data");
                        EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(item.Value.EntityId) as EntityPlayer;
                        if (entityPlayer != null)
                        {
                            OwnedEntityData[] ownedEntities = entityPlayer.GetOwnedEntities();
                            for (int j = 0; j < ownedEntities.Length; j++)
                            {
                                Log.Out($"[FindMyDrone] Located {cInfo.playerName}'s owned entities");
                                if (entityCreationData.id == ownedEntities[j].Id)
                                {
                                    Log.Out($"[FindMyDrone] Located {cInfo.playerName}'s drone data, checking location");
                                    EntityDrone entityDrone = GameManager.Instance.World.GetEntity(entityCreationData.id) as EntityDrone;
                                    if (entityDrone == null)
                                    {
                                        Log.Out($"[FindMyDrone] Located {cInfo.playerName}'s drone at {entityCreationData.pos.ToCultureInvariantString()}");
                                        //entityCreationData.pos = entityPlayer.getHeadPosition();
                                        return (true, entityCreationData.pos);
                                    }
                                    else
                                    {
                                        Log.Out($"[FindMyDrone] Located {cInfo.playerName}'s drone at {entityDrone.position.ToCultureInvariantString()}");
                                        //entityDrone.position = entityPlayer.getHeadPosition();
                                        return (true, entityDrone.position);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        Log.Out($"[FindMyDrone] Failed to Located {cInfo.playerName}'s drone. Player does not have a drone");
        return (false, new Vector3());
    }

    public (bool, Vector3) FindMyDroneEmergancy(ClientInfo cInfo)
    {
        Log.Out($"[FindMyDrone] Client {cInfo.playerName} requested EMERGANCY drone location");
        List<EntityCreationData> ecd_droneList = DroneManager.Instance.GetDronesList();
        for (int i = 0; i < ecd_droneList.Count; i++)
        {
            Log.Out($"[FindMyDrone] Scanning for {cInfo.playerName}'s drone");
            EntityCreationData entityCreationData = ecd_droneList[i];
            IDictionary<PlatformUserIdentifierAbs, PersistentPlayerData> players = GameManager.Instance.GetPersistentPlayerList().Players;
            if (players != null)
            {
                foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> item in players)
                {
                    if (item.Value.EntityId == cInfo.entityId)
                    {
                        Log.Out($"[FindMyDrone] Located {cInfo.playerName}'s player data");
                        EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(item.Value.EntityId) as EntityPlayer;
                        if (entityPlayer != null)
                        {
                            OwnedEntityData[] ownedEntities = entityPlayer.GetOwnedEntities();
                            for (int j = 0; j < ownedEntities.Length; j++)
                            {
                                Log.Out($"[FindMyDrone] Located {cInfo.playerName}'s owned entities");
                                if (entityCreationData.id == ownedEntities[j].Id)
                                {
                                    Log.Out($"[FindMyDrone] Located {cInfo.playerName}'s drone data, checking location");
                                    EntityDrone entityDrone = GameManager.Instance.World.GetEntity(entityCreationData.id) as EntityDrone;
                                    if (entityDrone == null)
                                    {
                                        Log.Out($"[FindMyDrone] Teleported {cInfo.playerName}'s drone to player location");
                                        entityCreationData.pos = entityPlayer.getHeadPosition();
                                        return (true, entityCreationData.pos);
                                    }
                                    else
                                    {
                                        Log.Out($"[FindMyDrone] Teleported {cInfo.playerName}'s drone to player location");
                                        entityDrone.setOrders(EntityDrone.Orders.Follow);
                                        entityDrone.setState(EntityDrone.State.Follow);
                                        entityDrone.position = entityPlayer.getHeadPosition();
                                        return (true, entityDrone.position);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        Log.Out($"[FindMyDrone] Failed to Located {cInfo.playerName}'s drone. Player does not have a drone");
        return (false, new Vector3());
    }

    public string MapCoords(Vector3 location)
    {
        var x = (int)location.x;
        var z = (int)location.z;
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
        return $"{x} {xdir}, {z} {zdir}";
    }

    public bool ChatMessage(ClientInfo cInfo, EChatType type, int senderId, string msg, string mainName, List<int> recipientEntityIds)
    {
        if (!string.IsNullOrEmpty(msg) && cInfo != null)
        {
            if (msg == "/drone" || msg == "/dudewheresmydrone")
            {
                bool found = false;
                Vector3 location = new Vector3();
                (found, location) = FindMyDroneRegular(cInfo);
                if (found)
                {
                    string reply = MapCoords(location);
                    string message = string.Format(Localization.Get("drone_loc", false), reply);
                    sayToServer(message, senderId);
                    return false;
                }
                // Not found
                string message2 = string.Format(Localization.Get("drone_none", false));
                sayToServer(message2, senderId);
                return false;
            }

            if (msg == "/dronefix")
            {
                bool found = false;
                Vector3 location = new Vector3();
                (found, location) = FindMyDroneEmergancy(cInfo);
                if (found)
                {
                    string message = string.Format(Localization.Get("drone_loc_fix", false));
                    sayToServer(message, senderId);
                    return false;
                }
                // Not found
                string message2 = string.Format(Localization.Get("drone_none", false));
                sayToServer(message2, senderId);
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