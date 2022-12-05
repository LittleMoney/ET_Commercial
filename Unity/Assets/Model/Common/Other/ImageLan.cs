using UnityEngine.UI;
using UnityEngine.U2D;
using ETModel;

public class ImageLan : Image
{
    public string toyDirName;
    public string atlasName;
    public string spriteName;
    public bool isSetNativeSize;
    protected override void Awake()
    {
        OnAwake().Coroutine();
    }

    /// <summary>
    /// 精灵的实际名称为 {spriteName}_{simple language}
    /// </summary>
    /// <returns></returns>
    private async ETVoid OnAwake()
    {
        if (string.IsNullOrEmpty(atlasName) && string.IsNullOrEmpty(spriteName) && string.IsNullOrEmpty(toyDirName))
        {
            UnityEngine.Object atlas = await ResourcesComponent.Instance.LoadAssetAsync($"{toyDirName}/Atlas/{atlasName}", typeof(SpriteAtlas));
            sprite = (atlas as SpriteAtlas).GetSprite(spriteName + "_"+LanguageComponent.Instance.GetCurrentLan()); //临时修改 + CSharpUtil.GetLanType());
            if (isSetNativeSize)
            {
                SetNativeSize();
            }
        }
    }
}