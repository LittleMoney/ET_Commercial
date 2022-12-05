using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ETModel;

public class UIBehaviour : ItemCollector
{

    [SerializeField]
    public UILayerType layerType = UILayerType.Normal;
    [SerializeField]
    public UIShowType showType = UIShowType.Pop;
    [SerializeField]
    public UIMaskType maskType = UIMaskType.None;
}