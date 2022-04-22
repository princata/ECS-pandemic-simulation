/*
 * This script generates an age for an agent, according to the age distribution in the city of Turin
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class AgentFeatures
{
    private static int[] minAge = new int[] { 0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95 };
    private static int[] maxAge = new int[] { 4, 9, 14, 19, 24, 29, 34, 39, 44, 49, 54, 59, 64, 69, 74, 79, 84, 89, 94, 99 };

    private static float[] menComfortable = new float[] { 1.393f, 1.458f, 1.462f, 1.393f, 1.359f, 1.330f };
    private static float[] womenComfortable = new float[] { 1.407f, 1.415f, 1.391f, 1.395f, 1.296f, 1.272f };
    private static float malePopulationPercentage = Human.conf.malePercentage;

    public enum AgentGender
    {
        None,
        Male,
        Female,
        Any
    }

    public static int GetAgentAge(int[] inputAge)
    {
        //returns agent age according to age distribution in Turin
        UnityLoadedDice loadedDie = new UnityLoadedDice(inputAge);

        int index = loadedDie.NextValue();
        return UnityEngine.Random.Range(minAge[index], maxAge[index]);

    }

    public static AgentGender GetAgentGender()
    {
        float maleFemale = UnityEngine.Random.Range(0.0f, 1.0f);
        malePopulationPercentage = malePopulationPercentage / 100f;
        if (maleFemale <= malePopulationPercentage)
        {
            return AgentGender.Male;
        }
        else
        {
            return AgentGender.Female;
        }
    }

    public static float GetSpeedForAgeComfortable(int age, AgentGender agentGender = AgentGender.Any)
    {
        //returns agent speed, according to their average speed for their age. Result is given in m/s

        int decade = age / 10 - 1;
        if (decade > 5)
            decade = 5;
        if (decade < 0)
            decade = 0;

        if (agentGender == AgentGender.Any)
        {
            agentGender = GetAgentGender();
        }

        if (agentGender == AgentGender.Male)
        {
            return menComfortable[decade];
        }
        else if (agentGender == AgentGender.Female)
        {
            return womenComfortable[decade];
        }
        else
        {
            Debug.LogError("ERROR: Invalid gender!");
            return 0f;
        }


    }

}


public class UnityLoadedDice
{
    //protected Random random = new Random();
    protected List<long> prob;
    protected List<int> alias;
    protected long total;
    protected int n;
    protected bool even;


    public UnityLoadedDice(IEnumerable<int> probabilities)
    {
        // Raise an error if nil
        if (probabilities == null) throw new ArgumentNullException("probs");
        this.prob = new List<long>();
        this.alias = new List<int>();
        this.total = 0;
        this.even = false;
        var small = new List<int>();
        var large = new List<int>();
        var tmpprobs = new List<long>();
        foreach (var p in probabilities)
        {
            tmpprobs.Add(p);
        }
        this.n = tmpprobs.Count;
        // Get the max and min choice and calculate total
        long mx = -1, mn = -1;
        foreach (var p in tmpprobs)
        {
            if (p < 0) throw new ArgumentException("probs contains a negative probability.");
            mx = (mx < 0 || p > mx) ? p : mx;
            mn = (mn < 0 || p < mn) ? p : mn;
            this.total += p;
        }
        // We use a shortcut if all probabilities are equal
        if (mx == mn)
        {
            this.even = true;
            return;
        }
        // Clone the probabilities and scale them by
        // the number of probabilities
        for (var i = 0; i < tmpprobs.Count; i++)
        {
            tmpprobs[i] *= this.n;
            this.alias.Add(0);
            this.prob.Add(0);
        }
        // Use Michael Vose's alias method
        for (var i = 0; i < tmpprobs.Count; i++)
        {
            if (tmpprobs[i] < this.total)
                small.Add(i); // Smaller than probability sum
            else
                large.Add(i); // Probability sum or greater
        }
        // Calculate probabilities and aliases
        while (small.Count > 0 && large.Count > 0)
        {
            var l = small[small.Count - 1]; small.RemoveAt(small.Count - 1);
            var g = large[large.Count - 1]; large.RemoveAt(large.Count - 1);
            this.prob[l] = tmpprobs[l];
            this.alias[l] = g;
            var newprob = (tmpprobs[g] + tmpprobs[l]) - this.total;
            tmpprobs[g] = newprob;
            if (newprob < this.total)
                small.Add(g);
            else
                large.Add(g);
        }
        foreach (var g in large)
            this.prob[g] = this.total;
        foreach (var l in small)
            this.prob[l] = this.total;
    }




    public int NextValue()
    {
        int i = UnityEngine.Random.Range(0, this.n);
        int anotherRandomNumber = UnityEngine.Random.Range(0, (int)this.total);

        return (this.even || anotherRandomNumber < this.prob[i]) ? i : this.alias[i];
    }


    private int Count
    {
        get
        {
            return this.n;
        }
    }

}