using TMPro;
using ETModel;

public class TMProLan : TextMeshProUGUI
{
    public string id;
    public bool isStatic;

    protected override void Awake()
    {
        base.Awake();
        if (isStatic)
        {
            SetText(LanguageComponent.Instance == null ? "" : LanguageComponent.Instance.GetStatic(id));
        }
        else
        {
            SetText(LanguageComponent.Instance == null ? "" : LanguageComponent.Instance.Get(id));
        }
    }
}