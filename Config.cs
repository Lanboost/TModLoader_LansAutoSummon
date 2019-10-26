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


		[DefaultValue(9)]
		[Label("Inventoryslot to use summon item from")]
		[Tooltip("Valid values are 0-49")]
		[Range(0, 49)]
		public int InventorySlot;

	}
}
