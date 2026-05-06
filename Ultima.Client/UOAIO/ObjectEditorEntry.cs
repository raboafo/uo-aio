using System;
using System.Reflection;

namespace UOAIO;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class ObjectEditorEntry : IComparable
{
	private PropertyInfo m_Property;

	private object m_Object;

	private OptionableAttribute m_Optionable;

	private OptionRangeAttribute m_Range;

	private OptionHueAttribute m_Hue;

	public PropertyInfo Property => this.m_Property;

	public object Object => this.m_Object;

	public OptionableAttribute Optionable => this.m_Optionable;

	public OptionRangeAttribute Range => this.m_Range;

	public OptionHueAttribute Hue => this.m_Hue;

	public ObjectEditorEntry(PropertyInfo prop, object obj, object optionable, object range, object hue)
	{
		this.m_Property = prop;
		this.m_Object = obj;
		this.m_Optionable = optionable as OptionableAttribute;
		this.m_Range = range as OptionRangeAttribute;
		this.m_Hue = hue as OptionHueAttribute;
	}

	public int CompareTo(object obj)
	{
		ObjectEditorEntry objectEditorEntry = (ObjectEditorEntry)obj;
		return this.m_Optionable.Name.CompareTo(objectEditorEntry.m_Optionable.Name);
	}
}
