using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class EnvironmentSelectButton : MonoBehaviour
{
    [SerializeField] private Environment environmentToActivate;

    private Button button;
    private Image image;

    void Start()
    {
        this.button = GetComponent<Button>();
        this.image = GetComponent<Image>();

        this.button.onClick.AddListener(this.OnButtonClick);
        GameMgr.OnEnvironmentChanged += env =>
        {
            if (this.environmentToActivate != env)
                this.image.color = Color.white;
        };
    }

    private void OnButtonClick()
    {
        this.image.color = Color.green;
        GameMgr.SetEnvironment(this.environmentToActivate);
    }
}