using blargle.TheMess;
using Kitchen;
using KitchenMods;
using System.Linq;
using TheMess.customs;
using Unity.Entities;

namespace TheMess.systems {

    public class ActivateWhenProviderPresentSystem : StartOfDaySystem, IModSystem {

        private EntityQuery gunProviderQuery;
        private EntityQuery menuItemQuery;

        protected override void Initialise() {
            base.Initialise();
            RequireSingletonForUpdate<SLayout>();

            gunProviderQuery = GetEntityQuery(typeof(CGunProvider));
            menuItemQuery = GetEntityQuery(typeof(CMenuItem));
        }

        protected override void OnUpdate() {
            TheMessMod.Log("Activate onupdate");
            using var menuItemEntities = menuItemQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
            using var entities = gunProviderQuery.ToEntityArray(Unity.Collections.Allocator.TempJob);

            var item = Refs.TheMessDish.UnlocksMenuItems.First();

            if (entities.Length >= 1 && !Has<STheMessIsActive>()) {
                TheMessMod.Log("Attempting to add dish...");
                Entity entity = EntityManager.CreateEntity((ComponentType) typeof (CMenuItem), (ComponentType) typeof (CAvailableIngredient));
                EntityManager.AddComponentData<CMenuItem>(entity, new CMenuItem() {
                    Item = item.Item.ID,
                    Weight = item.Weight,
                    Phase = item.Phase,
                    SourceDish = Refs.TheMessDish.ID,
                });
                EntityManager.AddComponent<CMenuItemMain>(entity);
                Set<STheMessIsActive>();
            }
        }
    }
}
