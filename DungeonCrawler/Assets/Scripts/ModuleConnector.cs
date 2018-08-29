using UnityEngine;
using UnityEngine.AI;

public class ModuleConnector : MonoBehaviour
{
	public string[] Tags;
	public bool IsDefault;
    private bool ExitMatched;
    private ModuleConnector otherSide;
    public NavMeshAgent player;

    void OnDrawGizmos()
	{
		var scale = 1.0f;

		Gizmos.color = Color.blue;
		Gizmos.DrawLine(transform.position, transform.position + transform.forward * scale);

		Gizmos.color = Color.red;
		Gizmos.DrawLine(transform.position, transform.position - transform.right * scale);
		Gizmos.DrawLine(transform.position, transform.position + transform.right * scale);

		Gizmos.color = Color.green;
		Gizmos.DrawLine(transform.position, transform.position + Vector3.up * scale);

		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(transform.position, 0.125f);
	}

    public void SetMatched(bool matched)
    {
        ExitMatched = matched;
    }

    public bool IsMatched() {
        return ExitMatched;
    }

    public void setOtherSide(ModuleConnector other) {
        otherSide = other;
    }
    public ModuleConnector getOtherSide() {
        return otherSide;
    }
    public void UpdateArrows() {
        SendMessageUpwards("UpdateModuleArrows");
        otherSide.SendMessageUpwards("UpdateModuleArrows");

        player.destination = otherSide.GetComponentInParent<Renderer>().bounds.center;
    }
}
