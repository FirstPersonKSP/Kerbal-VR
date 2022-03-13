using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KerbalVR.IVAAdaptors
{
    internal abstract class IVAKnob
    {
        public static IVAKnob ConstructKnob(GameObject gameObject)
        {
            return RPMKnob.TryConstruct(gameObject);
        }

        public abstract float MinRotation { get; protected set; }
        public abstract float MaxRotation { get; protected set; }

        public abstract float DesiredValue { get; protected set; }
    }

    internal class RPMKnob : IVAKnob
    {
        #region static members
        static readonly Type x_jsiVariableAnimatorType;
        static readonly FieldInfo x_useNewModeField;
        static readonly FieldInfo x_variableSetsField;

        static readonly Type x_variableAnimationSetType;
        static readonly FieldInfo x_variableField;

        static readonly Type x_variableOrNumberRangeType;
        static readonly MethodInfo x_inverseLerpMethod;


        static RPMKnob()
        {
            x_jsiVariableAnimatorType = AssemblyLoader.GetClassByName(typeof(InternalModule), "JSIVariableAnimator");

            if (x_jsiVariableAnimatorType != null)
            {
                x_useNewModeField = x_jsiVariableAnimatorType.GetField("useNewMode", BindingFlags.Instance | BindingFlags.NonPublic);
                x_variableSetsField = x_jsiVariableAnimatorType.GetField("variableSets", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            x_variableAnimationSetType = x_jsiVariableAnimatorType.Assembly.GetTypes().FirstOrDefault(type => type.FullName == "JSI.VariableAnimationSet");

            if (x_variableAnimationSetType != null)
            {
                x_variableField = x_variableAnimationSetType.GetField("variable", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            x_variableOrNumberRangeType = x_jsiVariableAnimatorType.Assembly.GetTypes().FirstOrDefault(type => type.FullName == "JSI.VariableOrNumber");

            if (x_variableOrNumberRangeType != null)
            {
                x_inverseLerpMethod = x_variableOrNumberRangeType.GetMethod("InverseLerp", BindingFlags.Instance);
            }
        }

        static public RPMKnob TryConstruct(GameObject gameObject)
        {
            var rpmComponent = gameObject.GetComponent(x_jsiVariableAnimatorType);

            if (rpmComponent != null)
            {
                return new RPMKnob(rpmComponent);
            }

            return null;
        }

        #endregion

        Component m_rpmComponent;

        public override float MinRotation { get; protected set; }

        public override float MaxRotation { get; protected set; }
        public override float DesiredValue { get; protected set; }

        public RPMKnob(Component knobComponent)
        {
            m_rpmComponent = knobComponent;

            // hack: setting useNewMode to true on the JSIVariableAnimator will prevent it from updating on its own
            x_useNewModeField.SetValue(m_rpmComponent, true);
        }
    }
}