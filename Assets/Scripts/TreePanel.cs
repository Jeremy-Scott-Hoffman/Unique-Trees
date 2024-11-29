using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
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


    public enum TreeStatus
    {
        empty,
        valid,
        wrongSize,
        dupe,
        // tooSmall, tooLarge, duped, locked     // maybe use these?
    }

    public TreeStatus status = TreeStatus.empty;

    public void UpdateInfo()
    {
        TreeStatus prevStatus = status;

        root.UpdateName(); // this shouldn't be necessary...
        int treeSize = root.allSubBranches.Count;//Root is not included in size!
        if (treeSize == 0)
        {
            infoText.text = "Make a tree...";
            status = TreeStatus.empty;
        }
        if (treeSize > TreeGoalSize())
        {
            infoText.text = "too large!";
            status = TreeStatus.wrongSize;
            //Debug.LogError("Tree too big!");
        }
        else if (treeSize < TreeGoalSize())
        {
            infoText.text = "incomplete";
            status = TreeStatus.wrongSize;
        }
        else
        {
            //if(root.treesDuplicated.Count > 0)
            //infoText.text = "contains tree";
            status = root.TreeHasDupe ? TreeStatus.dupe : TreeStatus.valid;
            infoText.text = root.TreeHasDupe ? "not unique" : "valid size";
        }

        numberText.text = "(" + TreeGoalSize() + ")";// + Random.Range(0, 10);


        // Updates sprites and such based on status!
        if (true)//status != prevStatus)
        {
            if (true)//status == TreeStatus.valid) // Valid tree
            {
                root.SetGrowButton(treeSize == TreeGoalSize());//true?
                foreach (Branch b in root.allSubBranches)
                {
                    b.SetGrowButton(treeSize == TreeGoalSize());//true?
                }
            }
            else //if (prevStatus == TreeStatus.valid) // Bad tree
            {
                root.SetGrowButton(treeSize == TreeGoalSize());//false?
                foreach (Branch b in root.allSubBranches)
                {
                    b.SetGrowButton(treeSize == TreeGoalSize());//false?
                }
            }
        }
    }


}
