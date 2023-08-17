using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace LansAutoSummon
{
    public class ModGlobalProjectile: GlobalProjectile
    {
        public override bool PreAI(Projectile projectile)
        {
            if(projectile.sentry)
            {
                LansAutoSummon.inst.soundIsSentry = true;
            }


            return base.PreAI(projectile);
        }

        public override void PostAI(Projectile projectile)
        {
            LansAutoSummon.inst.soundIsSentry = false;
            base.PostAI(projectile);
        }

        /*public override bool PreKill(Projectile projectile, int timeLeft)
        {
            if (projectile.sentry)
            {
                LansAutoSummon.inst.soundIsSentry = true;
            }

            return base.PreKill(projectile, timeLeft);
        }*/

    }
}
