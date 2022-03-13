using System;
using System.Collections;
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
        static readonly FieldInfo x_vectorStartField;
        static readonly FieldInfo x_vectorEndField;

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
                x_vectorStartField = x_variableAnimationSetType.GetField("vectorStart", BindingFlags.Instance | BindingFlags.NonPublic);
                x_vectorEndField = x_variableAnimationSetType.GetField("vectorEnd", BindingFlags.Instance | BindingFlags.NonPublic);
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

        Component m_jsiVariableAnimator;
        object m_variableAnimationSet;

        Vector3 m_vectorStart;
        Vector3 m_vectorEnd;

        public override float MinRotation { get; protected set; }

        public override float MaxRotation { get; protected set; }
        public override float DesiredValue { get; protected set; }

        public RPMKnob(Component knobComponent)
        {
            m_jsiVariableAnimator = knobComponent;

            // hack: setting useNewMode to true on the JSIVariableAnimator will prevent it from updating on its own
            x_useNewModeField.SetValue(m_jsiVariableAnimator, true);

            var variableSets = x_variableSetsField.GetValue(m_jsiVariableAnimator) as IList;
            if (variableSets != null && variableSets.Count > 0)
            {
                m_variableAnimationSet = variableSets[0];

                // note: VariableAnimationSet uses vectorStart/vectorEnd or rotationStart / rotationEnd depending on whether longPath is true
                // but I think longPath is true for all the props we care about
                m_vectorStart = (Vector3)x_vectorStartField.GetValue(m_variableAnimationSet);
                m_vectorEnd = (Vector3)x_vectorEndField.GetValue(m_variableAnimationSet);

                // NOTE: this assumes the rotation axis is 'up'
                MinRotation = m_vectorStart.y;
                MaxRotation = m_vectorEnd.y;
            }
        }
    }
}