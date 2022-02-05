using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR
{
    internal static class RPMWrapper
    {
        static Type jsiActionGroupSwitchType;

        static RPMWrapper()
        {
            jsiActionGroupSwitchType = AssemblyLoader.GetClassByName(typeof(InternalModule), "JSIActionGroupSwitch");
        }

        public static InternalModule GetJSIActionGroupSwitch(GameObject gameObject)
        {
            return gameObject.GetComponent(jsiActionGroupSwitchType) as InternalModule;
        }
    }
}
