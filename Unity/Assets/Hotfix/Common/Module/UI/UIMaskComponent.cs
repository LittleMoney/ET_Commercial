using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using ETModel;

namespace ETHotfix
{
    public class UIMaskComponent : Component, IUICycle
    {
        public Image image;
        public Canvas canvas;
        public GameObject rootGameObject;

        public void OnStart() { this.Start(); }
        public void OnFocus() { }
        public void OnHide() { }
        public void OnPause() { }
        public void OnShow() {}

    }

    [ObjectSystem]
	public class UIMaskComponentAwakeSystem : AwakeSystem<UIMaskComponent>
	{
		public override void Awake(UIMaskComponent self)
		{
            UI _ui = self.Parent as UI;
            GameObject _maskGo= new GameObject("UIMask");
            self.rootGameObject = _maskGo;
            self.rootGameObject.layer = LayerMask.NameToLayer(LayerNames.UI);
            _maskGo.transform.SetParent(_ui.g_rectTransform);

            self.image = _maskGo.AddComponent<Image>();
            self.image.color = Color.clear;

            self.canvas = _maskGo.AddComponent<Canvas>();
            self.canvas.overrideSorting = true;
            _maskGo.AddComponent<GraphicRaycaster>();


            RectTransform _rectTransform = self.rootGameObject.GetComponent<RectTransform>();
            _rectTransform.localPosition = Vector3.zero;
            _rectTransform.localScale = Vector3.one;
            _rectTransform.anchorMin = Vector2.zero;
            _rectTransform.anchorMax = Vector2.one;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
        }
	}

    [ObjectSystem]
    public class UIMaskComponentDestroySystem : DestroySystem<UIMaskComponent>
    {
        public override void Destroy(UIMaskComponent self)
        {
            self.rootGameObject = null;
            self.canvas = null;
        }
    }


    public static class UIMaskComponentSystem 
    {
        public static void Start(this UIMaskComponent self)
        {
            self.image.DOKill();
            self.image.DOColor(new Color(0, 0, 0, 0.4f), 0.8f);
            self.canvas.sortingOrder = (self.Parent as UI).g_sortOrder - 1;
        }
    }


}
