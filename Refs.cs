using BlargleBrew.cards;
using KitchenData;
using KitchenLib.Utils;
using TheMess.customs;

namespace blargle.TheMess {

    public class Refs {

        public static Item Gun => GDOUtils.GetCastedGDO<Item, Gun>();
        public static Appliance GunProvider => GDOUtils.GetCastedGDO<Appliance, GunProvider>();
        public static Dish TheMessDish => GDOUtils.GetCastedGDO<Dish, TheMessDish>();
    }
}