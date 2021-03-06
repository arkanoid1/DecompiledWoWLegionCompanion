using System;
using UnityEngine;
using UnityEngine.UI;
using WowJamMessages;
using WowJamMessages.MobileClientJSON;
using WowStatConstants;
using WowStaticData;

public class EquipmentPopup : MonoBehaviour
{
	[Header("New Equipment")]
	public Image m_equipmentIcon;

	public Text m_equipmentName;

	public Text m_equipmentQuantity;

	public Text m_equipmentDescription;

	[Header("Equipment Slots")]
	public GameObject m_followerEquipmentReplacementSlotArea;

	public GameObject m_followerEquipmentReplacementSlotPrefab;

	public Text m_tapASlotSlotMessage;

	public Text m_noEquipmentSlotsMessage;

	[Header("Error reporting")]
	public Text m_iconErrorText;

	private int m_garrFollowerID;

	private MobileFollowerEquipment m_item;

	public void OnEnable()
	{
		Main.instance.m_canvasBlurManager.AddBlurRef_MainCanvas();
	}

	private void OnDisable()
	{
		Main.instance.m_canvasBlurManager.RemoveBlurRef_MainCanvas();
	}

	public void SetEquipment(MobileFollowerEquipment item, int garrFollowerID)
	{
		this.m_garrFollowerID = garrFollowerID;
		this.m_item = item;
		ItemRec record = StaticDB.itemDB.GetRecord(item.ItemID);
		this.m_equipmentName.set_text(record.Display);
		GarrAbilityRec record2 = StaticDB.garrAbilityDB.GetRecord(item.GarrAbilityID);
		if (record2 != null)
		{
			this.m_equipmentDescription.set_text(record2.Description);
		}
		else
		{
			SpellTooltipRec record3 = StaticDB.spellTooltipDB.GetRecord(item.SpellID);
			if (record3 != null)
			{
				this.m_equipmentDescription.set_text(record3.Description);
			}
			else
			{
				this.m_equipmentDescription.set_text(string.Concat(new object[]
				{
					"ERROR. Ability ID:",
					item.GarrAbilityID,
					" Spell ID: ",
					item.SpellID,
					" Item ID:",
					item.ItemID
				}));
			}
		}
		this.m_equipmentDescription.set_text(WowTextParser.parser.Parse(this.m_equipmentDescription.get_text(), 0));
		if (this.m_iconErrorText != null)
		{
			this.m_iconErrorText.get_gameObject().SetActive(false);
		}
		Sprite sprite = GeneralHelpers.LoadIconAsset(AssetBundleType.Icons, record.IconFileDataID);
		if (sprite != null)
		{
			this.m_equipmentIcon.set_sprite(sprite);
		}
		else if (this.m_iconErrorText != null)
		{
			this.m_iconErrorText.get_gameObject().SetActive(true);
			this.m_iconErrorText.set_text(string.Empty + record.IconFileDataID);
		}
		this.m_equipmentQuantity.set_text((item.Quantity <= 1) ? string.Empty : (string.Empty + item.Quantity));
		FollowerEquipmentReplacementSlot[] componentsInChildren = this.m_followerEquipmentReplacementSlotArea.GetComponentsInChildren<FollowerEquipmentReplacementSlot>(true);
		FollowerEquipmentReplacementSlot[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			FollowerEquipmentReplacementSlot followerEquipmentReplacementSlot = array[i];
			Object.DestroyImmediate(followerEquipmentReplacementSlot.get_gameObject());
		}
		JamGarrisonFollower jamGarrisonFollower = PersistentFollowerData.followerDictionary.get_Item(garrFollowerID);
		for (int j = 0; j < jamGarrisonFollower.AbilityID.Length; j++)
		{
			GarrAbilityRec record4 = StaticDB.garrAbilityDB.GetRecord(jamGarrisonFollower.AbilityID[j]);
			if ((record4.Flags & 1u) != 0u)
			{
				GameObject gameObject = Object.Instantiate<GameObject>(this.m_followerEquipmentReplacementSlotPrefab);
				gameObject.get_transform().SetParent(this.m_followerEquipmentReplacementSlotArea.get_transform(), false);
				FollowerEquipmentReplacementSlot component = gameObject.GetComponent<FollowerEquipmentReplacementSlot>();
				component.SetAbility(jamGarrisonFollower.AbilityID[j]);
			}
		}
		FollowerEquipmentReplacementSlot[] componentsInChildren2 = this.m_followerEquipmentReplacementSlotArea.GetComponentsInChildren<FollowerEquipmentReplacementSlot>(true);
		bool flag = componentsInChildren2 != null && componentsInChildren2.Length > 0;
		this.m_noEquipmentSlotsMessage.get_gameObject().SetActive(!flag);
		this.m_tapASlotSlotMessage.get_gameObject().SetActive(flag);
	}

	public void UseEquipment(int replaceThisAbilityID)
	{
		Debug.Log(string.Concat(new object[]
		{
			"Attempting to equip item ",
			this.m_item.ItemID,
			" replacing ability ",
			replaceThisAbilityID
		}));
		Main.instance.UseEquipment(this.m_garrFollowerID, this.m_item.ItemID, replaceThisAbilityID);
		base.get_gameObject().SetActive(false);
	}
}
