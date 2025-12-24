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
        playButton.interactable = false;

    }

    void SelectCharacter(int index)
    {
        selectedIndex = index;

        if (maleBackground != null)
            maleBackground.color = (index == 0) ? selectedColor : normalColor;

        if (femaleBackground != null)
            femaleBackground.color = (index == 1) ? selectedColor : normalColor;

        playButton.interactable = true;
    }

    void OnPlayClicked()
    {
        Debug.Log("[CharacterSelector] Play clicked");

        // 1) Check chọn nhân vật
        if (selectedIndex < 0)
        {
            Debug.LogWarning("[CharacterSelector] Bạn chưa chọn nhân vật.");
            return;
        }

        // 2) Check nhập tên
        if (nameInput == null)
        {
            Debug.LogError("[CharacterSelector] nameInput chưa được kéo vào Inspector!");
            return;
        }

        string playerName = nameInput.text.Trim();
        if (string.IsNullOrEmpty(playerName) || playerName.Length < 2)
        {
            Debug.LogWarning("[CharacterSelector] Tên phải có ít nhất 2 ký tự.");
            return;
        }

        // 3) Lưu PlayerPrefs (phòng khi GameManager chưa kịp có)
        PlayerPrefs.SetInt("SelectedGender", selectedIndex);   // 0/1
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();

        // 4) Check GameManager
        if (GameManager.Instance == null)
        {
            Debug.LogError("[CharacterSelector] GameManager.Instance NULL! Scene phải có 1 GameObject gắn GameManager và Awake() set Instance + DontDestroyOnLoad.");
            return;
        }

        // 5) Set data + Start
        GameManager.Instance.SetPlayerData(selectedIndex, playerName);
        Debug.Log($"[CharacterSelector] Saved: selectedIndex={selectedIndex}, name='{playerName}'");

        GameManager.Instance.StartGameplay();
    }

}
    
