using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PlayerPathController : MonoBehaviour {

    private List<Transform> listNode = new List<Transform>();
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        List<Transform> pathtranfroms = new List<Transform>(GetComponentsInChildren<Transform>());

        foreach(Transform i in pathtranfroms)
        {
            if (i != transform)
                listNode.Add(i);
        }

        Debug.Log(listNode.Count);

        for(int i = 0; i < listNode.Count; i++)
        {
            Vector3 currentNode = listNode[i].position;
            Vector3 previousNode = Vector3.zero;
            if (i == 0)
                previousNode = listNode[listNode.Count - 1].position;
            else
                previousNode = listNode[i - 1].position;

            Gizmos.DrawLine(previousNode, currentNode);
            Gizmos.DrawSphere(currentNode, 0.5f);
        }
        
    }
}
