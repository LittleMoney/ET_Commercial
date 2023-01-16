using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ETModel
{
    public class Unit:EntityI
    {
		/// <summary>
		/// 
		/// </summary>
		public string g_unitType;

		/// <summary>
		/// 
		/// </summary>
		public object g_data;

		/// <summary>
		/// 
		/// </summary>
		public bool g_isHided;

		/// <summary>
		/// 
		/// </summary>
		public bool g_hasStarted;

		/// <summary>
		/// 
		/// </summary>
		public GameObject g_rootGameObject;

	}

	[ObjectSystem]
	public class UnitAwakeSystem : AwakeSystem<Unit, string, GameObject, object>
	{
		public override void Awake(Unit self, string unitType, GameObject gameObject, object data)
		{
			gameObject.layer = LayerMask.NameToLayer(LayerNames.UNIT);
			self.g_unitType = unitType;
			self.g_rootGameObject = gameObject;
			self.g_data = data;
			self.g_isHided = !self.g_rootGameObject.activeSelf;
			self.g_hasStarted = true;
		}
	}

	[ObjectSystem]
	public class UnitDestroySystem : DestroySystem<Unit>
	{

		public override void Destroy(Unit self)
		{
			self.g_rootGameObject=null;
		}
	}

	public static class UnitSystem
    {
		public static void Start(this Unit self)
        {
			if(!self.g_hasStarted)
            {
				foreach (Component component in self.GetComponents<IUnitCycle>())
				{
					(component as IUnitCycle).OnStart();
				}
				if (self.g_hasStarted) self.g_hasStarted = true;
			}
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		public static void Show(this Unit self)
		{
			self.g_rootGameObject.SetActive(true);
			self.g_isHided = false;

			if (self.g_hasStarted)
			{
				foreach (Component component in self.GetComponents<IUnitCycle>())
				{
					(component as IUnitCycle).OnStart();
				}
				self.g_hasStarted = true;
			}

			foreach (Component component in self.GetComponents<IUnitCycle>())
			{
				(component as IUnitCycle).OnShow();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <param name="isExclusive"></param>
		public static void Hide(this Unit self)
		{
			self.g_rootGameObject.SetActive(false);
			self.g_isHided = true;

			foreach (Component subCompoennt in self.GetComponents(typeof(IUnitCycle)))
			{
				(subCompoennt as IUnitCycle).OnHide();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <param name="isExclusive"></param>
		public static void Reset(this Unit self)
		{
			if (!self.g_isHided) self.Hide();

			foreach (Component subCompoennt in self.GetComponents(typeof(IUnitCycle)))
			{
				(subCompoennt as IUnitCycle).OnReset();
			}
			self.g_hasStarted = false;
		}

		/// <summary>
		/// 销毁但保留GameObject
		/// </summary>
		/// <param name="self"></param>
		/// <returns></returns>
		public static void Dispose(this Unit self)
		{
			self.Dispose();
		}
	}
}
