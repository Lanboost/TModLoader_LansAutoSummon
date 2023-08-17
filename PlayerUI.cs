using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;
using Terraria.ModLoader;
using LansUILib.ui;
using Terraria;
using LansUILib;
using System.Drawing.Printing;
using Terraria.ID;

namespace LansAutoSummon
{
    public class PlayerUI : ModPlayer
    {
        bool showingUI = false;

        LansUILib.ui.LComponent panel;
        bool needRefresh = false;
        LansUILib.ui.PanelSettings panelSettings = new PanelSettings();

        public override void Initialize()
        {
            base.Initialize();
            panelSettings.SetAnchor(LansUILib.ui.AnchorPosition.TopLeft);
            panelSettings.SetSize(250, 250, 400, 600);
        }

        protected void createPanel()
        {
            panel = LansUILib.UIFactory.CreatePanel("Unlimited Pets Main", panelSettings, true, false);

            //panel.SetAnchor(LansUILib.ui.AnchorPosition.Center);
            //panel.SetSize(-200, -150, 400, 300);

            var inner = new LComponent("Inner");
            inner.isMask = true;
            panel.Add(inner);

            inner.SetMargins(15, 15, 15, 15);
            inner.SetLayout(new LansUILib.ui.LayoutFlow(new bool[] { false, false }, new bool[] { true, true }, LayoutFlowType.Vertical, 0, 0, 0, 0, 10));


            inner.Add(LansUILib.UIFactory.CreateText("Summon panel (Show with pet ui)", true));
            var player = Player.GetModPlayer<FixPlayer>();
            {
                var displayMinionCount = player.minionCount.lastValue;
                var displayMaxMinionCount = player.minionMaxCount.lastValue;

                inner.Add(LansUILib.UIFactory.CreateText($"Minion Slots: {displayMinionCount} / {displayMaxMinionCount}", true));


                var scrollpanel = UIFactory.CreateScrollPanel();
                scrollpanel.wrapper.GetLayout().Flex = 1;

                //var recipePanel = LansUILib.UIFactory.CreatePanel("Recipe Panel", false, false);
                var scrollContentPanel = scrollpanel.contentPanel;
                scrollContentPanel.SetLayout(new LansUILib.ui.LayoutFlow(new bool[] { true, true }, new bool[] { false, false }, LayoutFlowType.Vertical, 0, 0, 0, 0, 10));
                inner.Add(scrollpanel.wrapper);

                var inventory = Player.GetModPlayer<FixPlayer>().summonItems;
                var itemSlots = inventory.slots;
                var emptySlot = inventory.emptySlot;

                foreach (var slot in itemSlots)
                {
                    var recipePanelCurr = LansUILib.UIFactory.CreatePanel("Recipe Panel Current", false, false);
                    recipePanelCurr.MouseInteraction = false;
                    recipePanelCurr.SetLayout(new LansUILib.ui.LayoutFlow(new bool[] { true, true }, new bool[] { false, true }, LayoutFlowType.Horizontal, 3, 3, 3, 3, 5));

                    var itemSlotPanel = LansUILib.UIFactory.CreateItemSlot(slot.summonWeapon);

                    recipePanelCurr.Add(itemSlotPanel);

                    if (slot.fill)
                    {
                        recipePanelCurr.Add(UIFactory.CreateText("Fill", true));
                        var fillButton = UIFactory.CreateButton("Unfill");
                        fillButton.Panel.SetLayout(new LayoutSize(80, 30));
                        fillButton.OnClicked += delegate (MouseState state)
                        {
                            slot.fill = false;
                            needRefresh = true;
                            inventory.dirty = true;
                        };
                        recipePanelCurr.Add(fillButton.Panel);
                    }
                    else
                    {
                        recipePanelCurr.Add(UIFactory.CreateText(slot.count.ToString(), true));
                        var minusButton = UIFactory.CreateButton("-1");
                        minusButton.Panel.SetLayout(new LayoutSize(30, 30));
                        minusButton.OnClicked += delegate (MouseState state)
                        {
                            slot.count -= 1;
                            if (slot.count <= 0)
                            {
                                slot.count = 1;
                            }
                            needRefresh = true;
                            inventory.dirty = true;
                        };
                        recipePanelCurr.Add(minusButton.Panel);
                        var plusButton = UIFactory.CreateButton("+1");
                        plusButton.Panel.SetLayout(new LayoutSize(30, 30));
                        plusButton.OnClicked += delegate (MouseState state)
                        {
                            slot.count += 1;
                            if (slot.count >= 100)
                            {
                                slot.count = 100;
                            }
                            needRefresh = true;
                            inventory.dirty = true;
                        };
                        recipePanelCurr.Add(plusButton.Panel);
                        var fillButton = UIFactory.CreateButton("Fill");
                        fillButton.Panel.SetLayout(new LayoutSize(60, 30));
                        fillButton.OnClicked += delegate (MouseState state)
                        {
                            slot.fill = true;
                            needRefresh = true;
                            inventory.dirty = true;
                        };
                        recipePanelCurr.Add(fillButton.Panel);

                    }


                    var minionCount = "?";
                    if (slot.summonWeapon.Item != null && !slot.summonWeapon.Item.IsAir)
                    {
                        Projectile p = new Projectile();
                        p.SetDefaults(slot.summonWeapon.Item.shoot);
                        minionCount = "" + p.minionSlots;
                    }
                    recipePanelCurr.Add(UIFactory.CreateText($"Minion Slot Cost: {minionCount}", true));



                    scrollContentPanel.Add(recipePanelCurr);
                }

                {
                    var itemSlotPanel = LansUILib.UIFactory.CreateItemSlot(emptySlot.summonWeapon, delegate (Item item)
                    {
                        if (item.shoot == ProjectileID.None)
                        {
                            return false;
                        }

                        Projectile p = new Projectile();
                        p.SetDefaults(item.shoot);

                        if (!p.minion)
                        {
                            return false;
                        }
                        if (p.sentry)
                        {
                            return false;
                        }

                        return true;
                    });
                    scrollContentPanel.Add(itemSlotPanel);
                }

                var settingsPanel = new LComponent("Settings");
                settingsPanel.isMask = true;
                inner.Add(settingsPanel);

                //settingsPanel.SetMargins(15, 15, 15, 15);
                settingsPanel.SetLayout(new LansUILib.ui.LayoutFlow(new bool[] { true, true }, new bool[] { false, false }, LayoutFlowType.Horizontal, 0, 0, 0, 0, 10));


                var summonEnabled = Player.GetModPlayer<FixPlayer>().autoSummonEnabled ? "Disable Auto" : "Enable Auto";

                var toggleAutoSpawn = UIFactory.CreateButton(summonEnabled);
                toggleAutoSpawn.Panel.SetLayout(new LayoutSize(100, 30));
                toggleAutoSpawn.OnClicked += delegate (MouseState state)
                {
                    Player.GetModPlayer<FixPlayer>().autoSummonEnabled = !Player.GetModPlayer<FixPlayer>().autoSummonEnabled;
                    Player.GetModPlayer<FixPlayer>().tempSummonDisabled = false;
                    needRefresh = true;
                };
                settingsPanel.Add(toggleAutoSpawn.Panel);

                var summonButton = UIFactory.CreateButton("Summon");
                summonButton.Panel.SetLayout(new LayoutSize(100, 30));
                summonButton.OnClicked += delegate (MouseState state)
                {
                    Player.GetModPlayer<FixPlayer>().forceSummon = true;
                    Player.GetModPlayer<FixPlayer>().tempSummonDisabled = false;
                    needRefresh = true;
                };
                settingsPanel.Add(summonButton.Panel);

                var desummonButton = UIFactory.CreateButton("De-Summon");
                desummonButton.Panel.SetLayout(new LayoutSize(100, 30));
                desummonButton.OnClicked += delegate (MouseState state)
                {
                    Player.GetModPlayer<FixPlayer>().tempSummonDisabled = true;
                    Player.GetModPlayer<FixPlayer>().CancelAllMinions();
                    needRefresh = true;
                };
                settingsPanel.Add(desummonButton.Panel);
            }

            {
                inner.Add(LansUILib.UIFactory.CreateText($"--- Sentry Weapons ---", true));

                var displayMinionCount = player.sentryCount.lastValue;
                var displayMaxMinionCount = player.sentryMaxCount.lastValue;

                inner.Add(LansUILib.UIFactory.CreateText($"Sentry Slots: {displayMinionCount} / {displayMaxMinionCount}", true));

                var scrollpanel = UIFactory.CreateScrollPanel();
                scrollpanel.wrapper.GetLayout().Flex = 1;

                //var recipePanel = LansUILib.UIFactory.CreatePanel("Recipe Panel", false, false);
                var scrollContentPanel = scrollpanel.contentPanel;
                scrollContentPanel.SetLayout(new LansUILib.ui.LayoutFlow(new bool[] { true, true }, new bool[] { false, false }, LayoutFlowType.Vertical, 0, 0, 0, 0, 10));
                inner.Add(scrollpanel.wrapper);

                var inventory = Player.GetModPlayer<FixPlayer>().sentryItems;
                var itemSlots = inventory.slots;
                var emptySlot = inventory.emptySlot;

                foreach (var slot in itemSlots)
                {
                    var recipePanelCurr = LansUILib.UIFactory.CreatePanel("Recipe Panel Current", false, false);
                    recipePanelCurr.MouseInteraction = false;
                    recipePanelCurr.SetLayout(new LansUILib.ui.LayoutFlow(new bool[] { true, true }, new bool[] { false, true }, LayoutFlowType.Horizontal, 3, 3, 3, 3, 5));

                    var itemSlotPanel = LansUILib.UIFactory.CreateItemSlot(slot.summonWeapon);

                    recipePanelCurr.Add(itemSlotPanel);

                    scrollContentPanel.Add(recipePanelCurr);
                }

                {
                    var itemSlotPanel = LansUILib.UIFactory.CreateItemSlot(emptySlot.summonWeapon, delegate (Item item)
                    {
                        if (item.shoot == ProjectileID.None)
                        {
                            return false;
                        }

                        Projectile p = new Projectile();
                        p.SetDefaults(item.shoot);

                        if (!p.sentry)
                        {
                            return false;
                        }

                        return true;
                    });
                    scrollContentPanel.Add(itemSlotPanel);
                }
            }

        }

