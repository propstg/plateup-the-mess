using blargle.TheMess;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace BlargleBrew.cards {

    public class TheMessDish : CustomDish {
#if DEBUG
        public static float WEIGHT = 10f;
        public override bool IsAvailableAsLobbyOption => true;
#else
        public static float WEIGHT = 0.1f;
        public override bool IsAvailableAsLobbyOption => false;
#endif

        public override string UniqueNameID => "TheMess - TheMess";
        public override DishType Type => DishType.Base;
        public override GameObject DisplayPrefab => TheMessMod.bundle.LoadAsset<GameObject>("Gun");
        public override GameObject IconPrefab => TheMessMod.bundle.LoadAsset<GameObject>("Gun");
        public override bool IsUnlockable => false;

        public override bool RequiredNoDishItem => true;

        public override HashSet<Item> MinimumIngredients => new HashSet<Item> {
            Refs.Gun,
        };

        public override List<Dish.MenuItem> ResultingMenuItems => new List<Dish.MenuItem>() {
            new Dish.MenuItem() { Phase = MenuPhase.Main, Item = Refs.Gun, Weight = WEIGHT },
        };

        public override Dictionary<Locale, string> Recipe => new Dictionary<Locale, string> {
            { Locale.English, "" }
        };

        public override List<(Locale, UnlockInfo)> InfoList => new List<(Locale, UnlockInfo)> {
            { (Locale.English, LocalisationUtils.CreateUnlockInfo("The Mess", "", "") )}
        };

        public override void SetupIconPrefab(GameObject prefab) {
            setupCommonDisplayPrefab(prefab);
        }

        public override void SetupDisplayPrefab(GameObject prefab) {
            setupCommonDisplayPrefab(prefab);
        }
        
        private void setupCommonDisplayPrefab(GameObject prefab) {
            MaterialUtils.ApplyMaterial(prefab, "Gun", CommonMaterials.metalBlack);
        }
    }
}