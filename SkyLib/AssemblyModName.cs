using System;

[AttributeUsage(AttributeTargets.Assembly)]
public class AssemblyModName : Attribute
{
    public string Value { get; set; }

    public AssemblyModName(): this("") { }

    public AssemblyModName(string value)
    {
        Value = value;
    }
}