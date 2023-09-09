using KitchenLib.Utils;
using UnityEngine;

namespace blargle.TheMess {

    class CommonMaterials {
        private static Material[] wrap(Material material) => new Material[] { material };

        public static Material[] metalBlack => wrap(MaterialUtils.GetExistingMaterial("Metal Black"));
    }
}
