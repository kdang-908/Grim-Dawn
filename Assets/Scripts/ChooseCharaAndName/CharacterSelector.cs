using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CharacterSelector : MonoBehaviour
{
    public Button maleButton;
    public Button femaleButton;
    public Button playButton;
    public TMP_InputField nameInput;
    public Image maleBackground;   // Ô xanh phía sau model nam
    public Image femaleBackground; // Ô xanh phía sau model nữ
    public Color normalColor = new Color(0.49f, 0.78f, 0.99f); // màu xanh nhạt
    public Color selectedColor = Color.yellow;                 // màu khi chọn


    // 0 = Male, 1 = Female, -1 = chưa chọn
    int selectedIndex = -1;

    void Start()
    {
        maleButton.onClick.AddListener(() => SelectCharacter(0));
        femaleButton.onClick.AddListener(() => SelectCharacter(1));
        playButton.onClick.AddListener(OnPlayClicked);
        GameManager.Instance.LoadMapAdditive();

    }

    void SelectCharacter(int index)
    {
        selectedIndex = index;

        if (maleBackground != null)
            maleBackground.color = (index == 0) ? selectedColor : normalColor;

        if (femaleBackground != null)
            femaleBackground.color = (index == 1) ? selectedColor : normalColor;
    }

    void OnPlayClicked()
    {
        Debug.Log("Play clicked");

        if (selectedIndex == -1)
        {
            Debug.LogWarning("Bạn chưa chọn nhân vật (Male/Female).");
            return;
        }

        string playerName = nameInput.text.Trim();
        if (string.IsNullOrEmpty(playerName) || playerName.Length < 2)
        {
            Debug.LogWarning("Tên phải có ít nhất 2 ký tự.");
            return;
        }

        GameManager.Instance.SetPlayerData(selectedIndex, playerName);
        GameManager.Instance.GoToGameplay();
    }
}
