using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
public class ConditionalHideAttribute : PropertyAttribute
{
    public string conditionalSourceField;
    public bool hideInInspector;

    public ConditionalHideAttribute(string conditionalSourceField, bool hideInInspector = true)
    {
        this.conditionalSourceField = conditionalSourceField;
        this.hideInInspector = hideInInspector;
    }
}