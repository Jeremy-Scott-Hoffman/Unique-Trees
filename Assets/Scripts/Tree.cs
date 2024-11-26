using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Handles an entire tree panel.
public class Tree : MonoBehaviour
{
    public Branch root;
    public Text numberText;
    public Text infoText;
    public int treeNumber;//this is probably the max size of the tree (3, 4, ...10)
    // Start is called before the first frame update
    void Start()
    {
        numberText.text = "(" + treeNumber + ")";
    }

    // Update is called once per frame
    void UpdateInfo()
    {
        int treeSize = root.allSubBranches.Count;
        if (treeSize > treeNumber)
            infoText.text = "too large!";
        else if (treeSize < treeNumber)
            infoText.text = "incomplete";
        else
        {
            //if(root.treesDuplicated.Count > 0)
            //infoText.text = "contains tree";
            infoText.text = "VALID SIZE";
        }
    }
}
