using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Events
{
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
    public class EventAttribute : Attribute
    {
    }
}
