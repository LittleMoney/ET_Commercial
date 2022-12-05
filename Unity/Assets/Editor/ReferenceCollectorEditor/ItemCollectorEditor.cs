using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
//Object并非C#基础中的Object，而是 UnityEngine.Object
using Object = UnityEngine.Object;

//自定义ReferenceCollector类在界面中的显示与功能
[CustomEditor(typeof(ItemCollector))]
//没有该属性的编辑器在选中多个物体时会提示“Multi-object editing not supported”
[CanEditMultipleObjects]
public class ItemCollectorEditor : Editor
{
	//输入在textfield中的字符串
	private string searchKey
	{
		get
		{
			return _searchKey;
		}
		set
		{
			if (_searchKey != value)
			{
				_searchKey = value;
				heroPrefab = itemCollector.Get<Object>(searchKey);
			}
		}
	}

	private ItemCollector itemCollector;

	private Object heroPrefab;

	private string _searchKey = "";

	private void DelNullReference()
	{
		var dataProperty = serializedObject.FindProperty("data");
		for (int i = dataProperty.arraySize - 1; i >= 0; i--)
		{
			var gameObjectProperty = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("obj");
			if (gameObjectProperty.objectReferenceValue == null)
			{
				dataProperty.DeleteArrayElementAtIndex(i);
			}
		}
	}

	private void OnEnable()
	{
		//将被选中的gameobject所挂载的ItemCollector赋值给编辑器类中的ItemCollector，方便操作
		itemCollector = (ItemCollector)target;
	}


	void OnTypeValueSelected(object userData)
	{
		Tuple<int, string> _tuple = userData as Tuple<int, string>;
		SerializedProperty _property=serializedObject.FindProperty("data").GetArrayElementAtIndex(_tuple.Item1).FindPropertyRelative("type");
		//_tuple.Item1.stringValue = _tuple.Item2;
		for (int i = 0; i < _property.enumDisplayNames.Length; i++)
        {
            if (_property.enumDisplayNames[i].Equals(_tuple.Item2))
            {
				_property.enumValueIndex = i;
				break;
            }
        }
		serializedObject.ApplyModifiedProperties();
		//serializedObject.UpdateIfRequiredOrScript();
	}

	public override void OnInspectorGUI()
	{
		//使ItemCollector支持撤销操作，还有Redo，不过没有在这里使用
		Undo.RecordObject(itemCollector, "Changed Settings");
		var dataProperty = serializedObject.FindProperty("data");

		//开始水平布局，如果是比较新版本学习U3D的，可能不知道这东西，这个是老GUI系统的知识，除了用在编辑器里，还可以用在生成的游戏中
		GUILayout.BeginHorizontal();
		//下面几个if都是点击按钮就会返回true调用里面的东西
		if (GUILayout.Button("添加引用"))
		{
			//添加新的元素，具体的函数注释
			// Guid.NewGuid().GetHashCode().ToString() 就是新建后默认的key
			AddReference(dataProperty, Guid.NewGuid().GetHashCode().ToString(), null);
		}
		if (GUILayout.Button("全部删除"))
		{
			dataProperty.ClearArray();
		}
		if (GUILayout.Button("删除空引用"))
		{
			DelNullReference();
		}
		if (GUILayout.Button("排序"))
		{
			itemCollector.Sort();
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		//可以在编辑器中对searchKey进行赋值，只要输入对应的Key值，就可以点后面的删除按钮删除相对应的元素
		searchKey = EditorGUILayout.TextField(searchKey);
		//添加的可以用于选中Object的框，这里的object也是(UnityEngine.Object
		//第三个参数为是否只能引用scene中的Object
		EditorGUILayout.ObjectField(heroPrefab, typeof(Object), false);
		if (GUILayout.Button("删除"))
		{
			itemCollector.Remove(searchKey);
			heroPrefab = null;
		}
		GUILayout.EndHorizontal();
		EditorGUILayout.Space();

		var delList = new List<int>();
		SerializedProperty property;
		//遍历ReferenceCollector中data list的所有元素，显示在编辑器中
		for (int i = itemCollector.data.Count - 1; i >= 0; i--)
		{
			GUILayout.BeginHorizontal();

			property = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("type");
			if (EditorGUILayout.DropdownButton(new GUIContent(property.enumDisplayNames[property.enumValueIndex]), FocusType.Keyboard, GUILayout.Width(80)))
			{
				GenericMenu _menu = new GenericMenu();
				foreach (var item in property.enumDisplayNames)
				{
					if (string.IsNullOrEmpty(item))
					{
						continue;
					}

					//添加菜单
					_menu.AddItem(new GUIContent(item), property.enumDisplayNames[property.enumValueIndex].Equals(item), OnTypeValueSelected,new Tuple<int,string>(i, item));
				}
				_menu.ShowAsContext();//显示菜单
			}

			property = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("key");
			property.stringValue=EditorGUILayout.TextField(property.stringValue, GUILayout.Width(100));

			property = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("obj");
			property.objectReferenceValue=EditorGUILayout.ObjectField(property.objectReferenceValue, typeof(Object), true);

			if (GUILayout.Button("X"))
			{
				//将元素添加进删除list
				delList.Add(i);
			}
			GUILayout.EndHorizontal();
		}
		var eventType = Event.current.type;
		//在Inspector 窗口上创建区域，向区域拖拽资源对象，获取到拖拽到区域的对象
		if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
		{
			// Show a copy icon on the drag
			DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

			if (eventType == EventType.DragPerform)
			{
				DragAndDrop.AcceptDrag();
				foreach (var o in DragAndDrop.objectReferences)
				{
					AddReference(dataProperty, o.name, o);
				}
			}

			Event.current.Use();
		}

		//遍历删除list，将其删除掉
		foreach (var i in delList)
		{
			dataProperty.DeleteArrayElementAtIndex(i);
		}
		serializedObject.ApplyModifiedProperties();
		serializedObject.UpdateIfRequiredOrScript();
	}

	//添加元素，具体知识点在ReferenceCollector中说了
	private void AddReference(SerializedProperty dataProperty, string key, Object obj)
	{
		int index = dataProperty.arraySize;
		dataProperty.InsertArrayElementAtIndex(index);
		var element = dataProperty.GetArrayElementAtIndex(index);
		element.FindPropertyRelative("type").enumValueIndex = 0;
		element.FindPropertyRelative("key").stringValue = key;
		element.FindPropertyRelative("obj").objectReferenceValue = obj;
	}
}
