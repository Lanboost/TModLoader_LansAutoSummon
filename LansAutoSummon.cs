using LansUILib;
using LansUILib.ui;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;

namespace LansAutoSummon
{
    public class LansAutoSummon : Mod
    {

        public static LansAutoSummon inst;
        public bool soundIsSentry = false;

        public LansAutoSummon()
        {
            inst = this;
        }

        public override void Load()
        {
            base.Load();

            Terraria.Audio.IL_SoundPlayer.Play += DisableSoundIfSentry;
        }

        public void DisableSoundIfSentry(ILContext iLContext)
        {
            LansUILib.InjectUtils.InjectSkipOnBooleanWithReturnValue(iLContext, ReturnSoundIsSentry, ReturnInvalidSoundSlot);
        }

        public static bool ReturnSoundIsSentry()
        {
            return inst.soundIsSentry;
        }

        public static object ReturnInvalidSoundSlot()
        {
            return SlotId.Invalid;
        }

        public override void Unload()
        {
            base.Unload();
            inst = null;
        }
    }

    public class ExpandingInventory
    {
        public List<SummonSlot> slots = new List<SummonSlot>();
        public SummonSlot emptySlot = new SummonSlot();

        public bool dirty;

        protected string saveTag;

        public ExpandingInventory(string saveTag)
        {
            this.saveTag = saveTag;
        }

        public void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey(this.saveTag))
            {
                slots = (List<SummonSlot>)tag.GetList<SummonSlot>(this.saveTag);
            }
        }

        public void SaveData(TagCompound tag)
        {
            tag[this.saveTag] = slots;
        }
    }

    public class TrackValue<T>
    {
        public Func<T> valueFunc;

        public T lastValue;
        public bool Dirty = true;

        public event Action<T> valueChanged;

        public TrackValue(Func<T> valueFunc)
        {
            this.valueFunc = valueFunc;
        }

        public void Check()
        {
            Dirty = false;
            var v = valueFunc();
            if(!v.Equals(lastValue))
            {
                Dirty = true;
                lastValue = v;
                valueChanged?.Invoke(v);
            }
        }
    }

    public class FixPlayer : ModPlayer
    {
        public ExpandingInventory summonItems = new ExpandingInventory("summonSlots");
        public ExpandingInventory sentryItems = new ExpandingInventory("sentrySlots");
        public ExpandingInventory otherItems = new ExpandingInventory("otherSlots");

        public bool isSpawning = false;

        public TrackValue<float> minionCount;
        public TrackValue<int> minionMaxCount;

        public TrackValue<int> sentryCount;
        public TrackValue<int> sentryMaxCount;

        public List<TrackValue<object>> trackValues = new List<TrackValue<object>>();

        public bool autoSummonEnabled = true;
        public bool tempSummonDisabled = false;
        public bool forceSummon = false;

        public int currentSentrySlotId = 0;


        public override void Initialize()
        {
            base.Initialize();

            minionCount = new TrackValue<float>(getCurrentMinionCount);
            minionMaxCount = new TrackValue<int>(delegate() { return Player.maxMinions; });
            sentryCount = new TrackValue<int>(getCurrentSentryCount);
            sentryMaxCount = new TrackValue<int>(delegate () { return Player.maxTurrets; });
        }

        public override void LoadData(TagCompound tag)
        {
            base.LoadData(tag);

            summonItems.LoadData(tag);
            sentryItems.LoadData(tag);
            otherItems.LoadData(tag);


            if (tag.ContainsKey("autoSummon"))
            {
                autoSummonEnabled = tag.GetBool("autoSummon");
            }
        }

        public override void SaveData(TagCompound tag)
        {
            base.SaveData(tag);

            summonItems.SaveData(tag);
            sentryItems.SaveData(tag);
            otherItems.SaveData(tag);

            tag["autoSummon"] = autoSummonEnabled;
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

        protected int getCurrentSentryCount()
        {
            var turrets = 0;
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].WipableTurret && Main.projectile[i].owner == this.Player.whoAmI)
                    turrets += 1;
            }
            return turrets;
        }

        public void CancelAllMinions()
        {
            for (int i = 0; i < Main.projectile.Length; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].minion && Main.projectile[i].owner == this.Player.whoAmI)
                {
                    Main.projectile[i].Kill();
                }
            }
        }

        protected bool shouldSummon()
        {
            // Do nothing if dead
            if (this.Player.dead)
            {
                Player.GetModPlayer<FixPlayer>().tempSummonDisabled = false;
                return false;
            }

            // Do notthing if holding an item
            if (Main.mouseItem != null && !Main.mouseItem.IsAir) {
                return false;
            }

            // Do nothing if in middle of swing
            if(this.Player.itemAnimation != 0)
            {
                return false;
            }

            if (forceSummon)
            {
                return true;
            }

            return autoSummonEnabled && !tempSummonDisabled && (summonItems.dirty || isSpawning || minionCount.Dirty || minionMaxCount.Dirty);
        }

        protected bool shouldSummonSentry()
        {
            // Do nothing if dead
            if (this.Player.dead)
            {
                Player.GetModPlayer<FixPlayer>().tempSummonDisabled = false;
                return false;
            }

            // Do notthing if holding an item
            if (Main.mouseItem != null && !Main.mouseItem.IsAir)
            {
                return false;
            }

            // Do nothing if in middle of swing
            if (this.Player.itemAnimation != 0)
            {
                return false;
            }


            // remove sentries to far away
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].WipableTurret)
                {
                    if(Vector2.DistanceSquared(Main.projectile[i].Center, Player.Center) > 16*50*16*50)
                    {

                        LansAutoSummon.inst.soundIsSentry = true;
                        Main.projectile[i].Kill();

                        LansAutoSummon.inst.soundIsSentry = false;
                    }
                }
            }


            return Player.maxTurrets != getCurrentSentryCount();

        }

        protected void summon(Item summonItem)
        {

            var lastScreenPosition = Main.screenPosition + Vector2.Zero;
            Main.screenPosition.X = Player.Center.X - Main.MouseScreen.X + Random.Shared.Next(0, 10) * 16 - 5 * 16;
            Main.screenPosition.Y = Player.Center.Y - Main.MouseScreen.Y + Random.Shared.Next(0, 10) * 16 - 5 * 16;
            
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
            var realSound = this.Player.HeldItem.UseSound;

            //this.Player.HeldItem.useTime = 0;
            this.Player.HeldItem.mana = 0;
            this.Player.HeldItem.UseSound = null;


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
            this.Player.HeldItem.UseSound = realSound;

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

                    if (item.active && (obj.minion|| obj.sentry)) //&& !item.sentry)
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
                    forceSummon = false;
                    summonItems.dirty = false;
                    CancelAllMinions();
                    foreach (var summon in summonItems.slots)
                    {
                        if (summon.fill)
                        {
                            trySummon(summon.summonWeapon.Item, 1, false, 5);
                            break;
                        }
                    }


                    foreach (var summon in summonItems.slots)
                    {
                        trySummon(summon.summonWeapon.Item, summon.count, summon.fill, 5);
                    }

                    isSpawning = false;

                }

                if(shouldSummonSentry())
                {
                    if(sentryItems.slots.Count > 0)
                    {
                        if(currentSentrySlotId >= sentryItems.slots.Count)
                        {
                            currentSentrySlotId = 0;
                        }
                        trySummon(sentryItems.slots[currentSentrySlotId].summonWeapon.Item, 1, false, 1);
                        currentSentrySlotId++;
                    }
                }

                minionCount.Check();
                minionMaxCount.Check();
                sentryCount.Check();
                sentryMaxCount.Check();
            }
        }
    }

}