using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

public class CubeRotate : MonoBehaviour
{
    [SerializeField]private float speed = 100;

    private UIManager uiManager = null;

    private void Awake()
    {
        uiManager = FindFirstObjectByType<UIManager>();
    }

    private void Update() {
        transform.Rotate(Vector3.forward * Time.deltaTime * speed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"{gameObject.name} Hit Player");
            if(uiManager != null)
                uiManager.RefreshGoldHitCount();
            Destroy(gameObject);
        }
    }
}
