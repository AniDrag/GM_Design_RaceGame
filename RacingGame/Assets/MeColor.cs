using Alteruna;
using UnityEngine;
using UnityEngine.UIElements;

public class MeColor : CommunicationBridge
{
    public Material material;
    public override void Possessed(bool isPossessor, User user)
    {
        enabled = isPossessor;

        if (isPossessor)
            material.color = Color.green;
        else
            material.color = Color.red;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

}
