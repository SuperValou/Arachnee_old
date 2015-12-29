﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class GraphBuilder
{
    private uint idCounter = 1;

    private Dictionary<uint, uint> movieIds = new Dictionary<uint, uint>();
    private Dictionary<uint, uint> artistIds = new Dictionary<uint, uint>();

    public Graph Graph
    {
        get;
        private set;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="coulombRepulsion"></param>
    /// <param name="hookAttraction"></param>
    public GraphBuilder(float coulombRepulsion, float hookeAttraction)
    {
        this.Graph = new Graph(coulombRepulsion, hookeAttraction);
    }


    /// <summary>
    /// Assign a new unique id to the movie
    /// </summary>
    /// <param name="entry"></param>
    public void AddEntryToGraph(Entry entry)
    {
        entry.GraphId = idCounter++;
        this.Graph.Vertices.Add(entry.GraphId, entry);
    }


    /// <summary>
    /// Update the matrix of the graph
    /// </summary>
    /// <param name="c"></param>
    public void AddConnectionToGraph(Connection c)
    {
        if (this.Graph.Edges.Contains(c))
        {
            UnityEngine.GameObject.Destroy(c.gameObject);
            return;
        }

        this.Graph.Edges.Add(c);       

        // update matrix
        // ...
    }

    /// <summary>
    /// Set up the left and right entries for each edges of the graph
    /// </summary>
    internal void InitEdges()
    {
        foreach (Connection e in this.Graph.Edges)
        {
            e.InitEntries(this.Graph.Vertices.Values);
        }
    }
}
