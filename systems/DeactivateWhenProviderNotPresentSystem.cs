using blargle.TheMess;
using Kitchen;
using KitchenMods;
using System;
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
            RequireSingletonForUpdate<STheMessHasBeenServed>();

            gunProviderQuery = GetEntityQuery(typeof(CGunProvider));
            gunQuery = GetEntityQuery(typeof(CGun));
            menuItemsQuery = GetEntityQuery(typeof(CMenuItem));
        }

        protected override void OnUpdate() {
            try {
                TheMessMod.Log("Deactivate OnUpdate");

                removeDish();
                removeGunProviders();
                removeGuns();

                Clear<STheMessHasBeenServed>();
                Clear<STheMessIsActive>();
                TheMessMod.Log("done in deactivate onupdate");
            } catch (Exception e) {
                TheMessMod.Log("caught exception?");
                TheMessMod.Log(e);
            }
        }

        private void removeDish() {
            TheMessMod.Log("Attempting to remove dish...");
            using var entities = menuItemsQuery.ToEntityArray(Unity.Collections.Allocator.TempJob);
            var itemId = Refs.TheMessDish.UnlocksMenuItems.Select(menuItem => menuItem.Item).First().ID;

            for (int i = 0; i < entities.Length; i++) {
                if (Require(entities[i], out CMenuItem menuItem) && (menuItem.Item == itemId)) {
                    TheMessMod.Log("Found dish. Removing");
                    EntityManager.DestroyEntity(entities[i]);
                }
            }
        }

        private void removeGunProviders() {
            TheMessMod.Log("Attempting to remove gun providers");
            using var entities = gunProviderQuery.ToEntityArray(Unity.Collections.Allocator.TempJob);

            for (int i = 0; i < entities.Length; i++) {
                TheMessMod.Log("Destroying gun provider");
                EntityManager.DestroyEntity(entities[i]);
            }
        }

        private void removeGuns() {
            TheMessMod.Log("Attempting to remove guns");
            using var entities = gunQuery.ToEntityArray(Unity.Collections.Allocator.TempJob);

            for (int i = 0; i < entities.Length; i++) {
                TheMessMod.Log("Destroying gun");
                EntityManager.DestroyEntity(entities[i]);
            }
        }
    }
}
