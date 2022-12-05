using UnityEngine;
using TMPro;
using TMPro.EditorUtilities;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(TMProLan), true)]
    [CanEditMultipleObjects]
    public class TMProLanEditor : TMP_EditorPanelUI
    {
        [MenuItem("CONTEXT/TextMeshProUGUI/ChangeToTMProLan")]
        static void ChangeToTMProLan(MenuCommand command)
        {
            TextMeshProUGUI tmpPro = (TextMeshProUGUI)command.context;
            GameObject body = tmpPro.gameObject;

            var isRightToLeftText = tmpPro.isRightToLeftText;
            var text = tmpPro.text;
            var textStyle = tmpPro.textStyle;
            var font = tmpPro.font;
            var fontMaterial = tmpPro.fontMaterial;
            var fontStyle = tmpPro.fontStyle;
            var fontSize = tmpPro.fontSize;
            var enableAutoSizing = tmpPro.enableAutoSizing;
            var fontSizeMin = tmpPro.fontSizeMin;
            var fontSizeMax = tmpPro.fontSizeMax;
            var color = tmpPro.color;
            var enableVertexGradient = tmpPro.enableVertexGradient;
            var colorGradientPreset = tmpPro.colorGradientPreset;
            var colorGradient = tmpPro.colorGradient;
            var characterSpacing = tmpPro.characterSpacing;
            var wordSpacing = tmpPro.wordSpacing;
            var lineSpacing = tmpPro.lineSpacing;
            var paragraphSpacing = tmpPro.paragraphSpacing;
            var alignment = tmpPro.alignment;
            var enableWordWrapping = tmpPro.enableWordWrapping;
            var overflowMode = tmpPro.overflowMode;
            var horizontalMapping = tmpPro.horizontalMapping;
            var verticalMapping = tmpPro.verticalMapping;
            var margin = tmpPro.margin;
            var geometrySortingOrder = tmpPro.geometrySortingOrder;
            var isTextObjectScaleStatic = tmpPro.isTextObjectScaleStatic;
            var richText = tmpPro.richText;
            var raycastTarget = tmpPro.raycastTarget;
            var maskable = tmpPro.maskable;
            var parseCtrlCharacters = tmpPro.parseCtrlCharacters;
            var useMaxVisibleDescender = tmpPro.useMaxVisibleDescender;
            var spriteAsset = tmpPro.spriteAsset;
            var styleSheet = tmpPro.styleSheet;
            var enableKerning = tmpPro.enableKerning;
            var extraPadding = tmpPro.extraPadding;

            GameObject.DestroyImmediate(tmpPro);
            TMProLan tmProLan = body.AddComponent<TMProLan>();

            tmProLan.id = "";
            tmProLan.isRightToLeftText = isRightToLeftText;
            tmProLan.text = text;
            tmProLan.textStyle = textStyle;
            tmProLan.font = font;
            tmProLan.fontMaterial = fontMaterial;
            tmProLan.fontStyle = fontStyle;
            tmProLan.fontSize = fontSize;
            tmProLan.enableAutoSizing = enableAutoSizing;
            tmProLan.fontSizeMin = fontSizeMin;
            tmProLan.fontSizeMax = fontSizeMax;
            tmProLan.color = color;
            tmProLan.enableVertexGradient = enableVertexGradient;
            tmProLan.colorGradientPreset = colorGradientPreset;
            tmProLan.colorGradient = colorGradient;
            tmProLan.characterSpacing = characterSpacing;
            tmProLan.wordSpacing = wordSpacing;
            tmProLan.lineSpacing = lineSpacing;
            tmProLan.paragraphSpacing = paragraphSpacing;
            tmProLan.alignment = alignment;
            tmProLan.enableWordWrapping = enableWordWrapping;
            tmProLan.overflowMode = overflowMode;
            tmProLan.horizontalMapping = horizontalMapping;
            tmProLan.verticalMapping = verticalMapping;
            tmProLan.margin = margin;
            tmProLan.geometrySortingOrder = geometrySortingOrder;
            tmProLan.isTextObjectScaleStatic = isTextObjectScaleStatic;
            tmProLan.richText = richText;
            tmProLan.raycastTarget = raycastTarget;
            tmProLan.maskable = maskable;
            tmProLan.parseCtrlCharacters = parseCtrlCharacters;
            tmProLan.useMaxVisibleDescender = useMaxVisibleDescender;
            tmProLan.spriteAsset = spriteAsset;
            tmProLan.styleSheet = styleSheet;
            tmProLan.enableKerning = enableKerning;
            tmProLan.extraPadding = extraPadding;

            EditorUtility.SetDirty(body);
        }

        TMProLan tmProLan;

        public override void OnInspectorGUI()
        {
            tmProLan = (TMProLan)target;
            tmProLan.id = EditorGUILayout.TextField("LanguageID:", tmProLan.id);
            tmProLan.isStatic = EditorGUILayout.Toggle("isStatic:", tmProLan.isStatic);
            base.OnInspectorGUI();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}