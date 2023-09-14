using BlargleBrew.cards;
using KitchenLib;
using KitchenMods;
using System.Linq;
using System.Reflection;
using TheMess.customs;
using UnityEngine;

namespace blargle.TheMess {

    public class TheMessMod : BaseMod {

        public const string MOD_ID = "blargle.TheMess";
        public const string MOD_NAME = "TheMess";
        public const string MOD_VERSION = "0.0.1";
        public const string MOD_AUTHOR = "blargle";

        public static AssetBundle bundle;

        public TheMessMod() : base(MOD_ID, MOD_NAME, MOD_AUTHOR, MOD_VERSION, ">=1.1.7", Assembly.GetExecutingAssembly()) { }

        protected override void OnPostActivate(Mod mod) {
            Log($"v{MOD_VERSION} initialized");
            Log($"Loading asset bundle...");
            bundle = mod.GetPacks<AssetBundleModPack>().SelectMany(e => e.AssetBundles).First();
            Log($"Asset bundle loaded.");

            AddGameDataObject<Gun>();
            AddGameDataObject<GunProvider>();
            AddGameDataObject<TheMessDish>();
        }

        public static void Log(object message) {
            Debug.Log($"[{MOD_ID}] {message}");
        }
    }
}