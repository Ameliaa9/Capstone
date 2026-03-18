using UnityEngine;

public class TouchFuzzy : MonoBehaviour
{
    public RadioPlayerMP3 radioMP3;
    public string songNameHere;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Bike"))
        {
            radioMP3.InstanceNewSong(songNameHere);
        }
    }
}
