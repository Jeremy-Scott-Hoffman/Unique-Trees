using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//using System.Numerics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class Branch : MonoBehaviour
{
    public static bool DEBUG_SHOW_LIST_INDEX = false; //enable text when setting to true
    [SerializeField] Sprite normalBranch;
    [SerializeField] Sprite dupeBranch;
    [SerializeField] Sprite dupeBranch_child;

    [SerializeField] Sprite[] rootSprites;
    [SerializeField] Sprite[] rootDupeSprites;
    [SerializeField] Image branchImage;
    public Branch branchPrefab;
    int depth = 1;// Depth 1 = root, 2 = child, 3 = grandchild, etc

    public int posIndex;// Where am I in my parent's child array? What angle am I branching at?
    public int listIndex = 0; // Where am I in my parent's list of children? (Used for dupe-finding solutions)
    public int treeIndex;// Which tree is this part of? (currently: matches rootName[] index, e.g. 0, 1, 2, ...)
                         // To do: Make this match the tree's size maybe?
    public int treeGoalSize
    {
        get
        {
            if (treeIndex == 0)
                return GameManager.Instance.ultraHardMode ? 3 : 4;
            else
                return treeIndex + 3;//treeIndex == 0 ? 4 : 
        } //Can change this to +4 reduce game difficulty
    }
    public int children = 0;
    /*{   //= 0;
        get { return childBranches.Count; }
    }*/
    [SerializeField] Button growBranch_button; // Button to add child
    [SerializeField] Button growBranch_DEAD_button; // Button to add child

    [SerializeField] Button cutBranch_button; // button to remove self
    [SerializeField] Sprite petalSprite;
    [SerializeField] Sprite petalSprite_dupe;
    [SerializeField] Sprite growSprite;
    [SerializeField] Sprite growSprite_dupe;
    public Text nameTag; // Only Root has this!
    public string branchName;// shows its structure, e.g. [1,2,(1,1)]
    public string displayName;// no brackets, e.g.  1,2,(1,1)

    public Branch parent;
    public Branch root; // Controls some of the game logic
    public TreePanel tree; // Mostly controls the UI and stuff.
    Branch[] _childArray = new Branch[MAX_CHILDREN];//lists children by their position. Can be empty!
    [SerializeField]
    Branch[] childArray // Don't use this for game logic, it has empty spaces!
    {
        get { return _childArray; }
        set
        {
            _childArray = value;
            //UpdateChildBranches();
        }
    }

    public const int MAX_CHILDREN = 14;
    public const int MAX_DEPTH = 14;

    public List<Branch> allSubBranches; // Only used by roots? Excludes self
    [SerializeField] List<Branch> _childBranches = new List<Branch>();
    public List<Branch> childBranches
    {
        get { return _childBranches; }
        set
        {
            _childBranches = value;
        }
    }
    public bool TreeHasDupe
    {
        get
        {
            if (tree) return tree.hasDupe;
            else
            {
                Debug.LogError($"no tree {treeIndex} set.");
                return false;
            }
        }
        set
        {
            if (tree)
                tree.hasDupe = value;
            else if (value)
                Debug.LogError("no tree to set as dupe!!");
            else Debug.Log("no tree yet");
        }
    }

    bool isflowering;
    public float randomPetalAngle;//angle for flower petal
    public void SetGrowButton() // Sets sprite AND clickability!
    {
        SetGrowButton(root.allSubBranches.Count == tree.TreeGoalSize());
    }
    public void SetGrowButton(bool toFlower) // Sets sprite AND clickability!
    {

        //print("Set: " + toFlower);
        isflowering = toFlower;

        growBranch_DEAD_button.enabled = !toFlower; // You can't grow from flowers
        growBranch_button.enabled = !toFlower; // Enable last, in case you can't enable inactive objects?

        if (toFlower)
        {
            if (TreeHasDupe)
            {
                growBranch_DEAD_button.gameObject.SetActive(true);
                growBranch_button.gameObject.SetActive(false);
                growBranch_DEAD_button.GetComponent<Image>().sprite = petalSprite_dupe;
                if (!isDupe && !isDupeChild)
                    growBranch_DEAD_button.gameObject.SetActive(false);//no button at all???
            }
            else
            {
                growBranch_DEAD_button.gameObject.SetActive(false);
                growBranch_button.gameObject.SetActive(true);
                growBranch_button.GetComponent<Image>().sprite = petalSprite;
            }
            //growBranch_button.GetComponent<Image>().sprite = TreeHasDupe ? petalSprite_dupe : petalSprite;
            //growBranch_button.transform.localRotation = Quaternion.identity;
            growBranch_button.transform.localRotation = Quaternion.Euler(0, 0, randomPetalAngle);
            growBranch_DEAD_button.transform.localRotation = Quaternion.Euler(0, 0, randomPetalAngle);
        }
        else
        {
            growBranch_button.transform.localRotation = Quaternion.identity;
            growBranch_DEAD_button.transform.localRotation = Quaternion.identity;
            if (isDupe || isDupeChild)
            {
                growBranch_DEAD_button.gameObject.SetActive(true);
                growBranch_button.gameObject.SetActive(false);
                growBranch_DEAD_button.GetComponent<Image>().sprite = growSprite_dupe;//toFlower ? petalSprite_dupe : 
            }
            else
            {
                growBranch_DEAD_button.gameObject.SetActive(false);
                growBranch_button.gameObject.SetActive(true);
                growBranch_button.GetComponent<Image>().sprite = growSprite;
            }
        }
    }
    const bool ROTATE_BUTTONS = false;
    void Update()
    {
        if (ROTATE_BUTTONS && !isflowering)
            growBranch_button.transform.Rotate(222f * Vector3.forward * Time.deltaTime);

        //if (UnityEngine.Random.Range(0, 120 * depth) == 1)
        //    AddBranch();
    }

    public void UpdateChildBranches() // This is currently called by SetAsdupeChild... don't make infinite loop!
    {
        List<Branch> x = new List<Branch>();
        int n = 0;
        for (int i = 0; i < MAX_CHILDREN; i++)
        {
            if (childArray[i] != null)
            {
                x.Add(childArray[i]);
                childArray[i].listIndex = n;
                if (DEBUG_SHOW_LIST_INDEX)
                    childArray[i].growBranch_button.GetComponentInChildren<Text>().text = $"{n}";
                n++;
            }
        }
        children = x.Count;
        //Debug.Log("children: " + children);
        childBranches = x;
    }

    public bool isDupe;//Does this node contain a previous tree? RESET BEFORE EVERY CHECK!
    public bool isDupeChild;//Is this part of a dupe subtree? RESET BEFORE EVERY CHECK!
    public void SetAsDupe(SolutionBranch solution, int subTree = -1)//bool _isDupe, 
    {
        isDupe = true;
        root.TreeHasDupe = true;
        //foreach (Branch b in allSubBranches)
        //    b.isDupeChild = true;
        if (solution.childrenExists == false)
        {
            Debug.LogError("no children in solution?");
            return;
        }
        for (int i = 0; i < solution.children.Length; i++)
        {

            SolutionBranch sol_child = solution.children[i];
            if (sol_child.index < 0 || sol_child.index > childBranches.Count)
            {
                Debug.Log($"Bad sol: {solution.ToString()} {subTree}<{treeGoalSize}");
                Debug.LogError($"R:sol_child.index = {sol_child.index} tree {subTree}<{treeGoalSize}");//childBranches.Count " + childBranches.Count);
            }
            else
            {

                Debug.Assert(childBranches[sol_child.index] != null);
                this.childBranches[sol_child.index].SetAsDupeChild(sol_child);
            }
        }

    }

    public void SetAsDupeChild(SolutionBranch solution)
    {
        isDupeChild = true;
        UpdateChildBranches();
        //branchImage.sprite = dupeBranch child;
        //foreach (Branch b in allSubBranches)
        //    b.isDupeChild = true;
        if (solution.children == null)
        {
            //Debug.LogError($"why null children? Index {solution.index}. Branch {branchName}");
            return;
        }
        for (int i = 0; i < solution.children.Length; i++)
        {
            SolutionBranch sol_child = solution.children[i];
            if (sol_child.index > childBranches.Count)
                Debug.LogError($"C:sol_child.index = {sol_child.index} > childBranches.Count " + childBranches.Count);
            else if (sol_child.index < 0)
                Debug.LogError($"C:sol_child.index = {sol_child.index}, childBranches.Count " + childBranches.Count);
            else
            {

                Debug.Assert(childBranches[sol_child.index] != null);
                this.childBranches[sol_child.index].SetAsDupeChild(sol_child);
            }
        }
        return;

        foreach (SolutionBranch sb in solution.children)
        {
            int i = sb.index;//Which child is 
            this.childBranches[i].SetAsDupeChild(solution.children[i]);
        }
    }

    public void UpdateSprite()
    {
        //Call after updating dupe status
        if (isDupe)
        {
            branchImage.sprite = isRoot ? rootDupeSprites[treeIndex % rootDupeSprites.Length] : dupeBranch;
        }
        else if (isDupeChild)
        {
            branchImage.sprite = dupeBranch_child;
        }
        else
            branchImage.sprite = isRoot ? rootSprites[treeIndex % rootSprites.Length] : normalBranch;

        //SetGrowButton();
    }

    public void AddChild(Branch c)
    {
        for (int i = 0; i < MAX_CHILDREN; i++)
        {
            if (childArray[i] == null)
            {
                childArray[i] = c;
                c.posIndex = i;
                UpdateChildBranches();
                return;
            }
        }
        Debug.LogError("children full");
    }

    public struct SolutionBranch
    {
        public bool isRoot;//is this the root of the solution? If so, should have no index?
        public int index;// What number child am *I*?
        public bool childrenExists;//has children array been initialized?
        public SolutionBranch[] children; // ?!?!?!?!?!?!?!?!?!?!?!?!?!?!? struct or gameobject?
        /*public SolutionBranch()
        {
            children = new SolutionBranch[0];
            index = -77;
        }*/
        public SolutionBranch(int numChildren, bool _isRoot, int _index = -5)
        {
            isRoot = _isRoot;
            children = new SolutionBranch[numChildren];
            childrenExists = true;
            index = _index;
        }

        public bool ContainsIndex(int n)
        {
            foreach (SolutionBranch child in children)
            {
                if (child.index == n)
                    return true;
            }
            return false;
        }

        public String ToString(int recursionLevel = 0)
        {
            if (recursionLevel > MAX_DEPTH + 1)
            {
                Debug.LogError("infinite recursion: SolutionBranch contains itself? " + recursionLevel);
                return "!!";
            }

            int ch = childrenExists ? children.Length : 0;
            if (ch == 0)
                return index.ToString();
            else
            {
                String name = "";
                for (int i = 0; i < ch; i++)
                {
                    name += (i > 0 ? "," : "") + children[i].ToString(recursionLevel + 1);
                }
                name = (!isRoot ? index + "" : "") + "[" + name + "]";

                return name;
            }
        }
    }

    // to-do: THIS IS tHE BIG ONE!!!!     FIND THE SUBTREE!!!!!!
    //PUBLIC INT[]?
    public bool FindSubtree(Branch sub, out SolutionBranch solution, int recursionLevel = 0) // Check if sub is actually a subtree
    {
        Debug.Assert(sub.children == sub.childBranches.Count);
        Debug.Assert(children == childBranches.Count);

        Predicate<Branch> sameBranch = (Branch b) => { return b.posIndex == this.posIndex; };

        Debug.Assert(listIndex >= 0);

        solution = new SolutionBranch( // Solution should contains index of bigger tree?
            sub.children,
            (recursionLevel == 0),
            listIndex);//parent.childBranches.FindIndex(sameBranch));// index=this.index???

        if (sub.children == 0)
        {
            //print("Trivial subset of " + treeNum);// + "." + branchName);
            return true;
        }

        if (sub.children > children)
        {
            //print("Can't be subtree w/ *more* children");
            return false;
        }

        //int[] solution = new int[sub.children];//Which this.children correspond to each sub.child

        Debug.Assert(sub.children == solution.children.Length);

        for (int i = 0; i < sub.children; i++)
            solution.children[i].index = -1;//This means to check 0, 1, 2...
        int sc = 0;//index of solution child (which sub.child we're looking at (to assign it a this.child))
        int loops = 0;//emergency while() stopper
        bool childfound;

        while (sc < sub.children && loops < 999)
        {
            childfound = false;
            int c = solution.children[sc].index;//The current this.child being tried (starts at 0 except when backtracking)
            Debug.Assert(c >= -1);
            while (c + 1 < children)
            {
                c++;
                //print("Check c =" + c);
                bool alreadyUsed_c = false;
                foreach (SolutionBranch sb in solution.children)
                {
                    if (sb.index == c)
                    {
                        alreadyUsed_c = true;
                        break;
                    }
                }
                if (alreadyUsed_c)//solution.ContainsIndex(c))
                {
                    //Already used this child!
                }
                else if (childBranches[c].FindSubtree(sub.childBranches[sc], out SolutionBranch childSolution, recursionLevel + 1))
                {
                    //print("c =" + c);
                    solution.children[sc].index = c;
                    solution.children[sc].children = (SolutionBranch[])childSolution.children.Clone();// To-Do: Was this the fix needed? I forget what this even means
                    //Debug.Log($"c = {c}, childSol.index = {childSolution.index}");//should match?
                    Debug.Assert(c == childSolution.index);
                    sc++;
                    childfound = true;
                    break;//c = 999;//
                }

                //c++;
            }
            if (!childfound)
            {
                if (sc == 0) // Tried all possiblities for the first value, so we're done
                    return false; // To-Do: add backtracking!
                else //backtrack
                {
                    solution.children[sc].index = -1;//After backtracking, check 0, 1, 2...
                    sc--;
                    //backTracking = true;
                    //next_c = solution[sc] + 1;
                }
            }
        }
        //if (solution?[0] != null)
        if (sc > sub.children)
            Debug.LogError("sc > sub.children");
        if (sc >= sub.children) // Intended outcome
        {
            //if (recursionLevel == 0)//Don't print all the boring sub-solutions
            //print("Sol" + recursionLevel + ": " + String.Join(",", solution));
            return true;            //solution found?
        }
        if (loops > 990)
            Debug.LogError("infinite loop! breaking out");
        else
        {
            print("huh? sc =" + sc + ", loops = " + loops);
        }
        return false;
    }


    void Awake()
    {
        if (isRoot) //Don't do anything at start for non-roots: Let the addBranch function handle it!
        {
            root = this;
        }
    }
    public void Start_Manually() // replaced Start()
    {
        if (isRoot) //Don't do anything at start for non-roots: Let the addBranch function handle it!
        {
            root = this;
            UpdateName();
            UpdateStuff();//This hopefully causes sprites to update when game is loaded
            //tree.UpdateInfo();
        }
        else
            Debug.Log("I'm not a root. -" + name);
    }




    public bool isRoot;

    public bool isRoot_calculated
    {
        get { return depth == 1; } // == 0?
    }




    public string _name = "?";//only used during caulcuateName?
    static int loops = 0;//to avoid infinite recursion
    public string calculateName() // Also updates allSubBranches??
    {
        allSubBranches = new List<Branch>();
        //UpdateChildBranches();
        if (depth == 0)
            Debug.LogError(gameObject.name + " at depth 0");
        if (isRoot)
            root = this;
        // RECURSION SAFETY
        if (isRoot)
            loops = 0;//Root branch resets counter
        loops++;
        if (loops > 99)
            return "?";//too many loops!

        Debug.Assert(children == childBranches.Count);

        // The actual function:
        branchName = "";//isRoot ? "R" : "";
        if (children == 0)
            branchName += "1";// the most basic branch is named "1"
        else if (children == 1)
        {
            string childName = childBranches[0].calculateName();
            allSubBranches = new List<Branch> { childBranches[0] };
            allSubBranches.AddRange(childBranches[0].allSubBranches);

            if (int.TryParse(childName, out int result))
            {
                branchName += (result + 1).ToString();
            }
            else
                branchName += childName + "*";
        }
        else // 2 or more children
        {
            List<string> _childNames = new List<string>();
            //name += childBranches[0] ? childBranches[0].calculateName() : "X"; // no comma before the first one
            for (int i = 0; i < children; i++)//each child except the first (0th)
            {
                if (childBranches[i] == null)
                    _childNames.Add("X"); // "X" = can't find this child. Not intended!
                else
                {
                    _childNames.Add(childBranches[i].calculateName());
                    allSubBranches.Add(childBranches[i]);
                    allSubBranches.AddRange(childBranches[i].allSubBranches);//For calculating all subbranches!
                }
            }
            _childNames.Sort();
            for (int i = 0; i < _childNames.Count; i++)//each child except the first (0th)
            {
                branchName += (i > 0 ? "," : "") + _childNames[i];
            }
            displayName = branchName;//display the name without brackets
            if (true)//!isRoot)
            {
                /*if (branchName[0] == '(')
                    branchName = "[" + branchName + "]";
                else*/ //square brackets could cause bugs!
                branchName = "(" + branchName + ")";
            }
            return branchName;
        }
        displayName = branchName;
        return branchName;
    }

    static Vector3 childPosition;//Placement of children relative to parent

    // Instead of random angles for branches, use these angles, scaled by a factor of 3 or so.
    float[] autoAngles = new float[] { 5, -8, 14, -16, -2,
                                        20, -21, 26, -27, 31, -32, 36, -37, 41, -45, 55 };

    public void AddBranch()
    {//float angle){
        if (depth == MAX_DEPTH)
        {
            Debug.Log("max depth");
            return;
        }
        if (children == MAX_CHILDREN)
        {
            Debug.Log("max children");
            return;
        }

        //children++;

        Branch newBranch = Instantiate<Branch>(branchPrefab, this.transform);
        newBranch.root = this.root;//children share same root as parent (een if parent IS root)
        newBranch.tree = this.tree;
        //int child_index = children;//index of added child branch  TO-DO: Can change?
        AddChild(newBranch); // determines index
        float angle = Mathf.Max(3f, 5f - 0.5f * depth) * autoAngles[newBranch.posIndex];//Random.Range(-12, 12);// rotation relative to parent, in degrees

        newBranch.gameObject.transform.localScale = Vector2.one * 0.8f;
        newBranch.gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        //newBranch.growBranch_button.transform.localRotation =
        //    Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(-30f, 30f));//Random angle for growButton
        // ==== BUTTONS =====
        //Deeper red hitboxes are wider but shorter
        //Deeper green hitboxes are larger
        newBranch.cutBranch_button.transform.localScale = new Vector3(Mathf.Pow(1.1f, depth), Mathf.Pow(1f, depth), 1);
        newBranch.growBranch_button.transform.localScale = new Vector3(Mathf.Pow(1.1f, depth), Mathf.Pow(1.1f, depth), 1);
        newBranch.randomPetalAngle = UnityEngine.Random.Range(-20f, 20f);

        childPosition = new Vector3(0, isRoot ? 64 : 61, 0);
        newBranch.gameObject.transform.localPosition = childPosition;
        newBranch.depth = this.depth + 1;
        if (newBranch.depth == MAX_DEPTH)
        {
            newBranch.growBranch_button.image.enabled = false;
            newBranch.growBranch_DEAD_button.image.enabled = false;
        }

        newBranch.parent = this;
        newBranch.treeIndex = this.treeIndex;
        //childArray[child_index] = newBranch;//childBranches.Add(newBranch);
        newBranch.name = "child " + depth + "-" + newBranch.posIndex;
        //UpdateChildBranches(); // <-- this is in the AddChild function
        //print("WAS: " + newBranch.GetComponent<Branch>().branchPrefab.name);
        newBranch.GetComponent<Branch>().branchPrefab = branchPrefab; // This removes the recursion bug.
        UpdateStuff();

    }

    public void MakeVisible(bool visible)
    {
        branchImage.enabled = visible;
        if (visible)
        {
            growBranch_button.image.enabled = true;
            growBranch_DEAD_button.image.enabled = true;
            SetGrowButton();
        }
        else
        {
            //tree.infoText.text = "...";
            growBranch_button.image.enabled = false;
            growBranch_DEAD_button.image.enabled = false;
        }
    }

    public void RemoveChild(Branch child)
    {
        childArray[child.posIndex] = null; //Remove(this);
        UpdateChildBranches();
        //children--;
        UpdateStuff();
        Destroy(child.gameObject);
    }

    public void RemoveBranch()
    {
        parent.RemoveChild(this);
    }



    public void UpdateStuff() // Update given that this branch gained/lost a child EXCEPT FOR TREE.UPDATEINFO!
    {
        if (isRoot)
            root = this;
        root.UpdateName();
        GameManager.Instance.FindDuplicates();
        //root.tree.UpdateInfo();    // Call this after doing ALL updateStuff!
    }

    public void UpdateName()
    {
        if (nameTag == null)
            Debug.LogError(gameObject.name + " has no nametag");
        else
        {
            calculateName();
            nameTag.text = displayName;//removes brackets
        }
    }
}
