using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KindredCommands.Data;
using KindredCommands.Models.Enums;
using ProjectM;

namespace KindredCommands.Models.Vip;
public class KitVip
{
	public VipEnum Vip { get; set; }
	public List<Item> ItemList { get; set; }

	public KitVip(VipEnum vip)
	{
		Vip = vip;
		switch (Vip)
		{
			case VipEnum.VipLuzArgenta:
				ItemList = new List<Item> {
					new Item()
						{
							GUID = Prefabs.Item_Ingredient_StoneBrick,
							Quantity = 400,
						},
						new Item()
						{
							GUID = Prefabs.Item_Ingredient_Plank,
							Quantity = 400,
						}
					};
				break;
			case VipEnum.VipGloomrot:
				ItemList = new List<Item> {
						new Item()
						{
							GUID = Prefabs.Item_Ingredient_StoneBrick,
							Quantity = 600,
						},
						new Item()
						{
							GUID = Prefabs.Item_Ingredient_Plank,
							Quantity = 600,
						}
					};
				break;
			case VipEnum.VipFarbane:
				ItemList = new List<Item> {
						new Item()
						{
							GUID = Prefabs.Item_Ingredient_StoneBrick,
							Quantity = 900,
						},
						new Item()
						{
							GUID = Prefabs.Item_Ingredient_Plank,
							Quantity = 900,
						}
					};
				break;
			default:
				new List<Item>();
				break;
		}

	}


}
