using LansUILib.ui;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
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

        public List<SummonSlot> summonSlots = new List<SummonSlot>();
        public SummonSlot emptySlot = new SummonSlot();

        public bool isSpawning = false;
        public float lastMinionCount = 0;
        public int lastMaxCount = 0;


        public override void Initialize()
        {
            base.Initialize();
        }

        public override void LoadData(TagCompound tag)
        {
            base.LoadData(tag);
            summonSlots = (List<SummonSlot>)tag.GetList<SummonSlot>("summonSlots");
        }

        public override void SaveData(TagCompound tag)
        {
            base.SaveData(tag);
            tag["summonSlots"] = summonSlots;
        }

        protected float getCurrentMinionCount()
        {
            float minCount = 0;
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].minion && Main.projectile[i].owner == this.Player.whoAmI)
                {
                    minCount += Main.projectile[i].minionSlots;
                }
            }
            return minCount;
        }

        public void CancelAllMinions()
        {
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].minion && Main.projectile[i].owner == this.Player.whoAmI)
                {
                    Main.projectile[i].Kill();
                }
            }
        }

        protected bool shouldSummon()
        {
            if (this.Player.dead)
            {
                return false;
            }
            return (Main.mouseItem == null || Main.mouseItem.IsAir) &&
                (isSpawning || lastMaxCount != this.Player.maxMinions || lastMinionCount != getCurrentMinionCount());

        }

        protected void summon(Item summonItem)
        {

            var lastScreenPosition = Main.screenPosition + Vector2.Zero;
            Main.screenPosition.X += Random.Shared.Next(0, 10) * 16 - 5 * 16;
            Main.screenPosition.Y += Random.Shared.Next(0, 10) * 16 - 5 * 16;

            var oldItem = this.Player.inventory[this.Player.selectedItem];

            this.Player.inventory[this.Player.selectedItem] = summonItem;
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

            var realUseTime = this.Player.HeldItem.useTime;
            var realMana = this.Player.HeldItem.mana;

            //this.Player.HeldItem.useTime = 0;
            this.Player.HeldItem.mana = 0;


            this.Player.controlUseItem = true;
            //this.Player.releaseUseItem = true;
            this.Player.itemAnimation = 0;
            this.Player.ItemCheck();
            this.Player.controlUseItem = false;
            while (this.Player.itemAnimation > 0)
            {
                //this.Player.releaseUseItem = true;
                this.Player.ItemCheck();
            }
            //this.Player.controlUseItem = true;
            //this.Player.releaseUseItem = true;
            //this.Player.ItemCheck(this.Player.whoAmI);
            //this.Player.itemAnimation = 2;

            //this.Player.ItemCheck(this.Player.whoAmI);

            //this.Player.HeldItem.useTime = realUseTime;
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

            this.Player.inventory[this.Player.selectedItem] = oldItem;
            Main.screenPosition.X = lastScreenPosition.X;
            Main.screenPosition.Y = lastScreenPosition.Y;
        }

        protected void trySummon(Item summonItem, int summonCount, bool fill, int retryCount)
        {
            var currentMinionCount = getCurrentMinionCount();

            int currentSummonCount = 0;
            int currentRetryCount = 0;
            while (true)
            {
                //this.Player.maxMinions
                Projectile obj = new Projectile();
                obj.SetDefaults(summonItem.shoot);
                if (currentMinionCount + obj.minionSlots > this.Player.maxMinions)
                {
                    Main.NewText($"LansAutoSummon: Cannot summon {summonItem.Name} max minions reached. Current: {currentMinionCount} Max: {this.Player.maxMinions} Needed: {obj.minionSlots}");
                    break;
                }

                summon(summonItem);
                var newCount = getCurrentMinionCount();
                if (newCount != currentMinionCount)
                {
                    currentMinionCount = newCount;
                    currentSummonCount += 1;
                    currentRetryCount = 0;
                }
                else
                {
                    currentRetryCount += 1;
                }

                if (currentRetryCount >= retryCount)
                {
                    break;
                }

                if (currentSummonCount >= summonCount && !fill)
                {
                    break;
                }
            }
        }

        public static bool ItemIsValidSummonItem(Item item)
        {
            if (item != null && !item.IsAir)
            {
                if (item.shoot > 0)
                {
                    Projectile obj = new Projectile();
                    obj.SetDefaults(item.shoot);

                    if (item.active && obj.minion && !item.sentry)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override void OnRespawn()
        {
            base.OnRespawn();
            isSpawning = true;
        }

        public override void PlayerConnect()
        {
            base.PlayerConnect();
            isSpawning = true;
        }

        public override void PostUpdate()
        {
            base.PostUpdate();
            if (!Main.dedServ && this.Player.whoAmI == Main.myPlayer)
            {

                if (shouldSummon())
                {
                    CancelAllMinions();
                    foreach (var summon in summonSlots)
                    {
                        if (summon.fill)
                        {
                            trySummon(summon.summonWeapon.Item, 1, false, 5);
                            break;
                        }
                    }


                    foreach (var summon in summonSlots)
                    {
                        trySummon(summon.summonWeapon.Item, summon.count, summon.fill, 5);
                    }

                    isSpawning = false;
                    lastMaxCount = this.Player.maxMinions;
                    lastMinionCount = getCurrentMinionCount();

                }
            }
        }
    }

}