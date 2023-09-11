using blargle.TheMess;
using Kitchen;
using KitchenData;
using KitchenLib.References;
using KitchenMods;
using System;
using System.Collections.Generic;
using System.Linq;
using TheMess.customs;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace TheMess.systems {

    class HandleServingTheMessSystem : DaySystem, IModSystem {

        private EntityQuery groups;
        private EntityQuery menuItemsQuery;

        protected override void Initialise() {
            base.Initialise();
            RequireSingletonForUpdate<STheMessIsActive>();

            groups = GetEntityQuery(new QueryHelper().All(typeof(CPatience), typeof(CCustomerSettings), typeof(CGroupMember), typeof(CGroupAwaitingOrder), typeof(CWaitingForItem), typeof(CAssignedTable)));
            menuItemsQuery = GetEntityQuery(typeof(CMenuItem));
        }

        protected override void OnUpdate() {
            if (GetOrDefault<SGameTime>().IsPaused) {
                return;
            }

            using NativeArray<Entity> entities = groups.ToEntityArray(Allocator.Temp);
            using NativeArray<CPatience> groupPatience = groups.ToComponentDataArray<CPatience>(Allocator.Temp);
            using NativeArray<CCustomerSettings> groupSettings = groups.ToComponentDataArray<CCustomerSettings>(Allocator.Temp);
            using NativeArray<CAssignedTable> tables = groups.ToComponentDataArray<CAssignedTable>(Allocator.Temp);

            for (int entityIndex = 0; entityIndex < entities.Length; entityIndex++) {
                Entity entity = entities[entityIndex];
                CPatience patience = groupPatience[entityIndex];
                CCustomerSettings settings = groupSettings[entityIndex];
                CAssignedTable assignedTable = tables[entityIndex];
                if (!RequireBuffer(entity, out DynamicBuffer<CWaitingForItem> waitingForItemsBuffer) || !RequireBuffer(assignedTable, out DynamicBuffer<CTableSetGrabPoints> grabPoints)) {
                    continue;
                }

                for (int grabPointIndex = 0; grabPointIndex < grabPoints.Length; grabPointIndex++) {
                    CTableSetGrabPoints grabPoint = grabPoints[grabPointIndex];
                    if (!Has<CApplianceTable>(grabPoint) ||
                        !Require(grabPoint, out CItemHolder holder) ||
                        holder.HeldItem == default ||
                        !Require(holder.HeldItem, out CItem candidate) ||
                        !GameData.Main.TryGet(candidate.ID, out Item candidateGDO) ||
                        !IsItemServable(candidate.ID)) {
                        continue;
                    }

                    bool satisfies = false;
                    for (int itemIndex = 0; itemIndex < waitingForItemsBuffer.Length; itemIndex++) {
                        if (waitingForItemsBuffer[itemIndex].Satisfied ||
                            !Require(waitingForItemsBuffer[itemIndex].Item, out CItem order) ||
                            !GameData.Main.TryGet(order.ID, out Item orderGDO) ||
                            orderGDO.ID != Refs.Gun.ID) {
                            continue;
                        }

                        if (ItemSatisfiesOrder(candidate, candidateGDO, order, orderGDO)) {
                            satisfies = true;
                            break;
                        }
                    }

                    if (!satisfies) {
                        continue;
                    }

                    TheMessMod.Log("GUN DELIVERED");
                    if (Require<CPosition>(entity, out CPosition center)) {
                        makePing(center);
                        for (int times = 0; times < 3; times++) {
                            for (int x = -2; x < 2; x++) {
                                for (float z = -2; z < 2; z++) {
                                    Vector3 position = center - new Vector3(x, 0, z);
                                    Entity mess = EntityManager.CreateEntity();
                                    EntityManager.AddComponentData<CPosition>(mess, position);
                                    EntityManager.AddComponentData<CMessRequest>(mess, new CMessRequest() {
                                        ID = AssetReference.CustomerMess
                                    });
                                    CSoundEvent.Create(EntityManager, SoundEvent.MessCreated);
                                }
                            }
                        }
                    } else {
                        TheMessMod.Log("NO POSITION?");
                    }

                    if (Require<CGun>(holder.HeldItem, out CGun gun)) {
                        TheMessMod.Log($"GUN PLACED BY PLAYER {gun.lastHeldByPlayer}");
                        PlayerInfo playerInfo = Players.Main.Get(gun.lastHeldByPlayer);
                        PlayerProfile profile = playerInfo.Profile;
                        while (profile.Cosmetics.Count > 0) {
                            profile.Cosmetics.RemoveAt(0);
                        }
                        profile.Cosmetics.Add(PlayerCosmeticReferences.GhostHat);
                        Players.Main.RequestProfileUpdate(gun.lastHeldByPlayer, profile);
                    }

                    //TODO  kill the player
                    removeDish();
                    Set<CGroupStartLeaving>(entity);
                    Set<CGroupStateChanged>(entity);
                    settings.AddPatience(ref patience, settings.Patience.ItemDeliverBonus);
                    Set(entity, patience);
                    Set<STheMessHasBeenServed>();
                    Clear<STheMessIsActive>();
                    break;
                }
            }
        }

        private bool ItemSatisfiesOrder(CItem candidate, Item candidateGDO, CItem order, Item orderGDO) {
            if (orderGDO.SatisfiedBy.Count == 0) {
                if (candidate.ID == order.ID && (!(candidateGDO is ItemGroup) || !(orderGDO is ItemGroup))) {
                    return true;
                }

                int candidateMatchingComponentCount = 0;
                foreach (int candidateComponent in candidate.Items) {
                    bool matchingComponentFound = false;
                    for (int i = 0; i < order.Items.Count; i++) {
                        int orderComponent = order.Items[i];
                        if (orderComponent == candidateComponent) {
                            order.Items[i] = 0;
                            matchingComponentFound = true;
                            candidateMatchingComponentCount++;
                            break;
                        }
                    }
                    if (!matchingComponentFound && GameData.Main.TryGet(candidateComponent, out Item componentGDO)) {
                        if (!componentGDO.IsMergeableSide) {
                            return false;
                        }
                    }
                }
                return candidateMatchingComponentCount == order.Items.Count;
            }
            return candidate.Satisfies(orderGDO);
        }

        private bool IsItemServable(int itemID) {
            IEnumerable<Item> items = GameData.Main.Get<Dish>().SelectMany(x => x.UnlocksMenuItems).Select(x => x.Item);
            if (items.Select(x => x.ID).Distinct().Contains(itemID))
                return true;
            if (items.SelectMany(x => x.SatisfiedBy).Select(x => x.ID).Distinct().Contains(itemID))
                return true;
            if (items.Select(x => x.AlwaysOrderAdditionalItem).Where(x => x != 0).Distinct().Contains(itemID))
                return true;
            return false;
        }

        private void removeDish() {
            TheMessMod.Log("Attempting to remove dish...");
            using var entities = menuItemsQuery.ToEntityArray(Allocator.TempJob);
            var itemId = Refs.TheMessDish.UnlocksMenuItems.Select(menuItem => menuItem.Item).First().ID;

            for (int i = 0; i < entities.Length; i++) {
                if (Require(entities[i], out CMenuItem menuItem) && (menuItem.Item == itemId)) {
                    TheMessMod.Log("Found gun dish. Zeroing weight?");
                    menuItem.Weight = 0;
                    SetComponent<CMenuItem>(entities[i], menuItem);
                }
            }
        }
        
        private void makePing(CPosition position) {
            var entity = EntityManager.CreateEntity();
            Set(entity, new CRequiresView() { Type = ViewType.Ping });
            Set(entity, new CPosition { Position = position } );
            Set(entity, new CLifetime() { RemainingLife = 0.3f });
            Set(entity, new CPlayerPing() { 
                Colour = new CPlayerColour() { 
                    Color = new Color(175f, 175f, 115f, 0.5f) 
                } 
            });
        }
    }
}
