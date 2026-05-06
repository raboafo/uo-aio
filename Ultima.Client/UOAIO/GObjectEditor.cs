using System;
using System.Collections;
using System.Reflection;
using System.Windows.Forms;
using UOAIO.Targeting;

namespace UOAIO;

public class GObjectEditor : GWindowsForm
{
	private object m_Object;

	private static GObjectEditor m_Instance;

	private GEditorPanel m_Panel;

	public object Object => this.m_Object;

	public static bool IsOpen => GObjectEditor.m_Instance != null;

	public static void Open(object obj)
	{
		if (GObjectEditor.m_Instance == null)
		{
			GObjectEditor.m_Instance = new GObjectEditor(obj);
			Gumps.Desktop.Children.Add(GObjectEditor.m_Instance);
			Gumps.Focus = GObjectEditor.m_Instance;
		}
	}

	protected internal override void OnDispose()
	{
		if (TargetManager.Client is SetItemPropertyTarget)
		{
			TargetManager.Client = null;
		}
		GObjectEditor.m_Instance = null;
	}

	protected internal override void OnDragStart()
	{
		base.OnDragStart();
		this.m_Panel.Reset();
	}

	protected internal override void OnMouseUp(int X, int Y, MouseButtons mb)
	{
		this.m_Panel.Reset();
	}

	protected internal override void Draw(int X, int Y)
	{
		base.Draw(X, Y);
		if (Gumps.Focus is GSliderBase || Gumps.Focus == null || !Gumps.Focus.IsChildOf(this))
		{
			this.m_Panel.Reset();
		}
	}

	private void BuildCategories(object obj, Hashtable categories)
	{
		Type type = obj.GetType();
		PropertyInfo[] properties = type.GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (!(this.GetAttribute(propertyInfo, typeof(OptionableAttribute)) is OptionableAttribute optionableAttribute) || (optionableAttribute.OnlyAOS && !Engine.ServerFeatures.AOS))
			{
				continue;
			}
			if (!propertyInfo.CanWrite)
			{
				this.BuildCategories(propertyInfo.GetValue(obj, null), categories);
				continue;
			}
			ArrayList arrayList = (ArrayList)categories[optionableAttribute.Category];
			if (arrayList == null)
			{
				arrayList = (ArrayList)(categories[optionableAttribute.Category] = new ArrayList());
			}
			arrayList.Add(new ObjectEditorEntry(propertyInfo, obj, optionableAttribute, this.GetAttribute(propertyInfo, typeof(OptionRangeAttribute)), this.GetAttribute(propertyInfo, typeof(OptionHueAttribute))));
		}
	}

	public GObjectEditor(object obj)
		: base(0, 0, 317, 392)
	{
		Gumps.Focus = this;
		this.m_Object = obj;
		base.m_NonRestrictivePicking = true;
		base.Text = "Option Editor";
		Hashtable hashtable = new Hashtable();
		this.BuildCategories(obj, hashtable);
		ArrayList arrayList = new ArrayList(hashtable);
		arrayList.Sort(new CategorySorter());
		ArrayList arrayList2 = new ArrayList();
		foreach (DictionaryEntry item in arrayList)
		{
			string category = (string)item.Key;
			ArrayList entries = (ArrayList)item.Value;
			GCategoryPanel value = new GCategoryPanel(obj, category, entries);
			arrayList2.Add(value);
		}
		GEditorPanel gEditorPanel = (this.m_Panel = new GEditorPanel(arrayList2, 360));
		gEditorPanel.X += 2;
		gEditorPanel.Y += 3;
		base.Client.m_NonRestrictivePicking = true;
		base.Client.Children.Add(gEditorPanel);
		this.Center();
	}

	private void KeyboardFlipper_OnClick(Gump g)
	{
	}

	public object GetAttribute(MemberInfo mi, Type type)
	{
		object[] customAttributes = mi.GetCustomAttributes(type, inherit: false);
		if (customAttributes == null)
		{
			return null;
		}
		if (customAttributes.Length == 0)
		{
			return null;
		}
		return customAttributes[0];
	}
}
