using MLAPI;
using MLAPI.Messaging;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GenericPopupController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void Awake()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        transform.SetParent(GameObject.Find("Canvas").transform);
    }
    public void Show(string text, float delay)
    {
        var textObj = transform.GetChild(0).gameObject;
        textObj.GetComponent<TextMeshProUGUI>().text = text;
        textObj.SetActive(true);
        StartCoroutine(KillAfter(delay));
    }
    //[ClientRpc]
    //public void ShowClientRpc(string text, float delay)
    //{
    //    var textObj = transform.GetChild(0).gameObject;
    //    textObj.GetComponent<TextMeshProUGUI>().text = text;
    //    textObj.SetActive(true);
    //    StartCoroutine(KillAfter(delay));
    //}
    private IEnumerator KillAfter(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
