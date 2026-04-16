using UnityEngine;

namespace UI.StateSwitcher
{
    [System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
    public class MeshMaterialSwitch : StateItem
    {
        [SerializeField] private MeshRenderer mesh;
        [SerializeField] private Material material;

        private Material defaultMaterial;

        public override void Set()
        {
            if(mesh == null) return;
            if (defaultMaterial == null)
                defaultMaterial = mesh.material;
            mesh.material = material;
        }

        public override void DefaultState()
        {
            if (defaultMaterial == null)
                return;
            mesh.material = defaultMaterial;
        }

#if UNITY_EDITOR
        public override StateItem Clone()
        {
            return new MeshMaterialSwitch()
            {
                material = material,
                mesh = mesh
            };
        }

        public override string DropDownItemName => "MeshMaterial";
        public override string DropDownGroupName => "MeshMaterial Switch";
#endif
    }
}