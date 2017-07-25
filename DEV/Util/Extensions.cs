using PoncheToolkit.Core;
using PoncheToolkit.Graphics2D;
using PoncheToolkit.Graphics3D;
using PoncheToolkit.Graphics3D.Effects;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Util
{
    /// <summary>
    ///  Class that contain all the extensions for the project.
    /// </summary>
    public static class Extensions
    {
        #region To SharpDX

        #region Vector
        /// <summary>
        /// Convert an <see cref="Assimp.Vector3D"/> object to <see cref="SharpDX.Vector3"/>.
        /// </summary>
        /// <param name="vec">The vector.</param>
        /// <returns></returns>
        public static Vector3 ConvertToLocal(Assimp.Vector3D vec)
        {
            Vector3 result = new Vector3(vec.X, vec.Y, vec.Z);
            return result;
        }

        /// <summary>
        /// Convert a list of <see cref="Assimp.Vector3D"/> objects to <see cref="SharpDX.Vector3"/>.
        /// </summary>
        /// <param name="vectors">The list of vectors.</param>
        /// <returns></returns>
        public static List<Vector3> ConvertToLocal(List<Assimp.Vector3D> vectors)
        {
            List<Vector3> result = new List<Vector3>();
            for (int i =0; i<vectors.Count; i++)
            {
                result.Add(ConvertToLocal(vectors[i]));
            }
            
            return result;
        }
        #endregion

        #region Matrices
        /// <summary>
        /// Convert an <see cref="Assimp.Matrix4x4"/> object to <see cref="SharpDX.Matrix"/>.
        /// </summary>
        /// <param name="matrix">The vector to convert.</param>
        /// <returns></returns>
        public static Matrix ConvertToLocal(this Assimp.Matrix4x4 matrix)
        {
            Matrix result = new Matrix(matrix.A1, matrix.A2, matrix.A3, matrix.A4, matrix.B1, matrix.B2, matrix.B3, matrix.B4, matrix.C1, matrix.C2, matrix.C3, matrix.C4, matrix.D1, matrix.D2, matrix.D3, matrix.D4);
            return result;
        }

        /// <summary>
        /// Convert a list of <see cref="Assimp.Matrix4x4"/> objects to <see cref="SharpDX.Matrix"/>.
        /// </summary>
        /// <param name="matrices">The list of <see cref="Assimp.Matrix4x4"/> matrices.</param>
        /// <returns></returns>
        public static List<Matrix> ConvertToLocal(this List<Assimp.Matrix4x4> matrices)
        {
            List<Matrix> result = new List<Matrix>();
            for (int i = 0; i < matrices.Count; i++)
            {
                result.Add(matrices[i].ConvertToLocal());
            }

            return result;
        }
        #endregion

        #region Color
        /// <summary>
        /// Convert an <see cref="Assimp.Vector3D"/> object to <see cref="SharpDX.Color"/>.
        /// </summary>
        /// <param name="color">The Color to convert.</param>
        /// <returns></returns>
        public static Color ConvertToLocalColor(this Assimp.Color4D color)
        {
            SharpDX.Color result = new SharpDX.Color(color.R, color.G, color.B, color.A);
            return result;
        }

        /// <summary>
        /// Convert an <see cref="Assimp.Vector3D"/> object to <see cref="SharpDX.Color"/>.
        /// </summary>
        /// <param name="colors">The Color to convert.</param>
        /// <returns></returns>
        public static List<Color> ConvertToLocalColor(this List<Assimp.Color4D> colors)
        {
            List<Color> result = new List<Color>();
            for (int i = 0; i < colors.Count; i++)
            {
                result.Add(colors[i].ConvertToLocalColor());
            }

            return result;
        }

        /// <summary>
        /// Convert an <see cref="Assimp.Vector3D"/> object to <see cref="SharpDX.Color"/>.
        /// </summary>
        /// <param name="color">The Color to convert.</param>
        /// <returns></returns>
        public static Vector4 ConvertToLocalVector(this Assimp.Color4D color)
        {
            SharpDX.Vector4 result = new SharpDX.Vector4(color.R, color.G, color.B, color.A);
            return result;
        }

        /// <summary>
        /// Convert a list of <see cref="Assimp.Color4D"/> objects to <see cref="SharpDX.Color"/>.
        /// </summary>
        /// <param name="colors">The list of vectors.</param>
        /// <returns></returns>
        public static List<Vector4> ConvertToLocalVector(this List<Assimp.Color4D> colors)
        {
            List<Vector4> result = new List<Vector4>();
            for (int i = 0; i < colors.Count; i++)
            {
                result.Add(colors[i].ConvertToLocalVector());
            }

            return result;
        }
        #endregion

        #endregion

        #region To PoncheToolkit

        #region Bone
        /// <summary>
        /// Convert an <see cref="Assimp.Bone"/> object to <see cref="PoncheToolkit.Graphics3D.PTBone"/>.
        /// </summary>
        /// <param name="bone">The bone.</param>
        /// <returns></returns>
        public static PTBone ConvertToLocal(this Assimp.Bone bone)
        {
            PTBone result = new PTBone();
            result.OffsetMatrix = bone.OffsetMatrix.ConvertToLocal();
            result.VertexWeights = bone.VertexWeights.ConvertToLocal();
            result.Name = bone.Name;
            return result;
        }

        /// <summary>
        /// Convert a list of <see cref="Assimp.Bone"/> objects to <see cref="PoncheToolkit.Graphics3D.PTBone"/>.
        /// </summary>
        /// <param name="bones">The list of bones.</param>
        /// <returns></returns>
        public static List<PTBone> ConvertToLocal(this List<Assimp.Bone> bones)
        {
            List<PTBone> result = new List<PTBone>();
            for (int i = 0; i < bones.Count; i++)
            {
                result.Add(bones[i].ConvertToLocal());
            }

            return result;
        }
        #endregion

        #region Mesh
        /// <summary>
        /// Convert an <see cref="Assimp.Mesh"/> object to <see cref="PoncheToolkit.Graphics3D.PTMesh"/>.
        /// </summary>
        /// <param name="assimpMesh">The MeshAnimationAttachment.</param>
        /// <param name="model">The model to which the mesh is part of.</param>
        /// <returns></returns>
        public static PTMesh ReadMesh(this Assimp.Mesh assimpMesh, ref PTModel model)
        {
            PTMesh mesh = new PTMesh(model);
            mesh.BiTangents = ConvertToLocal(assimpMesh.BiTangents);
            mesh.Bones = assimpMesh.Bones.ConvertToLocal();
            mesh.Faces = assimpMesh.Faces.ConvertToLocal(ref mesh);
            mesh.MaterialIndex = assimpMesh.MaterialIndex;
            mesh.MeshAnimationAttachments = assimpMesh.MeshAnimationAttachments.ConvertToLocal();
            mesh.Name = assimpMesh.Name;
            mesh.Normals = ConvertToLocal(assimpMesh.Normals);
            mesh.PrimitiveType = (AssimpPrimitiveType)assimpMesh.PrimitiveType;
            mesh.Tangents = ConvertToLocal(assimpMesh.Tangents);

            List<Vector3> verticesPosition = ConvertToLocal(assimpMesh.Vertices);
            for (int i = 0; i < assimpMesh.TextureCoordinateChannels.Length; i++)
                mesh.TextureCoordinateChannels[i] = ConvertToLocal(assimpMesh.TextureCoordinateChannels[i]);

            for (int i = 0; i < assimpMesh.VertexColorChannels.Length; i++)
                mesh.VertexColorChannels[i] = assimpMesh.VertexColorChannels[i].ConvertToLocalColor();

            // Set the vertex, normals and texCoords.
            for (int i = 0; i < verticesPosition.Count; i++)
                mesh.AddVertex(new VertexMainStruct(verticesPosition[i], mesh.Normals[i], mesh.TextureCoordinateChannels[0][i]));

            return mesh;
        }

        /// <summary>
        /// Convert a list of <see cref="Assimp.Mesh"/> objects to <see cref="PoncheToolkit.Graphics3D.PTMesh"/>.
        /// It adds all the read <see cref="PTMesh"/> meshes to the <see cref="PTModel.Meshes"/> List.
        /// </summary>
        /// <param name="assimpMeshes">The list of meshAnimations.</param>
        /// <param name="model">The model to which the meshes are part of.</param>
        /// <param name="scene">The assimp scene.</param>
        /// <returns></returns>
        public static void ReadMeshes(this List<Assimp.Mesh> assimpMeshes, ref PTModel model, ref Assimp.Scene scene)
        {
            // Read the materials.
            List<PTMaterial> materials = scene.Materials.ReadMaterials(ref model);
            model.ImportedMaterials.AddRange(materials);

            // Read the meshes
            for (int i = 0; i < assimpMeshes.Count; i++)
            {
                PTMesh mesh = assimpMeshes[i].ReadMesh(ref model);
                model.AddMesh(mesh);

                for (int j = 0; j<materials.Count; j++)
                {
                    if (mesh.MaterialIndex == j)
                        mesh.MaterialName = materials[j].Name;
                }
            }
        }
        #endregion

        #region Face
        /// <summary>
        /// Convert an <see cref="Assimp.Face"/> object to <see cref="PoncheToolkit.Graphics3D.PTFace"/>.
        /// </summary>
        /// <param name="face">The face.</param>
        /// /// <param name="mesh">The mesh that this face is part of.</param>
        /// <returns></returns>
        public static PTFace ConvertToLocal(this Assimp.Face face, ref PTMesh mesh)
        {
            PTFace result = new PTFace(face.Indices);
            mesh.IndicesCount += result.Indices.Count;
            return result;
        }

        /// <summary>
        /// Convert a list of <see cref="Assimp.Vector3D"/> objects to <see cref="PoncheToolkit.Graphics3D.PTFace"/>.
        /// </summary>
        /// <param name="faces">The list of faces.</param>
        /// <param name="mesh">The mesh that this face is part of.</param>
        /// <returns></returns>
        public static List<PTFace> ConvertToLocal(this List<Assimp.Face> faces, ref PTMesh mesh)
        {
            List<PTFace> result = new List<PTFace>();
            for (int i = 0; i < faces.Count; i++)
            {
                PTFace localFace = faces[i].ConvertToLocal(ref mesh);
                result.Add(localFace);
            }

            return result;
        }
        #endregion

        #region Material Texture
        /// <summary>
        /// Convert an <see cref="Assimp.TextureSlot"/> object to <see cref="PoncheToolkit.Graphics3D.MaterialTexture"/>.
        /// </summary>
        /// <param name="textureSlot">The textureSlot.</param>
        /// /// <param name="model">The model that this face is part of.</param>
        /// <returns></returns>
        internal static MaterialTexture ConvertToLocal(this Assimp.TextureSlot textureSlot, ref PTModel model)
        {
            MaterialTexture result = new MaterialTexture();
            result.BlendFactor = textureSlot.BlendFactor;
            result.FilePath = textureSlot.FilePath;
            result.Flags = textureSlot.Flags;
            result.Mapping = (AssimpTextureMapping)textureSlot.Mapping;
            result.Operation = (assimpTextureOperation)textureSlot.Operation;
            result.TextureIndex = textureSlot.TextureIndex;
            result.TextureType = (AssimpTextureType)textureSlot.TextureType;
            result.UVIndex = textureSlot.UVIndex;
            result.WrapModeU = (AssimpTextureWrapMode)textureSlot.WrapModeU;
            result.WrapModeV = (AssimpTextureWrapMode)textureSlot.WrapModeV;
            return result;
        }

        /// <summary>
        /// Convert a list of <see cref="Assimp.TextureSlot"/> objects to <see cref="PoncheToolkit.Graphics3D.MaterialTexture"/>.
        /// </summary>
        /// <param name="textureSlots">The list of textureSlots.</param>
        /// <param name="model">The model that this textureSlot is part of.</param>
        /// <returns></returns>
        internal static List<MaterialTexture> ConvertToLocal(this List<Assimp.TextureSlot> textureSlots, ref PTModel model)
        {
            List<MaterialTexture> result = new List<MaterialTexture>();
            for (int i = 0; i < textureSlots.Count; i++)
            {
                result.Add(textureSlots[i].ConvertToLocal(ref model));
            }

            return result;
        }
        #endregion

        #region Material
        /// <summary>
        /// Convert an <see cref="Assimp.Material"/> object to <see cref="PoncheToolkit.Graphics3D.AssimpMaterial"/>.
        /// </summary>
        /// <param name="material">The face.</param>
        /// <param name="model">The model that this Material is part of.</param>
        /// <returns></returns>
        internal static PTMaterial ConvertToLocal(this Assimp.Material material, ref PTModel model)
        {
            PTMaterial result = new PTMaterial(model.Game);
            //result.BlendMode = (AssimpBlendMode)material.BlendMode;
            //result.BumpScaling = material.BumpScaling;

            //result.EmissiveColor = Vector4.One; //new Vector4(0.5f, 0.5f, 0.5f, 1); //material.ColorEmissive.ConvertToLocalVector();
            result.EmissiveColor = material.ColorEmissive.ConvertToLocalVector();
            result.AmbientColor = material.ColorAmbient.ConvertToLocalVector();
            result.DiffuseColor = material.ColorDiffuse.ConvertToLocalVector();
            result.SpecularColor = Vector4.One;  // material.ColorSpecular.ConvertToLocalVector();
            //result.ReflectionColor = material.ColorReflective.ConvertToLocalVector();

            //result.ColorTransparent = material.ColorTransparent.ConvertToLocal();
            //result.IsTwoSided = material.IsTwoSided;
            //result.IsWireFrameEnabled = material.IsWireFrameEnabled;

            result.IsBumpEnabled = material.HasTextureHeight;
            result.IsSpecularEnabled = material.HasColorSpecular && material.HasShininess;
            result.SpecularPower = material.Shininess * 0.1f;
            result.Name = material.Name;
            result.Opacity = material.Opacity;
            result.Reflectivity = material.Reflectivity;

            //result.ShadingMode = (AssimpShadingMode)material.ShadingMode;
            //result.ShininessStrength = material.ShininessStrength;

            Assimp.TextureSlot[] textures = material.GetAllMaterialTextures();

            for (int i = 0; i < textures.Length; i++)
            {
                Assimp.TextureSlot textureSlot = textures[i];
                string replacedContent = model.Path.Replace(model.Game.ContentDirectoryName + "/", "");
                string path = System.IO.Path.Combine(replacedContent, textureSlot.FilePath);

                switch (textureSlot.TextureType)
                {
                    case Assimp.TextureType.Diffuse:
                        result.AddTexturePath(new PTTexturePath(path), true);
                        break;
                    case Assimp.TextureType.Height:
                        result.AddTexturePath(new PTTexturePath(path, PTTexture2D.TextureType.BumpMap), true);
                        break;
                    case Assimp.TextureType.Specular:
                        result.AddTexturePath(new PTTexturePath(path, PTTexture2D.TextureType.SpecularMap), true);
                        break;
                }
            }

            result.UpdateState();
            result.LoadContent(model.Game.ContentManager);

            material.Clear();
            material = null;

            return result;
        }

        /// <summary>
        /// Convert a list of <see cref="Assimp.Material"/> objects to <see cref="PoncheToolkit.Graphics3D.AssimpMaterial"/>.
        /// Add the textures paths as dirty properties from the model loaded, so the engine later update its status.
        /// </summary>
        /// <param name="materials">The list of faces.</param>
        /// <param name="mesh">The mesh containing this material.</param>
        /// <param name="model">The model that this Material is part of.</param>
        /// <returns></returns>
        //internal static List<PTMaterial> ReadMaterials(this List<Assimp.Material> materials, ref PTModel model, ref PTMesh mesh)
        internal static List<PTMaterial> ReadMaterials(this List<Assimp.Material> materials, ref PTModel model)
        {
            List<PTMaterial> result = new List<PTMaterial>();
            //PTMaterial[] resultArray = new PTMaterial[materials.Count];
            for (int i = 0; i < materials.Count; i++)
            {
                PTMaterial mat = materials[i].ConvertToLocal(ref model);
                if (!model.ImportedMaterials.Any(m => m == mat))
                    result.Add(mat);

                mat.Name = (string.IsNullOrEmpty(mat.Name) ? (model.Name + "_mat" + model.ImportedMaterials.Count + result.Count) : mat.Name);
                //mesh.MaterialName = mat.Name;
            }

            return result;
        }
        #endregion

        #region Embedded Textures
        /// <summary>
        /// Convert an <see cref="Assimp.EmbeddedTexture"/> object to <see cref="PoncheToolkit.Graphics2D.PTTexture2D"/>.
        /// </summary>
        /// <param name="texture">The Texture to convert.</param>
        /// <returns></returns>
        public static PTTexture2D ConvertToLocal(this Assimp.EmbeddedTexture texture)
        {
            //Texture2D result = Game11.Instance.ContentManager.LoadTexture2D(texture.NonCompressedData[i].;
            PTTexture2D result = null;

            return result;
        }

        /// <summary>
        /// Convert a list of <see cref="Assimp.EmbeddedTexture"/> objects to <see cref="PoncheToolkit.Graphics2D.PTTexture2D"/>.
        /// </summary>
        /// <param name="textures">The list of textures.</param>
        /// <param name="model">The model that the textures are part of.</param>
        /// <returns></returns>
        public static List<PTTexture2D> ConvertToLocal(this List<Assimp.EmbeddedTexture> textures, ref PTModel model)
        {
            List<PTTexture2D> result = new List<PTTexture2D>();
            for (int i = 0; i < textures.Count; i++)
            {
                result.Add(textures[i].ConvertToLocal());
            }

            return result;
        }
        #endregion

        #region Mesh Animation Attachments
        /// <summary>
        /// Convert an <see cref="Assimp.MeshAnimationAttachment"/> object to <see cref="PoncheToolkit.Graphics3D.MeshAnimationAttachment"/>.
        /// </summary>
        /// <param name="mesh">The MeshAnimationAttachment.</param>
        /// <returns></returns>
        public static MeshAnimationAttachment ConvertToLocal(this Assimp.MeshAnimationAttachment mesh)
        {
            MeshAnimationAttachment result = new MeshAnimationAttachment();
            //result.BiTangents = mesh.BiTangents.ConvertToLocal();
            return result;
        }

        /// <summary>
        /// Convert a list of <see cref="Assimp.MeshAnimationAttachment"/> objects to <see cref="PoncheToolkit.Graphics3D.MeshAnimationAttachment"/>.
        /// </summary>
        /// <param name="meshAnimations">The list of meshAnimations.</param>
        /// <returns></returns>
        public static List<MeshAnimationAttachment> ConvertToLocal(this List<Assimp.MeshAnimationAttachment> meshAnimations)
        {
            List<MeshAnimationAttachment> result = new List<MeshAnimationAttachment>();
            for (int i = 0; i < meshAnimations.Count; i++)
            {
                result.Add(meshAnimations[i].ConvertToLocal());
            }

            return result;
        }
        #endregion

        #region Vertex Weights
        /// <summary>
        /// Convert an <see cref="Assimp.VertexWeight"/> object to <see cref="PoncheToolkit.Graphics3D.VertexWeight"/>.
        /// </summary>
        /// <param name="weight">The <see cref="Assimp.VertexWeight"/> to convert.</param>
        /// <returns></returns>
        public static VertexWeight ConvertToLocal(this Assimp.VertexWeight weight)
        {
            VertexWeight result = new VertexWeight(weight.VertexID, weight.Weight);
            return result;
        }

        /// <summary>
        /// Convert a list of <see cref="Assimp.VertexWeight"/> objects to <see cref="PoncheToolkit.Graphics3D.VertexWeight"/>.
        /// </summary>
        /// <param name="weights">The list of <see cref="Assimp.VertexWeight"/> weights.</param>
        /// <returns></returns>
        public static List<VertexWeight> ConvertToLocal(this List<Assimp.VertexWeight> weights)
        {
            List<VertexWeight> result = new List<VertexWeight>();
            for (int i = 0; i < weights.Count; i++)
            {
                result.Add(weights[i].ConvertToLocal());
            }

            return result;
        }
        #endregion

        #endregion
    }
}
