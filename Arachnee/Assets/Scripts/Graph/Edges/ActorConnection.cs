﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ActorConnection : Connection
{
    internal override void InitEntries(Dictionary<uint, Entry>.ValueCollection valueCollection)
    {
        foreach (Entry e in valueCollection)
        {
            // left is artist
            if (e is Artist && e.DatabaseId == this.InitLeftId)
            {
                this.Left = e;
                continue;
            }
            // right is movie
            else if (e is Movie && e.DatabaseId == this.InitRightId)
            {
                this.Right = e;
                continue;
            }
        }
        Debug.Log("Actor " + Left.DatabaseId + " Movie " + Right.DatabaseId);
    }
}
