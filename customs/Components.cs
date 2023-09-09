﻿using KitchenData;
using KitchenMods;
using Unity.Entities;

namespace TheMess.customs {

    public struct CGun : IItemProperty, IModComponent { }
    public struct CGunProvider : IApplianceProperty, IModComponent { }
    public struct CGunProviderRemoveAtEndOfDay : IApplianceProperty, IModComponent { }
    public struct STheMessHasBeenServed : IComponentData, IModComponent { }
    public struct STheMessIsActive : IComponentData, IModComponent { }
}
