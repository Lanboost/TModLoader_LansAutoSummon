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
	}

	public class FixPlayer : ModPlayer
	{
		public override void Initialize()
		{
			base.Initialize();

			/*
			Main.LocalPlayer.AddBuff(BuffID.BabySlime, 3600, true);

			Projectile.NewProjectile(Main.LocalPlayer.position.X, Main.LocalPlayer.position.Y, 0, 0, ProjectileID.BabySlime, 9, 2.5f, Main.LocalPlayer.whoAmI, 0f, 0f);
			Main.PlaySound(19, (int)Main.LocalPlayer.position.X, (int)Main.LocalPlayer.position.Y, 1, 1f, 0f);
			*/
		}

		public override void PostUpdate()
		{

			int inventoryslot = GetInstance<Config>().InventorySlot;
			base.PostUpdate();
			float minCount = 0;
			for(int i=0; i<1000; i++)
			{
				if(Main.projectile[i].active && Main.projectile[i].minion && Main.projectile[i].owner == this.player.whoAmI)
				{
					minCount += Main.projectile[i].minionSlots;
				}
			}
			
			if (minCount < this.player.maxMinions) {

				var item = this.player.inventory[inventoryslot];
				if (item.active && item.summon && !item.sentry)
				{


					int selectedItem = this.player.selectedItem;
					var oldControlUseItem = this.player.controlUseItem;
					var oldreleaseUseItem = this.player.releaseUseItem;
					var olditemAnimation = this.player.itemAnimation;
					var olditemTime = this.player.itemTime;
					var olditemAnimationMax = this.player.itemAnimationMax;
					var olditemLocation = this.player.itemLocation;
					var olditemRotation = this.player.itemRotation;
					var olddirection = this.player.direction;
					var oldtoolTime = this.player.toolTime;
					var oldchannel = this.player.channel;
					var oldattackCD = this.player.attackCD;

					this.player.selectedItem = inventoryslot;

					this.player.HeldItem.useTime = 0;
					this.player.HeldItem.mana = 0;


					this.player.controlUseItem = true;
					this.player.releaseUseItem = true;
					this.player.itemAnimation = 0;
					this.player.ItemCheck(this.player.whoAmI);

					this.player.itemAnimation = 2;

					/*if (item.netID == 3531)
					{
							handleStardustDragon(item);
					}
					else
					{

						this.player.AddBuff(item.buffType, 3600, true);
						var p = Projectile.NewProjectile(this.player.position.X, this.player.position.Y, 0, 0, item.shoot, item.damage, item.knockBack, this.player.whoAmI, 0f, 0f);

						
						Main.PlaySound(19, (int)this.player.position.X, (int)this.player.position.Y, 1, 1f, 0f);
					}*/


					//this.player.controlUseItem = false;
					//this.player.releaseUseItem = false;
					this.player.ItemCheck(this.player.whoAmI);
					
					
					//ItemLoader.UseItem(this.player.HeldItem, this.player);
					
					this.player.controlUseItem = oldControlUseItem;
					this.player.releaseUseItem = oldreleaseUseItem;
					this.player.itemAnimation = olditemAnimation;
					this.player.itemTime = olditemTime;
					this.player.itemAnimationMax = olditemAnimationMax;
					this.player.itemLocation = olditemLocation;
					this.player.itemRotation = olditemRotation;
					this.player.direction = olddirection;
					this.player.toolTime = oldtoolTime;
					this.player.channel = oldchannel;
					this.player.attackCD = oldattackCD;
					this.player.selectedItem = selectedItem;
				}
			}
		}


		public void handleStardustDragon(Item item)
		{
			var num81 = 0f;
			var num82 = 0f;
			var vector2 = this.player.position;
			int num74 = 625;
			int num184 = -1;
			int num185 = -1;
			for (int num186 = 0; num186 < 1000; num186++)
			{
				if (Main.projectile[num186].active && Main.projectile[num186].owner == Main.myPlayer)
				{
					if (num184 == -1 && Main.projectile[num186].type == 625)
					{
						num184 = num186;
					}
					if (num185 == -1 && Main.projectile[num186].type == 628)
					{
						num185 = num186;
					}
					if (num184 != -1 && num185 != -1)
					{
						break;
					}
				}
			}
			if (num184 == -1 && num185 == -1)
			{
				int num187 = Projectile.NewProjectile(vector2.X, vector2.Y, num81, num82, num74, item.damage, item.knockBack, this.player.whoAmI, 0f, 0f);
				num187 = Projectile.NewProjectile(vector2.X, vector2.Y, num81, num82, num74 + 1, item.damage, item.knockBack, this.player.whoAmI, (float)num187, 0f);


				int num188 = num187;
				num187 = Projectile.NewProjectile(vector2.X, vector2.Y, num81, num82, num74 + 2, item.damage, item.knockBack, this.player.whoAmI, (float)num187, 0f);

				Main.projectile[num188].localAI[1] = (float)num187;
				num188 = num187;
				num187 = Projectile.NewProjectile(vector2.X, vector2.Y, num81, num82, num74 + 3, item.damage, item.knockBack, this.player.whoAmI, (float)num187, 0f);


				Main.projectile[num188].localAI[1] = (float)num187;
			}
			else if (num184 != -1 && num185 != -1)
			{
				int num189 = Projectile.NewProjectile(vector2.X, vector2.Y, num81, num82, num74 + 1, item.damage, item.knockBack, this.player.whoAmI, (float)Projectile.GetByUUID(Main.myPlayer, Main.projectile[num185].ai[0]), 0f);

				int num190 = num189;
				num189 = Projectile.NewProjectile(vector2.X, vector2.Y, num81, num82, num74 + 2, item.damage, item.knockBack, this.player.whoAmI, (float)num189, 0f);


				Main.projectile[num190].localAI[1] = (float)num189;
				Main.projectile[num190].netUpdate = true;
				Main.projectile[num190].ai[1] = 1f;
				Main.projectile[num189].localAI[1] = (float)num185;
				Main.projectile[num189].netUpdate = true;
				Main.projectile[num189].ai[1] = 1f;
				Main.projectile[num185].ai[0] = (float)Main.projectile[num189].projUUID;
				Main.projectile[num185].netUpdate = true;
				Main.projectile[num185].ai[1] = 1f;
			}
		}
	}

}