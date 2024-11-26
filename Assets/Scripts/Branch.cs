﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//using System.Numerics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class Branch : MonoBehaviour
{
    [SerializeField] Sprite normalBranch;
    [SerializeField] Sprite dupeBranch;
    [SerializeField] Sprite dupeBranch_child;
    [SerializeField] Image branchImage;
    public Branch branchPrefab;
    int depth = 1;// Depth 1 = root, 2 = child, 3 = grandchild, etc
    public int index;// Where is this in the child array? What angle is it branching at?
    public int treeIndex;// Which tree is this part of? (currently: matches rootName[] index, e.g. 0, 1, 2, ...)
                         // To do: Make this match the tree's size maybe?
    public int treeGoalSize
    {
        get { return treeIndex + 3; } //Can change this to +4 reduce game difficulty
    }
    public int children = 0;
    /*{   //= 0;
        get { return childBranches.Count; }
    }*/
    [SerializeField] Button growBranch; // Button to add child
    [SerializeField] Button cutBranch; // button to remove self
    public Text nameTag; // Only Root has this!
    public string branchName;// shows its structure, e.g. [1,2,(1,1)]
    public string displayName;// no brackets, e.g.  1,2,(1,1)

    public Branch parent;
    public Branch root; // Controls some of the game logic
    public Tree tree; // Mostly controls the UI and stuff.
    [SerializeField] Branch[] _childArray = new Branch[MAX_CHILDREN];//lists children by their position. Can be empty!
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

    public const int MAX_CHILDREN = 5;
    public const int MAX_DEPTH = 10;

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
    public void UpdateChildBranches()
    {
        List<Branch> x = new List<Branch>();
        for (int i = 0; i < MAX_CHILDREN; i++)
        {
            if (childArray[i] != null)
                x.Add(childArray[i]);
        }
        children = x.Count;
        //Debug.Log("children: " + children);
        childBranches = x;
    }

    public bool isDupe;//Does this node contain a previous tree?
    public void SetAsDupe(bool _isDupe)
    {
        isDupe = _isDupe;
        if (_isDupe)
        {
            branchImage.sprite = dupeBranch;
        }
        else
            branchImage.sprite = normalBranch;
    }

    public void AddChild(Branch c)
    {
        for (int i = 0; i < MAX_CHILDREN; i++)
        {
            if (childArray[i] == null)
            {
                childArray[i] = c;
                c.index = i;
                UpdateChildBranches();
                return;
            }
        }
        Debug.LogError("children full");
    }

    public struct SolutionBranch
    {
        int childIndex;
        SolutionBranch[] solutionBranches; // e.g. possible solution =    [ 0,   2[1,0],  1[3,1[1,0]]];
    }

    // to-do: THIS IS tHE BIG ONE!!!!     FIND THE SUBTREE!!!!!!
    //PUBLIC INT[]?
    public bool FindSubtree(Branch sub, int recursionLevel = 0) // Check is sub is actually a subtree
    {
        Debug.Assert(sub.children == sub.childBranches.Count);
        Debug.Assert(children == childBranches.Count);

        if (sub.children == 0)
        {
            //print("Trivial subset of " + treeNum);// + "." + branchName);
            return true;
        }

        int[] solution = new int[sub.children];//Which this.children correspond to each sub.child
        for (int i = 0; i < sub.children; i++)
            solution[i] = -1;
        int sc = 0;//index of solution (which sub.child we're looking at (to assign it a this.child))
        int loops = 0;//emergency while() stopper
        bool childfound;

        while (sc < sub.children && loops < 999)
        {
            childfound = false;
            int c = solution[sc];//The current this.child being tried (starts at 0 except when backtracking)
            while (c + 1 < children)
            {
                c++;
                //print("Check c =" + c);
                if (solution.Contains(c))
                {
                    //Already used this child!
                }
                else if (childBranches[c].FindSubtree(sub.childBranches[sc], recursionLevel + 1))
                {
                    //print("c =" + c);
                    solution[sc] = c;
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
                    solution[sc] = -1;
                    sc--;
                    //backTracking = true;
                    //next_c = solution[sc] + 1;
                }
            }
        }
        //if (solution?[0] != null)

        if (sc >= sub.children)
        {
            if (recursionLevel == 0)//Don't print all the boring sub-solutions
                print("Sol" + recursionLevel + ": " + String.Join(",", solution));
            //solution found?
            return true;
        }
        if (loops > 990)
            Debug.Log("infinite loop! breaking out");
        else
        {
            print("huh? sc =" + sc + ", loops = " + loops);
        }
        return false;
    }

    void Start()
    {
        if (isRoot)
        {
            root = this;
            UpdateName();
        }
    }

    void Update_UNuSED() //debug
    {
        if (UnityEngine.Random.Range(0, 120 * depth) == 1)
            AddBranch();
    }

    public bool isRoot;

    public bool isRoot_calculated
    {
        get { return depth == 1; }
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

    static Vector3 childPosition = new Vector3(0, 80, 0);

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

        //int child_index = children;//index of added child branch  TO-DO: Can change?
        AddChild(newBranch); // determines index
        float angle = 3.2f * autoAngles[newBranch.index];//Random.Range(-12, 12);// rotation relative to parent, in degrees



        newBranch.gameObject.transform.localScale = Vector2.one * 0.8f;
        newBranch.gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, angle);

        // ==== BUTTONS =====
        //Deeper red hitboxes are wider but shorter
        //Deeper green hitboxes are larger
        newBranch.cutBranch.transform.localScale = new Vector3(Mathf.Pow(1.2f, depth), Mathf.Pow(0.9f, depth), 1);
        newBranch.growBranch.transform.localScale = new Vector3(Mathf.Pow(1.2f, depth), Mathf.Pow(1.2f, depth), 1);


        newBranch.gameObject.transform.localPosition = childPosition;
        newBranch.depth = this.depth + 1;
        newBranch.root = this.root;//children share same root as parent (een if parent IS root)
        newBranch.parent = this;
        newBranch.treeIndex = this.treeIndex;
        //childArray[child_index] = newBranch;//childBranches.Add(newBranch);
        newBranch.name = "child " + depth + "-" + index;
        //UpdateChildBranches(); // <-- this is in the AddChild function
        //print("WAS: " + newBranch.GetComponent<Branch>().branchPrefab.name);
        newBranch.GetComponent<Branch>().branchPrefab = branchPrefab; // This removes the recursion bug.
        UpdateStuff();

    }

    public void UpdateStuff()
    {
        root.UpdateName();
        GameManager.instance.FindDuplicates();
        root.tree?.UpdateInfo();
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

    public void RemoveChild(Branch child)
    {
        childArray[child.index] = null; //Remove(this);
        UpdateChildBranches();
        //children--;
        root.UpdateName();
        GameManager.instance.FindDuplicates();
        Destroy(child.gameObject);
    }

    public void RemoveBranch()
    {
        parent.RemoveChild(this);
    }
}
