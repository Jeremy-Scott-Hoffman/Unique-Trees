using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Handles an entire tree panel.
public class TreePanel : MonoBehaviour
{
    public Branch root;
    public Text numberText;
    public Text infoText;
    public int treeNumber;//this is probably the max size of the tree (3, 4, ...10)
    public int TreeGoalSize()
    {
        return root.treeGoalSize;
    }
    // Start is called before the first frame update
    void Start()
    {
        numberText.text = "(" + TreeGoalSize() + ")";
    }

    // Update is called once per frame
    public void UpdateInfo()
    {
        int treeSize = root.allSubBranches.Count;
        if (treeSize > TreeGoalSize())
            infoText.text = "too large!";
        else if (treeSize < TreeGoalSize())
            infoText.text = "incomplete";
        else
        {
            //if(root.treesDuplicated.Count > 0)
            //infoText.text = "contains tree";
            infoText.text = "VALID SIZE";
        }
    }
}
