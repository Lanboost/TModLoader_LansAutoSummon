using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LansUILib.ui;
using Terraria;
using Terraria.ModLoader.IO;

namespace LansAutoSummon
{
    public class SummonSlot
    {
        public LansUILib.ui.LItemSlot summonWeapon;
        public int count;
        public bool fill;

        public SummonSlot(int count = 1, bool fill = false)
        {
            this.summonWeapon = new LItemSlot(LItemSlotType.Item);
            this.count = count;
            this.fill = fill;
        }

        public SummonSlot(LItemSlot itemSlot, int count, bool fill)
        {
            this.summonWeapon = itemSlot;
            this.count = count;
            this.fill = fill;
        }

    }

    public class SummonSlotSerializer : TagSerializer<SummonSlot, TagCompound>
    {
        public override TagCompound Serialize(SummonSlot value) => new TagCompound
        {
            ["item"] = value.summonWeapon,
            ["count"] = value.count,
            ["fill"] = value.fill,
        };

        public override SummonSlot Deserialize(TagCompound tag) => new SummonSlot(tag.Get<LItemSlot>("item"), tag.GetInt("count"), tag.GetBool("fill"));
    }
}
