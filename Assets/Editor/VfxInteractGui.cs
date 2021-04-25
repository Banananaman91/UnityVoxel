using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;
using VoxelTerrain.Interactions;
using VoxelTerrain.Voxel;

namespace Editor
{
    [CustomEditor(typeof(ScriptableVfxInteract))]
    public class VfxInteractGui : UnityEditor.Editor
    {
        private VoxelType _voxType = VoxelType.Default;
        public override void OnInspectorGUI()
        {
            var vi = (ScriptableVfxInteract) target;

            if (vi.VFXInteraction == null) vi.VFXInteraction = new VfxInteraction();

            if (vi.VFXInteraction.Vfx == null || vi.VFXInteraction.Vfx.Length == 0)
            {
                vi.VFXInteraction.Vfx = new VisualEffect[10];
            }
            
            EditorGUI.BeginChangeCheck();

            #region Shape
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Select Shape Type", GUILayout.MinWidth(80f), GUILayout.MaxWidth(120f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            //Button to select single shape type
            if (GUILayout.Button("Single"))
            {
                vi.VFXInteraction.Shape = FlattenShape.Single;
            }

            //Button to select square shape type
            if (GUILayout.Button("Square"))
            {
                vi.VFXInteraction.Shape = FlattenShape.Square;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            //Button to select circle shape type
            if (GUILayout.Button("Circle"))
            {
                vi.VFXInteraction.Shape = FlattenShape.Circular;
            }

            //Button to select sphere shape type
            if (GUILayout.Button("Sphere"))
            {
                vi.VFXInteraction.Shape = FlattenShape.Sphere;
            }

            EditorGUILayout.EndHorizontal();
            #endregion

            #region VoxelType
             EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Select Voxel Type", GUILayout.MinWidth(80f), GUILayout.MaxWidth(120f));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Destroy", GUILayout.MinWidth(30f), GUILayout.MaxWidth(60f)))
            {
                _voxType = VoxelType.Default;
            }

            if (GUILayout.Button("Grass", GUILayout.MinWidth(30f), GUILayout.MaxWidth(60f)))
            {
                _voxType = VoxelType.Grass;
            }
            
            if (GUILayout.Button("Dirt", GUILayout.MinWidth(30f), GUILayout.MaxWidth(60f)))
            {
                _voxType = VoxelType.Dirt;
            }
            
            if (GUILayout.Button("Stone", GUILayout.MinWidth(30f), GUILayout.MaxWidth(60f)))
            {
                _voxType = VoxelType.Stone;
            }

            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Sand", GUILayout.MinWidth(30f), GUILayout.MaxWidth(60f)))
            {
                _voxType = VoxelType.Sand;
            }

            if (GUILayout.Button("Snow", GUILayout.MinWidth(30f), GUILayout.MaxWidth(60f)))
            {
                _voxType = VoxelType.Snow;
            }
            
            if (GUILayout.Button("Water", GUILayout.MinWidth(30f), GUILayout.MaxWidth(60f)))
            {
                _voxType = VoxelType.Water;
            }
            
