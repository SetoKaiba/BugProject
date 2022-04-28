using System.Collections;
using System.Collections.Generic;
using ConsoleApp1;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var url_ssl = "wss://live.kaiba.net:8443/live-bilibili/5619408/0130ab9c-a07b-4465-bc89-a6f59ba4c5de";
        Program.RunClientAsync(url_ssl);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
