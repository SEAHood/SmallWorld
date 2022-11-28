using UnityEngine;
using UnityEngine.UI;

public class MainMenuUi : MonoBehaviour
{
    public Button HostButton;
    public Button JoinButton;
    public Button QuitButton;

    public GameObject Menu;
    public GameObject Loader;

    public NetworkManager NetworkManager;

    void Start()
    {
        HostButton.onClick.AddListener(HostClicked);
        JoinButton.onClick.AddListener(JoinClicked);
        QuitButton.onClick.AddListener(QuitClicked);
    }

    void HostClicked()
    {
        ShowLoader();
        NetworkManager.HostGame();
    }

    void JoinClicked()
    {
        ShowLoader();
        NetworkManager.JoinGame();
    }

    void QuitClicked()
    {
        Application.Quit();
    }

    private void ShowLoader()
    {
        Menu.SetActive(false);
        Loader.SetActive(true);
    }
}
