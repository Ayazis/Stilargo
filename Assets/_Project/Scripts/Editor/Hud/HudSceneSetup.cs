using Stilargo.Editor.World;
using Stilargo.Gameplay.Composition;
using Stilargo.Gameplay.Fortification;
using Stilargo.Gameplay.World;
using Stilargo.UI.Hud;
using UnityEngine;

namespace Stilargo.Editor.Hud
{
    /// <summary>
    /// Adds the HUD to a scene: its installer and view. The Main scene creator calls this so the scene stays
    /// generated from code.
    /// </summary>
    public static class HudSceneSetup
    {
        private const string HolderName = "Hud";
        private const string WorldField = "_world";
        private const string CombatField = "_combat";
        private const string FortificationField = "_fortification";
        private const string ViewField = "_view";
        private const string PanelSettingsField = "_panelSettings";

        public static HudInstaller Create(WorldInstaller world, CombatInstaller combat, FortificationInstaller fortification)
        {
            var holder = new GameObject(HolderName);
            var installer = holder.AddComponent<HudInstaller>();
            SerializedFields.Assign(installer, WorldField, world);
            SerializedFields.Assign(installer, CombatField, combat);
            SerializedFields.Assign(installer, FortificationField, fortification);
            var view = holder.AddComponent<HudView>();
            SerializedFields.Assign(view, PanelSettingsField, HudPanelSettingsAsset.LoadOrCreate());
            SerializedFields.Assign(installer, ViewField, view);
            return installer;
        }
    }
}
