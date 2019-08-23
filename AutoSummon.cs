using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace AutoSummon
{
	public class AutoSummon : Mod
	{

		public static AutoSummon inst;

		public AutoSummon()
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
			int inventoryslot = 9;
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
					this.player.AddBuff(item.buffType, 3600, true);

					var p = Projectile.NewProjectile(this.player.position.X, this.player.position.Y, 0, 0, item.shoot, item.damage, item.knockBack, this.player.whoAmI, 0f, 0f);
					//Main.projectile[p].npcProj = true;
					Main.PlaySound(19, (int)this.player.position.X, (int)this.player.position.Y, 1, 1f, 0f);
				}
			}
		}
	}

}