using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace LansAutoSummon
{
	class Config : ModConfig
	{
		// You MUST specify a ConfigScope.
		public override ConfigScope Mode => ConfigScope.ServerSide;


		[DefaultValue(10)]
		[Label("Inventoryslot to use summon item from")]
		[Tooltip("Valid values are 1-50")]
		[Range(1, 50)]
		public int InventorySlot;

	}
}
