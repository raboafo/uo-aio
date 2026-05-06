namespace UOAIO;

public interface IMobileStatus
{
	Gump Gump { get; }

	void OnStrChange(int Str);

	void OnHPCurChange(int HPCur);

	void OnHPMaxChange(int HPMax);

	void OnDexChange(int Dex);

	void OnStamCurChange(int StamCur);

	void OnStamMaxChange(int StamMax);

	void OnIntChange(int Int);

	void OnManaCurChange(int ManaCur);

	void OnManaMaxChange(int ManaMax);

	void OnFollCurChange(int current);

	void OnFollMaxChange(int maximum);

	void OnNameChange(string Name);

	void OnArmorChange(int Armor);

	void OnGoldChange(int Gold);

	void OnWeightChange(int Weight);

	void OnFlagsChange(MobileFlags Flags);

	void OnGenderChange(int Gender);

	void OnNotorietyChange(Notoriety n);

	void OnLuckChange();

	void OnDamageChange();

	void OnFireChange();

	void OnColdChange();

	void OnEnergyChange();

	void OnPoisonChange();

	void OnStatCapChange(int statCap);

	void OnRefresh();

	void Close();
}
