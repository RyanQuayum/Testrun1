using UnityEngine;

public class Interactable : MonoBehaviour
{
    public void Interact()
    {
        Debug.Log(gameObject.name + " clicked!");
    }
}