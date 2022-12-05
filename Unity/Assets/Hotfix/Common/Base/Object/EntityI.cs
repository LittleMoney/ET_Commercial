using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHotfix
{
	public class ComponentI : Component
	{

	}

	public class EntityI : Entity
	{
		private static Component[] nullComponentArray = new Component[0];

		public EntityI() : base()
		{
		}

		protected EntityI(long id) : base(id)
		{
		}

		public override Component AddComponent(Component component)
		{
			Type type = component.GetType();
			if (this.componentDict.ContainsKey(type))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {type.Name}");
			}

			component.Parent = this;

			this.componentDict.Add(type, component);
			AddComponentInterface(component);

			if (component is ISerializeToEntity)
			{
				this.components.Add(component);
			}

			return component;
		}

		public override Component AddComponent(Type type)
		{
			if (this.componentDict.ContainsKey(type))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {type.Name}");
			}

			Component component = ComponentFactory.CreateWithParent(type, this, this.IsFromPool);

			this.componentDict.Add(type, component);
			AddComponentInterface(component);

			if (component is ISerializeToEntity)
			{
				this.components.Add(component);
			}

			return component;
		}

		public override K AddComponent<K>()
		{
			Type type = typeof(K);
			if (this.componentDict.ContainsKey(type))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof(K).Name}");
			}

			K component = ComponentFactory.CreateWithParent<K>(this, this.IsFromPool);

			this.componentDict.Add(type, component);
			AddComponentInterface(component);

			if (component is ISerializeToEntity)
			{
				this.components.Add(component);
			}

			return component;
		}

		public override K AddComponent<K, P1>(P1 p1)
		{
			Type type = typeof(K);
			if (this.componentDict.ContainsKey(type))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof(K).Name}");
			}

			K component = ComponentFactory.CreateWithParent<K, P1>(this, p1, this.IsFromPool);

			this.componentDict.Add(type, component);
			AddComponentInterface(component);

			if (component is ISerializeToEntity)
			{
				this.components.Add(component);
			}

			return component;
		}

		public override K AddComponent<K, P1, P2>(P1 p1, P2 p2)
		{
			Type type = typeof(K);
			if (this.componentDict.ContainsKey(type))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof(K).Name}");
			}

			K component = ComponentFactory.CreateWithParent<K, P1, P2>(this, p1, p2, this.IsFromPool);

			this.componentDict.Add(type, component);
			AddComponentInterface(component);

			if (component is ISerializeToEntity)
			{
				this.components.Add(component);
			}

			return component;
		}

		public override K AddComponent<K, P1, P2, P3>(P1 p1, P2 p2, P3 p3)
		{
			Type type = typeof(K);
			if (this.componentDict.ContainsKey(type))
			{
				throw new Exception($"AddComponent, component already exist, id: {this.Id}, component: {typeof(K).Name}");
			}

			K component = ComponentFactory.CreateWithParent<K, P1, P2, P3>(this, p1, p2, p3, this.IsFromPool);

			this.componentDict.Add(type, component);
			AddComponentInterface(component);

			if (component is ISerializeToEntity)
			{
				this.components.Add(component);
			}

			return component;
		}

		public override void RemoveComponent<K>()
		{
			if (this.IsDisposed)
			{
				return;
			}
			Type type = typeof(K);
			Component component;
			if (!this.componentDict.TryGetValue(type, out component))
			{
				return;
			}

			if (component is ComponentI)
			{
				//清理接口
				Component _component2;
				ComponentLinkedList _componentList = null;
				foreach (Type _iType in type.GetInterfaces())
				{
					if (_iType == typeof(ISupportInitialize) || _iType == typeof(IDisposable)) continue;

					if (this.componentDict.TryGetValue(_iType, out _component2))
					{
						_componentList = _component2 as ComponentLinkedList;
						LinkedListNode<Component> _node = _componentList.linkedList.First;
						while (_node != null)
						{
							if (_node.Value == component)
							{
								_componentList.linkedList.Remove(_node);
								_node = _node.Next;
							}
						}
					}
				}
			}

			this.componentDict.Remove(type);
			this.components.Remove(component);

			component.Dispose();
		}

		public override void RemoveComponent(Type type)
		{
			if (this.IsDisposed)
			{
				return;
			}

			Component component;

			if (!this.componentDict.TryGetValue(type, out component))
			{
				return;
			}

			if (component is ComponentI)
			{
				//清理接口
				Component _component2;
				ComponentLinkedList _componentList = null;
				foreach (Type _iType in type.GetInterfaces())
				{
					if (_iType == typeof(ISupportInitialize) || _iType == typeof(IDisposable)) continue;

					if (this.componentDict.TryGetValue(_iType, out _component2))
					{
						_componentList = _component2 as ComponentLinkedList;
						LinkedListNode<Component> _node = _componentList.linkedList.First;
						while (_node != null)
						{
							if (_node.Value == component)
							{
								_componentList.linkedList.Remove(_node);
								_node = _node.Next;
							}
						}
					}
				}
			}

			this.componentDict.Remove(type);
			this.components.Remove(component);

			component.Dispose();
		}

		public override K GetComponent<K>()
		{
			Component component;
			if (!this.componentDict.TryGetValue(typeof(K), out component))
			{
				return default(K);
			}

			if (typeof(K).IsInterface)
			{
				return (component as ComponentLinkedList).linkedList.First<Component>() as K;
			}
			else
			{
				return component as K;

			}
		}

		public override Component GetComponent(Type type)
		{
			Component component;
			if (!this.componentDict.TryGetValue(type, out component))
			{
				return null;
			}

			if (type.IsInterface)
			{
				return (component as ComponentLinkedList).linkedList.First<Component>();
			}
			else
			{
				return component;
			}
		}

		public IEnumerable<Component> GetComponents(Type type)
		{
			Component component;
			if (!this.componentDict.TryGetValue(type, out component))
			{
				return nullComponentArray;
			}

			if (type.IsInterface)
			{
				return (component as ComponentLinkedList).linkedList;
			}
			else
			{
				return new Component[] { component };
			}
		}

		public IEnumerable<Component> GetComponents<T>()
		{
			Type type = typeof(T);
			Component component;
			if (!this.componentDict.TryGetValue(type, out component))
			{
				return nullComponentArray;
			}

			if (type.IsInterface)
			{
				return (component as ComponentLinkedList).linkedList;
			}
			else
			{
				return new Component[] { component };
			}
		}

		public override IEnumerable<Component> GetComponents()
		{
			LinkedList<Component> _componentList = new LinkedList<Component>();
			foreach (KeyValuePair<Type, Component> _keyValue in componentDict)
			{
				if (_keyValue.Value is ComponentLinkedList) continue;
				_componentList.AddLast(_keyValue.Value);

			}
			return _componentList;
		}

		protected void AddComponentInterface(Component component)
		{
			if (!(component is ComponentI)) return;

			Type type = component.GetType();

			Component component2;
			foreach (Type _iType in type.GetInterfaces())
			{
				if (_iType == typeof(ISupportInitialize) || _iType == typeof(IDisposable)) continue;

				if (this.componentDict.TryGetValue(_iType, out component2))
				{
					(component2 as ComponentLinkedList).linkedList.AddLast(component);
				}
				else
				{
					ComponentLinkedList _ll = ComponentFactory.CreateWithParent<ComponentLinkedList>(this);
					_ll.linkedList.AddLast(component);
					this.componentDict.Add(_iType, _ll);
				}
			}
		}
	}
}
