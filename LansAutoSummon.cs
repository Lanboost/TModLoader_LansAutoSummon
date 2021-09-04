using Microsoft.Xna.Framework;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace LansAutoSummon
{
	public class LansAutoSummon : Mod
	{

		public static LansAutoSummon inst;

		public LansAutoSummon()
		{
			inst = this;
        }

		public override void Unload()
		{
			base.Unload();
			inst = null;
		}
	}

	public class FixPlayer : ModPlayer
	{
		public override void Initialize()
		{
			base.Initialize();
		}

		public override void PostUpdate()
		{
			int inventoryslot = GetInstance<Config>().InventorySlot;
			base.PostUpdate();
			float minCount = 0;
			for(int i=0; i<1000; i++)
			{
				if(Main.projectile[i].active && Main.projectile[i].minion && Main.projectile[i].owner == this.Player.whoAmI)
				{
					minCount += Main.projectile[i].minionSlots;
				}
			}
			if (minCount < this.Player.maxMinions) {

				var item = this.Player.inventory[inventoryslot];
				var minion = false;
				if (item.shoot > 0) {
					Projectile obj = new Projectile();
					obj.SetDefaults(item.shoot);
					minion = obj.minion;

					if (item.active && minion && !item.sentry)
					{

						int selectedItem = this.Player.selectedItem;
						var oldControlUseItem = this.Player.controlUseItem;
						var oldreleaseUseItem = this.Player.releaseUseItem;
						var olditemAnimation = this.Player.itemAnimation;
						var olditemTime = this.Player.itemTime;
						var olditemAnimationMax = this.Player.itemAnimationMax;
						var olditemLocation = this.Player.itemLocation;
						var olditemRotation = this.Player.itemRotation;
						var olddirection = this.Player.direction;
						var oldtoolTime = this.Player.toolTime;
						var oldchannel = this.Player.channel;
						var oldattackCD = this.Player.attackCD;

						this.Player.selectedItem = inventoryslot;

						var realUseTime = this.Player.HeldItem.useTime;
						var realMana = this.Player.HeldItem.mana;

						this.Player.HeldItem.useTime = 0;
						this.Player.HeldItem.mana = 0;


						this.Player.controlUseItem = true;
						this.Player.releaseUseItem = true;
						this.Player.itemAnimation = 0;
						this.Player.ItemCheck(this.Player.whoAmI);

						this.Player.itemAnimation = 2;

						this.Player.ItemCheck(this.Player.whoAmI);

						this.Player.HeldItem.useTime = realUseTime;
						this.Player.HeldItem.mana = realMana;

						this.Player.controlUseItem = oldControlUseItem;
						this.Player.releaseUseItem = oldreleaseUseItem;
						this.Player.itemAnimation = olditemAnimation;
						this.Player.itemTime = olditemTime;
						this.Player.itemAnimationMax = olditemAnimationMax;
						this.Player.itemLocation = olditemLocation;
						this.Player.itemRotation = olditemRotation;
						this.Player.direction = olddirection;
						this.Player.toolTime = oldtoolTime;
						this.Player.channel = oldchannel;
						this.Player.attackCD = oldattackCD;
						this.Player.selectedItem = selectedItem;
					}
				}
			}
		}
	}

}