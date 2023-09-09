using blargle.TheMess;
using Kitchen;
using KitchenMods;
using System.Linq;
using TheMess.customs;
using Unity.Entities;

namespace TheMess.systems {

    public class DeactivateWhenProviderNotPresentSystem : StartOfNightSystem, IModSystem {

        private EntityQuery gunProviderQuery;
        private EntityQuery gunQuery;
        private EntityQuery menuItemsQuery;

        protected override void Initialise() {
            base.Initialise();
            RequireSingletonForUpdate<SLayout>();
            RequireSingletonForUpdate<STheMessIsActive>();
            //TODO set this somewhere else RequireSingletonForUpdate<STheMessHasBeenServed>();

            gunProviderQuery = GetEntityQuery(new QueryHelper().All(typeof(CGunProvider)));
            gunQuery = GetEntityQuery(new QueryHelper().All(typeof(CGun)));
            menuItemsQuery = GetEntityQuery(typeof(CMenuItem));
        }

        protected override void OnUpdate() {
            TheMessMod.Log("Deactivate OnUpdate");

            removeDish();
            removeGunProviders();
            removeGuns();

            Clear<STheMessHasBeenServed>();
            Clear<STheMessIsActive>();
        }

        private void removeDish() {
            TheMessMod.Log("Attempting to remove dish...");
            var entities = menuItemsQuery.ToEntityArray(Unity.Collections.Allocator.TempJob);
            var itemId = Refs.TheMessDish.UnlocksMenuItems.Select(menuItem => menuItem.Item).First().ID;

            for (int i = 0; i < entities.Length; i++) {
                if (Require(entities[i], out CMenuItem menuItem) && (menuItem.Item == itemId)) {
                    TheMessMod.Log("Found gun dish. Removing?");
                    EntityManager.DestroyEntity(entities[i]);
                }
            }

            entities.Dispose();
        }

        private void removeGunProviders() {
            TheMessMod.Log("Attempting to remove gun providers");
            var entities = gunProviderQuery.ToEntityArray(Unity.Collections.Allocator.TempJob);

            for (int i = 0; i < entities.Length; i++) {
                TheMessMod.Log("Destroying gun provider");
                EntityManager.DestroyEntity(entities[i]);
            }

            entities.Dispose();
        }

        private void removeGuns() {
            TheMessMod.Log("Attempting to remove guns");
            var entities = gunQuery.ToEntityArray(Unity.Collections.Allocator.TempJob);

            for (int i = 0; i < entities.Length; i++) {
                TheMessMod.Log("Destroying gun");
                EntityManager.DestroyEntity(entities[i]);
            }

            entities.Dispose();
        }
    }
}
