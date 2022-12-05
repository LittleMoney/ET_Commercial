using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETModel
{
    public class ComponentLinkedList : Component
    {
        public LinkedList<Component> linkedList=new LinkedList<Component>();

        public override void Dispose()
        {
            if (IsDisposed) return;
            base.Dispose();

            foreach(Component element in linkedList)
            {
                element.Dispose();
            }
            linkedList.Clear();
        }
    }
}
