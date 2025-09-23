using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class ModeSelectButton : MonoBehaviour
{
    [SerializeField] private Mode modeToActivate;

    private Button button;
    private Image image;

    void Start()
    {
        this.button = GetComponent<Button>();
        this.image = GetComponent<Image>();

        this.button.onClick.AddListener(this.OnButtonClick);
        GameMgr.OnModeChanged += env =>
        {
            if (this.modeToActivate != env)
                this.image.color = Color.white;
        };
    }

    private void OnButtonClick()
    {
        this.image.color = Color.green;
        GameMgr.SetMode(this.modeToActivate);
    }
}