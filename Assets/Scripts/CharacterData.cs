using UnityEngine;

[CreateAssetMenu(menuName = "Character Select/Character")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public Sprite portrait;
    public GameObject bikePrefab;
}
