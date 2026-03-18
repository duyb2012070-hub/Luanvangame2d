using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class TextWave : MonoBehaviour
{
    public float amplitude = 10f;
    public float frequency = 2f;

    TMP_Text textMesh;

    void Start()
    {
        textMesh = GetComponent<TMP_Text>();
    }

    void Update()
    {
        textMesh.ForceMeshUpdate();
        var textInfo = textMesh.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible) continue;

            var verts = textInfo.meshInfo[textInfo.characterInfo[i].materialReferenceIndex].vertices;
            int index = textInfo.characterInfo[i].vertexIndex;

            float offset = Mathf.Sin(Time.time * frequency + i * 0.5f) * amplitude;

            verts[index + 0].y += offset;
            verts[index + 1].y += offset;
            verts[index + 2].y += offset;
            verts[index + 3].y += offset;
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            textMesh.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
}