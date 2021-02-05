using TShockAPI;
using TerrariaUI.Base;
using Terraria;

namespace TUIPlugin
{
    public static class Extensions
    {
        public static TSPlayer Player(this Touch touch) =>
            TShock.Players[touch.PlayerIndex];

        public static void Firework(this TSPlayer player, int count = 1)
        {
            int type = 170; // yellow
            float dx = 16 * 15f;
            float beginX = 0;
            if (count > 1)
                beginX = -(dx / 2f) * (count - 1);
            float dy = -16 * 4f;
            for (int i = 0; i < count; i++)
            {
                int p = Projectile.NewProjectile(player.TPlayer.position.X + beginX + i * dx,
                    player.TPlayer.position.Y + dy, 0f, -8f, type, 0, (float)0);
                Main.projectile[p].Kill();
            }
        }
    }
}
