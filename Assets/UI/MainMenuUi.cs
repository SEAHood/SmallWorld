using UnityEngine;
using UnityEngine.UI;

public class MainMenuUi : MonoBehaviour
{
    public Button HostButton;
    public Button JoinButton;
    public Button QuitButton;

    public NetworkManager NetworkManager;

    void Start()
    {
        HostButton.onClick.AddListener(HostClicked);
        JoinButton.onClick.AddListener(JoinClicked);
        QuitButton.onClick.AddListener(QuitClicked);
    }

    void HostClicked()
    {
        NetworkManager.HostGame();
    }

    void JoinClicked()
    {
        NetworkManager.JoinGame();
    }

    void QuitClicked()
    {
        Application.Quit();
    }
}
