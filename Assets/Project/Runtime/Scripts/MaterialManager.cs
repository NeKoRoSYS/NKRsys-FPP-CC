using UnityEngine;

public class MaterialManager : MonoBehaviour
{
    public static MaterialManager Instance;

    [Header("Scene Object Tags")]
    [field : SerializeField] public string[] materialTags;

    private void Awake() => Instance = this;
}