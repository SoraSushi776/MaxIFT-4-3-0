using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;

namespace DancingLineFanmade.Editor
{
    [ScriptedImporter(version: 1, ext: "osu")]
    public class OszImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            byte[] bytes = File.ReadAllBytes(ctx.assetPath);
            string textContent = System.Text.Encoding.UTF8.GetString(bytes);
            TextAsset textAsset = new TextAsset(textContent);

            ctx.AddObjectToAsset("main", textAsset);
            ctx.SetMainObject(textAsset);
        }
    }
}