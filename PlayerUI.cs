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
        
        float displayMinionCount = 0;
        int displayMaxMinionCount = 0;

        LansUILib.ui.LComponent panel;
        bool needRefresh = false;
        LansUILib.ui.PanelSettings panelSettings = new PanelSettings();

        public override void Initialize()
        {
            base.Initialize();
            panelSettings.SetAnchor(LansUILib.ui.AnchorPosition.TopLeft);
            panelSettings.SetSize(250, 250, 400, 400);
        }

        protected void RefreshMinions()
        {
            FixPlayer fixPlayer = null;
            Main.LocalPlayer.TryGetModPlayer<FixPlayer>(out fixPlayer);
            if(fixPlayer != null)
            {
                fixPlayer.CancelAllMinions();
                fixPlayer.isSpawning = true;
            }
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
            inner.Add(LansUILib.UIFactory.CreateText("For summon order check description", true));


            displayMinionCount = Player.GetModPlayer<FixPlayer>().lastMinionCount;
            displayMaxMinionCount = Player.GetModPlayer<FixPlayer>().lastMaxCount;

            inner.Add(LansUILib.UIFactory.CreateText($"Current Minion Slots: {displayMinionCount}", true));
            inner.Add(LansUILib.UIFactory.CreateText($"Max Minion Slots: {displayMaxMinionCount}", true));

            var scrollpanel = UIFactory.CreateScrollPanel();
            scrollpanel.wrapper.GetLayout().Flex = 1;

            //var recipePanel = LansUILib.UIFactory.CreatePanel("Recipe Panel", false, false);
            var scrollContentPanel = scrollpanel.contentPanel;
            scrollContentPanel.SetLayout(new LansUILib.ui.LayoutFlow(new bool[] { true, true }, new bool[] { false, false }, LayoutFlowType.Vertical, 0, 0, 0, 0, 10));
            inner.Add(scrollpanel.wrapper);

            var itemSlots = Player.GetModPlayer<FixPlayer>().summonSlots;
            var emptySlot = Player.GetModPlayer<FixPlayer>().emptySlot;

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
                        RefreshMinions();
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
                        slot.count -=1;
                        if(slot.count <= 0)
                        {
                            slot.count = 1;
                        }
                        needRefresh = true;
                        RefreshMinions();
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
                        RefreshMinions();
                    };
                    recipePanelCurr.Add(plusButton.Panel);
                    var fillButton = UIFactory.CreateButton("Fill");
                    fillButton.Panel.SetLayout(new LayoutSize(60, 30));
                    fillButton.OnClicked += delegate (MouseState state)
                    {
                        slot.fill = true;
                        needRefresh = true;
                        RefreshMinions();
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
                var itemSlotPanel = LansUILib.UIFactory.CreateItemSlot(emptySlot.summonWeapon, delegate(Item item) { 
                    if(item.shoot == ProjectileID.None)
                    {
                        return false;
                    }

                    Projectile p = new Projectile();
                    p.SetDefaults(item.shoot);
                    
                    if(!p.minion)
                    {
                        return false;
                    }
                    if(p.sentry)
                    {
                        return false;
                    }

                    return true;
                });
                scrollContentPanel.Add(itemSlotPanel);
            }
        }

        public override void PostUpdateBuffs()
        {
            base.PostUpdateBuffs();
            if (!Main.dedServ && this.Player.whoAmI == Main.myPlayer)
            {
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

                var itemSlots = Player.GetModPlayer<FixPlayer>().summonSlots;
                var emptySlot = Player.GetModPlayer<FixPlayer>().emptySlot;


                for (int i = itemSlots.Count - 1; i >= 0; i--)
                {
                    if (itemSlots[i].summonWeapon == null || itemSlots[i].summonWeapon.Item.IsAir)
                    {
                        itemSlots.RemoveAt(i);
                        needRefresh = true;
                        RefreshMinions();
                    }
                }
                if (emptySlot.summonWeapon != null && !emptySlot.summonWeapon.Item.IsAir)
                {
                    itemSlots.Add(emptySlot);
                    Player.GetModPlayer<FixPlayer>().emptySlot = new SummonSlot();
                    needRefresh = true;
                    RefreshMinions();
                }

                if(displayMinionCount != Player.GetModPlayer<FixPlayer>().lastMinionCount || displayMaxMinionCount != Player.GetModPlayer<FixPlayer>().lastMaxCount)
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


                foreach (var slot in itemSlots)
                {
                    slot.summonWeapon?.Update();
                }
            }
        }
    }
}
