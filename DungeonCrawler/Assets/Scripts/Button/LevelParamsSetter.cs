using System;
using UnityEngine;
using UnityEngine.UI;

public class LevelParamsSetter : MonoBehaviour {
    [SerializeField]
    private InputField seedField;
    [SerializeField]
    private InputField maxRooms;
    [SerializeField]
    private InputField mainPathRooms;
    [SerializeField]
    private Slider maxExits;
    private bool applySettings;

    private void Start() {
        mainPathRooms.onValidateInput += delegate (string input, int charIndex, char addedChar) { return MainRoomValidate(input, charIndex, addedChar); };
        mainPathRooms.onEndEdit.AddListener(delegate { MainEndEdit(); });
        maxRooms.onValidateInput += delegate (string input, int charIndex, char addedChar) { return RoomValidate(input, charIndex, addedChar); };
        maxRooms.onEndEdit.AddListener(delegate { RoomEndEdit(); });
    }

    private void RoomEndEdit() {
        if (maxRooms.text.Length > 0) {
            int count = int.Parse(maxRooms.text);
            int mainCount = mainPathRooms.text.Length>0 ? int.Parse(mainPathRooms.text) : 1;
            if (count < mainCount) {
                maxRooms.text = mainCount.ToString();
            }
        }
    }
    private void MainEndEdit() {
        if (mainPathRooms.text.Length > 0 && maxRooms.text.Length > 0) {
            int count = int.Parse(mainPathRooms.text);
            int mainCount = maxRooms.text.Length > 0 ? int.Parse(maxRooms.text) : count;
            if (count * 3 < mainCount) {
                maxRooms.text = count * 3 + "";
            }
        } else if (mainPathRooms.text.Length > 0 && maxRooms.text.Length == 0) {
            maxRooms.text = mainPathRooms.text;
        }
    }

    private char MainRoomValidate(string input, int charIndex, char addedChar) {
        if (charIndex == 0 && addedChar == '0') {
            addedChar = '\0';
        } else if (charIndex == 0 && addedChar == '-') {
            addedChar = '\0';
        } else if(charIndex>=2){
            int mainPathCount = int.Parse(mainPathRooms.text+addedChar);
            if (mainPathCount > 300) {
                addedChar = '\0';
                mainPathRooms.text = "300";
            }
        } /*else{
            int count = int.Parse(input + addedChar);
            int maxCount = maxRooms.text.Equals("") ? int.Parse(maxRooms.text = "1") : int.Parse(maxRooms.text);
            if (count > maxCount) {
                addedChar = '\0';
                maxRooms.text = count + "";
            }
        }*/
        return addedChar;
    }
    private char RoomValidate(string input, int charIndex, char addedChar) {
        if (charIndex == 0 && addedChar == '0') {
            addedChar = '\0';
        } else if (charIndex == 0 && addedChar == '-') {
            addedChar = '\0';
        } else {
            int count = int.Parse(input + addedChar);
            int mainPathCount = mainPathRooms.text.Equals("") ? int.Parse(mainPathRooms.text = "1") : int.Parse(mainPathRooms.text);

            if (count > mainPathCount * 3) {
                addedChar = '\0';
                maxRooms.text = mainPathCount * 3 + "";
            }
        }
        return addedChar;
    }

    public void ApplySettings() {
        applySettings = true;
        if (seedField.text.Equals("")) {
            var cb = seedField.colors;
            cb.normalColor = new Color(1f, 0.5f, 0.5f, 1);
            seedField.colors = cb;
            applySettings = false;
        } else {
            var cb = seedField.colors;
            cb.normalColor = new Color(0.5f, 1, 0.5f); ;
            seedField.colors = cb;
        }
        if (maxRooms.text.Equals("")) {
            var cb = maxRooms.colors;
            cb.normalColor = new Color(1f, 0.5f, 0.5f, 1);
            maxRooms.colors = cb;
            applySettings = false;
        } else {
            var cb = maxRooms.colors;
            cb.normalColor = new Color(0.5f, 1, 0.5f); ;
            maxRooms.colors = cb;
        }
        if (mainPathRooms.text.Equals("")) {
            var cb = mainPathRooms.colors;
            cb.normalColor = new Color(1f, 0.5f, 0.5f, 1);
            mainPathRooms.colors = cb;
            applySettings = false;
        } else {
            var cb = mainPathRooms.colors;
            cb.normalColor = new Color(0.5f, 1, 0.5f);
            mainPathRooms.colors = cb;
        }
        if (applySettings) {
            LevelProperties.Changes = true;
            LevelProperties.SetSeed(seedField.text);
            LevelProperties.SetMaxRoomCount(maxRooms.text);
            LevelProperties.SetMainPathRooms(mainPathRooms.text);
            LevelProperties.SetMaxExits((int)maxExits.value);
        }
    }
}
