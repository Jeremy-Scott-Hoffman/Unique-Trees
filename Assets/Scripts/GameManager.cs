using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
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
    public Text hardModeText;
    public void ToggleHardMode()
    {
        ultraHardMode = !ultraHardMode;
        hardModeText.text = ultraHardMode ? "Ultra Challenge is ON" : "Ultra Challenge is OFF";
        allTrees[0].UpdateInfo();
    }
    // Update is called once per frame
    void Update()
    {
        bool shifty = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        // ======================== OPTIONS AND CHEAT CODES =============
        if (shifty && Input.GetKeyDown(KeyCode.W))
            YouWin();

        if (shifty && Input.GetKeyDown(KeyCode.U))
            ToggleHardMode();

        if (shifty && Input.GetKeyDown(KeyCode.P))
            SHOW_PROGRESS = !SHOW_PROGRESS;

        // =============== DEBUG only ========================
        if (shifty && Input.GetKeyDown(KeyCode.A))//Time.time > lastCheck + updateFrequency)
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
    public bool ultraHardMode = false;
    public bool SHOW_PROGRESS = false;
    public Branch[] allRoots;// All trees (roots)
    public TreePanel[] allTrees;
    public List<Branch> allBranches;// All branches

    void Start()
    {
        HideWinScreen();

        allRoots = new Branch[allTrees.Length];
        // Set up the treenum variable, so each tree knows where is is
        for (int i = 0; i < allTrees.Length; i++)
        {
            allRoots[i] = allTrees[i].root;
            allRoots[i].treeIndex = i;
            allTrees[i].treeNumber = i;
        }

        foreach (TreePanel panel in allTrees)
            panel.SetPanel(false);//daytime desert panels (alternating)  Do this AFTER treeNumber = i


        allTrees[0].IsUnlocked = true;//secret tree
        allTrees[1].IsUnlocked = true;// First tree (4)


        foreach (Branch r in allRoots)
        {
            r.root = r;//root.root = itself
        }
        foreach (Branch root in allRoots)
        {
            root.Start_Manually();
        }
        foreach (Branch root in allRoots) // or Tree tree in allTrees
        {
            root.tree.UpdateInfo();
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
            r.TreeHasDupe = false;
        }

        // ========== check EVERY BRANCH, tree by tree, to see if it contains a prev tree ==========
        foreach (Branch checkTree in allRoots)
        {
            List<Branch> checkTree_allBranches = new List<Branch>();
            checkTree_allBranches.Add(checkTree);//Let's check the roots first!
            checkTree_allBranches.AddRange(checkTree.allSubBranches);//Check all subbranches...

            //Debug.Log($"T {checkTree.treeIndex}: Check " + checkTree_allBranches.Count + " branches.");

            foreach (Branch check_b in checkTree_allBranches) // Reset dupe status before recalculating!
            {
                check_b.isDupe = false;
                check_b.isDupeChild = false;
                //check_b.tree.TreeHasDupe = false;
            }

            foreach (Branch check_b in checkTree_allBranches) // Check each branch to see if it contains a previous tree
            {
                bool _isDupe = false;
                int subtreeFound = -99;// Which tree is a match for check_branch?
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
                            //print($"Solution: {solution.ToString()} for {i}<<b>{check_b.treeGoalSize}</b>");
                            subtreeFound = allRoots[i].treeGoalSize;
                            break; // Stop checking THIS root for dupes.
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

            foreach (Branch root in allRoots)
                root.tree.UpdateInfo(); // This should sync everything as needed?


            foreach (Branch b in checkTree_allBranches) // Reset dupe status before recalculating!
            {
                b.UpdateSprite();
            }
        }

        CheckUnlock(); // This should sync everything as needed?

    }



    public bool game_won = false;

    public bool CheckUnlock() // Check this tree to see if we should unlock the NEXT tree
    {
        int nextUnlock = -99;
        for (int i = 1; i < allTrees.Length; i++)
        {
            if (allTrees[i].IsUnlocked == false)
            {
                nextUnlock = i;
                break;
            }
        }
        if (nextUnlock < 0)// Try to unlock the win screen
        {
            if (game_won)
            {
                print("nothing to unlock!");
                return false;
            }

            for (int i = 1; i < allTrees.Length; i++)
            {
                //print("Win? T:" + nextUnlock + ", Tree #" + i + " = " + allTrees[i].status);
                if (allTrees[i].status != TreePanel.TreeStatus.valid)
                {
                    print("Didn't win, due to Tree #" + i);
                    return false;
                }

            }
            YouWin();
            return true;
        }

        for (int i = 1; i < nextUnlock; i++)
        {
            //print("T:" + nextUnlock + ", Tree #" + i + " = " + allTrees[i].status);
            if (allTrees[i].status != TreePanel.TreeStatus.valid)
            {
                //print("T:" + nextUnlock + "not unlocked due to Tree #" + i);
                return false;
            }
        }

        print("T:" + nextUnlock + " Unlocked");
        allTrees[nextUnlock].Unlock();// Unlock next tree!


        return true;
    }

    //[SerializeField] Button hideTitle_button;
    [SerializeField] GameObject[] win_stuff;
    [SerializeField] GameObject[] not_win_stuff;
    void YouWin()
    {
        game_won = true;
        foreach (GameObject w in win_stuff) w.SetActive(true);
        foreach (GameObject l in not_win_stuff) l.SetActive(false);
        foreach (TreePanel panel in allTrees)
            panel.SetPanel(true);//night time!
    }

    void HideWinScreen()
    {
        game_won = false;
        foreach (GameObject w in win_stuff) w.SetActive(false);
        foreach (GameObject l in not_win_stuff) l.SetActive(true);

    }
}
