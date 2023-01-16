using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ETModel;

namespace ETHotfix
{
	public class UnitComponent : Component
	{
		public Dictionary<string, HashSet<Unit>> unitTypeDict;
		public Dictionary<long, Unit> unitDict;
		public GameObject g_rootGameObject;
		public IUnitFactory gs_defaultUIFactory = null;
		public Dictionary<string, IUnitFactory> unitFactorys = new Dictionary<string, IUnitFactory>();
		public bool isAutoLoadFactory;
	}

	[ObjectSystem]
	public class UnitComponentAwakeSystem : AwakeSystem<UnitComponent, GameObject, bool>
	{
		public override void Awake(UnitComponent self, GameObject rootGameObject, bool isAutoLoadFactory)
		{
			self.g_rootGameObject = rootGameObject;
			self.gs_defaultUIFactory = new UnitDefaultFactory();
			self.isAutoLoadFactory = isAutoLoadFactory;

			if (self.isAutoLoadFactory)
			{
				self.Load();
			}
		}
	}


	[ObjectSystem]
	public class UnitComponentLoadSystem : LoadSystem<UnitComponent>
	{
		public override void Load(UnitComponent self)
		{
			if (self.isAutoLoadFactory)
			{
				self.Load();
			}
		}
	}


	/// <summary>
	/// 
	/// </summary>
	public static class UnitComponentSystem
	{
		public static void Load(this UnitComponent self)
		{
			self.unitFactorys.Clear();

			List<Type> types = Game.EventSystem.GetTypes();

			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(UnitFactoryAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}

				UnitFactoryAttribute attribute = attrs[0] as UnitFactoryAttribute;
				if (self.unitFactorys.ContainsKey(attribute.Type))
				{
					Log.Debug($"已经存在同类UI Factory: {attribute.Type}");
					throw new Exception($"已经存在同类UI Factory: {attribute.Type}");
				}

				object o = Activator.CreateInstance(type);
				IUnitFactory factory = o as IUnitFactory;
				if (factory == null)
				{
					Log.Error($"{o.GetType().FullName} 没有继承 IUIFactory");
					continue;
				}
				self.unitFactorys.Add(attribute.Type, factory);
			}
		}

