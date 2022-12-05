using UnityEngine;
using UnityEngine.UI;
 
namespace UnityEditor.UI
{
    [CustomEditor(typeof(ImageLan), true)]
    [CanEditMultipleObjects]
    public class ImageLanEditor : UnityEditor.UI.ImageEditor
    {
        [MenuItem("CONTEXT/Image/ChangeToImageLan")]
        static void ChangeToImageLan(MenuCommand command)
        {
            Image img = (Image)command.context;
            GameObject body = img.gameObject;

            Sprite spr = img.sprite;
            Color color = img.color;
            Material material = img.material;
            bool raycastTarget = img.raycastTarget;
            bool maskable = img.maskable;
            Image.Type imgType = img.type;

            GameObject.DestroyImmediate(img);
            ImageLan imgLang = body.AddComponent<ImageLan>();

            imgLang.atlasName = "";
            imgLang.spriteName = spr.name;
            imgLang.sprite = spr;
            imgLang.color = color;
            imgLang.material = material;
            imgLang.raycastTarget = raycastTarget;
            imgLang.maskable = maskable;
            imgLang.type = imgType;

            EditorUtility.SetDirty(body);
        }

        ImageLan img;

        public override void OnInspectorGUI()
        {
            img = (ImageLan)target;
            img.toyDirName = EditorGUILayout.TextField("toyDirName:", img.toyDirName);
            img.atlasName = EditorGUILayout.TextField("AtlasName:", img.atlasName);
            img.spriteName = EditorGUILayout.TextField("SpriteName:", img.spriteName);
            img.isSetNativeSize = EditorGUILayout.Toggle("isSetNativeSize:", img.isSetNativeSize);
            base.OnInspectorGUI();
            if (GUI.changed){
                EditorUtility.SetDirty(target);
            }
        }
    }
}