        public override void PostUpdateBuffs()
        {
            base.PostUpdateBuffs();
            if (!Main.dedServ && this.Player.whoAmI == Main.myPlayer)
            {
                var player = Player.GetModPlayer<FixPlayer>();
                var newState = showingUI;

                if (Main.EquipPage == 2)
                {
                    newState = true;
                }
                else
                {
                    newState = false;
                }

                if (!newState && Main.playerInventory && FixPlayer.ItemIsValidSummonItem(Main.mouseItem))
                {
                    newState = true;
                    Main.EquipPageSelected = 2;
                }

                // update inventories

                var inventories = new ExpandingInventory[]
                {
                    player.summonItems,
                    player.sentryItems,
                    player.otherItems,
                };

                foreach (var inventory in inventories)
                {
                    var itemSlots = inventory.slots;
                    var emptySlot = inventory.emptySlot;

                    for (int i = itemSlots.Count - 1; i >= 0; i--)
                    {
                        if (itemSlots[i].summonWeapon == null || itemSlots[i].summonWeapon.Item.IsAir)
                        {
                            itemSlots.RemoveAt(i);
                            needRefresh = true;
                            inventory.dirty = true;
                        }
                    }
                    if (emptySlot.summonWeapon != null && !emptySlot.summonWeapon.Item.IsAir)
                    {
                        itemSlots.Add(emptySlot);
                        inventory.emptySlot = new SummonSlot();
                        needRefresh = true;
                        inventory.dirty = true;
                    }

                    foreach (var slot in itemSlots)
                    {
                        slot.summonWeapon?.Update();
                    }
                }

                if(player.minionCount.Dirty || player.minionMaxCount.Dirty || player.sentryCount.Dirty || player.sentryMaxCount.Dirty)
                {
                    needRefresh = true;
                }

                if (needRefresh)
                {
                    needRefresh = false;
                    if (showingUI)
                    {
                        LansUILib.UISystem.Instance.Screen.Remove(panel);
                    }
                    createPanel();

                    if (showingUI)
                    {
                        LansUILib.UISystem.Instance.Screen.Add(panel);
                        panel.Invalidate();
                    }

                }

                if (newState != showingUI)
                {
                    showingUI = newState;
                    if (showingUI)
                    {
                        if (panel == null)
                        {
                            createPanel();
                        }
                        LansUILib.UISystem.Instance.Screen.Add(panel);
                        panel.Invalidate();
                    }
                    else
                    {
                        LansUILib.UISystem.Instance.Screen.Remove(panel);
                    }
                }
            }
        }
    }
}
