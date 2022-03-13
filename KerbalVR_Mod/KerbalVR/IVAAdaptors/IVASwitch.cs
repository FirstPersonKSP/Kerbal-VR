using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalVR.IVAAdaptors
{
    // represents an abstraction for an interactive toggle switch
    internal class IVASwitch
    {
        public static IVASwitch ConstructSwitch(GameObject gameObject)
        {
            // if we ever make bindings for MAS, they'd go in here
            return RPMSwitch.TryConstruct(gameObject);
        }

        public virtual bool CurrentState { get; }
        public virtual void SetState(bool newState) { }
    }

    internal class RPMSwitch : IVASwitch
    {
        static readonly Type x_jsiActionGroupSwitchType;
        static readonly FieldInfo x_currentStateField;
        static readonly MethodInfo x_clickMethod;

        static RPMSwitch()
        {
            x_jsiActionGroupSwitchType = AssemblyLoader.GetClassByName(typeof(InternalModule), "JSIActionGroupSwitch");

            if (x_jsiActionGroupSwitchType != null)
            {
                x_currentStateField = x_jsiActionGroupSwitchType.GetField("currentState", BindingFlags.Instance | BindingFlags.NonPublic);
                x_clickMethod = x_jsiActionGroupSwitchType.GetMethod("Click", BindingFlags.Instance | BindingFlags.Public);
            }
        }
        
        static public RPMSwitch TryConstruct(GameObject gameObject)
        {
            var rpmComponent = gameObject.GetComponent(x_jsiActionGroupSwitchType);

            if (rpmComponent != null)
            {
                return new RPMSwitch(rpmComponent);
            }

            return null;
        }

        Component m_rpmComponent;

        public RPMSwitch(Component switchComponent)
        {
            m_rpmComponent = switchComponent;
        }

        public override bool CurrentState
        {
            get { return (bool)x_currentStateField.GetValue(m_rpmComponent); }
        }

        public override void SetState(bool newState)
        {
            if (newState != CurrentState)
            {
                x_clickMethod.Invoke(m_rpmComponent, null);
            }
        }
    }
}
