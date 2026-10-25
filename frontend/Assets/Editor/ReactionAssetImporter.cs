using UnityEditor;
using UnityEngine;

// Unity2019 must generate actual Sprite subassets from the standard emoji PNGs.
public sealed class ReactionAssetImporter : AssetPostprocessor {
 void OnPreprocessTexture(){if(assetPath.StartsWith("Assets/Resources/Reactions/") && assetPath.EndsWith(".png"))Configure((TextureImporter)assetImporter);}
 static void Configure(TextureImporter importer){importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=100;importer.spritePivot=new Vector2(.5f,.5f);importer.spriteBorder=Vector4.zero;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.npotScale=TextureImporterNPOTScale.None;importer.filterMode=FilterMode.Bilinear;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=128;}
 public static void Repair(){
  foreach(string key in SprintReactions.Keys){string path="Assets/Resources/Reactions/"+key+".png";var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(importer==null)throw new System.Exception("Missing emoji importer "+path);Configure(importer);importer.SaveAndReimport();var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(sprite==null||sprite.rect.width<64||sprite.rect.height<64)throw new System.Exception("Missing emoji sprite after import "+key);Debug.Log("REACTION_SPRITE_VERIFIED "+key+" "+sprite.rect);}
 }
}
