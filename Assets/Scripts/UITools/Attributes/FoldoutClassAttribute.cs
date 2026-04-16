using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif

namespace UI.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
    public class FoldoutClassAttribute : Attribute
    {
        public readonly string GroupName;
        public readonly string PropertyName = null;

        public FoldoutClassAttribute(string _groupName = "$ToString")
        {
            GroupName = _groupName;
        }

        public FoldoutClassAttribute(string _groupName, string _propertyName)
        {
            _propertyName = _propertyName.Replace("$", "");

            GroupName = _groupName;
            PropertyName = _propertyName;
        }
    }

#if UNITY_EDITOR
    // This AttributeProcessor will be found and used to processor attributes for the MyCustomClass class.
    public class FoldoutClassAttributeProcessor : OdinAttributeProcessor
    {
        private FoldoutClassAttribute attribute;

        public override bool CanProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member)
        {
            var index = -1;
            for (var i = parentProperty.Attributes.Count - 1; i >= 0; i--)
            {
                var propertyAttribute = parentProperty.Attributes[i];
                if (propertyAttribute is FoldoutClassAttribute)
                    index = i;
            }

            attribute = index >= 0 ? (FoldoutClassAttribute)parentProperty.Attributes[index] : null;

            return attribute != null;
        }

        public override void ProcessChildMemberAttributes(InspectorProperty _parentProperty, MemberInfo _member,
            List<Attribute> _attributes)
        {
            if (attribute != null)
            {
                _attributes.Add(attribute.PropertyName != null
                    ? new PropertyFoldoutGroupAttribute(attribute.GroupName) { PropertyName = attribute.PropertyName }
                    : new FoldoutGroupAttribute(attribute.GroupName));
                foreach (Attribute attr in _attributes)
                {
                    if (attr is Sirenix.OdinInspector.PropertyGroupAttribute grp)
                    {
                        if (!grp.GroupID.StartsWith(attribute.GroupName))
                            grp.GroupID = attribute.GroupName + "/" + grp.GroupID;
                    }
                }
            }
        }
    }

#endif
}