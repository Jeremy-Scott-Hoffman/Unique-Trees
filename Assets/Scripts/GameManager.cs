using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

// Singleton class that handles inter-tree behavior.  
//To-do: Make a new script for handling tree stuff?  This should only handle unlocks and UI and stuff?
public class GameManager : MonoBehaviour
{
    static GameManager _instance;
    public static GameManager Instance
    {
        set { _instance = value; }
        get
        {
            if (!_instance)
                _instance = GameObject.FindObjectOfType<GameManager>();
            return _instance;
        }
    }
    // Start is called before the first frame update
    void Awake()
    {
        _instance = this;
    }

    public bool autoCheck = false;
    public float lastCheck = 0;
    public float updateFrequency = 1f;//How often we check for dupes
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))//Time.time > lastCheck + updateFrequency)
        {
            print("AutoCheck toggled");
            autoCheck = !autoCheck;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            print("Dupe Check (D)");
            FindDuplicates();
        }
        if (autoCheck && Time.time > lastCheck + updateFrequency)
        {
            //print("Dupe Check");
            lastCheck = Time.time;
            FindDuplicates();
        }
    }

    public Branch[] allRoots;// All trees (roots)
    public TreePanel[] allTrees;
    public List<Branch> allBranches;// All branches

    void Start()
    {
        allRoots = new Branch[allTrees.Length];
        // Set up the treenum variable, so each tree knows where is is
        for (int i = 0; i < allTrees.Length; i++)
        {
            allRoots[i] = allTrees[i].root;
            allRoots[i].treeIndex = i;
        }
    }

    public void FindDuplicates() // Check ALL branches of ALL trees for subtrees within them.
    {
        //print(".");
        //List<string> names_so_far = new List<string>();
        List<string> rootNames = new List<string>();
        // Look at all branches, see if any are duplicated!


        // =========== Just some variable-setting stuff, might not be needed =========
        allBranches = allRoots.ToList<Branch>();// To do: Check allBranches, to see if they match allRoots

        foreach (Branch r in allRoots)
        {
            r.UpdateChildBranches();// This shouldn't be necessary, but it was?
            allBranches.AddRange(r.allSubBranches);
            rootNames.Add(r.branchName);
        }

        // ========== check EVERY BRANCH to see if it contains a prev tree ==========
        foreach (Branch checkTree in allRoots)
        {
            List<Branch> checkTree_allBranches = new List<Branch>();
            checkTree_allBranches.Add(checkTree);//Let's check the roots first!
            checkTree_allBranches.AddRange(checkTree.allSubBranches);//Check all subbranches...

            //Debug.Log("Checking " + allBranches.Count + " branches.");

            foreach (Branch check_b in checkTree_allBranches) // Reset dupe status before recalculating!
            {
                check_b.isDupe = false;
                check_b.isDupeChild = false;
                check_b.TreeHasDupe = false;
            }

            foreach (Branch check_b in checkTree_allBranches) // Check each branch to see if it contains a previous root
            {
                bool _isDupe = false;
                int subtreeFound = -99;
                Branch.SolutionBranch solution = new Branch.SolutionBranch();//if is_dupe, this is what sub-branches are in that subtree
                string currentName = check_b.branchName;
                if (currentName != "1") // don't check empty branches against empty roots, that's just silly.
                {
                    //foreach (string s in rootNames)
                    for (int i = 0; i < check_b.treeIndex; i++) // Check all PREVIOUS roots
                    {
                        if (rootNames[i] == "1")
                        {
                            //print("Ignore empty tree " + i);
                            continue;
                        }

                        //if (currentName == rootNames[i])
                        if (check_b.FindSubtree(allRoots[i], out solution))
                        {
                            //print("" + check_b.treeNum + "." + check_b.branchName + " --> tree " + i);
                            _isDupe = true;
                            print($"Solution: {solution.ToString()} for {i}<<b>{check_b.treeGoalSize}</b>");
                            subtreeFound = allRoots[i].treeGoalSize;
                            break;
                            //print("contains tree " + i);
                            //Debug.Log("DUPLICATE FOUND: " + currentName);
                        }
                        //else
                        //print("" + check_b.treeNum + "." + check_b.branchName + " NO tree " + i);
                    }
                    //names_so_far.Add(currentName);
                }
                if (_isDupe)
                    check_b.SetAsDupe(solution, subtreeFound);//check_b.treeGoalSize);//solution not used if _isDupe == false
            }


            foreach (Branch b in checkTree_allBranches) // Reset dupe status before recalculating!
            {
                b.UpdateSprite();
            }
        }
    }
}
