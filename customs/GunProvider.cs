using ApplianceLib.Api.Prefab;
using blargle.TheMess;
using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace TheMess.customs {

    class GunProvider : CustomAppliance {

        public override string UniqueNameID => "TheMess Gun Provider";
        public override PriceTier PriceTier => PriceTier.Free;
        public override bool IsPurchasable => true;
        public override ShoppingTags ShoppingTags => ShoppingTags.FrontOfHouse;
        public override GameObject Prefab => TheMessMod.bundle.LoadAsset<GameObject>("GunProvider");
        public override List<(Locale, ApplianceInfo)> InfoList => new List<(Locale, ApplianceInfo)> {
            (Locale.English, new ApplianceInfo() {
                Name = "Gun",
                Description = "Provides a gun"
            })
        };
        public override List<IApplianceProperty> Properties => new List<IApplianceProperty> {
            new CItemHolder(),
            new CGunProvider(),
            KitchenPropertiesUtils.GetCItemProvider(Refs.Gun.ID, 1, 1, false, false, true, false, false, true, false)
        };

        public override void SetupPrefab(GameObject prefab) {
            prefab.AttachCounter(CounterType.DoubleDoors);
            var holdTransform = prefab.GetChild("HoldPoint").transform;

            MaterialUtils.ApplyMaterial(Prefab, "HoldPoint/Gun", CommonMaterials.metalBlack);

            var holdPoint = prefab.AddComponent<HoldPointContainer>();
            holdPoint.HoldPoint = holdTransform;
            var sourceView = prefab.AddComponent<LimitedItemSourceView>();
            sourceView.HeldItemPosition = holdTransform;
            ReflectionUtils.GetField<LimitedItemSourceView>("Items").SetValue(sourceView, new List<GameObject>() {
                GameObjectUtils.GetChildObject(prefab, "HoldPoint/Gun")
            });
        }
    }
}
