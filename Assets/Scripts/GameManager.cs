using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

// Singleton class that handles inter-tree behavior.  
//To-do: Make a new script for handling tree stuff?  This should only handle unlocks and UI and stuff?
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    public float lastCheck = 0;
    public float updateFrequency = 1f;//How often we check for dupes
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))//Time.time > lastCheck + updateFrequency)
        {
            print("Dupe Check (D)");
            lastCheck = Time.time;
            FindDuplicates();
        }
    }

    public Branch[] allRoots;// All trees (roots)
    public List<Branch> allBranches;// All branches

    void Start()
    {
        // Set up the treenum variable, so each tree knows where is is
        for (int i = 0; i < allRoots.Length; i++)
        {
            allRoots[i].treeNum = i;
        }
    }

    public void FindDuplicates()
    {
        //print(".");
        //List<string> names_so_far = new List<string>();
        List<string> rootNames = new List<string>();
        // Look at all branches, see if any are duplicated!

        allBranches = allRoots.ToList<Branch>();// To do: Check allBranches, to see if they match allRoots

        foreach (Branch r in allRoots)
        {
            allBranches.AddRange(r.allSubBranches);
            rootNames.Add(r.branchName);
        }
        //Debug.Log("Checking " + allBranches.Count + " branches.");

        foreach (Branch b in allBranches) // Check each branch to see if it contains a previous root
        {
            bool _isDupe = false;
            string currentName = b.branchName;
            if (currentName != "1") // don't check empty root
            {
                //foreach (string s in rootNames)
                for (int i = 0; i < b.treeNum; i++) // Check all PREVIOUS roots
                {
                    //if (currentName == rootNames[i])
                    if (b.FindSubtree(allRoots[i]))
                    {
                        _isDupe = true;
                        //Debug.Log("DUPLICATE FOUND: " + currentName);
                    }
                }
                //names_so_far.Add(currentName);
            }
            b.SetAsDupe(_isDupe);
        }
    }
}
