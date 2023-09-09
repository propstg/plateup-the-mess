using blargle.TheMess;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.Utils;
using UnityEngine;

namespace TheMess.customs {

    public class Gun : CustomItem {

        public override string UniqueNameID => "TheMess - Gun";
        public override bool IsIndisposable => true;
        public override GameObject Prefab => TheMessMod.bundle.LoadAsset<GameObject>("Gun");
        public override string ColourBlindTag => "GUN";
        public override Appliance DedicatedProvider => Refs.GunProvider;
        public override bool IsConsumedByCustomer => false;
        public override int MaxOrderSharers => 4;
        public override Item DirtiesTo => Refs.Gun;

        public override void OnRegister(Item item) {
            MaterialUtils.ApplyMaterial(Prefab, "Gun", CommonMaterials.metalBlack);
        }
    }
}
