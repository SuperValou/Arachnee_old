﻿using System;
using SimpleJSON;
using UnityEngine;

public class ArtistRetriever : GraphElementRetriever
{
    protected override string GetQuery(string entryId)
    {
        return Constants.ArtistUrl + entryId + "?" + Credentials.ApiKey;
    }

    protected override object BuildResult(JSONNode node)
    {
        if (node["profile_path"].Value == "null")
        {
            return Entry.DefaultEntry;
        }
        var artist = new Artist(long.Parse(node["id"].Value));
        artist.PosterPath = node["profile_path"].Value;

        var name = node["name"].Value;
        int idx = name.LastIndexOf(' ');
        if (idx < 0)
        {
            artist.LastName = name;
            // also known as...
        }
        else
        {
            artist.FirstName = name.Substring(0, idx);
            artist.LastName = name.Substring(idx + 1);
        }
        
        return artist;
    }
}

