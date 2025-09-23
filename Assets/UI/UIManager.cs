using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager singleton;
    public static event Action<Environment> OnEnvironmentSet;

    [SerializeField] private GameObject content;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(this.content != null);
        Debug.Assert(singleton == null);
        singleton = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            this.content.SetActive(true);
    }
}
