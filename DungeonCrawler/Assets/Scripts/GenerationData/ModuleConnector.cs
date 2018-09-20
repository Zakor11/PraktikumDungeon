using UnityEngine;
using UnityEngine.AI;

public class ModuleConnector : MonoBehaviour {
    [SerializeField]
    [EnumFlagsAttribute]
    public TileTagsEnum tags;
    public bool IsDefault;
    public bool ExitMatched;
    public ModuleConnector otherSide;

    void OnDrawGizmos() {
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

    public void SetMatched(bool matched) {
        ExitMatched = matched;
    }

    public bool IsMatched() {
        return ExitMatched;
    }

    public void setOtherSide(ModuleConnector other) {
        otherSide = other;
        ExitMatched = other != null;
    }
    public ModuleConnector getOtherSide() {
        return otherSide;
    }

    public bool hasTag(TileTagsEnum tag) {
        return (tags & tag) != TileTagsEnum.None;
    }
}
