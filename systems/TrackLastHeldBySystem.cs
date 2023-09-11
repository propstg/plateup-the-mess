using Kitchen;
using KitchenMods;
using TheMess.customs;
using Unity.Collections;
using Unity.Entities;

namespace TheMess.systems {

    class TrackLastHeldBySystem : DaySystem, IModSystem {

        private EntityQuery gunsQuery;

        protected override void Initialise() {
            base.Initialise();
            RequireSingletonForUpdate<STheMessIsActive>();

            gunsQuery = GetEntityQuery(typeof(CGun));
        }

        protected override void OnUpdate() {
            using NativeArray<Entity> entities = gunsQuery.ToEntityArray(Allocator.Temp);
            using NativeArray<CGun> guns = gunsQuery.ToComponentDataArray<CGun>(Allocator.Temp);

            for (int i = 0; i < entities.Length; i++) {
                Entity entity = entities[i];
                CGun gun = guns[i];

                if (Require<CHeldBy>(entity, out CHeldBy heldBy) && heldBy.Holder != default && Require<CPlayer>(heldBy.Holder, out CPlayer player)) {
                    gun.lastHeldByPlayer = player.ID;
                    SetComponent<CGun>(entity, gun);
                }
            }
        }
    }
}