		/// <summary>
		/// 获取unit
		/// </summary>
		/// <param name="self"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public static Unit Get(this UnitComponent self, int id)
		{

			if (self.unitDict.TryGetValue(id, out Unit _holder))
			{
				return _holder;
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// 获取该类型的所有Unit
		/// </summary>
		/// <param name="self"></param>
		/// <param name="unitType"></param>
		/// <returns></returns>
		public static IEnumerable<Unit> Get(this UnitComponent self, string unitType)
		{
			if (self.unitTypeDict.TryGetValue(unitType, out HashSet<Unit> _holder))
			{
				return _holder;
			}
			else
			{
				return null;
			}
		}

		#region 注册工厂

		/// <summary>
		/// 注册工厂
		/// </summary>
		/// <param name="self"></param>
		/// <param name="uiType"></param>
		/// <param name="iUIFactory"></param>
		public static void Register(this UnitComponent self, string unitType, IUnitFactory iUnitFactory)
		{
			if (!self.unitFactorys.ContainsKey(unitType))
			{
				self.unitFactorys.Add(unitType, iUnitFactory);
			}
		}

		/// <summary>
		/// 移除工厂
		/// </summary>
		/// <param name="self"></param>
		/// <param name="uiType"></param>
		public static void UnRegister(this UnitComponent self, string unitType)
		{
			if (self.unitTypeDict.ContainsKey(unitType))
			{
				throw new Exception($"{unitType}类型在使用中，不能卸载其工厂接口");
			}

			if (self.unitFactorys.ContainsKey(unitType))
			{
				self.unitFactorys.Remove(unitType);
			}
		}

		#endregion

		#region UI操作

		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <param name="unitType"></param>
		/// <param name="id"></param>
		/// <param name="data"></param>
		/// <param name="suspendShow"></param>
		/// <returns></returns>
		public static Unit Create(this UnitComponent self, string unitType, long id, object data = null, bool suspendShow = false)
		{
			if (self.unitDict.TryGetValue(id, out Unit _holder))
			{
				if (_holder.g_unitType != unitType) throw new Exception($"打开unit失败,unit {id} 已经被类型 {unitType} 使用");
				return _holder;
			}

			IUnitFactory _unitFactory = null;
			if (!self.unitFactorys.TryGetValue(unitType, out _unitFactory))
			{
				_unitFactory = self.gs_defaultUIFactory;
			}

			if (_unitFactory == null) throw new Exception("打开unit失败");
			Unit _unit = _unitFactory.Create(unitType, id, self.g_rootGameObject, data);

			HashSet<Unit> _unitHashSet;
			if (!self.unitTypeDict.TryGetValue(_unit.g_unitType, out _unitHashSet))
			{
				_unitHashSet = new HashSet<Unit>();
				self.unitTypeDict.Add(_unit.g_unitType, _unitHashSet);
			}
			_unitHashSet.Add(_unit);
			self.unitDict.Add(_unit.Id, _unit);

			if (!_unit.g_isHided)
			{
				_unit.Start();
			}
            else if (!suspendShow)
			{
				_unit.Start();
				_unit.Show();
			}

			return _unit;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <param name="unitType"></param>
		/// <param name="id"></param>
		/// <param name="data"></param>
		/// <param name="suspendShow"></param>
		/// <returns></returns>
		public static async ETTask<Unit> CreateAsync(this UnitComponent self, string unitType, long id, object data = null, bool suspendShow = false)
		{
			if (self.unitDict.TryGetValue(id, out Unit _holder))
			{
				if (_holder.g_unitType != unitType) throw new Exception($"打开unit失败,unit {id} 已经被类型 {unitType} 使用");
				return _holder;
			}

			IUnitFactory _unitFactory = null;
			if (!self.unitFactorys.TryGetValue(unitType, out _unitFactory))
			{
				_unitFactory = self.gs_defaultUIFactory;
			}

			if (_unitFactory == null) throw new Exception("打开unit失败");
			Unit _unit = await _unitFactory.CreateAsync(unitType, id, self.g_rootGameObject, data) as Unit;

			HashSet<Unit> _unitHashSet;
			if (!self.unitTypeDict.TryGetValue(_unit.g_unitType, out _unitHashSet))
			{
				_unitHashSet = new HashSet<Unit>();
				self.unitTypeDict.Add(_unit.g_unitType, _unitHashSet);
			}
			_unitHashSet.Add(_unit);
			self.unitDict.Add(_unit.Id, _unit);
			_unit.Start();
			if (!suspendShow && _unit.g_isHided)
			{
				_unit.Show();
			}

			return _unit;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <param name="unitType"></param>
		public static void HideType(this UnitComponent self, string unitType)
		{
			if (self.unitTypeDict.TryGetValue(unitType, out HashSet<Unit> _holder))
			{
				foreach (Unit unit in _holder)
				{
					if (!unit.g_isHided)
					{
						unit.Hide();
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <param name="unitType"></param>
		public static void ShowType(this UnitComponent self, string unitType)
		{
			if (self.unitTypeDict.TryGetValue(unitType, out HashSet<Unit> _holder))
			{
				foreach (Unit unit in _holder)
				{
					if (unit.g_isHided)
					{
						unit.Show();
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <param name="unit"></param>
		/// <param name="isRemainGameObject"></param>
		public static void Remove(this UnitComponent self, Unit unit)
		{
			if (!self.unitDict.TryGetValue(unit.Id, out Unit _unit)) return;

			self.unitDict.Remove(_unit.Id);
			if (self.unitTypeDict.TryGetValue(_unit.g_unitType, out HashSet<Unit> hashSet))
			{
				hashSet.Remove(_unit);
			}

			//谁创建谁释放
			if (self.unitFactorys.TryGetValue(_unit.g_unitType, out IUnitFactory iFactory))
			{
				iFactory.Remove(_unit);
			}
			else
			{
				self.gs_defaultUIFactory.Remove(_unit);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="self"></param>
		/// <param name="unit"></param>
		/// <param name="isRemainGameObject"></param>
		public static void Remove(this UnitComponent self, long id)
		{
			if (!self.unitDict.TryGetValue(id, out Unit _unit)) return;

			self.unitDict.Remove(_unit.Id);
			if (self.unitTypeDict.TryGetValue(_unit.g_unitType, out HashSet<Unit> hashSet))
			{
				hashSet.Remove(_unit);
			}

			//谁创建谁释放
			if (self.unitFactorys.TryGetValue(_unit.g_unitType, out IUnitFactory iFactory))
			{
				iFactory.Remove(_unit);
			}
			else
			{
				self.gs_defaultUIFactory.Remove(_unit);
			}
		}

		#endregion

	}
}
