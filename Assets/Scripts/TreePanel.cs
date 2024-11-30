using System.Collections;
using System.Collections.Generic;
//using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

// Handles an entire tree panel.
public class TreePanel : MonoBehaviour
{
    public Branch root;
    public Text numberText;
    public Text infoText;
    [SerializeField] Image panelImage;
    [SerializeField] Sprite[] desert_day;
    [SerializeField] Sprite[] desert_night;
    public int treeNumber;//the index of the tree (0, 1, 2... 10?)
    public int TreeGoalSize()
    {
        return root.treeGoalSize;
    }
    // Start is called before the first frame update
    void Start_DO_NOT_USE()// Let GameManager handle that stuff
    {
        //numberText.text = "(" + TreeGoalSize() + ")";
    }

    public void SetPanel(bool night)
    {
        if (night)
            panelImage.sprite = desert_night[treeNumber % desert_night.Length];
        else
            panelImage.sprite = desert_day[treeNumber % desert_day.Length];
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

    public bool IsUnlocked = false;
    public void Unlock()
    {
        IsUnlocked = true;
        root.MakeVisible(true);
        UpdateInfo();
    }

    public bool hasDupe = false;
    public void UpdateInfo() // Updates this one tree?
    {
        if (!IsUnlocked)
        {
            numberText.text = "";
            root.MakeVisible(false);
            return;
        }
        //root.MakeVisible(true); //Redundant


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

        if (status == TreeStatus.valid)
            numberText.text = "" + TreeGoalSize() + "";
        else if (treeSize > 0)
        {
            if (GameManager.Instance.SHOW_PROGRESS)
                numberText.text = $"{treeSize} / " + TreeGoalSize() + "";// + Random.Range(0, 10);
            else
                numberText.text = "(" + TreeGoalSize() + ")";
        }
        else
            numberText.text = "(" + TreeGoalSize() + ")";


        // Updates sprites and such based on status!
        if (true)//status != prevStatus)
        {

            root.SetGrowButton(treeSize == TreeGoalSize());//true?
            foreach (Branch b in root.allSubBranches)
            {
                b.SetGrowButton(treeSize == TreeGoalSize());//true?
            }
        }
    }


}