            if (GUILayout.Button("Forest", GUILayout.MinWidth(30f), GUILayout.MaxWidth(60f)))
            {
                _voxType = VoxelType.Forest;
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Beach", GUILayout.MinWidth(30f), GUILayout.MaxWidth(60f)))
            {
                _voxType = VoxelType.Beach;
            }
            
            if (GUILayout.Button("Plains", GUILayout.MinWidth(30f), GUILayout.MaxWidth(60f)))
            {
                _voxType = VoxelType.Plains;
            }
            
            EditorGUILayout.EndHorizontal();
            #endregion

            EditorGUILayout.BeginHorizontal();
            //Set VFX field. Leave this outside as only one VFX is needed
            EditorGUILayout.LabelField("VFX", GUILayout.MinWidth(30f), GUILayout.MaxWidth(60f));
            vi.VFXInteraction.Vfx[(int) _voxType] = (VisualEffect) EditorGUILayout.ObjectField(vi.VFXInteraction.Vfx[(int) _voxType], typeof(VisualEffect), true,
                GUILayout.MinWidth(100f), GUILayout.MaxWidth(150f));
                    
            EditorGUILayout.EndHorizontal();

            switch (vi.VFXInteraction.Shape)
            {
                case FlattenShape.Default:
                    EditorGUILayout.HelpBox("Select a shape", MessageType.Warning);
                    break;
                case FlattenShape.Single:
                    break;
                case FlattenShape.Square:
                    #region Square
                    //New Line
                    EditorGUILayout.BeginHorizontal();
                    //Set spawn rate field label
                    EditorGUILayout.LabelField("SpawnRate", GUILayout.MinWidth(60f), GUILayout.MaxWidth(100f));
                    //spawnrate string text field, sets the variable
                    vi.VFXInteraction.SpawnRateStringId = EditorGUILayout.TextField(vi.VFXInteraction.SpawnRateStringId, GUILayout.Width(100f));
                    //spawn rate int field, sets the variable
                    vi.VFXInteraction.SpawnRate = EditorGUILayout.IntField(vi.VFXInteraction.SpawnRate, GUILayout.Width(60f));

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    //Set spawn point ID
                    EditorGUILayout.LabelField("Spawn Point ID", GUILayout.MinWidth(60f), GUILayout.MaxWidth(100f));
                    vi.VFXInteraction.SpawnPointStringId = EditorGUILayout.TextField(vi.VFXInteraction.SpawnPointStringId, GUILayout.Width(100f));

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    //Set particle life ID and value
                    EditorGUILayout.LabelField("Particle Life", GUILayout.MinWidth(60f), GUILayout.MaxWidth(100f));
                    vi.VFXInteraction.ParticleLifeStringId =
                        EditorGUILayout.TextField(vi.VFXInteraction.ParticleLifeStringId, GUILayout.Width(100f));
                    vi.VFXInteraction.ParticleLife = EditorGUILayout.FloatField(vi.VFXInteraction.ParticleLife, GUILayout.Width(60f));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    //Set box radius ID and value
                    EditorGUILayout.LabelField("Box Radius", GUILayout.MinWidth(60f), GUILayout.MaxWidth(100f));
                    vi.VFXInteraction.BoxRadiusXStringId = EditorGUILayout.TextField(vi.VFXInteraction.BoxRadiusXStringId, GUILayout.Width(100f));
                    vi.VFXInteraction.BoxRadiusX = EditorGUILayout.FloatField(vi.VFXInteraction.BoxRadiusX, GUILayout.Width(60f));
                    EditorGUILayout.EndHorizontal();
                    
                    #endregion
                    break;
                case FlattenShape.Circular:
                    break;
                case FlattenShape.Sphere:

                    #region Sphere

                    //New Line
                    EditorGUILayout.BeginHorizontal();
                    //Set spawn rate field label
                    EditorGUILayout.LabelField("SpawnRate", GUILayout.MinWidth(60f), GUILayout.MaxWidth(100f));
                    //spawnrate string text field, sets the variable
                    vi.VFXInteraction.SpawnRateStringId = EditorGUILayout.TextField(vi.VFXInteraction.SpawnRateStringId, GUILayout.Width(100f));
                    //spawn rate int field, sets the variable
                    vi.VFXInteraction.SpawnRate = EditorGUILayout.IntField(vi.VFXInteraction.SpawnRate, GUILayout.Width(60f));

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    //Set spawn point ID
                    EditorGUILayout.LabelField("Spawn Point ID", GUILayout.MinWidth(60f), GUILayout.MaxWidth(100f));
                    vi.VFXInteraction.SpawnPointStringId = EditorGUILayout.TextField(vi.VFXInteraction.SpawnPointStringId, GUILayout.Width(100f));

                    EditorGUILayout.EndHorizontal();
                    //Set particle life ID and value
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Particle Life", GUILayout.MinWidth(60f), GUILayout.MaxWidth(100f));
                    vi.VFXInteraction.ParticleLifeStringId =
                        EditorGUILayout.TextField(vi.VFXInteraction.ParticleLifeStringId, GUILayout.Width(100f));
                    vi.VFXInteraction.ParticleLife = EditorGUILayout.FloatField(vi.VFXInteraction.ParticleLife, GUILayout.Width(60f));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    //Set sphere radius ID and value
                    EditorGUILayout.LabelField("Sphere Radius", GUILayout.MinWidth(60f), GUILayout.MaxWidth(100f));
                    vi.VFXInteraction.SphereRadiusStringId =
                        EditorGUILayout.TextField(vi.VFXInteraction.SphereRadiusStringId, GUILayout.Width(100f));
                    vi.VFXInteraction.SphereRadius = EditorGUILayout.FloatField(vi.VFXInteraction.SphereRadius, GUILayout.Width(60f));
                    EditorGUILayout.EndHorizontal();

                    #endregion

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //If changes were made, set object as dirty.
            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(vi);

        }
    }
}
