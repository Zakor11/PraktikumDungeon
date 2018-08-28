using UnityEngine;

public class Module : MonoBehaviour
{
	public string[] Tags;

	public ModuleConnector[] GetExits()
	{
		return GetComponentsInChildren<ModuleConnector>();
	}
    public PlayerSpawn[] GetSpawns() {
        return GetComponentsInChildren<PlayerSpawn>();
    }
}
