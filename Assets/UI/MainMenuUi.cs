using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUi : MonoBehaviour
{
    public Button HostButton;
    public Button JoinButton;
    public Button SettingsButton;
    public Button QuitButton;
    public Button ProfileOkButton;

    public GameObject Menu;
    public GameObject Loader;
    public GameObject ProfileSettings;

    public TMP_InputField PlayerNameField;

    public NetworkManager NetworkManager;
    public GameLogic GameLogic;

    void Start()
    {
        HostButton.onClick.AddListener(HostClicked);
        JoinButton.onClick.AddListener(JoinClicked);
        SettingsButton.onClick.AddListener(SettingsClicked);
        QuitButton.onClick.AddListener(QuitClicked);

        ProfileOkButton.onClick.AddListener(ProfileOkClicked);
        PlayerNameField.text = PlayerPrefs.GetString("name");

        if (string.IsNullOrEmpty(PlayerPrefs.GetString("name")))
            ShowProfileSettings();
        else
            ShowMainMenu();
    }

    public void ResetUi()
    {
        ShowMainMenu();
    }

    void HostClicked()
    {
        ShowLoader();
        GameLogic.StartHost();
        /*EnsureNetworkManager();
        NetworkManager.HostGame();*/
    }

    void JoinClicked()
    {
        ShowLoader();
        GameLogic.StartJoin();
        /*EnsureNetworkManager();
        NetworkManager.JoinGame();*/
    }

    void SettingsClicked()
    {
        ShowProfileSettings();
    }

    void QuitClicked()
    {
        Application.Quit();
    }

    void ProfileOkClicked()
    {
        PlayerPrefs.SetString("name", PlayerNameField.text);
        ShowMainMenu();
    }

    void EnsureNetworkManager()
    {
        if (NetworkManager == null)
        {
            var nw = new GameObject("NetworkManager");
            nw.AddComponent<NetworkManager>();
        }
    }

    private void ShowLoader()
    {
        Menu.SetActive(false);
        Loader.SetActive(true);
        ProfileSettings.SetActive(false);
    }

    private void ShowProfileSettings()
    {
        Menu.SetActive(false);
        Loader.SetActive(false);
        ProfileSettings.SetActive(true);
    }

    private void ShowMainMenu()
    {
        Menu.SetActive(true);
        Loader.SetActive(false);
        ProfileSettings.SetActive(false);
    }
}
