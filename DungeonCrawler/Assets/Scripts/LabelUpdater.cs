using UnityEngine;
using UnityEngine.UI;

public class LabelUpdater : MonoBehaviour {
    private readonly string EXIT_LABEL_TEXT = "Max. Exits: ";
    [SerializeField]
    private Text label;

    public void UpdateLabel() {
        int exits = (int)GetComponent<Slider>().value;
        label.text = EXIT_LABEL_TEXT + GetComponent<Slider>().value;
        if (exits == 5)
            label.text += "+";
    }
}
