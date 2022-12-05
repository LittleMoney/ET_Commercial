using System;
using System.Collections;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace ETModel
{
	[ILAdapter]
	public class IEnumeratorClassInheritanceAdaptor : CrossBindingAdaptor
	{

		public override Type BaseCLRType
		{
			get
			{
				return typeof(IEnumerator);
			}
		}

		public override Type AdaptorType
		{
			get
			{
				return typeof(IEnumeratorAdaptor);
			}
		}

		public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
		{
			return new IEnumeratorAdaptor(appdomain, instance);
		}

		public class IEnumeratorAdaptor : IEnumerator, CrossBindingAdaptorType
		{
			private IMethod iCurrent;
			private IMethod iMoveNext;
			private IMethod iReset;

			private ILTypeInstance instance;
			private ILRuntime.Runtime.Enviorment.AppDomain appDomain;

			
			private readonly object[] param0 = new object[0];

			public IEnumeratorAdaptor()
			{
			}

			public IEnumeratorAdaptor(ILRuntime.Runtime.Enviorment.AppDomain appDomain, ILTypeInstance instance)
			{
				this.appDomain = appDomain;
				this.instance = instance;
			}

			public ILTypeInstance ILInstance
			{
				get
				{
					return instance;
				}
			}

			public override string ToString()
			{
				IMethod m = this.appDomain.ObjectType.GetMethod("ToString", 0);
				m = instance.Type.GetVirtualMethod(m);
				if (m == null || m is ILMethod)
				{
					return instance.ToString();
				}

				return instance.Type.FullName;
			}


			public object Current
			{
				get
				{
					if (iCurrent == null)
					{
						iCurrent = instance.Type.GetMethod("get_Current");
					}
					return this.appDomain.Invoke(iCurrent, instance);
				}
			}


			public bool MoveNext()
            {
				if (iMoveNext == null)
				{
					iMoveNext = instance.Type.GetMethod("MoveNext");
				}
				return (bool)this.appDomain.Invoke(iMoveNext, instance);
			}

            public void Reset()
            {
				if (iReset == null)
				{
					iReset = instance.Type.GetMethod("Reset");
				}
				this.appDomain.Invoke(iReset, instance);
			}
        }
	}
}
