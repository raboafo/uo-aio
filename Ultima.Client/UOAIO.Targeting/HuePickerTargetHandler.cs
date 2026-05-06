namespace UOAIO.Targeting;

internal class HuePickerTargetHandler : ClientTargetHandler
{
	private GHuePicker m_Picker;

	private GBrightnessBar m_Bar;

	public HuePickerTargetHandler(GHuePicker picker, GBrightnessBar bar)
	{
		this.m_Picker = picker;
		this.m_Bar = bar;
	}

	protected override bool OnTarget(Item item)
	{
		this.UpdateHue(item.Hue);
		return true;
	}

	protected override bool OnTarget(StaticTarget staticTarget)
	{
		this.UpdateHue(staticTarget.Hue.HueID());
		return true;
	}

	private void UpdateHue(int desiredHue)
	{
		desiredHue &= 0x3FFF;
		if (desiredHue >= 2 && desiredHue < 1002)
		{
			desiredHue -= 2;
			int brightness = desiredHue % 5;
			desiredHue /= 5;
			int shadeX = desiredHue % 20;
			desiredHue /= 20;
			int shadeY = desiredHue;
			this.m_Picker.Brightness = brightness;
			this.m_Picker.ShadeX = shadeX;
			this.m_Picker.ShadeY = shadeY;
			this.m_Bar.Refresh();
		}
		else if (desiredHue >= 2)
		{
			Engine.AddTextMessage("You cannot figure out the proper dye mixture for that color.");
		}
		else
		{
			Engine.AddTextMessage("Do you think that is colorful?");
		}
	}
}
