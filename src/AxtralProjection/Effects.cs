using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AxtralProjection;
/// <summary>Only changes renderer property blocks, never a tree's shared materials.</summary>
internal sealed class PreviewEffects
{
    private readonly Dictionary<int, Highlight> highlights = new Dictionary<int, Highlight>();
    private Material? material;
    private LineRenderer? cone;
    public void Set(List<Target> targets, Vector3 origin, Vector3 forward, float range, float angle, float width)
    {
        if (!material) material = SpellEffects.Material(new Color(0.15f, 1f, 0.3f));
        var ids = new HashSet<int>(targets.Where(t => t.Object).Select(t => t.Object.GetInstanceID()));
        foreach (var id in highlights.Keys.Where(id => !ids.Contains(id)).ToArray()) { highlights[id].Dispose(); highlights.Remove(id); }
        foreach (var target in targets)
            if (target.Object && !highlights.ContainsKey(target.Object.GetInstanceID())) highlights.Add(target.Object.GetInstanceID(), new Highlight(target, material));
        if (!cone) cone = SpellEffects.Line("Axtral aim cone", material, 0.06f);
        var right = new Vector3(forward.z, 0, -forward.x);
        var start = origin + Vector3.up * 0.2f;
        float nearHalf = width / 2, farHalf = nearHalf + range * Mathf.Tan(angle * Mathf.Deg2Rad / 2);
        cone!.positionCount = 5;
        cone.SetPositions(new[] { start - right * nearHalf, start + right * nearHalf,
            start + forward * range + right * farHalf, start + forward * range - right * farHalf,
            start - right * nearHalf });
    }
    public void Clear()
    {
        foreach (var h in highlights.Values) h.Dispose();
        highlights.Clear();
        if (cone) Object.Destroy(cone!.gameObject); cone = null;
    }
    public void Dispose() { Clear(); if (material) Object.Destroy(material); material = null; }
    private sealed class Highlight
    {
        private readonly List<(Renderer renderer, MaterialPropertyBlock original)> blocks = new List<(Renderer, MaterialPropertyBlock)>();
        private readonly GameObject marker;
        public Highlight(Target target, Material? mat)
        {
            foreach (var renderer in target.Object.GetComponentsInChildren<Renderer>())
            {
                var original = new MaterialPropertyBlock(); renderer.GetPropertyBlock(original);
                var tint = new MaterialPropertyBlock(); renderer.GetPropertyBlock(tint);
                tint.SetColor("_Color", new Color(0.15f, 1f, 0.3f)); tint.SetColor("_EmissionColor", new Color(0, 0.7f, 0.1f));
                renderer.SetPropertyBlock(tint); blocks.Add((renderer, original));
            }
            var line = SpellEffects.Line("Axtral eligible target", mat, 0.09f); marker = line.gameObject;
            line.positionCount = 33;
            var pos = target.Position + Vector3.up * 0.25f;
            var points = new Vector3[33];
            for (int i = 0; i < 33; i++) { float a = i * Mathf.PI * 2 / 32; points[i] = pos + new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a)) * 0.8f; }
            line.SetPositions(points);
        }
        public void Dispose()
        {
            foreach (var pair in blocks) if (pair.renderer) pair.renderer.SetPropertyBlock(pair.original);
            if (marker) Object.Destroy(marker);
        }
    }
}
internal static class SpellEffects
{
    public static Material? Material(Color color)
    {
        var shader = Shader.Find("Unlit/Color") ?? Shader.Find("Sprites/Default") ?? Shader.Find("Standard");
        return shader ? new Material(shader) { color = color } : null;
    }
    public static LineRenderer Line(string name, Material? material, float width)
    {
        var obj = new GameObject(name) { layer = 2 };
        var line = obj.AddComponent<LineRenderer>(); line.useWorldSpace = true;
        line.startWidth = width; line.endWidth = width;
        line.sharedMaterial = material;
        line.startColor = Color.white; line.endColor = Color.white;
        return line;
    }
    public static GameObject CreateAxe()
    {
        // TODO(real-axe): replace spectral mesh with a non-networked equipped-axe visual.
        var axe = new GameObject("Axtral astral axe") { layer = 2 };
        var cleanup = axe.AddComponent<GeneratedAssets>();
        var mat = Material(new Color(0.25f, 0.8f, 1f)); cleanup.Material = mat;
        var shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shaft.name = "Astral axe shaft"; shaft.layer = 2; shaft.transform.SetParent(axe.transform, false);
        shaft.transform.localScale = new Vector3(0.09f, 0.65f, 0.09f);
        Object.Destroy(shaft.GetComponent<Collider>());
        shaft.GetComponent<Renderer>().sharedMaterial = mat;
        // A code-generated crescent blade (front/back faces), not a copied game asset.
        var blade = new GameObject("Astral crescent blade") { layer = 2 }; blade.transform.SetParent(axe.transform, false);
        var mesh = new Mesh { name = "Axtral procedural blade" };
        mesh.vertices = new[] { new Vector3(0, 0.6f, 0), new Vector3(0.85f, 0.95f, 0), new Vector3(1.05f, 0.5f, 0), new Vector3(0.85f, 0.05f, 0), new Vector3(0, 0.3f, 0) };
        mesh.triangles = new[] { 0, 1, 2, 0, 2, 4, 4, 2, 3, 2, 1, 0, 4, 2, 0, 3, 2, 4 }; mesh.RecalculateNormals(); mesh.RecalculateBounds(); cleanup.Mesh = mesh;
        blade.AddComponent<MeshFilter>().sharedMesh = mesh; blade.AddComponent<MeshRenderer>().sharedMaterial = mat;
        var trail = axe.AddComponent<TrailRenderer>(); trail.sharedMaterial = mat; trail.time = 0.25f; trail.startWidth = 0.4f; trail.endWidth = 0;
        var wave = Line("Axtral cutting wave", mat, 0.12f); wave.transform.SetParent(axe.transform, false); wave.positionCount = 25;
        return axe;
    }
    public static void MoveAxe(GameObject axe, Vector3 position, Vector3 forward, float distance, float fullAngle)
    {
        axe.transform.position = position;
        axe.transform.rotation = Quaternion.LookRotation(forward) * Quaternion.Euler(0, 0, distance * 100);
        var wave = axe.GetComponentInChildren<LineRenderer>();
        var points = new Vector3[25];
        var origin = position - forward * distance;
        for (int i = 0; i < 25; i++) points[i] = origin + Quaternion.AngleAxis(-fullAngle / 2 + fullAngle * i / 24, Vector3.up) * forward * distance;
        wave.SetPositions(points);
    }
}
public sealed class GeneratedAssets : MonoBehaviour
{
    public Material? Material;
    public Mesh? Mesh;
    private void OnDestroy() { if (Material) Destroy(Material); if (Mesh) Destroy(Mesh); }
}
